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
                SELECT 
                    s.id AS Id,
                    s.name AS Name,
                    s.address_id AS AddressId,
                    s.contact_info AS ContactInfo,
                    s.brand_id AS BrandId,
                    s.created_at AS CreatedAt,
                    s.updated_at AS UpdatedAt,
                    a.id AS Id,
                    a.line1 AS Line1,
                    a.line2 AS Line2,
                    a.city AS City,
                    a.state AS State,
                    a.postal_code AS PostalCode,
                    a.country AS Country,
                    a.created_at AS CreatedAt,
                    a.updated_at AS UpdatedAt,
                    b.id AS Id,
                    b.name AS Name,
                    b.category AS Category,
                    b.logo AS Logo,
                    b.description AS Description,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt
                FROM stores s
                LEFT JOIN addresses a ON s.address_id = a.id
                LEFT JOIN brands b ON s.brand_id = b.id
                WHERE s.id = @Id";

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
                SELECT 
                    s.id AS Id,
                    s.name AS Name,
                    s.address_id AS AddressId,
                    s.contact_info AS ContactInfo,
                    s.brand_id AS BrandId,
                    s.created_at AS CreatedAt,
                    s.updated_at AS UpdatedAt,
                    a.id AS Id,
                    a.line1 AS Line1,
                    a.line2 AS Line2,
                    a.city AS City,
                    a.state AS State,
                    a.postal_code AS PostalCode,
                    a.country AS Country,
                    a.created_at AS CreatedAt,
                    a.updated_at AS UpdatedAt,
                    b.id AS Id,
                    b.name AS Name,
                    b.category AS Category,
                    b.logo AS Logo,
                    b.description AS Description,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt
                FROM stores s
                LEFT JOIN addresses a ON s.address_id = a.id
                LEFT JOIN brands b ON s.brand_id = b.id
                ORDER BY s.name
                LIMIT @PageSize OFFSET @Offset";
            
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
                SELECT 
                    s.id AS Id,
                    s.name AS Name,
                    s.address_id AS AddressId,
                    s.contact_info AS ContactInfo,
                    s.brand_id AS BrandId,
                    s.created_at AS CreatedAt,
                    s.updated_at AS UpdatedAt,
                    a.id AS Id,
                    a.line1 AS Line1,
                    a.line2 AS Line2,
                    a.city AS City,
                    a.state AS State,
                    a.postal_code AS PostalCode,
                    a.country AS Country,
                    a.created_at AS CreatedAt,
                    a.updated_at AS UpdatedAt,
                    b.id AS Id,
                    b.name AS Name,
                    b.category AS Category,
                    b.logo AS Logo,
                    b.description AS Description,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt
                FROM stores s
                LEFT JOIN addresses a ON s.address_id = a.id
                LEFT JOIN brands b ON s.brand_id = b.id
                WHERE s.brand_id = @BrandId
                ORDER BY s.name";
            
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
                INSERT INTO addresses (id, line1, line2, city, state, postal_code, country, created_at, updated_at)
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
                INSERT INTO stores (id, name, address_id, contact_info, brand_id, created_at, updated_at)
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
                UPDATE addresses
                SET line1 = @Line1,
                    line2 = @Line2,
                    city = @City,
                    state = @State,
                    postal_code = @PostalCode,
                    country = @Country,
                    updated_at = @UpdatedAt
                WHERE id = @Id";
            
            var dbConnection = await _connection.GetConnectionAsync();
            
            // Get the address ID from the database
            const string getAddressIdSql = @"SELECT address_id FROM stores WHERE id = @Id";
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
                UPDATE stores
                SET name = @Name,
                    contact_info = @ContactInfo,
                    updated_at = @UpdatedAt
                WHERE id = @Id";
            
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
                SELECT 
                    s.id AS Id,
                    s.name AS Name,
                    s.address_id AS AddressId,
                    s.contact_info AS ContactInfo,
                    s.brand_id AS BrandId,
                    s.created_at AS CreatedAt,
                    s.updated_at AS UpdatedAt,
                    a.id AS Id,
                    a.line1 AS Line1,
                    a.line2 AS Line2,
                    a.city AS City,
                    a.state AS State,
                    a.postal_code AS PostalCode,
                    a.country AS Country,
                    a.created_at AS CreatedAt,
                    a.updated_at AS UpdatedAt,
                    b.id AS Id,
                    b.name AS Name,
                    b.category AS Category,
                    b.logo AS Logo,
                    b.description AS Description,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt,
                    (6371 * acos(cos(radians(@Latitude)) * cos(radians(cast(json_extract(s.location::json, '$.Latitude') as float))) * 
                    cos(radians(cast(json_extract(s.location::json, '$.Longitude') as float)) - radians(@Longitude)) + 
                    sin(radians(@Latitude)) * sin(radians(cast(json_extract(s.location::json, '$.Latitude') as float))))) AS Distance
                FROM stores s
                LEFT JOIN addresses a ON s.address_id = a.id
                LEFT JOIN brands b ON s.brand_id = b.id
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
                SELECT 
                    t.id AS Id,
                    t.card_id AS CardId,
                    t.type::int AS Type,
                    t.reward_id AS RewardId,
                    t.quantity AS Quantity,
                    t.points_amount AS PointsAmount,
                    t.transaction_amount AS TransactionAmount,
                    t.store_id AS StoreId,
                    t.staff_id AS StaffId,
                    t.pos_transaction_id AS PosTransactionId,
                    t.timestamp AS Timestamp,
                    t.created_at AS CreatedAt,
                    t.metadata AS Metadata,
                    lc.id AS Id,
                    lc.program_id AS ProgramId,
                    lc.customer_id AS CustomerId,
                    lc.type::int AS Type,
                    lc.stamps_collected AS StampsCollected,
                    lc.points_balance AS PointsBalance,
                    lc.status::int AS Status,
                    lc.qr_code AS QrCode,
                    lc.created_at AS CreatedAt,
                    lc.expires_at AS ExpiresAt,
                    lc.updated_at AS UpdatedAt,
                    r.id AS Id,
                    r.program_id AS ProgramId,
                    r.title AS Title,
                    r.description AS Description,
                    r.required_value AS RequiredValue,
                    r.valid_from AS ValidFrom,
                    r.valid_to AS ValidTo,
                    r.is_active AS IsActive,
                    r.created_at AS CreatedAt,
                    r.updated_at AS UpdatedAt
                FROM transactions t
                JOIN loyalty_cards lc ON t.card_id = lc.id
                LEFT JOIN rewards r ON t.reward_id = r.id
                WHERE t.store_id = @StoreId AND t.timestamp BETWEEN @Start AND @End
                ORDER BY t.timestamp DESC
                LIMIT @PageSize OFFSET @Offset";

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
                FROM transactions
                WHERE store_id = @StoreId AND timestamp BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<int> GetTotalStampsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COALESCE(SUM(quantity), 0)
                FROM transactions
                WHERE store_id = @StoreId 
                AND type = 'StampIssuance'::transaction_type 
                AND timestamp BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<decimal> GetTotalPointsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COALESCE(SUM(points_amount), 0)
                FROM transactions
                WHERE store_id = @StoreId 
                AND type = 'PointsIssuance'::transaction_type 
                AND timestamp BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<decimal>(sql, parameters);
        }

        public async Task<int> GetRedemptionCountAsync(StoreId storeId, DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM transactions
                WHERE store_id = @StoreId 
                AND type = 'RewardRedemption'::transaction_type 
                AND timestamp BETWEEN @Start AND @End";

            var parameters = new { StoreId = storeId.Value, Start = start, End = end };
            var dbConnection = await _connection.GetConnectionAsync();
            return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        }
    }
} 