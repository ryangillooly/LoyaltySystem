using FluentAssertions;
using LoyaltySystem.API.Controllers;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LoyaltySystem.Tests.Controllers;

// ... (Assuming the necessary using directives and test infrastructure are already in place)

public class LoyaltyCardsControllerTests
{
    // Assuming you have a TestFixture or similar setup for common properties and setup.
    private readonly Mock<ILoyaltyCardService> _loyaltyCardServiceMock = new Mock<ILoyaltyCardService>();
    private readonly LoyaltyCardsController _controller;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _businessId = Guid.NewGuid();
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _rewardId = Guid.NewGuid();

    public LoyaltyCardsControllerTests()
    {
        _controller = new LoyaltyCardsController(_loyaltyCardServiceMock.Object);
    }

    [Fact]
    public async Task CreateLoyaltyCard_ShouldReturnCreatedAtAction_WhenCreated()
    {
        // Arrange
        var loyaltyCardDto = new CreateLoyaltyCardDto { BusinessId = _businessId };
        var createdLoyaltyCard = new LoyaltyCard { UserId = _userId, BusinessId = _businessId };
        _loyaltyCardServiceMock.Setup(x => x.CreateLoyaltyCardAsync(_userId, _businessId))
            .ReturnsAsync(createdLoyaltyCard);

        // Act
        var result = await _controller.CreateLoyaltyCard(_userId, loyaltyCardDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        createdAtActionResult.Value.Should().BeEquivalentTo(createdLoyaltyCard);
    }

    [Fact]
    public async Task DeleteLoyaltyCard_ShouldReturnNoContent_WhenDeleted()
    {
        // Arrange
        _loyaltyCardServiceMock.Setup(x => x.DeleteLoyaltyCardAsync(_userId, _businessId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteLoyaltyCard(_userId, _businessId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateLoyaltyCard_ShouldReturnOk_WhenUpdated()
    {
        // Arrange
        var updateDto = new UpdateLoyaltyCardDto { Status = LoyaltyStatus.Active };
        var updatedLoyaltyCard = new LoyaltyCard { UserId = _userId, BusinessId = _businessId, Status = updateDto.Status };
        _loyaltyCardServiceMock.Setup(x => x.UpdateLoyaltyCardAsync(_userId, _businessId, updateDto.Status))
            .ReturnsAsync(updatedLoyaltyCard);

        // Act
        var result = await _controller.UpdateLoyaltyCard(_userId, _businessId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.Value.Should().BeEquivalentTo(updatedLoyaltyCard);
    }

    // Other tests...

    // Ensure to write negative tests as well (e.g., Not Found, Exception handling) for each method.
}

// Use the 'continue' command to proceed with the remaining tests...
