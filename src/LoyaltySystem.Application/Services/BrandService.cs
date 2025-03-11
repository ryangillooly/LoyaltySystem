using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.Common;

namespace LoyaltySystem.Application.Services
{
    public class BrandService
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

                if (brand == null)
                {
                    return OperationResult<BrandDto>.FailureResult($"Brand with ID {id} not found");
                }

                return OperationResult<BrandDto>.SuccessResult(MapToDto(brand));
            }
            catch (Exception ex)
            {
                return OperationResult<BrandDto>.FailureResult($"Failed to get brand: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<BrandDto>>> GetAllBrandsAsync(int page, int pageSize)
        {
            try
            {
                var brands = await _brandRepository.GetAllAsync(page, pageSize);
                var brandDtos = new List<BrandDto>();

                foreach (var brand in brands)
                {
                    brandDtos.Add(MapToDto(brand));
                }

                var totalCount = brandDtos.Count; // This is not accurate, but we don't have a count method in the repository
                var result = new PagedResult<BrandDto>(brandDtos, totalCount, page, pageSize);

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
                var brandId = EntityId.New<BrandId>();
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
                    address
                );

                await _unitOfWork.BeginTransactionAsync();
                await _brandRepository.AddAsync(brand);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<BrandDto>.SuccessResult(MapToDto(brand));
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

        private BrandDto MapToDto(Brand brand)
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
    }

    public class BrandDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public AddressDto Address { get; set; } = new AddressDto();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBrandDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public AddressDto Address { get; set; } = new AddressDto();
    }

    public class UpdateBrandDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ContactInfoDto ContactInfo { get; set; } = new ContactInfoDto();
        public AddressDto Address { get; set; } = new AddressDto();
    }
} 