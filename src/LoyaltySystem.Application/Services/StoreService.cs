using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Application.Interfaces;

namespace LoyaltySystem.Application.Services
{
    public class StoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StoreService(
            IStoreRepository storeRepository,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<OperationResult<StoreDto>> GetStoreByIdAsync(string id)
        {
            try
            {
                var storeId = EntityId.Parse<StoreId>(id);
                var store = await _storeRepository.GetByIdAsync(storeId);

                if (store == null)
                {
                    return OperationResult<StoreDto>.FailureResult($"Store with ID {id} not found");
                }

                return OperationResult<StoreDto>.SuccessResult(MapToDto(store));
            }
            catch (Exception ex)
            {
                return OperationResult<StoreDto>.FailureResult($"Failed to get store: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<StoreDto>>> GetAllStoresAsync(int page, int pageSize)
        {
            try
            {
                var stores = await _storeRepository.GetAllAsync(page, pageSize);
                var storeDtos = new List<StoreDto>();

                foreach (var store in stores)
                {
                    storeDtos.Add(MapToDto(store));
                }

                var totalCount = storeDtos.Count; // This is not accurate, but we don't have a count method in the repository
                var result = new PagedResult<StoreDto>(storeDtos, totalCount, page, pageSize);

                return OperationResult<PagedResult<StoreDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<StoreDto>>.FailureResult($"Failed to get stores: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<StoreDto>>> GetStoresByBrandIdAsync(string brandId)
        {
            try
            {
                var bId = EntityId.Parse<BrandId>(brandId);
                var stores = await _storeRepository.GetByBrandIdAsync(bId);
                var storeDtos = new List<StoreDto>();

                foreach (var store in stores)
                {
                    storeDtos.Add(MapToDto(store));
                }

                return OperationResult<List<StoreDto>>.SuccessResult(storeDtos);
            }
            catch (Exception ex)
            {
                return OperationResult<List<StoreDto>>.FailureResult($"Failed to get stores for brand: {ex.Message}");
            }
        }

        public async Task<OperationResult<StoreDto>> CreateStoreAsync(CreateStoreDto dto)
        {
            try
            {
                var brandId = EntityId.Parse<BrandId>(dto.BrandId);
                var brand = await _brandRepository.GetByIdAsync(brandId);

                if (brand == null)
                {
                    return OperationResult<StoreDto>.FailureResult($"Brand with ID {dto.BrandId} not found");
                }

                var address = new Address(
                    dto.Address.Line1,
                    dto.Address.Line2,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.PostalCode,
                    dto.Address.Country
                );

                var contactInfo = new ContactInfo(
                    dto.ContactInfo.Email,
                    dto.ContactInfo.Phone,
                    dto.ContactInfo.Website
                );

                var location = new GeoLocation(
                    dto.Location.Latitude,
                    dto.Location.Longitude
                );

                var store = new Store(
                    brandId.Value,
                    dto.Name,
                    address,
                    location,
                    new OperatingHours(new Dictionary<DayOfWeek, TimeRange>()),
                    dto.ContactInfo.Email
                );

                await _unitOfWork.BeginTransactionAsync();
                var newStore = await _storeRepository.AddAsync(store);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<StoreDto>.SuccessResult(MapToDto(newStore));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<StoreDto>.FailureResult($"Failed to create store: {ex.Message}");
            }
        }

        public async Task<OperationResult<StoreDto>> UpdateStoreAsync(string id, UpdateStoreDto dto)
        {
            try
            {
                var storeId = EntityId.Parse<StoreId>(id);
                var store = await _storeRepository.GetByIdAsync(storeId);

                if (store == null)
                {
                    return OperationResult<StoreDto>.FailureResult($"Store with ID {id} not found");
                }

                var brandId = EntityId.Parse<BrandId>(dto.BrandId);
                var brand = await _brandRepository.GetByIdAsync(brandId);

                if (brand == null)
                {
                    return OperationResult<StoreDto>.FailureResult($"Brand with ID {dto.BrandId} not found");
                }

                var address = new Address(
                    dto.Address.Line1,
                    dto.Address.Line2,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.PostalCode,
                    dto.Address.Country
                );

                var contactInfo = new ContactInfo(
                    dto.ContactInfo.Email,
                    dto.ContactInfo.Phone,
                    dto.ContactInfo.Website
                );

                var location = new GeoLocation(
                    dto.Location.Latitude,
                    dto.Location.Longitude
                );

                store.Update(
                    dto.Name,
                    address,
                    location,
                    new OperatingHours(new Dictionary<DayOfWeek, TimeRange>()),
                    dto.ContactInfo.Email
                );

                await _unitOfWork.BeginTransactionAsync();
                await _storeRepository.UpdateAsync(store);
                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<StoreDto>.SuccessResult(MapToDto(store));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return OperationResult<StoreDto>.FailureResult($"Failed to update store: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<TransactionDto>>> GetStoreTransactionsAsync(
            string storeId, DateTime start, DateTime end, int page, int pageSize)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                var transactions = await _storeRepository.GetTransactionsAsync(sId, start, end, page, pageSize);
                var transactionDtos = new List<TransactionDto>();

                foreach (var transaction in transactions)
                {
                    transactionDtos.Add(new TransactionDto
                    {
                        Id = transaction.Id.ToString(),
                        CardId = transaction.CardId.ToString(),
                        StoreId = transaction.StoreId.ToString(),
                        StoreName = transaction.Store?.Name ?? string.Empty,
                        TransactionDate = transaction.Timestamp,
                        TransactionType = transaction.Type.ToString(),
                        Amount = transaction.TransactionAmount ?? 0,
                        PointsEarned = transaction.PointsAmount ?? 0,
                        StampsEarned = transaction.Quantity ?? 0,
                        RewardId = transaction.RewardId?.ToString(),
                        RewardTitle = transaction.Reward?.Title ?? string.Empty,
                        PosTransactionId = transaction.PosTransactionId ?? string.Empty
                    });
                }

                var count = await _storeRepository.GetTransactionCountAsync(sId, start, end);
                var result = new PagedResult<TransactionDto>(transactionDtos, count, page, pageSize);

                return OperationResult<PagedResult<TransactionDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult<PagedResult<TransactionDto>>.FailureResult($"Failed to get store transactions: {ex.Message}");
            }
        }

        public async Task<OperationResult<StoreStatsDto>> GetStoreStatsAsync(string storeId, DateTime start, DateTime end)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                var transactionCount = await _storeRepository.GetTransactionCountAsync(sId, start, end);
                var stampsIssued = await _storeRepository.GetTotalStampsIssuedAsync(sId, start, end);
                var pointsIssued = await _storeRepository.GetTotalPointsIssuedAsync(sId, start, end);
                var redemptionCount = await _storeRepository.GetRedemptionCountAsync(sId, start, end);

                var stats = new StoreStatsDto
                {
                    TransactionCount = transactionCount,
                    StampsIssued = stampsIssued,
                    PointsIssued = pointsIssued,
                    RedemptionCount = redemptionCount,
                    StartDate = start,
                    EndDate = end
                };

                return OperationResult<StoreStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                return OperationResult<StoreStatsDto>.FailureResult($"Failed to get store statistics: {ex.Message}");
            }
        }

        public async Task<OperationResult<OperatingHoursDto>> GetStoreOperatingHoursAsync(string storeId)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                var store = await _storeRepository.GetByIdAsync(sId);

                if (store == null)
                {
                    return OperationResult<OperatingHoursDto>.FailureResult($"Store with ID {storeId} not found");
                }

                var hoursDto = new OperatingHoursDto
                {
                    Monday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Monday)),
                    Tuesday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Tuesday)),
                    Wednesday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Wednesday)),
                    Thursday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Thursday)),
                    Friday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Friday)),
                    Saturday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Saturday)),
                    Sunday = MapDayHoursToDto(store.Hours.GetHoursForDay(DayOfWeek.Sunday))
                };

                return OperationResult<OperatingHoursDto>.SuccessResult(hoursDto);
            }
            catch (Exception ex)
            {
                return OperationResult<OperatingHoursDto>.FailureResult($"Failed to get store operating hours: {ex.Message}");
            }
        }

        public async Task<int> GetTransactionCountAsync(string storeId, DateTime start, DateTime end)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                return await _storeRepository.GetTransactionCountAsync(sId, start, end);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> GetTotalStampsIssuedAsync(string storeId, DateTime start, DateTime end)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                return await _storeRepository.GetTotalStampsIssuedAsync(sId, start, end);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<decimal> GetTotalPointsIssuedAsync(string storeId, DateTime start, DateTime end)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                return await _storeRepository.GetTotalPointsIssuedAsync(sId, start, end);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> GetRedemptionCountAsync(string storeId, DateTime start, DateTime end)
        {
            try
            {
                var sId = EntityId.Parse<StoreId>(storeId);
                return await _storeRepository.GetRedemptionCountAsync(sId, start, end);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private StoreDto MapToDto(Store store)
        {
            return new StoreDto
            {
                Id = store.Id.ToString(),
                Name = store.Name,
                Address = new AddressDto
                {
                    Line1 = store.Address.Line1,
                    Line2 = store.Address.Line2,
                    City = store.Address.City,
                    State = store.Address.State,
                    PostalCode = store.Address.PostalCode,
                    Country = store.Address.Country
                },
                ContactInfo = new ContactInfoDto
                {
                    Email = store.ContactInfo ?? string.Empty,
                    Phone = string.Empty,
                    Website = string.Empty
                },
                Location = new GeoLocationDto
                {
                    Latitude = store.Location.Latitude,
                    Longitude = store.Location.Longitude
                },
                BrandId = store.Brand.Id.ToString(),
                BrandName = store.Brand.Name
            };
        }

        private DayHoursDto MapDayHoursToDto(TimeRange timeRange)
        {
            if (timeRange == null)
            {
                return new DayHoursDto
                {
                    IsOpen = false,
                    OpenTime = null,
                    CloseTime = null
                };
            }
            
            return new DayHoursDto
            {
                IsOpen = true,
                OpenTime = timeRange.OpenTime,
                CloseTime = timeRange.CloseTime
            };
        }
    }
} 