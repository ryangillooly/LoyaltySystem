using LoyaltySystem.Core.Enums;
using LoyaltySystem.Core.Models;

namespace LoyaltySystem.Core.Exceptions;

public class BusinessExceptions
{
    public abstract class BusinessExceptionBase : Exception
    {
        public Guid UserId { get; set; }
        public Guid BusinessId { get; }
        public Guid CampaignId { get; }
        public BusinessStatus BusinessStatus { get; set; }

        protected BusinessExceptionBase(Guid businessId, string message)
            : base(message) => (BusinessId) = (businessId);
        
        protected BusinessExceptionBase(Guid businessId, BusinessStatus businessStatus, string message)
            : base(message) => (BusinessId, businessStatus) = (businessId, businessStatus);
        
        protected BusinessExceptionBase(Guid userId, Guid businessId, string message)
            : base(message) => (UserId, BusinessId) = (businessId, userId);
        
        protected BusinessExceptionBase(Guid userId, Guid campaignId, Guid businessId, string message)
            : base(message) => (CampaignId, BusinessId) = (campaignId, businessId);
    }
    
    public class BusinessNotFoundException : BusinessExceptionBase
    {
        public BusinessNotFoundException(Guid businessId)
            : base(businessId, $"The Business {businessId} was not found.") { }
    }
    
    public class BusinessNotActiveException : BusinessExceptionBase
    {
        public BusinessNotActiveException(Guid businessId, BusinessStatus businessStatus)
            : base(businessId, businessStatus, $"Business {businessId} is currently not in Active status. The status is {businessStatus}") { }
    }
    
    public class BusinessUsersNotFoundException : BusinessExceptionBase
    {
        public BusinessUsersNotFoundException(Guid businessId)
            : base(businessId, $"No users were found for the Business {businessId}") { }
    }

    public class BusinessUserPermissionNotFoundException : BusinessExceptionBase
    {
        public BusinessUserPermissionNotFoundException(Guid userId, Guid businessId)
            : base(userId, businessId, $"The permission for user {userId} to access business {businessId} was not found.") { }
    }
    
    public class CampaignNotFoundException : BusinessExceptionBase
    {
        public CampaignNotFoundException(Guid campaignId, Guid businessId)
            : base(campaignId, businessId, $"The loyalty campaign {campaignId} for business {businessId} was not found.") { }
    }
    
    public class NoCampaignsFoundException : BusinessExceptionBase
    {
        public NoCampaignsFoundException(Guid businessId)
            : base(businessId, $"No Campaigns found for Business {businessId}") { }
    }
    
    public class CampaignNotActiveException : BusinessExceptionBase
    {
        public CampaignNotActiveException(Guid businessId, Guid campaignId)
            : base(businessId, $"The Campaign {campaignId}") { }
    }
}