using System;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Request model for adding points to a loyalty card.
    /// </summary>
    public class AddPointsRequest
    {
        /// <summary>
        /// The identifier of the loyalty card.
        /// Can be null if using QrCode or CardHash instead.
        /// </summary>
        public Guid? CardId { get; set; }
        
        /// <summary>
        /// The QR code of the loyalty card.
        /// Can be null if using CardId or CardHash instead.
        /// </summary>
        public string QrCode { get; set; }
        
        /// <summary>
        /// Card hash for POS integration.
        /// Can be null if using CardId or QrCode instead.
        /// </summary>
        public string CardHash { get; set; }
        
        /// <summary>
        /// The number of points to add.
        /// If not specified, will be calculated based on TransactionAmount
        /// and the program's conversion rate.
        /// </summary>
        public decimal? PointsAmount { get; set; }
        
        /// <summary>
        /// The monetary value of the transaction.
        /// </summary>
        public decimal TransactionAmount { get; set; }
        
        /// <summary>
        /// The store where the points are being added.
        /// </summary>
        public Guid StoreId { get; set; }
        
        /// <summary>
        /// The staff member adding the points, if applicable.
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// The POS transaction ID.
        /// </summary>
        public string PosTransactionId { get; set; }
    }
} 