using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class StatsConverter : JsonConverter<AnalyzerPlayerStats.Stats>
{
    public override AnalyzerPlayerStats.Stats Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stats = new AnalyzerPlayerStats.Stats();
        var extensionData = new Dictionary<string, JsonElement>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                stats.ExtensionData = extensionData;
                return stats;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read(); // Move to the value

                switch (propertyName)
                {
                    case "Average K/R Ratio":
                    case "AverageKRRatio":
                        stats.AverageKRRatio = reader.GetString();
                        break;
                    case "Average K/D Ratio":
                    case "AverageKDRatio":
                        stats.AverageKDRatio = reader.GetString();
                        break;
                    case "Kills":
                        stats.Kills = reader.GetString();
                        break;
                    case "Average Headshots %":
                        stats.AverageHeadshots = reader.GetString();
                        break;
                    case "Assists":
                        stats.Assists = reader.GetString();
                        break;
                    case "Average Kills":
                        stats.AverageKills = reader.GetString();
                        break;
                    case "Headshots per Match":
                        stats.HeadshotsperMatch = reader.GetString();
                        break;
                    case "Average Quadro Kills":
                        stats.AverageQuadroKills = reader.GetString();
                        break;
                    case "Matches":
                        stats.Matches = reader.GetString();
                        break;
                    case "Win Rate %":
                    case "WinRate":
                        stats.WinRate = reader.GetString();
                        break;
                    case "Rounds":
                        stats.Rounds = reader.GetString();
                        break;
                    case "TotalHeadshots":
                        stats.TotalHeadshots = reader.GetString();
                        break;
                    case "KRRatio":
                        stats.KRRatio = reader.GetString();
                        break;
                    case "Deaths":
                        stats.Deaths = reader.GetString();
                        break;
                    case "KDRatio":
                        stats.KDRatio = reader.GetString();
                        break;
                    case "Average Assists":
                        stats.AverageAssists = reader.GetString();
                        break;
                    case "Headshots":
                        stats.Headshots = reader.GetString();
                        break;
                    case "Wins":
                        stats.Wins = reader.GetString();
                        break;
                    case "Average Deaths":
                        stats.AverageDeaths = reader.GetString();
                        break;
                    default:
                        extensionData[propertyName] = JsonElement.ParseValue(ref reader);
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AnalyzerPlayerStats.Stats value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Average K/R Ratio", value.AverageKRRatio);
        writer.WriteString("Average K/D Ratio", value.AverageKDRatio);
        writer.WriteString("Kills", value.Kills);
        writer.WriteString("Average Headshots %", value.AverageHeadshots);
        writer.WriteString("Assists", value.Assists);
        writer.WriteString("Average Kills", value.AverageKills);
        writer.WriteString("Headshots per Match", value.HeadshotsperMatch);
        writer.WriteString("Average Quadro Kills", value.AverageQuadroKills);
        writer.WriteString("Matches", value.Matches);
        writer.WriteString("Win Rate %", value.WinRate);
        writer.WriteString("Rounds", value.Rounds);
        writer.WriteString("TotalHeadshots", value.TotalHeadshots);
        writer.WriteString("KRRatio", value.KRRatio);
        writer.WriteString("Deaths", value.Deaths);
        writer.WriteString("KDRatio", value.KDRatio);
        writer.WriteString("Average Assists", value.AverageAssists);
        writer.WriteString("Headshots", value.Headshots);
        writer.WriteString("Wins", value.Wins);
        writer.WriteString("Average Deaths", value.AverageDeaths);

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
