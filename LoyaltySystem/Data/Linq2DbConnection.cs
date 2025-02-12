using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using System.Collections.Generic;

namespace LoyaltySystem.Data;

// A custom DataConnection to your DB
public class Linq2DbConnection : DataConnection
{
    public Linq2DbConnection(string connectionString) 
        : base("PostgreSQL", connectionString) // or "SqlServer", "MySql", etc.
    {
    }

    public ITable<User> Users => this.GetTable<User>();
    
    public ITable<Business> Businesses => this.GetTable<Business>();
    public ITable<BusinessUser> BusinessUsers => this.GetTable<BusinessUser>();
    
    public ITable<LoyaltyCardTemplate> LoyaltyCardTemplates => this.GetTable<LoyaltyCardTemplate>();
    public ITable<UserLoyaltyCard> UserLoyaltyCards => this.GetTable<UserLoyaltyCard>();
    
    public ITable<Promotion> Promotions => this.GetTable<Promotion>();
    public ITable<RedemptionTransaction> RedemptionTransactions => this.GetTable<RedemptionTransaction>();
    
    public ITable<Reward> Rewards => this.GetTable<Reward>();
    
    public ITable<StampTransaction> StampTransactions => this.GetTable<StampTransaction>();
    public ITable<Store> Stores => this.GetTable<Store>();
    public ITable<FraudSettings> FraudSettings => this.GetTable<FraudSettings>();
    
    public ITable<Member> Members => this.GetTable<Member>();
}

// You can also create a custom IConnectionStringSettings if needed
public class MyConnectionSettings : IConnectionStringSettings
{
    public string ConnectionString { get; set; }
    public string Name { get; set; }
    public string ProviderName { get; set; }
    public bool IsGlobal => false;
}

