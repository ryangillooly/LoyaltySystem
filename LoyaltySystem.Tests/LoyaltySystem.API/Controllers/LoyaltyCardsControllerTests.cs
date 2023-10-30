using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using LoyaltySystem.API.Controllers;
using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace LoyaltySystem.Tests.Controllers;

public class LoyaltyCardsControllerTests
{
    private readonly ILoyaltyCardService _loyaltyCardServiceSubstitute = Substitute.For<ILoyaltyCardService>();
    private readonly LoyaltyCardsController _controller;
    private readonly Guid _userId     = Guid.NewGuid();
    private readonly Guid _businessId = Guid.NewGuid();
    private readonly Guid _campaignId = Guid.NewGuid();
    private readonly Guid _rewardId   = Guid.NewGuid();

    public LoyaltyCardsControllerTests()
    {
        _controller = new LoyaltyCardsController(_loyaltyCardServiceSubstitute);
    }

    // CREATE
    [Fact]
    public async Task CreateLoyaltyCard_ShouldReturnCreatedAtAction_WhenCreated()
    {
        // Arrange
        var loyaltyCardDto = new CreateLoyaltyCardDto { BusinessId = _businessId };
        var createdLoyaltyCard = new LoyaltyCard { UserId = _userId, BusinessId = _businessId };
        _loyaltyCardServiceSubstitute.CreateLoyaltyCardAsync(_userId, _businessId).Returns(createdLoyaltyCard);

        // Act
        var result = await _controller.CreateLoyaltyCard(_userId, loyaltyCardDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        createdAtActionResult.Value.Should().BeEquivalentTo(createdLoyaltyCard);
    }
    
    // GET
    [Fact]
    public async Task GetLoyaltyCard_ShouldReturnOk_WhenCardExists()
    {
        // Arrange
        var expectedLoyaltyCard = new LoyaltyCard { UserId = _userId, BusinessId = _businessId };
        _loyaltyCardServiceSubstitute.GetLoyaltyCardAsync(_userId, _businessId).Returns(expectedLoyaltyCard);

        // Act
        var result = await _controller.GetLoyaltyCard(_userId, _businessId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedLoyaltyCard);
    }
    [Fact]
    public async Task GetLoyaltyCard_ShouldReturnNotFound_WhenCardDoesNotExist()
    {
        // Arrange
        var exceptionMessage = "Loyalty card not found.";
        _loyaltyCardServiceSubstitute.GetLoyaltyCardAsync(_userId, _businessId)
            .Returns(Task.FromException<LoyaltyCard>(new ResourceNotFoundException(exceptionMessage)));

        // Act
        var result = await _controller.GetLoyaltyCard(_userId, _businessId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be(exceptionMessage);
    }
    [Fact]
    public async Task GetLoyaltyCard_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        _loyaltyCardServiceSubstitute.GetLoyaltyCardAsync(_userId, _businessId)
            .Returns(Task.FromException<LoyaltyCard>(new Exception("Unexpected error")));

        // Act
        var result = await _controller.GetLoyaltyCard(_userId, _businessId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
    }

    // DELETE
    [Fact]
    public async Task DeleteLoyaltyCard_ShouldReturnNoContent_WhenDeleted()
    {
        // Arrange
        _loyaltyCardServiceSubstitute.DeleteLoyaltyCardAsync(_userId, _businessId).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteLoyaltyCard(_userId, _businessId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    
    // UPDATE
    [Fact]
    public async Task UpdateLoyaltyCard_ShouldReturnOk_WhenUpdated()
    {
        // Arrange
        var updateDto = new UpdateLoyaltyCardDto { Status = LoyaltyStatus.Active };
        var updatedLoyaltyCard = new LoyaltyCard { UserId = _userId, BusinessId = _businessId, Status = updateDto.Status };
        _loyaltyCardServiceSubstitute.UpdateLoyaltyCardAsync(_userId, _businessId, updateDto.Status)
            .Returns(updatedLoyaltyCard);

        // Act
        var result = await _controller.UpdateLoyaltyCard(_userId, _businessId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.Value.Should().BeEquivalentTo(updatedLoyaltyCard);
    }
}
