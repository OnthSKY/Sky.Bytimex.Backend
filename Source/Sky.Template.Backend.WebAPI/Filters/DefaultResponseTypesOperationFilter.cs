
using Microsoft.OpenApi.Models;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.BaseResponse;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sky.Template.Backend.WebAPI.Filters;
public class DefaultResponseTypesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var schemaGenerator = context.SchemaGenerator;
        var schemaRepository = context.SchemaRepository;

        // 200 varsa karýþma ama diðerlerini ekle
        if (!operation.Responses.ContainsKey("200"))
        {
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = GetContent<BaseControllerResponse>(schemaGenerator, schemaRepository)
            });
        }

        AddIfNotExists(operation.Responses, "400", "Bad Request", GetContent<ErrorDetails>(schemaGenerator, schemaRepository));
        AddIfNotExists(operation.Responses, "401", "Unauthorized", GetContent<ErrorDetails>(schemaGenerator, schemaRepository));
        AddIfNotExists(operation.Responses, "403", "Forbidden", GetContent<ErrorDetails>(schemaGenerator, schemaRepository));
        AddIfNotExists(operation.Responses, "422", "Validation Error", GetContent<ValidationErrorDetails>(schemaGenerator, schemaRepository));
        AddIfNotExists(operation.Responses, "500", "Internal Server Error", GetContent<ErrorDetails>(schemaGenerator, schemaRepository));
    }

    private void AddIfNotExists(OpenApiResponses responses, string statusCode, string description, Dictionary<string, OpenApiMediaType> content)
    {
        if (!responses.ContainsKey(statusCode))
        {
            responses.Add(statusCode, new OpenApiResponse
            {
                Description = description,
                Content = content
            });
        }
    }

    private Dictionary<string, OpenApiMediaType> GetContent<T>(ISchemaGenerator generator, SchemaRepository repository)
    {
        return new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType
            {
                Schema = generator.GenerateSchema(typeof(T), repository)
            }
        };
    }
}
