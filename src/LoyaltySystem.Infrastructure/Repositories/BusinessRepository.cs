using System.Data;
using Dapper;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Infrastructure.Data;

namespace LoyaltySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Business entities using Dapper.
    /// </summary>
    public class BusinessRepository : IBusinessRepository
    {
        private readonly IDatabaseConnection _dbConnection;

        public BusinessRepository(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<Business> GetByIdAsync(BusinessId id)
        {
            const string sql = @"
                SELECT 
                    b.id AS Id,
                    b.name AS Name,
                    b.description AS Description,
                    b.tax_id AS TaxId,
                    b.logo AS Logo,
                    b.website AS Website,
                    b.founded_date AS FoundedDate,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt,
                    b.is_active AS IsActive,
                    
                    -- Contact Information
                    c.email AS Email,
                    c.phone AS Phone,
                    c.website AS ContactWebsite,
                    
                    -- Headquarters Address
                    a.line1 AS Line1,
                    a.line2 AS Line2,
                    a.city AS City,
                    a.state AS State,
                    a.postal_code AS PostalCode,
                    a.country AS Country
                FROM 
                    businesses b                                LEFT JOIN 
                    business_contacts c ON b.id = c.business_id LEFT JOIN 
                    business_addresses a ON b.id = a.business_id 
                WHERE 
                    b.id = @Id";

            var parameters = new { Id = id.Value };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var businessDto = await dbConnection.QueryFirstOrDefaultAsync<BusinessDto>(sql, parameters);
            
            if (businessDto == null)
                return null;

            return MapFromDto(businessDto);
        }
        
        public async Task<IEnumerable<Business>> GetAllAsync(int skip = 0, int limit = 50)
        {
            const string sql = @"
                SELECT 
                    -- Business Information
                    b.id AS Id,
                    b.name AS Name,
                    b.description AS Description,
                    b.tax_id AS TaxId,
                    b.logo AS Logo,
                    b.website AS Website,
                    b.founded_date AS FoundedDate,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt,
                    b.is_active AS IsActive,
                    
                    -- Contact Information
                    bc.email AS Email,
                    bc.phone AS Phone,
                    bc.website AS ContactWebsite,
                    
                    -- Headquarters Address
                    ba.line1 AS Line1,
                    ba.line2 AS Line2,
                    ba.city AS City,
                    ba.state AS State,
                    ba.postal_code AS PostalCode,
                    ba.country AS Country
                FROM 
                    businesses b                                  LEFT JOIN 
                    business_contacts bc ON b.id = bc.business_id LEFT JOIN 
                    business_addresses ba ON b.id = ba.business_id 
                ORDER BY b.name
                LIMIT @Limit 
                OFFSET @Skip";
            
            var parameters = new { Limit = limit, Skip = skip };
            var dbConnection = await _dbConnection.GetConnectionAsync();
            var businessDtos = await dbConnection.QueryAsync<BusinessDto>(sql, parameters);
            
            return businessDtos.Select(MapFromDto).ToList();
        }
        
        public async Task<int> GetTotalCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM businesses";
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            return await dbConnection.ExecuteScalarAsync<int>(sql);
        }
        
        public async Task<Business> AddAsync(Business business, IDbTransaction transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            bool ownsTransaction = transaction == null;
            transaction = transaction ?? dbConnection.BeginTransaction();
            
            try
            {
                await dbConnection.ExecuteAsync("""     
                    INSERT INTO 
                        businesses 
                            (id, name, description, tax_id, logo, website, founded_date, created_at, updated_at, is_active) 
                        VALUES 
                            (@Id, @Name, @Description, @TaxId, @Logo, @Website, @FoundedDate, @CreatedAt, @UpdatedAt, @IsActive)
                    """, 
                    new 
                    {
                        Id = business.Id.Value,
                        business.Name,
                        business.Description,
                        business.TaxId,
                        business.Logo,
                        business.Website,
                        business.FoundedDate,
                        business.CreatedAt,
                        business.UpdatedAt,
                        business.IsActive
                    }, 
                    transaction
                );
                
                await dbConnection.ExecuteAsync("""
                    INSERT INTO 
                        business_contacts 
                            (business_id, email, phone, website, created_at, updated_at) 
                        VALUES 
                            (@BusinessId, @Email, @Phone, @Website, @CreatedAt, @UpdatedAt)
                    """, 
                    new
                    {
                        BusinessId = business.Id.Value,
                        business.Contact.Email,
                        business.Contact.Phone,
                        business.Contact.Website,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }, 
                    transaction
                );
                
                await dbConnection.ExecuteAsync("""
                    INSERT INTO 
                        business_addresses 
                            (business_id, line1, line2, city, state, postal_code, country, created_at, updated_at) 
                        VALUES 
                            (@BusinessId, @Line1, @Line2, @City, @State, @PostalCode, @Country, @CreatedAt, @UpdatedAt)
                    """, 
                    new
                    {
                        BusinessId = business.Id.Value,
                        business.HeadquartersAddress.Line1,
                        business.HeadquartersAddress.Line2,
                        business.HeadquartersAddress.City,
                        business.HeadquartersAddress.State,
                        business.HeadquartersAddress.PostalCode,
                        business.HeadquartersAddress.Country,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }, 
                    transaction
                );
                
                if (ownsTransaction)
                    transaction.Commit();
                
                return business;
            }
            catch
            {
                if (ownsTransaction)
                    transaction.Rollback();
                
                throw;
            }
            finally
            {
                if (ownsTransaction && transaction != null)
                    transaction.Dispose();
            }
        }
        
        public async Task UpdateAsync(Business business, IDbTransaction transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            // Track whether we created the transaction or are using an existing one
            bool ownsTransaction = transaction == null;
            
            // If no transaction was provided, create one
            transaction = transaction ?? dbConnection.BeginTransaction();
            
            try
            {
                // Update businesses table
                const string businessSql = @"
                    UPDATE businesses SET
                        name = @Name,
                        description = @Description,
                        tax_id = @TaxId,
                        logo = @Logo,
                        website = @Website,
                        founded_date = @FoundedDate,
                        updated_at = @UpdatedAt,
                        is_active = @IsActive
                    WHERE id = @Id";

                var businessParams = new
                {
                    Id = business.Id.Value,
                    Name = business.Name,
                    Description = business.Description,
                    TaxId = business.TaxId,
                    Logo = business.Logo,
                    Website = business.Website,
                    FoundedDate = business.FoundedDate,
                    UpdatedAt = business.UpdatedAt,
                    IsActive = business.IsActive
                };
                
                await dbConnection.ExecuteAsync(businessSql, businessParams, transaction);

                // Update business_contacts table
                const string contactSql = @"
                    UPDATE business_contacts SET
                        email = @Email,
                        phone = @Phone,
                        website = @Website,
                        updated_at = @UpdatedAt
                    WHERE business_id = @BusinessId";

                var contactParams = new
                {
                    BusinessId = business.Id.Value,
                    Email = business.Contact.Email,
                    Phone = business.Contact.Phone,
                    Website = business.Contact.Website,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await dbConnection.ExecuteAsync(contactSql, contactParams, transaction);

                // Update business_addresses table
                const string addressSql = @"
                    UPDATE business_addresses SET
                        line1 = @Line1,
                        line2 = @Line2,
                        city = @City,
                        state = @State,
                        postal_code = @PostalCode,
                        country = @Country,
                        updated_at = @UpdatedAt
                    WHERE business_id = @BusinessId";

                var addressParams = new
                {
                    BusinessId = business.Id.Value,
                    Line1 = business.HeadquartersAddress.Line1,
                    Line2 = business.HeadquartersAddress.Line2,
                    City = business.HeadquartersAddress.City,
                    State = business.HeadquartersAddress.State,
                    PostalCode = business.HeadquartersAddress.PostalCode,
                    Country = business.HeadquartersAddress.Country,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await dbConnection.ExecuteAsync(addressSql, addressParams, transaction);

                // Only commit if we created the transaction
                if (ownsTransaction)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                // Only rollback if we created the transaction
                if (ownsTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                // Only dispose if we created the transaction
                if (ownsTransaction && transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
        
        public async Task DeleteAsync(BusinessId id, IDbTransaction transaction = null)
        {
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            // Track whether we created the transaction or are using an existing one
            bool ownsTransaction = transaction == null;
            
            // If no transaction was provided, create one
            transaction = transaction ?? dbConnection.BeginTransaction();
            
            try
            {
                // Delete from business_addresses table
                const string addressSql = @"
                    DELETE FROM business_addresses 
                    WHERE business_id = @Id";
                
                await dbConnection.ExecuteAsync(addressSql, new { Id = id.Value }, transaction);
                
                // Delete from business_contacts table
                const string contactSql = @"
                    DELETE FROM business_contacts 
                    WHERE business_id = @Id";
                
                await dbConnection.ExecuteAsync(contactSql, new { Id = id.Value }, transaction);
                
                // Delete from businesses table
                const string businessSql = @"
                    DELETE FROM businesses 
                    WHERE id = @Id";
                
                await dbConnection.ExecuteAsync(businessSql, new { Id = id.Value }, transaction);
                
                // Only commit if we created the transaction
                if (ownsTransaction)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                // Only rollback if we created the transaction
                if (ownsTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                // Only dispose if we created the transaction
                if (ownsTransaction && transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
        
        public async Task<bool> ExistsByNameAsync(string name)
        {
            const string sql = "SELECT COUNT(*) FROM businesses WHERE LOWER(name) = LOWER(@Name)";
            
            var parameters = new { Name = name };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            var count = await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            
            return count > 0;
        }
        
        public async Task<bool> ExistsByTaxIdAsync(string taxId)
        {
            const string sql = "SELECT COUNT(*) FROM businesses WHERE tax_id = @TaxId";
            
            var parameters = new { TaxId = taxId };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            var count = await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            
            return count > 0;
        }
        
        public async Task<IEnumerable<Business>> SearchByNameAsync(string nameQuery, int page = 1, int pageSize = 20)
        {
            const string sql = @"
                SELECT 
                    b.id AS Id,
                    b.name AS Name,
                    b.description AS Description,
                    b.tax_id AS TaxId,
                    b.logo AS Logo,
                    b.website AS Website,
                    b.founded_date AS FoundedDate,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt,
                    b.is_active AS IsActive,
                    
                    -- Contact Information
                    bc.email AS Email,
                    bc.phone AS Phone,
                    bc.website AS ContactWebsite,
                    
                    -- Headquarters Address
                    ba.line1 AS Line1,
                    ba.line2 AS Line2,
                    ba.city AS City,
                    ba.state AS State,
                    ba.postal_code AS PostalCode,
                    ba.country AS Country
                FROM 
                    businesses b
                LEFT JOIN business_contacts bc ON b.id = bc.business_id
                LEFT JOIN business_addresses ba ON b.id = ba.business_id 
                WHERE LOWER(b.name) LIKE LOWER(@NameQuery)
                ORDER BY b.name
                LIMIT @PageSize OFFSET @Offset";

            var offset = (page - 1) * pageSize;
            var parameters = new { NameQuery = $"%{nameQuery}%", PageSize = pageSize, Offset = offset };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            var businessDtos = await dbConnection.QueryAsync<BusinessDto>(sql, parameters);
            
            return businessDtos.Select(MapFromDto).ToList();
        }
        
        public async Task<IEnumerable<Brand>> GetBrandsForBusinessAsync(BusinessId businessId)
        {
            const string sql = @"
                SELECT 
                    b.id AS Id,
                    b.name AS Name,
                    b.category AS Category,
                    b.logo AS Logo,
                    b.description AS Description,
                    b.business_id AS BusinessId,
                    b.created_at AS CreatedAt,
                    b.updated_at AS UpdatedAt,
                    
                    -- Contact Information
                    bc.email AS Email,
                    bc.phone AS Phone,
                    bc.website AS ContactWebsite,
                    
                    -- Address
                    ba.line1 AS Line1,
                    ba.line2 AS Line2,
                    ba.city AS City,
                    ba.state AS State,
                    ba.postal_code AS PostalCode,
                    ba.country AS Country
                FROM 
                    brands b
                LEFT JOIN brand_contacts bc ON b.id = bc.brand_id
                LEFT JOIN brand_addresses ba ON b.id = ba.brand_id 
                WHERE 
                    b.business_id = @BusinessId
                ORDER BY 
                    b.name";

            var parameters = new { BusinessId = businessId.Value };
            
            var dbConnection = await _dbConnection.GetConnectionAsync();
            
            var brandDtos = await dbConnection.QueryAsync<BrandDto>(sql, parameters);
            
            return brandDtos.Select(MapBrandFromDto).ToList();
        }

        #region Private Helper Methods

        private class BusinessDto
        {
            // Business Properties
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string TaxId { get; set; }
            public string Logo { get; set; }
            public string Website { get; set; }
            public DateTime? FoundedDate { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public bool IsActive { get; set; }
            
            // Contact Info
            public string Email { get; set; }
            public string Phone { get; set; }
            public string ContactWebsite { get; set; } // Renamed to avoid conflict with Business.Website
            
            // Address
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        private class BrandDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Logo { get; set; }
            public string Description { get; set; }
            public string BusinessId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            
            // Contact Info
            public string Email { get; set; }
            public string Phone { get; set; }
            public string ContactWebsite { get; set; } // Renamed to avoid conflict with Brand.Website
            
            // Address
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        private Business MapFromDto(BusinessDto dto)
        {
            var contact = new ContactInfo(dto.Email, dto.Phone, dto.ContactWebsite);
            var address = new Address(dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Country);
            
            // Use reflection to create a Business entity with the provided ID
            var business = new Business(
                dto.Name,
                dto.Description,
                dto.TaxId,
                contact,
                address,
                dto.Logo,
                dto.Website,
                dto.FoundedDate
            );
            
            // Set the ID using reflection
            typeof(Business).GetProperty("Id").SetValue(business, new BusinessId(dto.Id));
            
            // Set the timestamps using reflection
            typeof(Business).GetProperty("CreatedAt").SetValue(business, dto.CreatedAt);
            typeof(Business).GetProperty("UpdatedAt").SetValue(business, dto.UpdatedAt);
            typeof(Business).GetProperty("IsActive").SetValue(business, dto.IsActive);
            
            return business;
        }

        private Brand MapBrandFromDto(BrandDto dto)
        {
            var contact = new ContactInfo(dto.Email, dto.Phone, dto.ContactWebsite);
            var address = new Address(dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Country);
            
            // Use reflection to create a Brand entity with the provided ID
            var brand = new Brand(
                dto.Name,
                dto.Category,
                dto.Logo,
                dto.Description,
                contact,
                address,
                EntityId.Parse<BusinessId>(dto.BusinessId));
            
            // Set the ID using reflection
            typeof(Brand).GetProperty("Id").SetValue(brand, new BrandId(dto.Id));
            
            // Set the timestamps using reflection
            typeof(Brand).GetProperty("CreatedAt").SetValue(brand, dto.CreatedAt);
            typeof(Brand).GetProperty("UpdatedAt").SetValue(brand, dto.UpdatedAt);
            
            return brand;
        }

        #endregion
    }
}