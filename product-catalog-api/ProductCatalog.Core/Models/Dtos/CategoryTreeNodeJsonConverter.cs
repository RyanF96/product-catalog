using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductCatalog.Core.Models.Dtos;

public class CategoryTreeNodeJsonConverter : JsonConverter<CategoryTreeNode>
{
    public override CategoryTreeNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Deserialization is not supported for CategoryTreeNode.");

    public override void Write(Utf8JsonWriter writer, CategoryTreeNode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumberProperty(options, "id", value.Id);
        writer.WriteStringProperty(options, "name", value.Name);
        writer.WriteStringProperty(options, "description", value.Description);

        if (value.Children.Count > 0)
        {
            writer.WritePropertyName(GetPropertyName(options, "children"));
            writer.WriteStartArray();
            foreach (var child in value.Children)
            {
                Write(writer, child, options);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static string GetPropertyName(JsonSerializerOptions options, string name)
        => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
}

internal static class Utf8JsonWriterExtensions
{
    public static void WriteStringProperty(this Utf8JsonWriter writer, JsonSerializerOptions options, string name, string value)
    {
        writer.WritePropertyName(GetPropertyName(options, name));
        writer.WriteStringValue(value);
    }

    public static void WriteNumberProperty(this Utf8JsonWriter writer, JsonSerializerOptions options, string name, int value)
    {
        writer.WritePropertyName(GetPropertyName(options, name));
        writer.WriteNumberValue(value);
    }

    private static string GetPropertyName(JsonSerializerOptions options, string name)
        => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
}
