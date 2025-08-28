using System.Text.Json.Serialization;

namespace UserManagement.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RolePermission
    {
        User,
        Admin
    }
}
