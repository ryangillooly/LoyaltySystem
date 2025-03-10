using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace LoyaltySystem.API.Infrastructure
{
    /// <summary>
    /// Model binder provider for EntityId types
    /// </summary>
    public class EntityIdModelBinderProvider : IModelBinderProvider
    {
        private static readonly Dictionary<Type, Type> SupportedTypes = new()
        {
            { typeof(BrandId), typeof(EntityIdModelBinder<BrandId>) },
            { typeof(StoreId), typeof(EntityIdModelBinder<StoreId>) },
            { typeof(CustomerId), typeof(EntityIdModelBinder<CustomerId>) },
            { typeof(LoyaltyProgramId), typeof(EntityIdModelBinder<LoyaltyProgramId>) },
            { typeof(LoyaltyCardId), typeof(EntityIdModelBinder<LoyaltyCardId>) },
            { typeof(RewardId), typeof(EntityIdModelBinder<RewardId>) },
            { typeof(TransactionId), typeof(EntityIdModelBinder<TransactionId>) }
        };

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (SupportedTypes.TryGetValue(context.Metadata.ModelType, out var binderType))
            {
                return (IModelBinder)Activator.CreateInstance(binderType);
            }

            return null;
        }
    }

    /// <summary>
    /// Model binder for EntityId types - converts string prefixed IDs to EntityId
    /// </summary>
    public class EntityIdModelBinder<T> : IModelBinder where T : EntityId, new()
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            // Get the value from the value provider
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            try
            {
                var result = EntityId.Parse<T>(value);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
            }

            return Task.CompletedTask;
        }
    }
} 