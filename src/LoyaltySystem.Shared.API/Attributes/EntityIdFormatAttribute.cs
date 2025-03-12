using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LoyaltySystem.Domain.Common;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LoyaltySystem.Shared.API.Attributes
{
    /// <summary>
    /// Attribute to document the format requirements for EntityId parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class EntityIdFormatAttribute : Attribute
    {
        public Type EntityIdType { get; }
        public string Example { get; }

        public EntityIdFormatAttribute(Type entityIdType)
        {
            if (!typeof(EntityId).IsAssignableFrom(entityIdType))
            {
                throw new ArgumentException($"Type {entityIdType.Name} must be a subclass of EntityId");
            }

            EntityIdType = entityIdType;
            
            // Create a fake example ID
            var instance = (EntityId)Activator.CreateInstance(entityIdType);
            Example = $"{instance.Prefix}abc123";
        }
    }
    
    /// <summary>
    /// Operation filter to populate the Swagger documentation with EntityId format requirements
    /// </summary>
    public class EntityIdFormatOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                // Fix the CustomAttributes handling
                EntityIdFormatAttribute entityIdAttr = null;
                
                // Get attributes from ApiParameterDescription
                var attributes = parameter.ModelMetadata?.GetType().GetProperty("Attributes")?.GetValue(parameter.ModelMetadata, null) as IReadOnlyList<object>;
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        if (attr is EntityIdFormatAttribute formatAttr)
                        {
                            entityIdAttr = formatAttr;
                            break;
                        }
                    }
                }
                
                // If not found in model metadata, try other approaches
                if (entityIdAttr == null && parameter.ParameterDescriptor?.ParameterType != null)
                {
                    var paramAttrs = parameter.ParameterDescriptor.ParameterType
                        .GetCustomAttributes(typeof(EntityIdFormatAttribute), true);
                    
                    if (paramAttrs.Length > 0)
                    {
                        entityIdAttr = paramAttrs[0] as EntityIdFormatAttribute;
                    }
                }
                
                if (entityIdAttr == null) continue;
                
                var prefix = string.Empty;
                var instance = (EntityId)Activator.CreateInstance(entityIdAttr.EntityIdType);
                prefix = instance.Prefix;
                
                if (operation.Parameters.Any(p => p.Name == parameter.Name))
                {
                    var param = operation.Parameters.First(p => p.Name == parameter.Name);
                    param.Schema.Format = $"{prefix}xxx";
                    param.Schema.Example = new OpenApiString(entityIdAttr.Example);
                    param.Description = $"{param.Description}\nMust be in the format {prefix}xxx.";
                }
            }
        }
    }
} 