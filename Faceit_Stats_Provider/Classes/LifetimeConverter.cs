using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class LifetimeConverter : JsonConverter<AnalyzerPlayerStats.Lifetime>
{
    public override AnalyzerPlayerStats.Lifetime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var lifetime = new AnalyzerPlayerStats.Lifetime();
        var extensionData = new Dictionary<string, JsonElement>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                lifetime.ExtensionData = extensionData;
                return lifetime;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read(); // Move to the value

                switch (propertyName)
                {
                    case "Average K/D Ratio":
                    case "AverageKDRatio":
                        lifetime.AverageKDRatio = reader.GetString();
                        break;
                    case "Win Rate %":
                    case "WinRate":
                        lifetime.WinRate = reader.GetString();
                        break;
                    case "Wins":
                        lifetime.Wins = reader.GetString();
                        break;
                    case "TotalHeadshots":
                        lifetime.TotalHeadshots = reader.GetString();
                        break;
                    case "LongestWinStreak":
                        lifetime.LongestWinStreak = reader.GetString();
                        break;
                    case "KDRatio":
                        lifetime.KDRatio = reader.GetString();
                        break;
                    case "Matches":
                        lifetime.Matches = reader.GetString();
                        break;
                    case "RecentResults":
                        lifetime.RecentResults = JsonSerializer.Deserialize<string[]>(ref reader, options);
                        break;
                    case "AverageHeadshots":
                        lifetime.AverageHeadshots = reader.GetString();
                        break;
                    case "CurrentWinStreak":
                        lifetime.CurrentWinStreak = reader.GetString();
                        break;
                    default:
                        extensionData[propertyName] = JsonElement.ParseValue(ref reader);
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AnalyzerPlayerStats.Lifetime value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Average K/D Ratio", value.AverageKDRatio);
        writer.WriteString("Win Rate %", value.WinRate);
        writer.WriteString("Wins", value.Wins);
        writer.WriteString("TotalHeadshots", value.TotalHeadshots);
        writer.WriteString("LongestWinStreak", value.LongestWinStreak);
        writer.WriteString("KDRatio", value.KDRatio);
        writer.WriteString("Matches", value.Matches);
        writer.WritePropertyName("RecentResults");
        JsonSerializer.Serialize(writer, value.RecentResults, options);
        writer.WriteString("AverageHeadshots", value.AverageHeadshots);
        writer.WriteString("CurrentWinStreak", value.CurrentWinStreak);

        if (value.ExtensionData != null)
        {
            foreach (var kvp in value.ExtensionData)
            {
                writer.WritePropertyName(kvp.Key);
                kvp.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}
