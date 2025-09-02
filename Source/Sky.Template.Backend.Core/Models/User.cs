namespace Sky.Template.Backend.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string UserImagePath { get; set; }
    public List<CustomDynamicField> CustomFieldList { get; set; }
}


public class CustomDynamicField
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Value { get; set; }
}
