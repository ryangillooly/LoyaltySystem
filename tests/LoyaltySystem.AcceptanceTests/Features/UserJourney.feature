Feature: User Loyalty Program Journey
    As a customer
    I want to register, login, and join a loyalty program
    So that I can earn rewards when I make purchases

@authentication
Scenario: User registers for an account
    When I register a new account with email "test.user@example.com" and password "Test@123!"
    Then I should receive a successful registration response
    And the response should contain user details for "test.user@example.com"

@authentication
Scenario: User logs in and receives JWT token
    Given I have registered with email "test.user@example.com" and password "Test@123!"
    When I login with email "test.user@example.com" and password "Test@123!"
    Then I should receive a successful login response
    And the response should contain a valid JWT token

@loyaltyprogram
Scenario: User views available loyalty programs
    Given I am authenticated with a valid JWT token
    When I request available loyalty programs
    Then I should receive a list of loyalty programs
    And each program should contain details about rewards

@loyaltycard
Scenario: User signs up for a loyalty card
    Given I am authenticated with a valid JWT token
    And there is an available loyalty program with id "{program-id}"
    When I request to join the loyalty program
    Then I should receive a new loyalty card
    And the card should be linked to my account
    And the card should have zero points or stamps

@transactions
Scenario: User receives points for a transaction
    Given I am authenticated with a valid JWT token
    And I have a loyalty card for program with id "{program-id}"
    When a transaction of $50.00 is recorded for my card
    Then my loyalty card should be credited with the correct points
    And I should receive a transaction confirmation

@rewards
Scenario: User redeems a reward
    Given I am authenticated with a valid JWT token
    And I have a loyalty card with sufficient points for a reward
    When I request to redeem a reward with id "{reward-id}"
    Then the reward should be marked as redeemed
    And my loyalty card points balance should be reduced accordingly
    And I should receive a redemption confirmation 