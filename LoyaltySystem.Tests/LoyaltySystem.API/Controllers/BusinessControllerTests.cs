using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using LoyaltySystem.API.Controllers;
using LoyaltySystem.Core.DTOs;
using LoyaltySystem.Core.Interfaces;
using LoyaltySystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Core.Enums;

namespace LoyaltySystem.Tests.Controllers
{
    public class BusinessesControllerTests
    {
        private readonly Mock<IBusinessService> _businessServiceMock = new ();
        private readonly IMapper                _mapper;
        private readonly BusinessesController  _controller;

        public BusinessesControllerTests(IMapper mapper)
        {
            _controller = new BusinessesController(_businessServiceMock.Object);
            _mapper     = mapper;
        }

        // CREATE BUSINESS
        /*
        [Fact]
        public async Task CreateBusiness_ShouldReturnCreatedAtAction_WhenBusinessIsCreated()
        {
            // Arrange
            var createBusinessDto = new CreateBusinessDto();
            var createdBusiness = new Business { Id = Guid.NewGuid() };
            _businessServiceMock.Setup(service => service.CreateBusinessAsync(It.IsAny<Business>()))
                .ReturnsAsync(createdBusiness);

            // Act
            var actionResult = await _controller.CreateBusiness(createBusinessDto);

            // Assert
            actionResult.Should().BeOfType<CreatedAtActionResult>();
            var createdAtActionResult = (CreatedAtActionResult)actionResult;
            createdAtActionResult.Value.Should().BeEquivalentTo(createdBusiness);
        }
        */
        
        // UPDATE BUSINESS
        [Fact]
        public async Task UpdateBusiness_ShouldReturnOk_WhenBusinessIsUpdated()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var business = new Business { Id = businessId };
            _businessServiceMock.Setup(service => service.UpdateBusinessAsync(It.IsAny<Business>())).ReturnsAsync(business);

            // Act
            var actionResult = await _controller.UpdateBusiness(businessId, new Business());

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            okResult.Value.Should().BeEquivalentTo(business);
        }

        [Fact]
        public async Task UpdateBusiness_ShouldReturnNotFound_WhenBusinessDoesNotExist()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            _businessServiceMock.Setup(service => service.UpdateBusinessAsync(It.IsAny<Business>())).ReturnsAsync((Business)null);

            // Act
            var actionResult = await _controller.UpdateBusiness(businessId, new Business());

            // Assert
            actionResult.Should().BeOfType<NotFoundResult>();
        }
        
        // GET BUSINESS
        [Fact]
        public async Task GetBusiness_ShouldReturnOk_WhenBusinessExists()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            var business = new Business { Id = businessId };
            _businessServiceMock.Setup(service => service.GetBusinessAsync(businessId)).ReturnsAsync(business);

            // Act
            var actionResult = await _controller.GetBusiness(businessId);

            // Assert
            actionResult.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            okResult.Value.Should().BeEquivalentTo(business);
        }

        [Fact]
        public async Task GetBusiness_ShouldReturnNotFound_WhenBusinessDoesNotExist()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            _businessServiceMock.Setup(service => service.GetBusinessAsync(businessId)).ThrowsAsync(new ResourceNotFoundException("Business not found."));

            // Act
            var actionResult = await _controller.GetBusiness(businessId);

            // Assert
            actionResult.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult) actionResult;
            notFoundResult.Value.Should().Be("Business not found.");
        }
        
        // DELETE BUSINESS
        [Fact]
        public async Task DeleteBusiness_ShouldReturnNoContent_WhenBusinessIsDeleted()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            _businessServiceMock.Setup(service => service.DeleteBusinessAsync(businessId)).Returns(Task.CompletedTask);

            // Act
            var actionResult = await _controller.DeleteBusiness(businessId);

            // Assert
            actionResult.Should().BeOfType<NoContentResult>();
        }
        /*
        [Fact]
        public async Task DeleteBusiness_ShouldReturnNotFound_WhenBusinessDoesNotExist()
        {
            // Arrange
            var businessId = Guid.NewGuid();
            //_businessServiceMock.Setup(service => service.DeleteBusinessAsync(businessId)).Thr
               // .ThrowsAsync(new ResourceNotFoundException("Business not found."));

            // Act
            Func<Task> act = async () => await _controller.DeleteBusiness(businessId);

            // Assert
            await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("Business not found.");
        }
        */
    }
}
