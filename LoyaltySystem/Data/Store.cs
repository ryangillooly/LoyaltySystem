namespace LoyaltySystem.Data;

using LinqToDB.Mapping;
using System;

[Table("stores")]
public class Store
{
    [PrimaryKey, Identity]
    [Column("storeid")]
    public int StoreId { get; set; }

    [Column("businessid"), NotNull]
    public int BusinessId { get; set; }

    [Column("storename"), NotNull]
    public string StoreName { get; set; }

    [Column("phonenumber")]
    public string PhoneNumber { get; set; }

    [Column("address"), NotNull]
    public string Address { get; set; }

    [Column("postcode"), NotNull]
    public string Postcode { get; set; }

    [Column("qrcodedata")]
    public string QRCodeData { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
}