namespace LoyaltySystem.Services;

using LoyaltySystem.Data;
using LoyaltySystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public interface IStoresService
{
    Task<int> CreateStoreAsync(int businessId, CreateStoreDto dto);
    Task<Store> GetStoreAsync(int businessId, int storeId);
    Task<List<Store>> GetStoresAsync(int businessId);
    Task UpdateStoreAsync(int businessId, int storeId, UpdateStoreDto dto);
    Task DeleteStoreAsync(int businessId, int storeId);
}

public class StoresService : IStoresService
{
    private readonly IStoresRepository _repo;

    public StoresService(IStoresRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> CreateStoreAsync(int businessId, CreateStoreDto dto)
    {
        var store = new Store
        {
            BusinessId = businessId,
            StoreName = dto.StoreName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Postcode = dto.Postcode,
            QRCodeData = dto.QRCodeData
        };
        return await _repo.CreateStoreAsync(store);
    }

    public async Task<Store> GetStoreAsync(int businessId, int storeId)
    {
        return await _repo.GetStoreByIdAsync(businessId, storeId);
    }

    public async Task<List<Store>> GetStoresAsync(int businessId)
    {
        return await _repo.GetStoresByBusinessAsync(businessId);
    }

    public async Task UpdateStoreAsync(int businessId, int storeId, UpdateStoreDto dto)
    {
        var existing = await _repo.GetStoreByIdAsync(businessId, storeId);
        if (existing == null) throw new Exception("Store not found");

        existing.StoreName = dto.StoreName ?? existing.StoreName;
        existing.PhoneNumber = dto.PhoneNumber ?? existing.PhoneNumber;
        existing.Address = dto.Address ?? existing.Address;
        existing.Postcode = dto.Postcode ?? existing.Postcode;
        existing.QRCodeData = dto.QRCodeData ?? existing.QRCodeData;

        await _repo.UpdateStoreAsync(existing);
    }

    public async Task DeleteStoreAsync(int businessId, int storeId)
    {
        await _repo.DeleteStoreAsync(businessId, storeId);
    }
}

// DTOs:
public class CreateStoreDto
{
    public string StoreName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Postcode { get; set; }
    public string QRCodeData { get; set; }
}

public class UpdateStoreDto
{
    public string StoreName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string Postcode { get; set; }
    public string QRCodeData { get; set; }
}
