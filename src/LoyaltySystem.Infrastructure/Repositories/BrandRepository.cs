using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Brand entities using Dapper.
    /// </summary>
    public class BrandRepository : IBrandRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public BrandRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// Gets a brand by its ID.
        /// </summary>
        public async Task<Brand> GetByIdAsync(Guid id)
        {
            const string sql = @"
                SELECT 
                    b.Id, b.Name, b.Category, b.Logo, b.Description, b.CreatedAt, b.UpdatedAt,
                    c.Email, c.Phone, c.Website,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country
                FROM Brands b
                LEFT JOIN BrandContacts c ON b.Id = c.BrandId
                LEFT JOIN BrandAddresses a ON b.Id = a.BrandId
                WHERE b.Id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var brandDictionary = new Dictionary<Guid, Brand>();
            
            var brands = await connection.QueryAsync<Brand, ContactInfo, Address, Brand>(
                sql,
                (brand, contact, address) =>
                {
                    if (!brandDictionary.TryGetValue(brand.Id, out var existingBrand))
                    {
                        existingBrand = brand;
                        existingBrand.SetContactInfo(contact);
                        existingBrand.SetAddress(address);
                        brandDictionary.Add(existingBrand.Id, existingBrand);
                    }
                    
                    return existingBrand;
                },
                new { Id = id },
                splitOn: "Email,Line1");
                
            return brands.FirstOrDefault();
        }

        /// <summary>
        /// Gets all brands with optional paging.
        /// </summary>
        public async Task<IEnumerable<Brand>> GetAllAsync(int skip = 0, int take = 50)
        {
            const string sql = @"
                SELECT 
                    b.Id, b.Name, b.Category, b.Logo, b.Description, b.CreatedAt, b.UpdatedAt,
                    c.Email, c.Phone, c.Website,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country
                FROM Brands b
                LEFT JOIN BrandContacts c ON b.Id = c.BrandId
                LEFT JOIN BrandAddresses a ON b.Id = a.BrandId
                ORDER BY b.Name
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var brandDictionary = new Dictionary<Guid, Brand>();
            
            var brands = await connection.QueryAsync<Brand, ContactInfo, Address, Brand>(
                sql,
                (brand, contact, address) =>
                {
                    if (!brandDictionary.TryGetValue(brand.Id, out var existingBrand))
                    {
                        existingBrand = brand;
                        existingBrand.SetContactInfo(contact);
                        existingBrand.SetAddress(address);
                        brandDictionary.Add(existingBrand.Id, existingBrand);
                    }
                    
                    return existingBrand;
                },
                new { Skip = skip, Take = take },
                splitOn: "Email,Line1");
                
            return brands.Distinct();
        }

        /// <summary>
        /// Gets brands by category.
        /// </summary>
        public async Task<IEnumerable<Brand>> GetByCategoryAsync(string category, int skip = 0, int take = 50)
        {
            const string sql = @"
                SELECT 
                    b.Id, b.Name, b.Category, b.Logo, b.Description, b.CreatedAt, b.UpdatedAt,
                    c.Email, c.Phone, c.Website,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country
                FROM Brands b
                LEFT JOIN BrandContacts c ON b.Id = c.BrandId
                LEFT JOIN BrandAddresses a ON b.Id = a.BrandId
                WHERE b.Category = @Category
                ORDER BY b.Name
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var brandDictionary = new Dictionary<Guid, Brand>();
            
            var brands = await connection.QueryAsync<Brand, ContactInfo, Address, Brand>(
                sql,
                (brand, contact, address) =>
                {
                    if (!brandDictionary.TryGetValue(brand.Id, out var existingBrand))
                    {
                        existingBrand = brand;
                        existingBrand.SetContactInfo(contact);
                        existingBrand.SetAddress(address);
                        brandDictionary.Add(existingBrand.Id, existingBrand);
                    }
                    
                    return existingBrand;
                },
                new { Category = category, Skip = skip, Take = take },
                splitOn: "Email,Line1");
                
            return brands.Distinct();
        }

        /// <summary>
        /// Adds a new brand.
        /// </summary>
        public async Task AddAsync(Brand brand)
        {
            const string insertBrandSql = @"
                INSERT INTO Brands (Id, Name, Category, Logo, Description, CreatedAt, UpdatedAt)
                VALUES (@Id, @Name, @Category, @Logo, @Description, @CreatedAt, @UpdatedAt)";
                
            const string insertContactSql = @"
                INSERT INTO BrandContacts (BrandId, Email, Phone, Website)
                VALUES (@BrandId, @Email, @Phone, @Website)";
                
            const string insertAddressSql = @"
                INSERT INTO BrandAddresses (BrandId, Line1, Line2, City, State, PostalCode, Country)
                VALUES (@BrandId, @Line1, @Line2, @City, @State, @PostalCode, @Country)";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(insertBrandSql, brand, transaction);
                    
                    if (brand.Contact != null)
                    {
                        await connection.ExecuteAsync(insertContactSql, new 
                        { 
                            BrandId = brand.Id, 
                            brand.Contact.Email, 
                            brand.Contact.Phone, 
                            brand.Contact.Website 
                        }, transaction);
                    }
                    
                    if (brand.Address != null)
                    {
                        await connection.ExecuteAsync(insertAddressSql, new 
                        { 
                            BrandId = brand.Id, 
                            brand.Address.Line1, 
                            brand.Address.Line2, 
                            brand.Address.City, 
                            brand.Address.State, 
                            brand.Address.PostalCode, 
                            brand.Address.Country 
                        }, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates an existing brand.
        /// </summary>
        public async Task UpdateAsync(Brand brand)
        {
            const string updateBrandSql = @"
                UPDATE Brands
                SET Name = @Name, 
                    Category = @Category, 
                    Logo = @Logo, 
                    Description = @Description, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
                
            const string updateContactSql = @"
                UPDATE BrandContacts
                SET Email = @Email, 
                    Phone = @Phone, 
                    Website = @Website
                WHERE BrandId = @BrandId";
                
            const string updateAddressSql = @"
                UPDATE BrandAddresses
                SET Line1 = @Line1, 
                    Line2 = @Line2, 
                    City = @City, 
                    State = @State, 
                    PostalCode = @PostalCode, 
                    Country = @Country
                WHERE BrandId = @BrandId";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(updateBrandSql, brand, transaction);
                    
                    if (brand.Contact != null)
                    {
                        await connection.ExecuteAsync(updateContactSql, new 
                        { 
                            BrandId = brand.Id, 
                            brand.Contact.Email, 
                            brand.Contact.Phone, 
                            brand.Contact.Website 
                        }, transaction);
                    }
                    
                    if (brand.Address != null)
                    {
                        await connection.ExecuteAsync(updateAddressSql, new 
                        { 
                            BrandId = brand.Id, 
                            brand.Address.Line1, 
                            brand.Address.Line2, 
                            brand.Address.City, 
                            brand.Address.State, 
                            brand.Address.PostalCode, 
                            brand.Address.Country 
                        }, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets all stores for a brand.
        /// </summary>
        public async Task<IEnumerable<Store>> GetStoresForBrandAsync(Guid brandId)
        {
            const string sql = @"
                SELECT 
                    s.Id, s.BrandId, s.Name, s.ContactInfo, s.CreatedAt, s.UpdatedAt,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country,
                    g.Latitude, g.Longitude
                FROM Stores s
                LEFT JOIN StoreAddresses a ON s.Id = a.StoreId
                LEFT JOIN StoreGeoLocations g ON s.Id = g.StoreId
                WHERE s.BrandId = @BrandId
                ORDER BY s.Name";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var storeDictionary = new Dictionary<Guid, Store>();
            
            var stores = await connection.QueryAsync<Store, Address, GeoLocation, Store>(
                sql,
                (store, address, location) =>
                {
                    if (!storeDictionary.TryGetValue(store.Id, out var existingStore))
                    {
                        existingStore = store;
                        existingStore.SetAddress(address);
                        existingStore.SetLocation(location);
                        storeDictionary.Add(existingStore.Id, existingStore);
                    }
                    
                    return existingStore;
                },
                new { BrandId = brandId },
                splitOn: "Line1,Latitude");
                
            return stores.Distinct();
        }

        /// <summary>
        /// Gets a specific store.
        /// </summary>
        public async Task<Store> GetStoreByIdAsync(Guid storeId)
        {
            const string sql = @"
                SELECT 
                    s.Id, s.BrandId, s.Name, s.ContactInfo, s.CreatedAt, s.UpdatedAt,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country,
                    g.Latitude, g.Longitude
                FROM Stores s
                LEFT JOIN StoreAddresses a ON s.Id = a.StoreId
                LEFT JOIN StoreGeoLocations g ON s.Id = g.StoreId
                WHERE s.Id = @Id";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var storeDictionary = new Dictionary<Guid, Store>();
            
            var stores = await connection.QueryAsync<Store, Address, GeoLocation, Store>(
                sql,
                (store, address, location) =>
                {
                    if (!storeDictionary.TryGetValue(store.Id, out var existingStore))
                    {
                        existingStore = store;
                        existingStore.SetAddress(address);
                        existingStore.SetLocation(location);
                        storeDictionary.Add(existingStore.Id, existingStore);
                    }
                    
                    return existingStore;
                },
                new { Id = storeId },
                splitOn: "Line1,Latitude");
                
            return stores.FirstOrDefault();
        }

        /// <summary>
        /// Adds a new store.
        /// </summary>
        public async Task AddStoreAsync(Store store)
        {
            const string insertStoreSql = @"
                INSERT INTO Stores (Id, BrandId, Name, ContactInfo, CreatedAt, UpdatedAt)
                VALUES (@Id, @BrandId, @Name, @ContactInfo, @CreatedAt, @UpdatedAt)";
                
            const string insertAddressSql = @"
                INSERT INTO StoreAddresses (StoreId, Line1, Line2, City, State, PostalCode, Country)
                VALUES (@StoreId, @Line1, @Line2, @City, @State, @PostalCode, @Country)";
                
            const string insertLocationSql = @"
                INSERT INTO StoreGeoLocations (StoreId, Latitude, Longitude)
                VALUES (@StoreId, @Latitude, @Longitude)";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(insertStoreSql, store, transaction);
                    
                    if (store.Address != null)
                    {
                        await connection.ExecuteAsync(insertAddressSql, new 
                        { 
                            StoreId = store.Id, 
                            store.Address.Line1, 
                            store.Address.Line2, 
                            store.Address.City, 
                            store.Address.State, 
                            store.Address.PostalCode, 
                            store.Address.Country 
                        }, transaction);
                    }
                    
                    if (store.Location != null)
                    {
                        await connection.ExecuteAsync(insertLocationSql, new 
                        { 
                            StoreId = store.Id, 
                            store.Location.Latitude, 
                            store.Location.Longitude 
                        }, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates an existing store.
        /// </summary>
        public async Task UpdateStoreAsync(Store store)
        {
            const string updateStoreSql = @"
                UPDATE Stores
                SET Name = @Name, 
                    ContactInfo = @ContactInfo, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
                
            const string updateAddressSql = @"
                UPDATE StoreAddresses
                SET Line1 = @Line1, 
                    Line2 = @Line2, 
                    City = @City, 
                    State = @State, 
                    PostalCode = @PostalCode, 
                    Country = @Country
                WHERE StoreId = @StoreId";
                
            const string updateLocationSql = @"
                UPDATE StoreGeoLocations
                SET Latitude = @Latitude, 
                    Longitude = @Longitude
                WHERE StoreId = @StoreId";

            var connection = await _dbConnection.GetConnectionAsync();
            
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    await connection.ExecuteAsync(updateStoreSql, store, transaction);
                    
                    if (store.Address != null)
                    {
                        await connection.ExecuteAsync(updateAddressSql, new 
                        { 
                            StoreId = store.Id, 
                            store.Address.Line1, 
                            store.Address.Line2, 
                            store.Address.City, 
                            store.Address.State, 
                            store.Address.PostalCode, 
                            store.Address.Country 
                        }, transaction);
                    }
                    
                    if (store.Location != null)
                    {
                        await connection.ExecuteAsync(updateLocationSql, new 
                        { 
                            StoreId = store.Id, 
                            store.Location.Latitude, 
                            store.Location.Longitude 
                        }, transaction);
                    }
                    
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets stores near a location with optional distance constraint.
        /// </summary>
        public async Task<IEnumerable<Store>> GetStoresNearLocationAsync(double latitude, double longitude, double maxDistanceKm)
        {
            // Here we need to retrieve all stores and calculate distances in memory
            // In a real application, this should be handled by the database's spatial functions
            
            const string sql = @"
                SELECT 
                    s.Id, s.BrandId, s.Name, s.ContactInfo, s.CreatedAt, s.UpdatedAt,
                    a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country,
                    g.Latitude, g.Longitude
                FROM Stores s
                LEFT JOIN StoreAddresses a ON s.Id = a.StoreId
                LEFT JOIN StoreGeoLocations g ON s.Id = g.StoreId
                WHERE g.Latitude IS NOT NULL AND g.Longitude IS NOT NULL";

            var connection = await _dbConnection.GetConnectionAsync();
            
            var stores = await connection.QueryAsync<Store, Address, GeoLocation, Store>(
                sql,
                (store, address, location) =>
                {
                    store.SetAddress(address);
                    store.SetLocation(location);
                    return store;
                },
                splitOn: "Line1,Latitude");
                
            var targetLocation = new GeoLocation(latitude, longitude);
            
            return stores
                .Where(s => maxDistanceKm <= 0 || s.Location.DistanceToInKilometers(targetLocation) <= maxDistanceKm)
                .OrderBy(s => s.Location.DistanceToInKilometers(targetLocation))
                .ToList();
        }
    }
} 