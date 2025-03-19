using Dapper;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;
using LoyaltySystem.Infrastructure.Json;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace LoyaltySystem.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly IDatabaseConnection _dbConnection;
    private readonly JsonSerializerOptions _jsonOptions;

    public StoreRepository(IDatabaseConnection connection)
    {
        _dbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        TypeHandlerConfig.Initialize(); // Ensure type handlers are registered
        
        // Create JSON options with the OperatingHoursConverter
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        _jsonOptions.Converters.Add(new OperatingHoursConverter());
    }

    public async Task<Store?> GetByIdAsync(StoreId id)
    {
        const string sql = @"
            SELECT 
                -- Store Information
                s.id, 
                s.brand_id, 
                s.name, 
                s.created_at, 
                s.updated_at,
                
                -- Store Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code, 
                a.country,
                ST_Y(a.location::geometry) AS latitude, 
                ST_X(a.location::geometry) AS longitude,
                
                -- Opening Hours as JSONB
                s.opening_hours,
                
                -- Store Contact Information
                c.email,
                c.phone,
                c.website
            FROM 
                stores s                               LEFT JOIN 
                store_addresses a ON s.id = a.store_id LEFT JOIN 
                store_contacts c ON s.id = c.store_id
            WHERE 
                s.id = @Id";

        var parameters = new { Id = id.Value };
        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        // Use dynamic query instead of strongly-typed DTO
        var row = await dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);
        
        if (row == null)
            return null;
        
        // Create address
        var address = new Address(
            row.line1?.ToString() ?? string.Empty,
            row.line2?.ToString() ?? string.Empty,
            row.city?.ToString() ?? string.Empty,
            row.state?.ToString() ?? string.Empty,
            row.postal_code?.ToString() ?? string.Empty,
            row.country?.ToString() ?? string.Empty
        );
        
        // Create geolocation
        var location = new GeoLocation(
            row.latitude != null ? (double)row.latitude : 0,
            row.longitude != null ? (double)row.longitude : 0
        );
        
        // Create contact info
        var contactInfo = new ContactInfo(
            row.email?.ToString() ?? string.Empty,
            row.phone?.ToString() ?? string.Empty,
            row.website?.ToString() ?? string.Empty
        );
        
        // Parse opening hours from JSONB
        var openingHoursJson = row.opening_hours?.ToString();
        OperatingHours operatingHours;
        
        if (!string.IsNullOrEmpty(openingHoursJson))
        {
            // Use the JsonSerializer with our converter options
            operatingHours = JsonSerializer.Deserialize<OperatingHours>(openingHoursJson, _jsonOptions);
        }
        else
        {
            // Default empty hours
            operatingHours = new OperatingHours(new Dictionary<DayOfWeek, TimeRange>());
        }
        
        // Create store
        var store = new Store
        {
            Id = new StoreId(Guid.Parse(row.id.ToString())),
            BrandId = new BrandId(Guid.Parse(row.brand_id.ToString())),
            Name = row.name?.ToString() ?? string.Empty,
            Address = address,
            Location = location,
            OperatingHours = operatingHours,
            ContactInfo = contactInfo,
            CreatedAt = row.created_at != null ? (DateTime)row.created_at : DateTime.UtcNow,
            UpdatedAt = row.updated_at != null ? (DateTime)row.updated_at : DateTime.UtcNow
        };
        
        return store;
    }

    public async Task<IEnumerable<Store>> GetAllAsync(int skip = 0, int limit = 50)
    {
        const string sql = @"
            SELECT 
                -- Store Information
                s.id, 
                s.brand_id, 
                s.name, 
                s.created_at, 
                s.updated_at,
                
                -- Store Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code, 
                a.country,
                ST_Y(a.location::geometry) AS latitude, 
                ST_X(a.location::geometry) AS longitude,
                
                -- Opening Hours as JSONB
                s.opening_hours,
                
                -- Store Contact Information
                c.email,
                c.phone,
                c.website
            FROM 
                stores s                               LEFT JOIN 
                store_addresses a ON s.id = a.store_id LEFT JOIN 
                store_contacts c ON s.id = c.store_id
            ORDER BY s.name
            LIMIT @Limit OFFSET @Skip";
        
        var parameters = new { Skip = skip, Limit = limit };
        var connection = await _dbConnection.GetConnectionAsync();
        
        // Option 1: Using Dapper's multi-mapping with custom handling for JSONB
        var stores = new Dictionary<Guid, Store>();
        
        await connection.QueryAsync<dynamic>(sql, parameters)
            .ContinueWith(t => 
            {
                foreach (var row in t.Result)
                {
                    var storeId = Guid.Parse(row.id.ToString());
                    
                    if (!stores.TryGetValue(storeId, out Store? store))
                    {
                        // Create address
                        var address = new Address(
                            row.line1?.ToString() ?? string.Empty,
                            row.line2?.ToString() ?? string.Empty,
                            row.city?.ToString() ?? string.Empty,
                            row.state?.ToString() ?? string.Empty,
                            row.postal_code?.ToString() ?? string.Empty,
                            row.country?.ToString() ?? string.Empty
                        );
                        
                        // Create geolocation
                        var location = new GeoLocation(
                            row.latitude  != null ? (double) row.latitude : 0,
                            row.longitude != null ? (double) row.longitude : 0
                        );
                        
                        // Create contact info
                        var contactInfo = new ContactInfo(
                            row.email?.ToString() ?? string.Empty,
                            row.phone?.ToString() ?? string.Empty,
                            row.website?.ToString() ?? string.Empty
                        );
                        
                        // Parse opening hours from JSONB
                        var openingHoursJson = row.opening_hours?.ToString();
                        OperatingHours operatingHours;
                        
                        if (!string.IsNullOrEmpty(openingHoursJson))
                        {
                            // Use the JsonSerializer with our converter options
                            operatingHours = JsonSerializer.Deserialize<OperatingHours>(openingHoursJson, _jsonOptions);
                        }
                        else
                        {
                            // Default empty hours
                            operatingHours = new OperatingHours(new Dictionary<DayOfWeek, TimeRange>());
                        }
                        
                        // Create store with BrandId from string
                        store = new Store
                        {
                            Id = new StoreId(storeId),
                            BrandId = new BrandId(Guid.Parse(row.brand_id.ToString())),
                            Name = row.name?.ToString() ?? string.Empty,
                            Address = address,
                            Location = location,
                            OperatingHours = operatingHours,
                            ContactInfo = contactInfo,
                            CreatedAt = row.created_at != null ? (DateTime)row.created_at : DateTime.UtcNow,
                            UpdatedAt = row.updated_at != null ? (DateTime)row.updated_at : DateTime.UtcNow
                        };
                        
                        stores.Add(storeId, store);
                    }
                }
            });
        
        return stores.Values;
    }

    public async Task<IEnumerable<Store>> GetByBrandIdAsync(BrandId brandId)
    {
        const string sql = @"
            SELECT 
                -- Store Information
                s.id, 
                s.brand_id, 
                s.name, 
                s.contact_info, 
                s.created_at, 
                s.updated_at,
                
                -- Store Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code, 
                a.country,
                
                -- Store GeoLocation Information
                g.latitude, 
                g.longitude
            FROM 
                stores s                              LEFT JOIN 
                store_addresses a ON s.id = a.store_id LEFT JOIN 
                store_geo_locations g ON s.id = g.store_id
            WHERE 
                s.brand_id = @BrandId
            ORDER BY 
                s.name";

        var connection = await _dbConnection.GetConnectionAsync();
        
        var storeDictionary = new Dictionary<Guid, Store>();
        
        await connection.QueryAsync<StoreRecord, AddressRecord, GeoLocationRecord, Store>(
            sql,
            (storeRecord, addressRecord, locationRecord) =>
            {
                if (!storeDictionary.TryGetValue(storeRecord.Id, out var existingStore))
                {
                    // Create address object
                    var address = addressRecord != null ? new Address(
                        addressRecord.Line1 ?? string.Empty,
                        addressRecord.Line2 ?? string.Empty,
                        addressRecord.City ?? string.Empty,
                        addressRecord.State ?? string.Empty,
                        addressRecord.PostalCode ?? string.Empty,
                        addressRecord.Country ?? string.Empty
                    ) : null;
                    
                    // Create location object
                    var location = locationRecord != null ? new GeoLocation(
                        locationRecord.Latitude,
                        locationRecord.Longitude
                    ) : null;
                    
                    // Create store
                    existingStore = DomainMapper.MapToStore(storeRecord, address, location);
                    storeDictionary.Add(existingStore.Id, existingStore);
                }
                
                return existingStore;
            },
            new { BrandId = brandId.Value },
            splitOn: "line1,latitude");
            
        return storeDictionary.Values;
    }

    public async Task<Store> AddAsync(Store store, IDbTransaction transaction = null)
    {
        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        // Track whether we created the transaction or are using an existing one
        bool ownsTransaction = transaction == null;
        
        // If no transaction was provided, create one
        transaction ??= dbConnection.BeginTransaction();

        try
        {
            // Insert the main store record with opening hours as JSONB using our converter
            await dbConnection.ExecuteAsync(@"
                INSERT INTO stores 
                    (id, brand_id, name, opening_hours, created_at, updated_at)
                VALUES 
                    (@StoreId, @BrandId, @Name, @OpeningHours::jsonb, @CreatedAt, @UpdatedAt)",
                new
                {
                    StoreId = store.Id.Value,
                    BrandId = store.BrandId.Value,
                    store.Name,
                    OpeningHours = JsonSerializer.Serialize(store.OperatingHours, _jsonOptions),
                    store.CreatedAt,
                    store.UpdatedAt
                },
                transaction);

            // Insert address record if available
            if (store.Address is { })
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO store_addresses 
                        (store_id, line1, line2, city, state, postal_code, country, location)
                    VALUES 
                        (@StoreId, @Line1, @Line2, @City, @State, @PostalCode, @Country, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography)",
                    new
                    {
                        StoreId = store.Id.Value,
                        store.Address.Line1,
                        store.Address.Line2,
                        store.Address.City,
                        store.Address.State,
                        store.Address.PostalCode,
                        store.Address.Country,
                        store.Location.Longitude,
                        store.Location.Latitude
                    },
                    transaction);
            }
            
            // Insert contact information if available
            if (store.ContactInfo is { })
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO store_contacts
                        (store_id, email, phone, website)
                    VALUES
                        (@StoreId, @Email, @Phone, @Website)",
                    new
                    {
                        StoreId = store.Id.Value,
                        store.ContactInfo.Email,
                        store.ContactInfo.Phone,
                        store.ContactInfo.Website
                    },
                    transaction);
            }

            if (ownsTransaction)
                transaction.Commit();

            return store;
        }
        catch (Exception ex)
        {
            if (ownsTransaction)
                transaction.Rollback();
            
            throw new Exception($"Error adding store: {ex.Message}", ex);
        }
        finally
        {
            if (ownsTransaction && transaction != null)
                transaction.Dispose();
        }
    }

    public async Task UpdateAsync(Store store)
    {
        var connection = await _dbConnection.GetConnectionAsync();
        
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                await connection.ExecuteAsync(@"
                    UPDATE stores
                    SET name = @Name, 
                        contact_info = @ContactInfo, 
                        updated_at = @UpdatedAt
                    WHERE id = @Id",
                    new
                    {
                        store.Id,
                        store.Name,
                        store.ContactInfo,
                        store.UpdatedAt
                    }, 
                    transaction);
                
                if (store.Address != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE store_addresses
                        SET line1 = @Line1, 
                            line2 = @Line2, 
                            city = @City, 
                            state = @State, 
                            postal_code = @PostalCode, 
                            country = @Country
                        WHERE store_id = @StoreId",
                        new 
                        { 
                            StoreId = store.Id, 
                            store.Address.Line1, 
                            store.Address.Line2, 
                            store.Address.City, 
                            store.Address.State, 
                            store.Address.PostalCode, 
                            store.Address.Country 
                        }, 
                        transaction);
                }
                
                if (store.Location != null)
                {
                    await connection.ExecuteAsync(@"
                        UPDATE store_geo_locations
                        SET latitude = @Latitude, 
                            longitude = @Longitude
                        WHERE store_id = @StoreId",
                        new 
                        { 
                            StoreId = store.Id, 
                            store.Location.Latitude, 
                            store.Location.Longitude 
                        }, 
                        transaction);
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

    public async Task<IEnumerable<Store>> FindNearbyStoresAsync(double latitude, double longitude, double radiusKm)
    {
        const string sql = @"
            SELECT 
                -- Store Information
                s.id, 
                s.brand_id, 
                s.name, 
                s.contact_info, 
                s.created_at, 
                s.updated_at,
                
                -- Store Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code, 
                a.country,
                
                -- Store GeoLocation Information
                g.latitude, 
                g.longitude
            FROM 
                stores s                              LEFT JOIN 
                store_addresses a ON s.id = a.store_id LEFT JOIN 
                store_geo_locations g ON s.id = g.store_id
            WHERE 
                g.latitude IS NOT NULL AND g.longitude IS NOT NULL";

        var connection = await _dbConnection.GetConnectionAsync();
        
        var stores = await connection.QueryAsync<StoreRecord, AddressRecord, GeoLocationRecord, Store>(
            sql,
            (storeRecord, addressRecord, locationRecord) =>
            {
                // Create address object
                var address = addressRecord != null ? new Address(
                    addressRecord.Line1 ?? string.Empty,
                    addressRecord.Line2 ?? string.Empty,
                    addressRecord.City ?? string.Empty,
                    addressRecord.State ?? string.Empty,
                    addressRecord.PostalCode ?? string.Empty,
                    addressRecord.Country ?? string.Empty
                ) : null;
                
                // Create location object
                var location = locationRecord != null ? new GeoLocation(
                    locationRecord.Latitude,
                    locationRecord.Longitude
                ) : null;
                
                // Create store
                return DomainMapper.MapToStore(storeRecord, address, location);
            },
            splitOn: "line1,latitude");
            
        var targetLocation = new GeoLocation(latitude, longitude);
        
        return stores
            .Where(s => radiusKm <= 0 || s.Location.DistanceToInKilometers(targetLocation) <= radiusKm)
            .OrderBy(s => s.Location.DistanceToInKilometers(targetLocation))
            .ToList();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(StoreId storeId, DateTime start, DateTime end, int skip = 0, int limit = 50)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                card_id AS CardId,
                type::int AS Type,
                reward_id AS RewardId,
                quantity AS Quantity,
                points_amount AS PointsAmount,
                transaction_amount AS TransactionAmount,
                store_id AS StoreId,
                staff_id AS StaffId,
                pos_transaction_id AS PosTransactionId,
                timestamp AS Timestamp,
                created_at AS CreatedAt,
                metadata AS Metadata
            FROM transactions
            WHERE store_id = @StoreId
            AND timestamp BETWEEN @Start AND @End
            ORDER BY timestamp DESC
            LIMIT @Limit OFFSET @Skip";

        var parameters = new 
        { 
            StoreId = storeId.Value, 
            Start = start, 
            End = end, 
            Skip = skip, 
            Limit = limit 
        };
        
        var connection = await _dbConnection.GetConnectionAsync();
        return await connection.QueryAsync<Transaction>(sql, parameters);
    }

    public async Task<int> GetTransactionCountAsync(StoreId storeId, DateTime start, DateTime end)
    {
        // Implementation needed
        return 0;
    }

    public async Task<int> GetTotalStampsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
    {
        // Implementation needed
        return 0;
    }

    public async Task<decimal> GetTotalPointsIssuedAsync(StoreId storeId, DateTime start, DateTime end)
    {
        // Implementation needed
        return 0;
    }

    public async Task<int> GetRedemptionCountAsync(StoreId storeId, DateTime start, DateTime end)
    {
        // Implementation needed
        return 0;
    }

    // DTO records for database mapping
    private class StoreRecord
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    private class AddressRecord
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
    
    private class GeoLocationRecord
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    
    // Domain mapper for mapping from records to domain entities
    private static class DomainMapper
    {
        public static Store MapToStore(StoreRecord record, Address address, GeoLocation location)
        {
            try
            {
                // Create a default OperatingHours if needed
                var hours = new OperatingHours(new Dictionary<DayOfWeek, TimeRange>());
                
                // Use the specialized factory for Dapper materialization
                return DapperEntityFactory.CreateStore(
                    record.Id,
                    record.BrandId,
                    record.Name ?? string.Empty,
                    address,
                    location,
                    hours,
                    record.ContactInfo ?? string.Empty,
                    record.CreatedAt,
                    record.UpdatedAt
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error mapping store from record: {ex.Message}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Special factory for Dapper materialization that creates domain entities with specific property values
    /// directly from database records. This is used only for Dapper's multi-mapping functionality.
    /// </summary>
    internal static class DapperEntityFactory
    {
        /// <summary>
        /// Creates a Store entity with specific property values directly from database records.
        /// </summary>
        public static Store CreateStore(
            Guid id,
            Guid brandId,
            string name,
            Address address,
            GeoLocation location,
            OperatingHours hours,
            string contactInfo,
            DateTime createdAt,
            DateTime updatedAt)
        {
            try
            {
                // Use the parameterless private constructor
                var store = (Store)Activator.CreateInstance(typeof(Store), true);
                if (store == null)
                {
                    throw new InvalidOperationException("Failed to create Store instance using reflection");
                }
                
                // Set all properties
                SetProperty(store, "Id", id);
                SetProperty(store, "BrandId", brandId);
                SetProperty(store, "Name", name ?? string.Empty);
                SetProperty(store, "Address", address);
                SetProperty(store, "Location", location);
                SetProperty(store, "Hours", hours);
                SetProperty(store, "ContactInfo", contactInfo ?? string.Empty);
                SetProperty(store, "CreatedAt", createdAt);
                SetProperty(store, "UpdatedAt", updatedAt);
                
                return store;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating Store entity: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Helper method to set a property on an object using reflection
        /// </summary>
        private static void SetProperty(object target, string propertyName, object value)
        {
            var property = target.GetType().GetProperty(propertyName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (property == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found on type '{target.GetType().Name}'");
            }
            
            if (!property.CanWrite)
            {
                throw new InvalidOperationException($"Property '{propertyName}' on type '{target.GetType().Name}' is read-only");
            }
            
            property.SetValue(target, value);
        }
    }
    
    
    private static Store MapFromDto(StoreDto dto) =>
        new ()
        {
            Id = EntityId.Parse<StoreId>(dto.Id),
            BrandId = EntityId.Parse<BrandId>(dto.BrandId),
            Name = dto.Name,
            Address = dto.Address,
            Location = dto.Location,
            ContactInfo = dto.ContactInfo,
            OperatingHours = dto.OperatingHours,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            
        };
}