using DataImporter.WebApp.Models;
using NUnit.Framework;

namespace DataImporter.WebApp.Tests.Models
{
    [TestFixture]
    public class ErrorViewModelTests
    {
        [Test]
        public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = null
            };

            // Act
            var result = errorViewModel.ShowRequestId;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShowRequestId_WhenRequestIdIsEmpty_ReturnsFalse()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = string.Empty
            };

            // Act
            var result = errorViewModel.ShowRequestId;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShowRequestId_WhenRequestIdIsWhiteSpace_ReturnsFalse()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = "   "
            };

            // Act
            var result = errorViewModel.ShowRequestId;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ShowRequestId_WhenRequestIdHasValue_ReturnsTrue()
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = "test-request-id"
            };

            // Act
            var result = errorViewModel.ShowRequestId;

            // Assert
            Assert.That(result, Is.True);
        }

        [TestCase(null, false, Description = "Null RequestId should return false")]
        [TestCase("", false, Description = "Empty RequestId should return false")]
        [TestCase("   ", false, Description = "Whitespace RequestId should return false")]
        [TestCase("\t", false, Description = "Tab character should return false")]
        [TestCase("\n", false, Description = "New line should return false")]
        [TestCase("test-id", true, Description = "Valid RequestId should return true")]
        [TestCase("123", true, Description = "Numeric RequestId should return true")]
        [TestCase(" test ", true, Description = "RequestId with leading/trailing spaces should return true")]
        public void ShowRequestId_WithVariousInputs_ReturnsExpectedResult(string? requestId, bool expectedResult)
        {
            // Arrange
            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId
            };

            // Act
            var result = errorViewModel.ShowRequestId;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult),
                $"Failed for RequestId: '{requestId ?? "null"}'");
        }
    }
}