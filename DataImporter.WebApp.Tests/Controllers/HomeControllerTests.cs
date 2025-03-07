using DataImporter.Interfaces;
using DataImporter.Models;
using DataImporter.TestBase;
using DataImporter.WebApp.Controllers;
using DataImporter.WebApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Http;

namespace DataImporter.WebApp.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests : BaseAutoMock<HomeController>
    {
        private const string ErrorMessage = "Cannot read data from file CSV.";
        private string _mockWebRootPath;
        private string _mockCsvFilePath;

        [OneTimeSetUp]
        public void TestSetup()
        {
            _mockWebRootPath = Path.Combine(Path.GetTempPath(), "wwwroot");
            _mockCsvFilePath = Path.Combine(_mockWebRootPath, "sampleSheet.csv");

            if (!Directory.Exists(_mockWebRootPath))
            {
                Directory.CreateDirectory(_mockWebRootPath);
            }

            File.WriteAllText(_mockCsvFilePath, "Date,Price\n2024-01-01 00:00,10\n2024-01-01 00:30,20");

            base.BaseSetup();
        }

        protected override void InitialiseMocks()
        {
            // Setup WebHostEnvironment
            var mockWebHostEnvironment = GetMock<IWebHostEnvironment>();
            mockWebHostEnvironment
                .Setup(env => env.WebRootPath)
                .Returns(_mockWebRootPath);

            // Setup default behavior for MarketDataService
            var mockMarketDataService = GetMock<IMarketDataService>();
            mockMarketDataService
                .Setup(service => service.ReadDataFromCsv(It.IsAny<string>()))
                .Returns([]);
        }

        [OneTimeTearDown]
        public void TestCleanup()
        {
            if (File.Exists(_mockCsvFilePath))
            {
                File.Delete(_mockCsvFilePath);
            }

            if (Directory.Exists(_mockWebRootPath))
            {
                Directory.Delete(_mockWebRootPath, true);
            }

            base.BaseTearDown();
        }

        [Test]
        public void Index_ValidData_ReturnsViewWithDataInViewBag()
        {
            // Arrange
            var testDate = DateTime.Now;
            var marketDataList = new List<MarketData>
            {
                new MarketData { Date = testDate, MarketPrice = 10 },
                new MarketData { Date = testDate.AddMinutes(30), MarketPrice = 20 }
            };

            var mockMarketDataService = GetMock<IMarketDataService>();

            mockMarketDataService
                .Setup(service => service.ReadDataFromCsv(It.Is<string>(path => path == _mockCsvFilePath)))
                .Returns(marketDataList);

            mockMarketDataService
                .Setup(service => service.GetMinimumPrice(It.IsAny<List<MarketData>>()))
                .Returns(10m);

            mockMarketDataService
                .Setup(service => service.GetMaximumPrice(It.IsAny<List<MarketData>>()))
                .Returns(20m);

            mockMarketDataService
                .Setup(service => service.GetAveragePrice(It.IsAny<List<MarketData>>()))
                .Returns(15m);

            mockMarketDataService
                .Setup(service => service.GetMostExpensiveHourWindow(It.IsAny<List<MarketData>>()))
                .Returns((30m, testDate));

            // Act
            var result = ClassUnderTest.Index() as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewData["MarketData"], Is.EqualTo(marketDataList));
                Assert.That(result.ViewData["MinPrice"], Is.EqualTo(10m));
                Assert.That(result.ViewData["MaxPrice"], Is.EqualTo(20m));
                Assert.That(result.ViewData["AveragePrice"], Is.EqualTo(15m));
                Assert.That(result.ViewData["MostExpensiveHourPrice"], Is.EqualTo(30m));
                Assert.That(result.ViewData["MostExpensiveHourStart"], Is.EqualTo(testDate));
            });
        }

        [Test]
        public void Index_FileNotFound_ReturnsViewWithFileNotFoundError()
        {
            // Arrange
            var expectedErrorMessage = "File not found: CSV file not found";
            GetMock<IMarketDataService>()
                .Setup(service => service.ReadDataFromCsv(It.Is<string>(path => path == _mockCsvFilePath)))
                .Throws(new FileNotFoundException("CSV file not found", _mockCsvFilePath));

            // Act
            var result = ClassUnderTest.Index() as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewData["ErrorMessage"], Is.EqualTo(expectedErrorMessage));
                Assert.That(result.ViewData["MarketData"], Is.Null);
            });
        }

        [Test]
        public void Index_InvalidData_ReturnsViewWithDataError()
        {
            // Arrange
            var expectedErrorMessage = "Data error: The CSV file is empty";
            GetMock<IMarketDataService>()
                .Setup(service => service.ReadDataFromCsv(It.Is<string>(path => path == _mockCsvFilePath)))
                .Throws(new InvalidDataException("The CSV file is empty"));

            // Act
            var result = ClassUnderTest.Index() as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewData["ErrorMessage"], Is.EqualTo(expectedErrorMessage));
                Assert.That(result.ViewData["MarketData"], Is.Null);
            });
        }

        [Test]
        public void Index_UnexpectedError_ReturnsViewWithUnexpectedError()
        {
            // Arrange
            var exceptionMessage = "Unexpected test error";
            var expectedErrorMessage = $"An unexpected error occurred: {exceptionMessage}";
            GetMock<IMarketDataService>()
                .Setup(service => service.ReadDataFromCsv(It.Is<string>(path => path == _mockCsvFilePath)))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = ClassUnderTest.Index() as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewData["ErrorMessage"], Is.EqualTo(expectedErrorMessage));
                Assert.That(result.ViewData["MarketData"], Is.Null);
            });
        }

        [Test]
        public void Privacy_ReturnsViewResult()
        {
            var result = ClassUnderTest.Privacy();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Error_ReturnsViewResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "test-trace-id"
            };
            
            ClassUnderTest.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = ClassUnderTest.Error();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ViewResult>());
                var viewResult = result as ViewResult;
                Assert.That(viewResult?.Model, Is.InstanceOf<ErrorViewModel>());
                var errorModel = viewResult?.Model as ErrorViewModel;
                Assert.That(errorModel?.RequestId, Is.EqualTo("test-trace-id"));
            });
        }
    }
}