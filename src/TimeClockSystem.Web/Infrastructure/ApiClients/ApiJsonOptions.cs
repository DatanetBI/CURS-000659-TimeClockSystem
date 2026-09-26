using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeClockSystem.Web.Infrastructure.ApiClients;

/// <summary>Opciones JSON compartidas por todos los clientes del Backend (enums como texto).</summary>
public static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
