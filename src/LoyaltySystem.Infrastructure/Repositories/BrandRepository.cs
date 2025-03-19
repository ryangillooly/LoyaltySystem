using Dapper;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.ValueObjects;
using LoyaltySystem.Infrastructure.Data;
using LoyaltySystem.Infrastructure.Data.Extensions;
using LoyaltySystem.Infrastructure.Data.TypeHandlers;
using System.Data;
using System.Reflection;

namespace LoyaltySystem.Infrastructure.Repositories;
public class BrandRepository : IBrandRepository
{
    private readonly IDatabaseConnection _dbConnection;

    public BrandRepository(IDatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        TypeHandlerConfig.Initialize();
    }
    
    public async Task<Brand> GetByIdAsync(BrandId id)
    {
        const string sql = @"
            SELECT 
                -- Brand Information
                b.id,
                b.business_id as BusinessId, 
                b.name, 
                b.category, 
                b.logo, 
                b.description, 
                b.created_at as CreatedAt, 
                b.updated_at as UpdatedAt,  

                -- Brand Contact Information
                c.email, 
                c.phone, 
                c.website,     

                -- Brand Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code AS PostalCode, 
                a.country
            FROM 
                brands b                              LEFT JOIN 
                brand_contacts c ON b.id = c.brand_id LEFT JOIN 
                brand_addresses a ON b.id = a.brand_id
            WHERE 
                b.id = @Id";

        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        var brand = await dbConnection.QueryAsync<BrandRecord, ContactRecord, AddressRecord, Brand>(
            sql,
            (brandRecord, contactRecord, addressRecord) =>
            {
                // Create contact and address objects
                var contact = new ContactInfo(
                    contactRecord?.Email ?? string.Empty,
                    contactRecord?.Phone ?? string.Empty,
                    contactRecord?.Website ?? string.Empty
                );
                
                var address = new Address(
                    addressRecord?.Line1 ?? string.Empty,
                    addressRecord?.Line2 ?? string.Empty,
                    addressRecord?.City ?? string.Empty,
                    addressRecord?.State ?? string.Empty,
                    addressRecord?.PostalCode ?? string.Empty,
                    addressRecord?.Country ?? string.Empty
                );
                
                // Create the brand using our mapper
                return DomainMapper.MapToBrand(brandRecord, contact, address);
            },
            new { Id = id.Value },
            splitOn: "email,line1");
        
        return brand.FirstOrDefault();
    }
    
    public async Task<IEnumerable<Brand>> GetAllAsync(int skip = 0, int limit = 50)
    {
        const string sql = @"
            SELECT 
                -- Brand Information
                b.id,
                b.business_id as BusinessId, 
                b.name, 
                b.category, 
                b.logo, 
                b.description, 
                b.created_at as CreatedAt, 
                b.updated_at as UpdatedAt,  

                -- Brand Contact Information
                c.email, 
                c.phone, 
                c.website,     

                -- Brand Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code AS PostalCode, 
                a.country
            FROM 
                brands b                              LEFT JOIN 
                brand_contacts c ON b.id = c.brand_id LEFT JOIN 
                brand_addresses a ON b.id = a.brand_id
            ORDER BY b.name
            LIMIT @Limit OFFSET @Skip";

        var connection = await _dbConnection.GetConnectionAsync();
        
        // Use a dictionary to handle duplicate brands that might be returned due to the JOINs
        var brandDictionary = new Dictionary<Guid, Brand>();
        
        // Execute single query with multi-mapping
        await connection.QueryAsync<BrandRecord, ContactRecord, AddressRecord, Brand>(
            sql,
            (brandRecord, contactRecord, addressRecord) =>
            {
                // Check if we've already seen this brand
                if (!brandDictionary.TryGetValue(brandRecord.Id, out var existingBrand))
                {
                    // Create contact and address objects
                    var contact = new ContactInfo(
                        contactRecord?.Email ?? string.Empty,
                        contactRecord?.Phone ?? string.Empty,
                        contactRecord?.Website ?? string.Empty
                    );
                    
                    var address = new Address(
                        addressRecord?.Line1 ?? string.Empty,
                        addressRecord?.Line2 ?? string.Empty,
                        addressRecord?.City ?? string.Empty,
                        addressRecord?.State ?? string.Empty,
                        addressRecord?.PostalCode ?? string.Empty,
                        addressRecord?.Country ?? string.Empty
                    );
                    
                    // Create the brand using our mapper
                    existingBrand = DomainMapper.MapToBrand(brandRecord, contact, address);
                    brandDictionary.Add(existingBrand.Id.Value, existingBrand);
                }
                
                return existingBrand;
            },
            new { Skip = skip, Limit = limit },
            splitOn: "email,line1");
        
        return brandDictionary.Values;
    }
    
    public async Task<int> GetTotalCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM brands";
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        return await dbConnection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<Brand> AddAsync(Brand brand, IDbTransaction transaction = null)
    {
        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        // Track whether we created the transaction or are using an existing one
        bool ownsTransaction = transaction == null;
        
        // If no transaction was provided, create one
        transaction ??= dbConnection.BeginTransaction();

        try
        {
            await dbConnection.ExecuteAsync(@"
                INSERT INTO 
                    brands 
                        (id, business_id, name, category, logo, description, created_at, updated_at)
                    VALUES 
                        (@BrandId, @BusinessId, @Name, @Category, @Logo, @Description, @CreatedAt, @UpdatedAt)
                ", 
                new
                {
                    BrandId = brand.Id.Value,
                    BusinessId = brand.BusinessId.Value,
                    brand.Name,
                    brand.Category,
                    brand.Logo,
                    brand.Description,
                    brand.CreatedAt,
                    brand.UpdatedAt
                },
                transaction
            );
            
            if (brand.Contact is { })
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        brand_contacts 
                            (brand_id, email, phone, website)
                        VALUES 
                            (@BrandId, @Email, @Phone, @Website)", 
                    new
                    {
                        BrandId = brand.Id.Value,
                        brand.Contact.Email,
                        brand.Contact.Phone,
                        brand.Contact.Website
                    },
                    transaction
                );
            }

            if (brand.Address is { })
            {
                await dbConnection.ExecuteAsync(@"
                    INSERT INTO 
                        brand_addresses 
                            (brand_id, line1, line2, city, state, postal_code, country)
                        VALUES 
                            (@BrandId, @Line1, @Line2, @City, @State, @PostalCode, @Country)", 
                    new
                    {
                        BrandId = brand.Id.Value,
                        brand.Address.Line1,
                        brand.Address.Line2,
                        brand.Address.City,
                        brand.Address.State,
                        brand.Address.PostalCode,
                        brand.Address.Country
                    },
                    transaction
                );
            }
            
            if (ownsTransaction)
                transaction.Commit();

            return brand;
        }
        catch (Exception ex)
        {
            if (ownsTransaction)
                transaction.Rollback();
            
            throw new Exception($"Error adding brand: {ex.Message}", ex);
        }
        finally
        {
            if (ownsTransaction && transaction != null)
                transaction.Dispose();
        }
    }

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
        
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                await connection.ExecuteAsync(updateBrandSql, new
                {
                    Id = brand.Id.Value,
                    brand.Name,
                    brand.Category,
                    brand.Logo,
                    brand.Description,
                    brand.UpdatedAt
                }, transaction);
                
                if (brand.Contact != null)
                {
                    await connection.ExecuteAsync(updateContactSql, new 
                    { 
                        BrandId = brand.Id.Value, 
                        brand.Contact.Email, 
                        brand.Contact.Phone, 
                        brand.Contact.Website 
                    }, transaction);
                }
                
                if (brand.Address != null)
                {
                    await connection.ExecuteAsync(updateAddressSql, new 
                    { 
                        BrandId = brand.Id.Value, 
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
    
    public async Task<IEnumerable<Brand>> GetByCategoryAsync(string category, int skip = 0, int limit = 50)
    {
        const string sql = @"
            SELECT 
                -- Brand Information
                b.id,
                b.business_id as BusinessId, 
                b.name, 
                b.category, 
                b.logo, 
                b.description, 
                b.created_at as CreatedAt, 
                b.updated_at as UpdatedAt,  

                -- Brand Contact Information
                c.email, 
                c.phone, 
                c.website,     

                -- Brand Address Information
                a.line1, 
                a.line2, 
                a.city, 
                a.state, 
                a.postal_code AS PostalCode, 
                a.country
            FROM 
                brands b                              LEFT JOIN 
                brand_contacts c ON b.id = c.brand_id LEFT JOIN 
                brand_addresses a ON b.id = a.brand_id
            WHERE 
                b.category = @Category
            ORDER BY b.name
            LIMIT @Limit OFFSET @Skip";

        var connection = await _dbConnection.GetConnectionAsync();
        
        // Use a dictionary to handle duplicate brands that might be returned due to the JOINs
        var brandDictionary = new Dictionary<Guid, Brand>();
        
        // Execute single query with multi-mapping
        await connection.QueryAsync<BrandRecord, ContactRecord, AddressRecord, Brand>(
            sql,
            (brandRecord, contactRecord, addressRecord) =>
            {
                // Check if we've already seen this brand
                if (!brandDictionary.TryGetValue(brandRecord.Id, out var existingBrand))
                {
                    // Create contact and address objects
                    var contact = new ContactInfo(
                        contactRecord?.Email ?? string.Empty,
                        contactRecord?.Phone ?? string.Empty,
                        contactRecord?.Website ?? string.Empty
                    );
                    
                    var address = new Address(
                        addressRecord?.Line1 ?? string.Empty,
                        addressRecord?.Line2 ?? string.Empty,
                        addressRecord?.City ?? string.Empty,
                        addressRecord?.State ?? string.Empty,
                        addressRecord?.PostalCode ?? string.Empty,
                        addressRecord?.Country ?? string.Empty
                    );
                    
                    // Create the brand using our mapper
                    existingBrand = DomainMapper.MapToBrand(brandRecord, contact, address);
                    brandDictionary.Add(existingBrand.Id.Value, existingBrand);
                }
                
                return existingBrand;
            },
            new { Category = category, Skip = skip, Limit = limit },
            splitOn: "email,line1");
        
        return brandDictionary.Values;
    }
    
    public async Task<bool> ExistsByNameAsync(string name)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM brands 
            WHERE LOWER(name) = LOWER(@Name)";
        
        var parameters = new { Name = name };
        
        var dbConnection = await _dbConnection.GetConnectionAsync();
        
        var count = await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
        
        return count > 0;
    }

    
    // DTO records for database mapping
    private class BrandRecord
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    private class ContactRecord
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
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
    
    /// <summary>
    /// Special factory for Dapper materialization that creates domain entities with specific property values
    /// directly from database records. This is used only for Dapper's multi-mapping functionality.
    /// 
    /// NOTE: This factory uses reflection to create and populate domain entities, but isolates all reflection
    /// to this single factory class. This approach is necessary because:
    /// 1. Domain entities have private setters for ID and timestamps
    /// 2. We need to preserve the database IDs when materializing entities
    /// 3. The domain models don't provide public APIs to set these values
    /// </summary>
    internal static class DapperEntityFactory
    {
        /// <summary>
        /// Creates a Brand entity with specific property values directly from database records.
        /// </summary>
        /// <param name="id">The brand ID from the database</param>
        /// <param name="businessId">The business ID from the database</param>
        /// <param name="name">The brand name</param>
        /// <param name="category">The brand category</param>
        /// <param name="logo">The brand logo URL</param>
        /// <param name="description">The brand description</param>
        /// <param name="contact">The contact info object</param>
        /// <param name="address">The address object</param>
        /// <param name="createdAt">The creation timestamp from the database</param>
        /// <param name="updatedAt">The last update timestamp from the database</param>
        /// <returns>A fully populated Brand entity</returns>
        public static Brand CreateBrand(
            Guid id,
            Guid businessId,
            string name,
            string category,
            string logo,
            string description,
            ContactInfo contact,
            Address address,
            DateTime createdAt,
            DateTime updatedAt)
        {
            try
            {
                // Use the parameterless private constructor
                var brand = (Brand)Activator.CreateInstance(typeof(Brand), true);
                if (brand == null)
                {
                    throw new InvalidOperationException("Failed to create Brand instance using reflection");
                }
                
                // Set all properties
                SetProperty(brand, "Id", new BrandId(id));
                SetProperty(brand, "BusinessId", new BusinessId(businessId));
                SetProperty(brand, "Name", name ?? string.Empty);
                SetProperty(brand, "Category", category ?? string.Empty);
                SetProperty(brand, "Logo", logo ?? string.Empty);
                SetProperty(brand, "Description", description ?? string.Empty);
                SetProperty(brand, "Contact", contact);
                SetProperty(brand, "Address", address);
                SetProperty(brand, "CreatedAt", createdAt);
                SetProperty(brand, "UpdatedAt", updatedAt);
                
                return brand;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating Brand entity: {ex.Message}", ex);
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
    
    // Single unified DTO mapper that handles all entity mappings consistently
    // Reflection is isolated to the DapperEntityFactory
    private static class DomainMapper
    {
        public static Brand MapToBrand(BrandRecord record, ContactInfo contact, Address address)
        {
            try
            {
                // Use the specialized factory for Dapper materialization
                return DapperEntityFactory.CreateBrand(
                    record.Id,
                    record.BusinessId,
                    record.Name ?? string.Empty,
                    record.Category ?? string.Empty,
                    record.Logo ?? string.Empty,
                    record.Description ?? string.Empty,
                    contact,
                    address,
                    record.CreatedAt,
                    record.UpdatedAt
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error mapping brand from record: {ex.Message}");
                throw;
            }
        }
    }
}