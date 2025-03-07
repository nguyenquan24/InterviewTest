using DataImporter.Models;
using DataImporter.TestBase;

namespace DataImporter.Services.Tests
{
    [TestFixture]
    public class MarketDataServiceTests : BaseAutoMock<MarketDataService>
    {
        private string _tempFilePath;
        private const string ValidCsvContent = "Date,Market Price EX1\n10/1/2017,50.29000092\n10/1/2017 0:30,50";

        private const string InvalidPriceCsvContent =
            "Date,Market Price EX1\n10/1/2017,invalid_price\n10/1/2017 0:30,50";

        private const string MissingHeaderCsvContent = "Date\n10/1/2017\n10/1/2017 0:30";

        [OneTimeSetUp]
        public void TestSetup()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), "testMarket.csv");
            base.BaseSetup();
        }

        [OneTimeTearDown]
        public void TestCleanup()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }

            base.BaseTearDown();
        }

        private void WriteTestFile(string content)
        {
            File.WriteAllText(_tempFilePath, content);
        }

        [Test]
        public void ReadDataFromCsv_ValidFile_ReturnsMarketDataList()
        {
            // Arrange
            WriteTestFile(ValidCsvContent);

            // Act
            var result = ClassUnderTest.ReadDataFromCsv(_tempFilePath);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result[0].Date, Is.EqualTo(new DateTime(2017, 10, 1)));
                Assert.That(result[0].MarketPrice, Is.EqualTo(50.29000092m));
                Assert.That(result[1].Date, Is.EqualTo(new DateTime(2017, 10, 1, 0, 30, 0)));
                Assert.That(result[1].MarketPrice, Is.EqualTo(50m));
            });
        }

        [Test]
        public void GetMinimumPrice_ValidData_ReturnsMinimumPrice()
        {
            // Arrange
            var marketDataList = new List<MarketData>
            {
                new() { MarketPrice = 50 },
                new() { MarketPrice = 20.5m },
                new() { MarketPrice = 100 }
            };

            // Act
            var minPrice = ClassUnderTest.GetMinimumPrice(marketDataList);

            // Assert
            Assert.That(minPrice, Is.EqualTo(20.5m));
        }

        [Test]
        public void GetMaximumPrice_ValidData_ReturnsMaximumPrice()
        {
            // Arrange
            var marketDataList = new List<MarketData>
            {
                new() { MarketPrice = 50 },
                new() { MarketPrice = 20.5m },
                new() { MarketPrice = 451.5299988m }
            };

            // Act
            var maxPrice = ClassUnderTest.GetMaximumPrice(marketDataList);

            // Assert
            Assert.That(maxPrice, Is.EqualTo(451.5299988m));
        }

        [Test]
        public void GetAveragePrice_ValidData_ReturnsAveragePrice()
        {
            // Arrange
            var marketDataList = new List<MarketData>
            {
                new() { MarketPrice = 50 },
                new() { MarketPrice = 50 },
                new() { MarketPrice = 20 }
            };

            // Act
            var averagePrice = ClassUnderTest.GetAveragePrice(marketDataList);

            // Assert
            Assert.That(averagePrice, Is.EqualTo(40m));
        }

        [Test]
        public void GetMostExpensiveHourWindow_ValidData_ReturnsMostExpensiveHour()
        {
            // Arrange
            var marketDataList = new List<MarketData>
            {
                new() { Date = new DateTime(2017, 10, 1, 18, 0, 0), MarketPrice = 350 },
                new() { Date = new DateTime(2017, 10, 1, 18, 30, 0), MarketPrice = 356.0699997m },
                new() { Date = new DateTime(2017, 10, 1, 19, 0, 0), MarketPrice = 100 },
                new() { Date = new DateTime(2017, 10, 1, 19, 30, 0), MarketPrice = 50 }
            };

            // Act
            var expensiveHour = ClassUnderTest.GetMostExpensiveHourWindow(marketDataList);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(expensiveHour.MaxHourPrice, Is.EqualTo(706.0699997m));
                Assert.That(expensiveHour.StartHour, Is.EqualTo(new DateTime(2017, 10, 1, 18, 0, 0)));
            });
        }

        [Test]
        public void ReadDataFromCsv_TypeConverterException_ThrowsInvalidDataException()
        {
            // Arrange
            WriteTestFile(InvalidPriceCsvContent);

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() =>
                ClassUnderTest.ReadDataFromCsv(_tempFilePath));

            Assert.That(exception.Message, Does.StartWith("Unexpected error reading CSV file:"));
        }

        [Test]
        public void ReadDataFromCsv_MissingFieldException_ThrowsInvalidDataException()
        {
            // Arrange
            WriteTestFile(MissingHeaderCsvContent);

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() =>
                ClassUnderTest.ReadDataFromCsv(_tempFilePath));

            Assert.That(exception.Message, Does.StartWith("Invalid CSV header:"));
        }

        [Test]
        public void ReadDataFromCsv_FileNotFound_ThrowsFileNotFoundException()
        {
            // Arrange
            const string nonExistentPath = "path/does/not/exist.csv";

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                ClassUnderTest.ReadDataFromCsv(nonExistentPath));

            Assert.Multiple(() =>
            {
                Assert.That(exception.Message, Does.StartWith("CSV file not found"));
                Assert.That(exception.FileName, Is.EqualTo(nonExistentPath));
            });
        }
    }
}