using System;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Common;
using LoyaltySystem.Shared.API.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace LoyaltySystem.Shared.API.ModelBinding
{
    /// <summary>
    /// Model binder for EntityId types to ensure they are always bound from their prefixed string representation
    /// </summary>
    public class EntityIdModelBinder<T> : IModelBinder where T : EntityId, new()
    {
        private readonly ILogger<EntityIdModelBinder<T>> _logger;

        public EntityIdModelBinder(ILogger<EntityIdModelBinder<T>> logger)
        {
            _logger = logger;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            
            // Try to bind the value
            if (bindingContext.TryGetEntityId(modelName, out T id))
            {
                _logger.LogDebug("Successfully bound {ModelType} from {Value} to {Id}", 
                    typeof(T).Name, bindingContext.ValueProvider.GetValue(modelName).FirstValue, id);
                bindingContext.Result = ModelBindingResult.Success(id);
                return Task.CompletedTask;
            }

            // Binding failed, add a model error
            var prefix = new T().Prefix;
            _logger.LogWarning("Failed to bind {ModelType} from {Value}. Expected format: {Prefix}xxx", 
                typeof(T).Name, bindingContext.ValueProvider.GetValue(modelName).FirstValue, prefix);
            bindingContext.ModelState.AddModelError(modelName, 
                $"Invalid {typeof(T).Name} format. Please provide a prefixed ID in the format {prefix}xxx.");
            
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Model binder provider for EntityId types
    /// </summary>
    public class EntityIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Check if the model type is an EntityId
            if (typeof(EntityId).IsAssignableFrom(context.Metadata.ModelType) && 
                !context.Metadata.ModelType.IsAbstract)
            {
                // Create a generic instance of the EntityIdModelBinder for the specific EntityId type
                var binderType = typeof(EntityIdModelBinder<>).MakeGenericType(context.Metadata.ModelType);
                return (IModelBinder)Activator.CreateInstance(binderType, 
                    context.Services.GetService(typeof(ILogger<>).MakeGenericType(binderType)));
            }

            return null;
        }
    }
} 