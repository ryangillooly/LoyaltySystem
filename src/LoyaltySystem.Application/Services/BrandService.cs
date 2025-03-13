using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BrandService(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<OperationResult<BrandDto>> GetBrandByIdAsync(string id)
        {
            try
            {
                var brandId = EntityId.Parse<BrandId>(id);
                var brand = await _brandRepository.GetByIdAsync(brandId);

                return brand is null 
                    ? OperationResult<BrandDto>.FailureResult($"Brand with ID {id} not found") 
                    : OperationResult<BrandDto>.SuccessResult(MapToDto(brand));
            }
            catch (Exception ex)
            {
                return OperationResult<BrandDto>.FailureResult($"Failed to get brand: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<BrandDto>>> GetAllBrandsAsync(int skip, int limit)
        {
            try
            {
                var brands = await _brandRepository.GetAllAsync(skip, limit);
                var brandDtos = brands.Select(MapToDto).ToList();

                var totalCount = brandDtos.Count; // This is not accurate, but we don't have a count method in the repository
                var result = new PagedResult<BrandDto>(brandDtos, totalCount, skip, limit);

                return OperationResult<PagedResult<BrandDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<BrandDto>>.FailureResult($"Failed to get brands: {ex.Message}");
            }
        }

        public async Task<OperationResult<BrandDto>> CreateBrandAsync(CreateBrandDto dto)
        {
            try
            {
                // Validate brand name is unique
                if (await _brandRepository.ExistsByNameAsync(dto.Name))
                    return OperationResult<BrandDto>.FailureResult($"A brand with the name '{dto.Name}' already exists");
                
                var contactInfo = new ContactInfo(
                    dto.ContactInfo.Email,
                    dto.ContactInfo.Phone,
                    dto.ContactInfo.Website
                );

                var address = new Address(
                    dto.Address.Line1,
                    dto.Address.Line2,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.PostalCode,
                    dto.Address.Country
                );

                var brand = new Brand(
                    dto.Name,
                    dto.Category,
                    dto.Logo,
                    dto.Description,
                    contactInfo,
                    address,
                    EntityId.Parse<BusinessId>(dto.BusinessId)
                );

                // Use ExecuteInTransactionAsync to handle the transaction properly
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Pass the current transaction to the repository method
                    await _brandRepository.AddAsync(brand, _unitOfWork.CurrentTransaction);
                    return OperationResult<BrandDto>.SuccessResult(MapToDto(brand));
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<BrandDto>.FailureResult($"Failed to create brand: {ex.Message}");
            }
        }

        public async Task<OperationResult<BrandDto>> UpdateBrandAsync(string id, UpdateBrandDto dto)
        {
            try
            {
                var brandId = EntityId.Parse<BrandId>(id);
                var brand = await _brandRepository.GetByIdAsync(brandId);

                if (brand == null)
                {
                    return OperationResult<BrandDto>.FailureResult($"Brand with ID {id} not found");
                }

                var contactInfo = new ContactInfo(
                    dto.ContactInfo.Email,
                    dto.ContactInfo.Phone,
                    dto.ContactInfo.Website
                );

                var address = new Address(
                    dto.Address.Line1,
                    dto.Address.Line2,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.PostalCode,
                    dto.Address.Country
                );

                brand.Update(
                    dto.Name,
                    dto.Category,
                    dto.Logo,
                    dto.Description,
                    contactInfo,
                    address
                );

                await _unitOfWork.BeginTransactionAsync();
                await _brandRepository.UpdateAsync(brand);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<BrandDto>.SuccessResult(MapToDto(brand));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<BrandDto>.FailureResult($"Failed to update brand: {ex.Message}");
            }
        }

        private static BrandDto MapToDto(Brand brand) =>
            new ()
            {
                Id = brand.Id.ToString(),
                BusinessId = brand.BusinessId.ToString(),
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
} 