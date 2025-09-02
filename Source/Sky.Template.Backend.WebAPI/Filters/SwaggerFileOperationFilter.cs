using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sky.Template.Backend.WebAPI.Filters;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFormFile = context.ApiDescription.ParameterDescriptions
            .Any(p => p.Type == typeof(IFormFile) || p.Type == typeof(IEnumerable<IFormFile>));

        if (!hasFormFile) return;

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new HashSet<string>()
        };

        foreach (var param in context.ApiDescription.ParameterDescriptions)
        {
            var paramType = param.Type;

            // Dosya ise binary format
            if (paramType == typeof(IFormFile) || paramType == typeof(IEnumerable<IFormFile>))
            {
                schema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else
            {
                // String, int gibi diðer parametreler için text
                schema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = GetOpenApiType(paramType),
                    Format = GetOpenApiFormat(paramType)
                };
            }

            if (param.IsRequired)
                schema.Required.Add(param.Name);
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }

    private static string GetOpenApiType(Type type)
    {
        if (type == typeof(int) || type == typeof(long)) return "integer";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(IFormFile) || type == typeof(IEnumerable<IFormFile>)) return "string";
        return "string"; // default
    }

    private static string? GetOpenApiFormat(Type type)
    {
        if (type == typeof(int)) return "int32";
        if (type == typeof(long)) return "int64";
        if (type == typeof(DateTime)) return "date-time";
        return null;
    }
}
