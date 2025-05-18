using LoyaltySystem.Application.Common;
using LoyaltySystem.Application.DTOs;
using LoyaltySystem.Application.Interfaces;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Repositories;
using LoyaltySystem.Domain.ValueObjects;

namespace LoyaltySystem.Application.Services
{
    public class StoreService : IStoreService
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

                return store == null
                    ? OperationResult<StoreDto>.FailureResult($"Store with ID {id} not found") 
                    : OperationResult<StoreDto>.SuccessResult(MapToDto(store));

            }
            catch (Exception ex)
            {
                return OperationResult<StoreDto>.FailureResult($"Failed to get store: {ex.Message}");
            }
        }

        public async Task<OperationResult<PagedResult<StoreDto>>> GetAllStoresAsync(int skip, int limit)
        {
            try
            {
                var stores = await _storeRepository.GetAllAsync(skip, limit);
                var storeDtos = stores.Select(MapToDto).ToList();
                var result = new PagedResult<StoreDto>(storeDtos, storeDtos.Count, skip, limit);

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
                    return OperationResult<StoreDto>.FailureResult($"Brand with Id {dto.BrandId} not found");

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
                
                var store = new Store
                (
                    brandId,
                    dto.Name,
                    address,
                    location,
                    dto.OperatingHours,
                    contactInfo
                );

                // Use ExecuteInTransactionAsync to handle the transaction properly
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Pass the current transaction to the repository method
                    await _storeRepository.AddAsync(store, _unitOfWork.CurrentTransaction);
                    return OperationResult<StoreDto>.SuccessResult(MapToDto(store));
                });
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
                    contactInfo
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
                        // StoreName = transaction.Store?.Name ?? string.Empty,
                        TransactionDate = transaction.Timestamp,
                        TransactionType = transaction.Type.ToString(),
                        Amount = transaction.TransactionAmount ?? 0,
                        PointsEarned = transaction.PointsAmount ?? 0,
                        StampsEarned = transaction.Quantity ?? 0,
                        RewardId = transaction.RewardId?.ToString(),
                        // RewardTitle = transaction.Reward?.Title ?? string.Empty,
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
                    Monday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Monday)),
                    Tuesday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Tuesday)),
                    Wednesday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Wednesday)),
                    Thursday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Thursday)),
                    Friday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Friday)),
                    Saturday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Saturday)),
                    Sunday = MapDayHoursToDto(store.OperatingHours.GetHoursForDay(DayOfWeek.Sunday))
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

        private static StoreDto MapToDto(Store store) =>
            new ()
            {
                Id = store.Id.ToString(),
                Name = store.Name,
                Address = new Address
                (
                    store.Address.Line1,
                    store.Address.Line2,
                    store.Address.City,
                    store.Address.State,
                    store.Address.PostalCode,
                    store.Address.Country
                ),
                ContactInfo = new ContactInfo(store.ContactInfo.Email, store.ContactInfo.Phone, store.ContactInfo.Website),
                Location = new GeoLocation(store.Location.Latitude, store.Location.Longitude),
                OperatingHours = new OperatingHours(
                    new Dictionary<DayOfWeek, TimeRange>
                    {
                        { DayOfWeek.Monday,    store.OperatingHours.GetHoursForDay(DayOfWeek.Monday)},
                        { DayOfWeek.Tuesday,   store.OperatingHours.GetHoursForDay(DayOfWeek.Tuesday)},
                        { DayOfWeek.Wednesday, store.OperatingHours.GetHoursForDay(DayOfWeek.Wednesday)},
                        { DayOfWeek.Thursday,  store.OperatingHours.GetHoursForDay(DayOfWeek.Thursday)},
                        { DayOfWeek.Friday,    store.OperatingHours.GetHoursForDay(DayOfWeek.Friday)},
                        { DayOfWeek.Saturday,  store.OperatingHours.GetHoursForDay(DayOfWeek.Saturday)},
                        { DayOfWeek.Sunday,    store.OperatingHours.GetHoursForDay(DayOfWeek.Sunday)}
                    }
                ),
                BrandId = store.BrandId.ToString()
            };

        private static DayHours MapDayHoursToDto(TimeRange timeRange)
        {
            if (timeRange == null)
            {
                return new DayHours
                {
                    IsOpen = false,
                    OpenTime = null,
                    CloseTime = null
                };
            }
            
            return new DayHours
            {
                IsOpen = true,
                OpenTime = timeRange.OpenTime,
                CloseTime = timeRange.CloseTime
            };
        }
    }
} 