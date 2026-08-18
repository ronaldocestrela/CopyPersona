using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonaScript.BuildingBlocks.Domain;

public sealed class FlexibleStringCollectionJsonConverter : JsonConverter<IReadOnlyCollection<string>>
{
    public override IReadOnlyCollection<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<string>();

            return str.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .ToList()
                      .AsReadOnly();
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list.AsReadOnly();

                if (reader.TokenType == JsonTokenType.String)
                {
                    var item = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(item))
                        list.Add(item.Trim());
                }
            }
            return list.AsReadOnly();
        }

        return Array.Empty<string>();
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if (value != null)
        {
            foreach (var item in value)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    writer.WriteStringValue(item);
                }
            }
        }
        writer.WriteEndArray();
    }
}
