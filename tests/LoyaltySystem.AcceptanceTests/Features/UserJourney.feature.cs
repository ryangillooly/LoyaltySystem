﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace LoyaltySystem.AcceptanceTests.Features
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class UserLoyaltyProgramJourneyFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
        private static string[] featureTags = ((string[])(null));
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "User Loyalty Program Journey", "    As a customer\n    I want to register, login, and join a loyalty program\n    S" +
                "o that I can earn rewards when I make purchases", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "UserJourney.feature"
#line hidden
        
        public virtual Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static async System.Threading.Tasks.Task FeatureSetupAsync(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute(Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupBehavior.EndOfClass)]
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
            {
                await testRunner.OnFeatureEndAsync();
            }
            if ((testRunner.FeatureContext == null))
            {
                await testRunner.OnFeatureStartAsync(featureInfo);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
            global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(_testContext);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User registers for an account")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("authentication")]
        public async System.Threading.Tasks.Task UserRegistersForAnAccount()
        {
            string[] tagsOfScenario = new string[] {
                    "authentication"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User registers for an account", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 7
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 8
    await testRunner.WhenAsync("I register a new account with email \"test.user@example.com\" and password \"Test@12" +
                        "3!\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 9
    await testRunner.ThenAsync("I should receive a successful registration response", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 10
    await testRunner.AndAsync("the response should contain user details for \"test.user@example.com\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User logs in and receives JWT token")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("authentication")]
        public async System.Threading.Tasks.Task UserLogsInAndReceivesJWTToken()
        {
            string[] tagsOfScenario = new string[] {
                    "authentication"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User logs in and receives JWT token", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 13
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 14
    await testRunner.GivenAsync("I have registered with email \"test.user@example.com\" and password \"Test@123!\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 15
    await testRunner.WhenAsync("I login with email \"test.user@example.com\" and password \"Test@123!\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 16
    await testRunner.ThenAsync("I should receive a successful login response", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 17
    await testRunner.AndAsync("the response should contain a valid JWT token", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User views available loyalty programs")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("loyaltyprogram")]
        public async System.Threading.Tasks.Task UserViewsAvailableLoyaltyPrograms()
        {
            string[] tagsOfScenario = new string[] {
                    "loyaltyprogram"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User views available loyalty programs", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 20
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 21
    await testRunner.GivenAsync("I am authenticated with a valid JWT token", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 22
    await testRunner.WhenAsync("I request available loyalty programs", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 23
    await testRunner.ThenAsync("I should receive a list of loyalty programs", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 24
    await testRunner.AndAsync("each program should contain details about rewards", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User signs up for a loyalty card")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("loyaltycard")]
        public async System.Threading.Tasks.Task UserSignsUpForALoyaltyCard()
        {
            string[] tagsOfScenario = new string[] {
                    "loyaltycard"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User signs up for a loyalty card", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 27
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 28
    await testRunner.GivenAsync("I am authenticated with a valid JWT token", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 29
    await testRunner.AndAsync("there is an available loyalty program with id \"{program-id}\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 30
    await testRunner.WhenAsync("I request to join the loyalty program", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 31
    await testRunner.ThenAsync("I should receive a new loyalty card", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 32
    await testRunner.AndAsync("the card should be linked to my account", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 33
    await testRunner.AndAsync("the card should have zero points or stamps", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User receives points for a transaction")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("transactions")]
        public async System.Threading.Tasks.Task UserReceivesPointsForATransaction()
        {
            string[] tagsOfScenario = new string[] {
                    "transactions"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User receives points for a transaction", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 36
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 37
    await testRunner.GivenAsync("I am authenticated with a valid JWT token", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 38
    await testRunner.AndAsync("I have a loyalty card for program with id \"{program-id}\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 39
    await testRunner.WhenAsync("a transaction of $50.00 is recorded for my card", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 40
    await testRunner.ThenAsync("my loyalty card should be credited with the correct points", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 41
    await testRunner.AndAsync("I should receive a transaction confirmation", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("User redeems a reward")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "User Loyalty Program Journey")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute("rewards")]
        public async System.Threading.Tasks.Task UserRedeemsAReward()
        {
            string[] tagsOfScenario = new string[] {
                    "rewards"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("User redeems a reward", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 44
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 45
    await testRunner.GivenAsync("I am authenticated with a valid JWT token", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 46
    await testRunner.AndAsync("I have a loyalty card with sufficient points for a reward", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 47
    await testRunner.WhenAsync("I request to redeem a reward with id \"{reward-id}\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 48
    await testRunner.ThenAsync("the reward should be marked as redeemed", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 49
    await testRunner.AndAsync("my loyalty card points balance should be reduced accordingly", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 50
    await testRunner.AndAsync("I should receive a redemption confirmation", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
