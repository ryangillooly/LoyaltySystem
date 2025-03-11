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
                    b.id AS Id, 
                    b.name AS Name, 
                    b.category AS Category, 
                    b.logo AS Logo, 
                    b.description AS Description, 
                    b.created_at AS CreatedAt, 
                    b.updated_at AS UpdatedAt,
                    c.email AS Email, 
                    c.phone AS Phone, 
                    c.website AS Website,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country
                FROM brands b
                LEFT JOIN brand_contacts c ON b.id = c.brand_id
                LEFT JOIN brand_addresses a ON b.id = a.brand_id
                WHERE b.id = @Id";

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
                    b.id AS Id, 
                    b.name AS Name, 
                    b.category AS Category, 
                    b.logo AS Logo, 
                    b.description AS Description, 
                    b.created_at AS CreatedAt, 
                    b.updated_at AS UpdatedAt,
                    c.email AS Email, 
                    c.phone AS Phone, 
                    c.website AS Website,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country
                FROM brands b
                LEFT JOIN brand_contacts c ON b.id = c.brand_id
                LEFT JOIN brand_addresses a ON b.id = a.brand_id
                ORDER BY b.name
                LIMIT @Take OFFSET @Skip";

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
                    b.id AS Id, 
                    b.name AS Name, 
                    b.category AS Category, 
                    b.logo AS Logo, 
                    b.description AS Description, 
                    b.created_at AS CreatedAt, 
                    b.updated_at AS UpdatedAt,
                    c.email AS Email, 
                    c.phone AS Phone, 
                    c.website AS Website,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country
                FROM brands b
                LEFT JOIN brand_contacts c ON b.id = c.brand_id
                LEFT JOIN brand_addresses a ON b.id = a.brand_id
                WHERE b.category = @Category
                ORDER BY b.name
                LIMIT @Take OFFSET @Skip";

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
                INSERT INTO brands (id, name, category, logo, description, created_at, updated_at)
                VALUES (@Id, @Name, @Category, @Logo, @Description, @CreatedAt, @UpdatedAt)";
                
            const string insertContactSql = @"
                INSERT INTO brand_contacts (brand_id, email, phone, website)
                VALUES (@BrandId, @Email, @Phone, @Website)";
                
            const string insertAddressSql = @"
                INSERT INTO brand_addresses (brand_id, line1, line2, city, state, postal_code, country)
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
                UPDATE brands
                SET name = @Name, 
                    category = @Category, 
                    logo = @Logo, 
                    description = @Description, 
                    updated_at = @UpdatedAt
                WHERE id = @Id";
                
            const string updateContactSql = @"
                UPDATE brand_contacts
                SET email = @Email, 
                    phone = @Phone, 
                    website = @Website
                WHERE brand_id = @BrandId";
                
            const string updateAddressSql = @"
                UPDATE brand_addresses
                SET line1 = @Line1, 
                    line2 = @Line2, 
                    city = @City, 
                    state = @State, 
                    postal_code = @PostalCode, 
                    country = @Country
                WHERE brand_id = @BrandId";

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
                    s.id AS Id, 
                    s.brand_id AS BrandId, 
                    s.name AS Name, 
                    s.contact_info AS ContactInfo, 
                    s.created_at AS CreatedAt, 
                    s.updated_at AS UpdatedAt,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country,
                    g.latitude AS Latitude, 
                    g.longitude AS Longitude
                FROM stores s
                LEFT JOIN store_addresses a ON s.id = a.store_id
                LEFT JOIN store_geo_locations g ON s.id = g.store_id
                WHERE s.brand_id = @BrandId
                ORDER BY s.name";

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
                    s.id AS Id, 
                    s.brand_id AS BrandId, 
                    s.name AS Name, 
                    s.contact_info AS ContactInfo, 
                    s.created_at AS CreatedAt, 
                    s.updated_at AS UpdatedAt,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country,
                    g.latitude AS Latitude, 
                    g.longitude AS Longitude
                FROM stores s
                LEFT JOIN store_addresses a ON s.id = a.store_id
                LEFT JOIN store_geo_locations g ON s.id = g.store_id
                WHERE s.id = @Id";

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
                INSERT INTO stores (id, brand_id, name, contact_info, created_at, updated_at)
                VALUES (@Id, @BrandId, @Name, @ContactInfo, @CreatedAt, @UpdatedAt)";
                
            const string insertAddressSql = @"
                INSERT INTO store_addresses (store_id, line1, line2, city, state, postal_code, country)
                VALUES (@StoreId, @Line1, @Line2, @City, @State, @PostalCode, @Country)";
                
            const string insertLocationSql = @"
                INSERT INTO store_geo_locations (store_id, latitude, longitude)
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
                UPDATE stores
                SET name = @Name, 
                    contact_info = @ContactInfo, 
                    updated_at = @UpdatedAt
                WHERE id = @Id";
                
            const string updateAddressSql = @"
                UPDATE store_addresses
                SET line1 = @Line1, 
                    line2 = @Line2, 
                    city = @City, 
                    state = @State, 
                    postal_code = @PostalCode, 
                    country = @Country
                WHERE store_id = @StoreId";
                
            const string updateLocationSql = @"
                UPDATE store_geo_locations
                SET latitude = @Latitude, 
                    longitude = @Longitude
                WHERE store_id = @StoreId";

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
                    s.id AS Id, 
                    s.brand_id AS BrandId, 
                    s.name AS Name, 
                    s.contact_info AS ContactInfo, 
                    s.created_at AS CreatedAt, 
                    s.updated_at AS UpdatedAt,
                    a.line1 AS Line1, 
                    a.line2 AS Line2, 
                    a.city AS City, 
                    a.state AS State, 
                    a.postal_code AS PostalCode, 
                    a.country AS Country,
                    g.latitude AS Latitude, 
                    g.longitude AS Longitude
                FROM stores s
                LEFT JOIN store_addresses a ON s.id = a.store_id
                LEFT JOIN store_geo_locations g ON s.id = g.store_id
                WHERE g.latitude IS NOT NULL AND g.longitude IS NOT NULL";

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