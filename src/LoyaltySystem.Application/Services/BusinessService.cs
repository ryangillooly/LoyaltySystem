using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.Services
{
    /// <summary>
    /// Service for managing businesses in the loyalty system.
    /// </summary>
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BusinessService(
            IBusinessRepository businessRepository,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _businessRepository = businessRepository ?? throw new ArgumentNullException(nameof(businessRepository));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Gets a business by its ID.
        /// </summary>
        public async Task<OperationResult<BusinessDto>> GetBusinessByIdAsync(string id)
        {
            try
            {
                var businessId = EntityId.Parse<BusinessId>(id);
                var business = await _businessRepository.GetByIdAsync(businessId);

                if (business == null)
                {
                    return OperationResult<BusinessDto>.FailureResult($"Business with ID {id} not found");
                }

                return OperationResult<BusinessDto>.SuccessResult(MapToDto(business));
            }
            catch (Exception ex)
            {
                return OperationResult<BusinessDto>.FailureResult($"Failed to get business: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all businesses with pagination.
        /// </summary>
        public async Task<OperationResult<PagedResult<BusinessDto>>> GetAllBusinessesAsync(int skip, int limit)
        {
            try
            {
                var businesses = await _businessRepository.GetAllAsync(skip, limit);
                var businessDtos = businesses.Select(MapToDto).ToList();
                var result = new PagedResult<BusinessDto>(businessDtos, businessDtos.Count, skip, limit);

                return OperationResult<PagedResult<BusinessDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<BusinessDto>>.FailureResult($"Failed to get businesses: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new business.
        /// </summary>
        public async Task<OperationResult<BusinessDto>> CreateBusinessAsync(CreateBusinessDto dto)
        {
            try
            {
                // Validate business name is unique
                if (await _businessRepository.ExistsByNameAsync(dto.Name))
                    return OperationResult<BusinessDto>.FailureResult($"A business with the name '{dto.Name}' already exists");

                // Validate tax ID is unique if provided
                if (!string.IsNullOrWhiteSpace(dto.TaxId) && await _businessRepository.ExistsByTaxIdAsync(dto.TaxId))
                    return OperationResult<BusinessDto>.FailureResult($"A business with the tax ID '{dto.TaxId}' already exists");

                var contactInfo = new ContactInfo(
                    dto.Contact.Email,
                    dto.Contact.Phone,
                    dto.Contact.Website
                );

                var address = new Address(
                    dto.HeadquartersAddress.Line1,
                    dto.HeadquartersAddress.Line2,
                    dto.HeadquartersAddress.City,
                    dto.HeadquartersAddress.State,
                    dto.HeadquartersAddress.PostalCode,
                    dto.HeadquartersAddress.Country
                );

                var business = new Business(
                    dto.Name,
                    dto.Description,
                    dto.TaxId,
                    contactInfo,
                    address,
                    dto.Logo,
                    dto.Website,
                    dto.FoundedDate
                );

                // Use ExecuteInTransactionAsync to handle the transaction properly
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Pass the current transaction to the repository method
                    await _businessRepository.AddAsync(business, _unitOfWork.CurrentTransaction);
                    return OperationResult<BusinessDto>.SuccessResult(MapToDto(business));
                });
            }
            catch (Exception ex)
            {
                return OperationResult<BusinessDto>.FailureResult($"Failed to create business: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing business.
        /// </summary>
        public async Task<OperationResult<BusinessDto>> UpdateBusinessAsync(string id, UpdateBusinessDto dto)
        {
            try
            {
                var businessId = EntityId.Parse<BusinessId>(id);
                var business = await _businessRepository.GetByIdAsync(businessId);

                if (business == null)
                {
                    return OperationResult<BusinessDto>.FailureResult($"Business with ID {id} not found");
                }

                // Check if name is being changed and if it's already taken
                if (business.Name != dto.Name && await _businessRepository.ExistsByNameAsync(dto.Name))
                {
                    return OperationResult<BusinessDto>.FailureResult($"A business with the name '{dto.Name}' already exists");
                }

                // Check if tax ID is being changed and if it's already taken
                if (!string.IsNullOrWhiteSpace(dto.TaxId) && business.TaxId != dto.TaxId && 
                    await _businessRepository.ExistsByTaxIdAsync(dto.TaxId))
                {
                    return OperationResult<BusinessDto>.FailureResult($"A business with the tax ID '{dto.TaxId}' already exists");
                }

                var contactInfo = new ContactInfo(
                    dto.Contact.Email,
                    dto.Contact.Phone,
                    dto.Contact.Website
                );

                var address = new Address(
                    dto.HeadquartersAddress.Line1,
                    dto.HeadquartersAddress.Line2,
                    dto.HeadquartersAddress.City,
                    dto.HeadquartersAddress.State,
                    dto.HeadquartersAddress.PostalCode,
                    dto.HeadquartersAddress.Country
                );

                business.Update(
                    dto.Name,
                    dto.Description,
                    dto.TaxId,
                    contactInfo,
                    address,
                    dto.Logo,
                    dto.Website,
                    dto.FoundedDate
                );

                // Update active status
                if (dto.IsActive && !business.IsActive)
                {
                    business.Activate();
                }
                else if (!dto.IsActive && business.IsActive)
                {
                    business.Deactivate();
                }

                // Use ExecuteInTransactionAsync to handle the transaction properly
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Pass the current transaction to the repository method
                    await _businessRepository.UpdateAsync(business, _unitOfWork.CurrentTransaction);
                    return OperationResult<BusinessDto>.SuccessResult(MapToDto(business));
                });
            }
            catch (Exception ex)
            {
                return OperationResult<BusinessDto>.FailureResult($"Failed to update business: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a business.
        /// </summary>
        public async Task<OperationResult<bool>> DeleteBusinessAsync(string id)
        {
            try
            {
                var businessId = EntityId.Parse<BusinessId>(id);
                var business = await _businessRepository.GetByIdAsync(businessId);

                if (business == null)
                {
                    return OperationResult<bool>.FailureResult($"Business with ID {id} not found");
                }

                // Check if business has any brands
                var brands = await _businessRepository.GetBrandsForBusinessAsync(businessId);
                if (brands.GetEnumerator().MoveNext())
                {
                    return OperationResult<bool>.FailureResult(
                        "Cannot delete business because it has associated brands. " +
                        "Please delete all brands first or deactivate the business instead.");
                }

                // Use ExecuteInTransactionAsync to handle the transaction properly
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Pass the current transaction to the repository method
                    await _businessRepository.DeleteAsync(businessId, _unitOfWork.CurrentTransaction);
                    return OperationResult<bool>.SuccessResult(true);
                });
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.FailureResult($"Failed to delete business: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a business detail with all its brands.
        /// </summary>
        public async Task<OperationResult<BusinessDto>> GetBusinessDetailAsync(string id)
        {
            try
            {
                var businessId = EntityId.Parse<BusinessId>(id);
                var business = await _businessRepository.GetByIdAsync(businessId);

                if (business == null)
                {
                    return OperationResult<BusinessDto>.FailureResult($"Business with ID {id} not found");
                }

                var dto = MapToDto(business);

                // Get all brands for this business
                var brands = await _businessRepository.GetBrandsForBusinessAsync(businessId);
                var brandDtos = new List<BrandDto>();

                foreach (var brand in brands)
                {
                    brandDtos.Add(MapBrandToDto(brand));
                }

                dto.Brands = brandDtos;

                return OperationResult<BusinessDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return OperationResult<BusinessDto>.FailureResult($"Failed to get business details: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches for businesses by name.
        /// </summary>
        public async Task<OperationResult<PagedResult<BusinessSummaryDto>>> SearchBusinessesByNameAsync(string nameQuery, int page, int pageSize)
        {
            try
            {
                var businesses = await _businessRepository.SearchByNameAsync(nameQuery, page, pageSize);
                var businessDtos = new List<BusinessSummaryDto>();

                foreach (var business in businesses)
                {
                    var brands = await _businessRepository.GetBrandsForBusinessAsync(business.Id);
                    int brandCount = 0;
                    
                    // Count the brands
                    foreach (var _ in brands)
                    {
                        brandCount++;
                    }

                    businessDtos.Add(new BusinessSummaryDto
                    {
                        Id = business.Id.ToString(),
                        Name = business.Name,
                        Logo = business.Logo,
                        BrandCount = brandCount,
                        CreatedAt = business.CreatedAt
                    });
                }

                // Approximate total count for search
                var totalCount = businessDtos.Count;
                var result = new PagedResult<BusinessSummaryDto>(businessDtos, totalCount, page, pageSize);

                return OperationResult<PagedResult<BusinessSummaryDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<BusinessSummaryDto>>.FailureResult($"Failed to search businesses: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private BusinessDto MapToDto(Business business)
        {
            return new BusinessDto
            {
                Id = business.Id.ToString(),
                Name = business.Name,
                Description = business.Description,
                TaxId = business.TaxId,
                Contact = new ContactInfoDto
                {
                    Email = business.Contact.Email,
                    Phone = business.Contact.Phone,
                    Website = business.Contact.Website
                },
                HeadquartersAddress = new AddressDto
                {
                    Line1 = business.HeadquartersAddress.Line1,
                    Line2 = business.HeadquartersAddress.Line2,
                    City = business.HeadquartersAddress.City,
                    State = business.HeadquartersAddress.State,
                    PostalCode = business.HeadquartersAddress.PostalCode,
                    Country = business.HeadquartersAddress.Country
                },
                Logo = business.Logo,
                Website = business.Website,
                FoundedDate = business.FoundedDate,
                CreatedAt = business.CreatedAt,
                UpdatedAt = business.UpdatedAt,
                IsActive = business.IsActive,
                Brands = new List<BrandDto>()
            };
        }

        private BrandDto MapBrandToDto(Brand brand)
        {
            return new BrandDto
            {
                Id = brand.Id.ToString(),
                Name = brand.Name,
                Category = brand.Category,
                Logo = brand.Logo,
                Description = brand.Description,
                ContactInfo = new ContactInfoDto
                {
                    Email = brand.Contact.Email,
                    Phone = brand.Contact.Phone,
                    Website = brand.Contact.Website
                },
                Address = new AddressDto
                {
                    Line1 = brand.Address.Line1,
                    Line2 = brand.Address.Line2,
                    City = brand.Address.City,
                    State = brand.Address.State,
                    PostalCode = brand.Address.PostalCode,
                    Country = brand.Address.Country
                },
                CreatedAt = brand.CreatedAt
            };
        }

        #endregion
    }
} 