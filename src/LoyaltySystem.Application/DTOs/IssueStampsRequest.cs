using System;

namespace LoyaltySystem.Application.DTOs
{
    /// <summary>
    /// Request model for issuing stamps to a loyalty card.
    /// </summary>
    public class IssueStampsRequest
    {
        /// <summary>
        /// The identifier of the loyalty card.
        /// Can be null if using QrCode instead.
        /// </summary>
        public Guid? CardId { get; set; }
        
        /// <summary>
        /// The QR code of the loyalty card.
        /// Can be null if using CardId instead.
        /// </summary>
        public string QrCode { get; set; }
        
        /// <summary>
        /// The number of stamps to issue.
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// The store where the stamps are being issued.
        /// </summary>
        public Guid StoreId { get; set; }
        
        /// <summary>
        /// The staff member issuing the stamps, if applicable.
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// The POS transaction ID, if connected to a POS system.
        /// </summary>
        public string PosTransactionId { get; set; }
    }
} 