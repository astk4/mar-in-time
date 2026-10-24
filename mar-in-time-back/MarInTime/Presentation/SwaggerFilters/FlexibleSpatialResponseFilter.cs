using MarInTime.Infrastructure.TransportModels;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarInTime.Presentation.SwaggerFilters
{
    public class FlexibleSpatialResponseFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(FlexibleSpatialResponse)
                || schema is not OpenApiSchema oas) { return; }

            oas.Properties = null;
            oas.Type = null;

            oas.OneOf = new List<IOpenApiSchema>
            {
                context.SchemaGenerator.GenerateSchema(typeof(SpatialEntityDisplayDto), context.SchemaRepository),
                context.SchemaGenerator.GenerateSchema(typeof(SpatialUpdateDto), context.SchemaRepository)
            };
        }
    }
}
