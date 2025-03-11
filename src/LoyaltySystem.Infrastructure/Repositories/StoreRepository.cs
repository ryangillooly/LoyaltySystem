using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly IDatabaseConnection _connection;

        public StoreRepository(IDatabaseConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<Store?> GetByIdAsync(StoreId id)
        {
            const string sql = @"
                SELECT s.*, a.*, b.*
                FROM Stores s
                LEFT JOIN Addresses a ON s.AddressId = a.Id
                LEFT JOIN Brands b ON s.BrandId = b.Id
                WHERE s.Id = @Id";

            var parameters = new { Id = id.Value };
            
            var dbConnection = await _connection.GetConnectionAsync();
            
            var stores = await dbConnection.QueryAsync<Store, Address, Brand, Store>(
                sql,
                (store, address, brand) =>
                {
                    if (address != null)
                    {
                        store.SetAddress(address);
                    }
                    
                    return store;
                },
                parameters,
                splitOn: "Id,Id"
            );
            
            return stores.FirstOrDefault();
        }

        public async Task<IEnumerable<Store>> GetAllAsync(int page, int pageSize)
        {
            const string sql = @"
                SELECT s.*, a.*, b.*
                FROM Stores s
                LEFT JOIN Addresses a ON s.AddressId = a.Id
                LEFT JOIN Brands b ON s.BrandId = b.Id
                ORDER BY s.Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";
            
            var parameters = new { Offset = (page - 1) * pageSize, PageSize = pageSize };
            var dbConnection = await _connection.GetConnectionAsync();
            
            var stores = await dbConnection.QueryAsync<Store, Address, Brand, Store>(
                sql,
                (store, address, brand) =>
                {
                    if (address != null)
                    {
                        store.SetAddress(address);
                    }
                    
                    return store;
                },
                parameters,
                splitOn: "Id,Id"
            );
            
            return stores;
        }

        public async Task<IEnumerable<Store>> GetByBrandIdAsync(BrandId brandId)
        {
            const string sql = @"
                SELECT s.*, a.*, b.*
                FROM Stores s
                LEFT JOIN Addresses a ON s.AddressId = a.Id
                LEFT JOIN Brands b ON s.BrandId = b.Id
                WHERE s.BrandId = @BrandId
                ORDER BY s.Name";
            
            var parameters = new { BrandId = brandId.Value };
            var dbConnection = await _connection.GetConnectionAsync();
            
            var stores = await dbConnection.QueryAsync<Store, Address, Brand, Store>(
                sql,
                (store, address, brand) =>
                {
                    if (address != null)
                    {
                        store.SetAddress(address);
                    }
                    
                    return store;
                },
                parameters,
                splitOn: "Id,Id"
            );
            
            return stores;
        }

        public async Task<Store> AddAsync(Store store)
        {
            // First insert the address
            var addressId = Guid.NewGuid();
            const string addressSql = @"
                INSERT INTO Addresses (Id, Line1, Line2, City, State, PostalCode, Country, CreatedAt, UpdatedAt)
                VALUES (@Id, @Line1, @Line2, @City, @State, @PostalCode, @Country, @CreatedAt, @UpdatedAt)
                RETURNING *";
            
            var dbConnection = await _connection.GetConnectionAsync();
            var addressParams = new
            {
                Id = addressId,
                store.Address.Line1,
                store.Address.Line2,
                store.Address.City,
                store.Address.State,
                store.Address.PostalCode,
                store.Address.Country,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var address = await dbConnection.QuerySingleAsync<Address>(addressSql, addressParams);
            
            // Finally insert the store
            const string storeSql = @"
                INSERT INTO Stores (Id, Name, AddressId, ContactInfo, BrandId, CreatedAt, UpdatedAt)
                VALUES (@Id, @Name, @AddressId, @ContactInfo, @BrandId, @CreatedAt, @UpdatedAt)
                RETURNING *";
            
            var storeParams = new
            {
                Id = store.Id,
                store.Name,
                AddressId = addressId,
                store.ContactInfo,
                BrandId = store.BrandId,
                store.CreatedAt,
                store.UpdatedAt
            };
            
            var newStore = await dbConnection.QuerySingleAsync<Store>(storeSql, storeParams);
            
            // Set the address
            newStore.SetAddress(address);
            
            return newStore;
        }

        public async Task UpdateAsync(Store store)
        {
            // Update address
            const string addressSql = @"
                UPDATE Addresses
                SET Line1 = @Line1,
                    Line2 = @Line2,
                    City = @City,
                    State = @State,
                    PostalCode = @PostalCode,
                    Country = @Country,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            var dbConnection = await _connection.GetConnectionAsync();
            
            // Get the address ID from the database
            const string getAddressIdSql = @"SELECT AddressId FROM Stores WHERE Id = @Id";
            var addressId = await dbConnection.ExecuteScalarAsync<Guid>(getAddressIdSql, new { Id = store.Id });
            
            var addressParams = new
            {
                Id = addressId,
                store.Address.Line1,
                store.Address.Line2,
                store.Address.City,
                store.Address.State,
                store.Address.PostalCode,
                store.Address.Country,
                UpdatedAt = DateTime.UtcNow
            };
            
            await dbConnection.ExecuteAsync(addressSql, addressParams);
            
            // Update store
            const string storeSql = @"
                UPDATE Stores
                SET Name = @Name,
                    ContactInfo = @ContactInfo,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            var storeParams = new
            {
                Id = store.Id,
                store.Name,
                store.ContactInfo,
                UpdatedAt = DateTime.UtcNow
            };
            
            await dbConnection.ExecuteAsync(storeSql, storeParams);
        }

        public async Task<IEnumerable<Store>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm)
        {
            // This is a simplified approach. In a real application, you would use a spatial database or a more sophisticated algorithm
            const string sql = @"
                SELECT s.*, a.*, b.*,
                       (6371 * acos(cos(radians(@Latitude)) * cos(radians(JSON_VALUE(s.Location, '$.Latitude'))) * 
                        cos(radians(JSON_VALUE(s.Location, '$.Longitude')) - radians(@Longitude)) + 
                        sin(radians(@Latitude)) * sin(radians(JSON_VALUE(s.Location, '$.Latitude'))))) AS Distance
                FROM Stores s
                LEFT JOIN Addresses a ON s.AddressId = a.Id
                LEFT JOIN Brands b ON s.BrandId = b.Id
                HAVING Distance < @Radius
                ORDER BY Distance";

            var parameters = new { Latitude = latitude, Longitude = longitude, Radius = radiusKm };
            
            var storeDict = new Dictionary<Guid, Store>();
            
            var dbConnection = await _connection.GetConnectionAsync();
            var stores = await dbConnection.QueryAsync<Store, Address, Brand, Store>(
                sql,
                (store, address, brand) =>
                {
                    if (!storeDict.TryGetValue(store.Id, out var existingStore))
                    {
                        existingStore = store;
                        if (address != null)
                        {
                            existingStore.SetAddress(address);
                        }
                        
                        storeDict.Add(existingStore.Id, existingStore);
                    }
                    
                    return existingStore;
                },
                parameters,
                splitOn: "Id,Id"
            );
            
            return storeDict.Values;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(StoreId storeId, DateTime start, DateTime end, int page, int pageSize)
        {
            const string sql = @"
                SELECT t.*, lc.*, r.*
                FROM Transactions t
                JOIN LoyaltyCards lc ON t.LoyaltyCardId = lc.Id
                LEFT JOIN Rewards r ON t.RewardId = r.Id
                WHERE t.StoreId = @StoreId AND t.TransactionDate BETWEEN @Start AND @End
                ORDER BY t.TransactionDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                StoreId = storeId.Value,
                Start = start,
                End = end,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };
            
            var dbConnection = await _connection.GetConnectionAsync();
            var transactions = await dbConnection.QueryAsync<Transaction, LoyaltyCard, Reward, Transaction>(
                sql,
                (transaction, loyaltyCard, reward) =>
                {
                    // Don't try to set properties directly, use methods if available
                    return transaction;
                },
                parameters,
                splitOn: "Id,Id"
            );
            
            return transactions;
        }

        public async Task<int> GetTransactionCountAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Transactions
                WHERE StoreId = @StoreId AND TransactionDate BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<int> GetTotalStampsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COALESCE(SUM(StampsEarned), 0)
                FROM Transactions
                WHERE StoreId = @StoreId AND TransactionDate BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<decimal> GetTotalPointsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COALESCE(SUM(PointsEarned), 0)
                FROM Transactions
                WHERE StoreId = @StoreId AND TransactionDate BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<decimal>(sql, parameters);
        }

        public async Task<int> GetRedemptionCountAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Transactions
                WHERE StoreId = @StoreId AND RewardId IS NOT NULL AND TransactionDate BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }
    }
} 