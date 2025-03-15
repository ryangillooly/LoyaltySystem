using FluentAssertions;
using LoyaltySystem.AcceptanceTests.Support;
using LoyaltySystem.AcceptanceTests.DTOs;
using Reqnroll;

namespace LoyaltySystem.AcceptanceTests.StepDefinitions;

[Binding]
public class RewardSteps
{
    private readonly ApiTestContext _context;
    private RewardDto? _selectedReward;
    private int _initialPoints;
    
    public RewardSteps(ScenarioContext scenarioContext)
    {
        _context = new ApiTestContext(scenarioContext);
    }
    
    [Given(@"I have a loyalty card with sufficient points for a reward")]
    public async Task GivenIHaveALoyaltyCardWithSufficientPointsForAReward()
    {
        // First ensure we have a loyalty card
        var loyaltyCardSteps = new LoyaltyCardSteps(ScenarioContext.Current);
        await loyaltyCardSteps.GivenIHaveALoyaltyCardForProgramWithId("{program-id}");
        
        // Get the current loyalty card details
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyCards/" + _context.LoyaltyCardId);
        await _context.SendAsync(request);
        
        var loyaltyCard = await _context.ReadResponseAs<LoyaltyCardDto>();
        _initialPoints = loyaltyCard!.CurrentPoints;
        
        // If we don't have enough points, simulate some transactions
        if (_initialPoints < 50) // Assuming 50 points is enough for a basic reward
        {
            // Add enough points for a reward
            int transactionsNeeded = (int)Math.Ceiling((50 - _initialPoints) / 10.0); // Assume 10 points per $10
            
            for (int i = 0; i < transactionsNeeded; i++)
            {
                await loyaltyCardSteps.WhenATransactionIsRecordedForMyCard(10.00m);
            }
            
            // Get updated point balance
            request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyCards/" + _context.LoyaltyCardId);
            await _context.SendAsync(request);
            
            loyaltyCard = await _context.ReadResponseAs<LoyaltyCardDto>();
            _initialPoints = loyaltyCard!.CurrentPoints;
        }
        
        // Get available rewards for the program
        request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyPrograms/" + _context.LoyaltyProgramId + "/rewards");
        await _context.SendAsync(request);
        
        var rewards = await _context.ReadResponseAs<List<RewardDto>>();
        rewards.Should().NotBeNull();
        rewards!.Should().NotBeEmpty();
        
        // Find a reward we can afford
        _selectedReward = rewards.FirstOrDefault(r => r.PointsCost <= _initialPoints);
        _selectedReward.Should().NotBeNull("A reward that can be redeemed with the current points should be available");
    }
    
    [When(@"I request to redeem a reward with id ""(.*)""")]
    public async Task WhenIRequestToRedeemARewardWithId(string rewardId)
    {
        // If rewardId is a placeholder, use the one we found earlier
        string actualRewardId = rewardId == "{reward-id}" ? _selectedReward!.Id : rewardId;
        
        var redemptionRequest = new RedeemRewardDto
        {
            LoyaltyCardId = _context.LoyaltyCardId!,
            RewardId = actualRewardId
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/rewards/redeem");
        request.Content = _context.CreateJsonContent(redemptionRequest);
        
        await _context.SendAsync(request);
    }
    
    [Then(@"the reward should be marked as redeemed")]
    public async Task ThenTheRewardShouldBeMarkedAsRedeemed()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
        
        var redemptionResponse = await _context.ReadResponseAs<RedemptionResponseDto>();
        redemptionResponse.Should().NotBeNull();
        redemptionResponse!.Success.Should().BeTrue();
        redemptionResponse.RedemptionCode.Should().NotBeNullOrEmpty();
        redemptionResponse.Reward.Id.Should().Be(_selectedReward!.Id);
    }
    
    [Then(@"my loyalty card points balance should be reduced accordingly")]
    public async Task ThenMyLoyaltyCardPointsBalanceShouldBeReducedAccordingly()
    {
        // Get the updated card details
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/loyaltyCards/" + _context.LoyaltyCardId);
        await _context.SendAsync(request);
        
        var updatedCard = await _context.ReadResponseAs<LoyaltyCardDto>();
        updatedCard.Should().NotBeNull();
        
        // Verify points were deducted correctly
        updatedCard!.CurrentPoints.Should().Be(_initialPoints - _selectedReward!.PointsCost);
    }
    
    [Then(@"I should receive a redemption confirmation")]
    public async Task ThenIShouldReceiveARedemptionConfirmation()
    {
        var redemptionResponse = await _context.ReadResponseAs<RedemptionResponseDto>();
        redemptionResponse.Should().NotBeNull();
        redemptionResponse!.Success.Should().BeTrue();
        redemptionResponse.Message.Should().NotBeNullOrEmpty();
        redemptionResponse.RedemptionCode.Should().NotBeNullOrEmpty();
        
        // Here you might also check for an email confirmation if your system sends one
    }
} 