using System.Net.Http.Json;
using FluentAssertions;
using LoyaltySystem.AcceptanceTests.Support;
using LoyaltySystem.AcceptanceTests.DTOs;
using Reqnroll;

namespace LoyaltySystem.AcceptanceTests.StepDefinitions;

[Binding]
public class LoyaltyCardSteps
{
    private readonly ApiTestContext _context;
    private List<LoyaltyProgramDto>? _loyaltyPrograms;
    private LoyaltyCardDto? _loyaltyCard;
    
    public LoyaltyCardSteps(ScenarioContext scenarioContext)
    {
        _context = new ApiTestContext(scenarioContext);
    }
    
    [When(@"I request available loyalty programs")]
    public async Task WhenIRequestAvailableLoyaltyPrograms()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyPrograms");
        await _context.SendAsync(request);
    }
    
    [Then(@"I should receive a list of loyalty programs")]
    public async Task ThenIShouldReceiveAListOfLoyaltyPrograms()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        _loyaltyPrograms = await _context.ReadResponseAs<List<LoyaltyProgramDto>>();
        _loyaltyPrograms.Should().NotBeNull();
        _loyaltyPrograms!.Should().NotBeEmpty();
    }
    
    [Then(@"each program should contain details about rewards")]
    public void ThenEachProgramShouldContainDetailsAboutRewards()
    {
        foreach (var program in _loyaltyPrograms!)
        {
            program.Should().NotBeNull();
            program.Name.Should().NotBeNullOrEmpty();
            program.Description.Should().NotBeNullOrEmpty();
            
            // Assuming rewards are returned with programs, adjust as needed
            if (program.Rewards != null && program.Rewards.Any())
            {
                foreach (var reward in program.Rewards)
                {
                    reward.Name.Should().NotBeNullOrEmpty();
                    reward.PointsCost.Should().BeGreaterThan(0);
                }
            }
        }
    }
    
    [Given(@"there is an available loyalty program with id ""(.*)""")]
    public async Task GivenThereIsAnAvailableLoyaltyProgramWithId(string programId)
    {
        // First try to get available programs
        await WhenIRequestAvailableLoyaltyPrograms();
        await ThenIShouldReceiveAListOfLoyaltyPrograms();
        
        // Use the programId placeholder if it's a test placeholder
        if (programId == "{program-id}" && _loyaltyPrograms!.Any())
        {
            // Just use the first program for testing
            _context.LoyaltyProgramId = _loyaltyPrograms.First().Id;
        }
        else
        {
            _context.LoyaltyProgramId = programId;
        }
        
        // Verify the program exists
        _context.LoyaltyProgramId.Should().NotBeNullOrEmpty();
    }
    
    [When(@"I request to join the loyalty program")]
    public async Task WhenIRequestToJoinTheLoyaltyProgram()
    {
        var createCardRequest = new CreateCardRequest
        {
            LoyaltyProgramId = _context.LoyaltyProgramId!
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/loyaltyCards");
        request.Content = _context.CreateJsonContent(createCardRequest);
        
        await _context.SendAsync(request);
    }
    
    [Then(@"I should receive a new loyalty card")]
    public async Task ThenIShouldReceiveANewLoyaltyCard()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        _loyaltyCard = await _context.ReadResponseAs<LoyaltyCardDto>();
        _loyaltyCard.Should().NotBeNull();
        _loyaltyCard!.Id.Should().NotBeNullOrEmpty();
        
        _context.LoyaltyCardId = _loyaltyCard.Id;
    }
    
    [Then(@"the card should be linked to my account")]
    public void ThenTheCardShouldBeLinkedToMyAccount()
    {
        _loyaltyCard!.CustomerId.Should().Be(_context.UserId);
    }
    
    [Then(@"the card should have zero points or stamps")]
    public void ThenTheCardShouldHaveZeroPointsOrStamps()
    {
        _loyaltyCard!.CurrentPoints.Should().Be(0);
        // For stamp-based programs, also check stamps if applicable
    }
    
    [Given(@"I have a loyalty card for program with id ""(.*)""")]
    public async Task GivenIHaveALoyaltyCardForProgramWithId(string programId)
    {
        // Ensure we have a program ID
        await GivenThereIsAnAvailableLoyaltyProgramWithId(programId);
        
        // Check if we already have a card
        if (string.IsNullOrEmpty(_context.LoyaltyCardId))
        {
            // Get a card
            await WhenIRequestToJoinTheLoyaltyProgram();
            await ThenIShouldReceiveANewLoyaltyCard();
        }
    }
    
    [When(@"a transaction of \$([0-9.]+) is recorded for my card")]
    public async Task WhenATransactionIsRecordedForMyCard(decimal amount)
    {
        var transactionDto = new CreateTransactionDto
        {
            LoyaltyCardId = _context.LoyaltyCardId!,
            Amount = amount,
            Description = "Test purchase"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/transactions");
        request.Content = _context.CreateJsonContent(transactionDto);
        
        await _context.SendAsync(request);
    }
    
    [Then(@"my loyalty card should be credited with the correct points")]
    public async Task ThenMyLoyaltyCardShouldBeCreditedWithTheCorrectPoints()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        // Get the updated card details
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyCards/" + _context.LoyaltyCardId);
        await _context.SendAsync(request);
        
        var updatedCard = await _context.ReadResponseAs<LoyaltyCardDto>();
        updatedCard.Should().NotBeNull();
        updatedCard!.CurrentPoints.Should().BeGreaterThan(0);
    }
    
    [Then(@"I should receive a transaction confirmation")]
    public async Task ThenIShouldReceiveATransactionConfirmation()
    {
        var transactionResult = await _context.ReadResponseAs<TransactionDto>();
        transactionResult.Should().NotBeNull();
        transactionResult!.Id.Should().NotBeNullOrEmpty();
        transactionResult.LoyaltyCardId.Should().Be(_context.LoyaltyCardId);
        transactionResult.PointsEarned.Should().BeGreaterThan(0);
    }
} 