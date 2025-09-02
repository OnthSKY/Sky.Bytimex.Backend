using Newtonsoft.Json;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.User;

public class BaseUserEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("first_name")]
    public string Name { get; set; }
    [DbManager.mColumn("last_name")]
    public string Surname { get; set; }
    [DbManager.mColumn("email")]
    public string Email { get; set; }
    [DbManager.mColumn("status")]
    public string Status { get; set; }
    [DbManager.mColumn("image_path")]
    public string ImagePath { get; set; }

    private string _strCustomFields;

    [DbManager.mColumn("custom_fields")]
    public string StrCustomFields
    {
        get => _strCustomFields;
        set
        {
            _strCustomFields = value;
            DeserializeCustomFields(value);
        }
    }

    public List<CustomDynamicField> CustomFields { get; private set; }

    private void DeserializeCustomFields(string json)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                CustomFields = JsonConvert.DeserializeObject<List<CustomDynamicField>>(json) ?? new List<CustomDynamicField>();
            }
            else
            {
                CustomFields = new List<CustomDynamicField>();
            }
        }
        catch (JsonException)
        {
            CustomFields = new List<CustomDynamicField>();
        }
    }
}
