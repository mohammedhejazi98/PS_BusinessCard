using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using PS_BusinessCard.Controllers;
using PS_BusinessCard.Dtos;
using PS_BusinessCard.IService;
using PS_BusinessCard.Models;
using PS_BusinessCard.Repositories;

namespace PS_BusinessCard.Test
{
    public class Test
    {
        #region Private Fields

        private readonly BusinessCardsController _controller;

        private readonly Mock<IExcelService> _mockExcelService;

        private readonly Mock<ILogger<BusinessCardsController>> _mockLogger;

        private readonly Mock<IQrCodeService> _mockQrCodeService;

        private readonly Mock<IBusinessCardRepository> _mockRepository;
        private readonly Mock<IXmlService> _mockXmlService;

        #endregion Private Fields

        #region Public Constructors

        public Test()
        {
            _mockRepository = new Mock<IBusinessCardRepository>();
            _mockExcelService = new Mock<IExcelService>();
            _mockXmlService = new Mock<IXmlService>();
            _mockQrCodeService = new Mock<IQrCodeService>();
            _mockLogger = new Mock<ILogger<BusinessCardsController>>();

            _controller = new BusinessCardsController(
                _mockRepository.Object,
                _mockExcelService.Object,
                _mockXmlService.Object,
                _mockQrCodeService.Object,
                _mockLogger.Object);
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        public async Task AddBusinessCard_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.AddBusinessCard(new CreateBusinessCardDto
            {
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"

            });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddBusinessCard_WithValidModel_ReturnsCreatedResult()
        {
            // Arrange
            var businessCardDto = new CreateBusinessCardDto
            {
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
                // Add other properties as needed
            };

            _mockRepository.Setup(r => r.Add(It.IsAny<BusinessCard>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddBusinessCard(businessCardDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult);
            Assert.Equal("GetBusinessCard", createdResult.ActionName);
        }
        [Fact]
        public async Task DeleteBusinessCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            int businessCardId = 1;
            _mockRepository.Setup(r => r.Delete(businessCardId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteBusinessCard(businessCardId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ExportToExcel_ReturnsFileResult_WhenBusinessCardsExist()
        {
            // Arrange
            var businessCards = new List<BusinessCard>
            {
                new BusinessCard
                {
                    Name = "John Doe",
                    Phone = "123456789",
                    Email = "john@example.com",
                    Gender = "Male"
                },
                new BusinessCard
                {
                    Name = "Jane Doe",
                    Phone = "987654321",
                    Email = "jane@example.com",
                    Gender = "Female"
                }
            };

            // Mock repository to return the business cards
            _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(businessCards);

            // Mock Excel service to return a byte array representing an Excel file
            var excelBytes = new byte[] { 0x20, 0x20, 0x20 }; // Example byte array to simulate an Excel file
            _mockExcelService.Setup(e => e.GenerateExcelAsync(businessCards)).ReturnsAsync(excelBytes);

            // Act
            var result = await _controller.ExportToExcel();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("BusinessCards.xlsx", fileResult.FileDownloadName);

        }


        [Fact]
        public async Task ExportToXml_ReturnsFileResult()
        {
            // Arrange
            var businessCards = new List<BusinessCard>
        {
            new BusinessCard {                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            },
            new BusinessCard {                 Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            }
        };

            _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(businessCards);
            _mockXmlService.Setup(e => e.ExportToXmlAsync(businessCards)).ReturnsAsync(new byte[0]);

            // Act
            var result = await _controller.ExportToXml();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/xml", fileResult.ContentType);
        }

        [Fact]
        public async Task GenerateQr_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int businessCardId = 1;
            _mockRepository.Setup(r => r.GetById(businessCardId)).ReturnsAsync((BusinessCard)null);

            // Act
            var result = await _controller.GenerateQr(businessCardId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GenerateQr_WithValidId_ReturnsImageFile()
        {
            // Arrange
            int businessCardId = 1;
            var card = new BusinessCard
            {
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            };

            _mockRepository.Setup(r => r.GetById(businessCardId)).ReturnsAsync(card);

            // Act
            var result = await _controller.GenerateQr(businessCardId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
        }
        [Fact]
        public async Task GetBusinessCard_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int businessCardId = 1;
            _mockRepository.Setup(r => r.GetById(businessCardId)).ReturnsAsync((BusinessCard)null);

            // Act
            var result = await _controller.GetBusinessCard(businessCardId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBusinessCard_WithValidId_ReturnsOkResult()
        {
            // Arrange
            int businessCardId = 1;
            var card = new BusinessCard
            {
                Id = businessCardId,
                Name = "John Doe",
                Gender = "Male",
                Phone = "123456789",
            };
            _mockRepository.Setup(r => r.GetById(businessCardId)).ReturnsAsync(card);

            // Act
            var result = await _controller.GetBusinessCard(businessCardId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnCard = Assert.IsType<BusinessCard>(okResult.Value);
            Assert.Equal(businessCardId, returnCard.Id);
        }
        [Fact]
        public async Task GetBusinessCards_ReturnsOkResult()
        {
            // Arrange
            var businessCards = new List<BusinessCard>
        {
            new BusinessCard {                 Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            },
            new BusinessCard {                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            }
        };

            _mockRepository.Setup(r => r.GetAll()).ReturnsAsync(businessCards);

            // Act
            var result = await _controller.GetBusinessCards();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnCards = Assert.IsType<List<BusinessCard>>(okResult.Value);
            Assert.Equal(2, returnCards.Count);
        }

        [Fact]
        public async Task UpdateBusinessCard_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var businessCard = new BusinessCard
            {
                Id = 2,
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            }; // ID mismatch

            // Act
            var result = await _controller.UpdateBusinessCard(1, businessCard);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateBusinessCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            int businessCardId = 1;
            var businessCard = new BusinessCard
            {
                Id = businessCardId,
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com",
                Gender = "Male"
            };

            _mockRepository.Setup(r => r.Update(businessCard)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateBusinessCard(businessCardId, businessCard);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion Public Methods
    }
}