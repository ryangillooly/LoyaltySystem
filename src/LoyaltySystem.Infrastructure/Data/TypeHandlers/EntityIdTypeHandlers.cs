using System;
using System.Data;
using Dapper;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Domain.Enums;

namespace LoyaltySystem.Infrastructure.Data.TypeHandlers
{
    /// <summary>
    /// Type handler for EntityId types that can map between database UUIDs and .NET EntityId objects
    /// </summary>
    public class EntityIdTypeHandler<T> : SqlMapper.TypeHandler<T> where T : EntityId, new()
    {
        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value?.Value;
        }

        public override T Parse(object value)
        {
            if (value is null or DBNull) return null;
            
            if (value is Guid guidValue)
            {
                // Create a new instance using the constructor with Guid parameter
                return (T)Activator.CreateInstance(typeof(T), guidValue);
            }
            
            // Try to parse string representation
            if (value is string strValue && Guid.TryParse(strValue, out var parsedGuid))
            {
                return (T)Activator.CreateInstance(typeof(T), parsedGuid);
            }
            
            // Handle byte arrays (PostgreSQL UUID representation)
            if (value is byte[] byteArray && byteArray.Length == 16)
            {
                var guid = new Guid(byteArray);
                return (T)Activator.CreateInstance(typeof(T), guid);
            }
            
            throw new InvalidCastException($"Cannot convert {value.GetType().Name} to {typeof(T).Name}");
        }
    }
    
    /// <summary>
    /// Special handler for treating CustomerId as a string in User entity
    /// </summary>
    public class CustomerIdStringHandler : SqlMapper.TypeHandler<string>
    {
        public override void SetValue(IDbDataParameter parameter, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                parameter.Value = DBNull.Value;
            }
            else if (CustomerId.TryParse<CustomerId>(value, out var customerId))
            {
                parameter.Value = customerId.Value;
            }
            else
            {
                parameter.Value = value;
            }
        }

        public override string Parse(object value)
        {
            if (value == null || value is DBNull)
                return null;
                
            // If it's a GUID, create a CustomerId and return its string representation
            if (value is Guid guid)
            {
                var customerId = new CustomerId(guid);
                return customerId.ToString();
            }
            
            // Just return the string value
            return value.ToString();
        }
    }
    
    /// <summary>
    /// Type handler for UserStatus enum
    /// </summary>
    public class UserStatusTypeHandler : SqlMapper.TypeHandler<UserStatus>
    {
        public override void SetValue(IDbDataParameter parameter, UserStatus value)
        {
            parameter.Value = (int)value;
        }

        public override UserStatus Parse(object value)
        {
            return value == null ? UserStatus.Active : (UserStatus)Convert.ToInt32(value);
        }
    }
    
    /// <summary>
    /// Static class to register all type handlers in one place
    /// </summary>
    public static class TypeHandlerConfig
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            lock (_lock)
            {
                if (_initialized) return;
                
                // Register all EntityId handlers
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<UserId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<CustomerId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<LoyaltyCardId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<LoyaltyProgramId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<StoreId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<RewardId>());
                SqlMapper.AddTypeHandler(new EntityIdTypeHandler<TransactionId>());
                
                // Register other special handlers
                SqlMapper.AddTypeHandler(new CustomerIdStringHandler());
                SqlMapper.AddTypeHandler(new UserStatusTypeHandler());
                
                _initialized = true;
            }
        }
    }
} 