using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class StatsConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(AnalyzerPlayerStats.Stats) || typeToConvert == typeof(AnalyzerPlayerStatsForCsgo.Stats);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(AnalyzerPlayerStats.Stats))
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
        else if (typeToConvert == typeof(AnalyzerPlayerStatsForCsgo.Stats))
        {
            var stats = new AnalyzerPlayerStatsForCsgo.Stats();
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

        throw new JsonException($"Unsupported type: {typeToConvert}");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is AnalyzerPlayerStats.Stats stats)
        {
            writer.WriteStartObject();

            writer.WriteString("Average K/R Ratio", stats.AverageKRRatio);
            writer.WriteString("Average K/D Ratio", stats.AverageKDRatio);
            writer.WriteString("Kills", stats.Kills);
            writer.WriteString("Average Headshots %", stats.AverageHeadshots);
            writer.WriteString("Assists", stats.Assists);
            writer.WriteString("Average Kills", stats.AverageKills);
            writer.WriteString("Headshots per Match", stats.HeadshotsperMatch);
            writer.WriteString("Average Quadro Kills", stats.AverageQuadroKills);
            writer.WriteString("Matches", stats.Matches);
            writer.WriteString("Win Rate %", stats.WinRate);
            writer.WriteString("Rounds", stats.Rounds);
            writer.WriteString("TotalHeadshots", stats.TotalHeadshots);
            writer.WriteString("KRRatio", stats.KRRatio);
            writer.WriteString("Deaths", stats.Deaths);
            writer.WriteString("KDRatio", stats.KDRatio);
            writer.WriteString("Average Assists", stats.AverageAssists);
            writer.WriteString("Headshots", stats.Headshots);
            writer.WriteString("Wins", stats.Wins);
            writer.WriteString("Average Deaths", stats.AverageDeaths);

            if (stats.ExtensionData != null)
            {
                foreach (var kvp in stats.ExtensionData)
                {
                    writer.WritePropertyName(kvp.Key);
                    kvp.Value.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
        else if (value is AnalyzerPlayerStatsForCsgo.Stats csgoStats)
        {
            writer.WriteStartObject();

            writer.WriteString("Average K/R Ratio", csgoStats.AverageKRRatio);
            writer.WriteString("Average K/D Ratio", csgoStats.AverageKDRatio);
            writer.WriteString("Kills", csgoStats.Kills);
            writer.WriteString("Average Headshots %", csgoStats.AverageHeadshots);
            writer.WriteString("Assists", csgoStats.Assists);
            writer.WriteString("Average Kills", csgoStats.AverageKills);
            writer.WriteString("Headshots per Match", csgoStats.HeadshotsperMatch);
            writer.WriteString("Average Quadro Kills", csgoStats.AverageQuadroKills);
            writer.WriteString("Matches", csgoStats.Matches);
            writer.WriteString("Win Rate %", csgoStats.WinRate);
            writer.WriteString("Rounds", csgoStats.Rounds);
            writer.WriteString("TotalHeadshots", csgoStats.TotalHeadshots);
            writer.WriteString("KRRatio", csgoStats.KRRatio);
            writer.WriteString("Deaths", csgoStats.Deaths);
            writer.WriteString("KDRatio", csgoStats.KDRatio);
            writer.WriteString("Average Assists", csgoStats.AverageAssists);
            writer.WriteString("Headshots", csgoStats.Headshots);
            writer.WriteString("Wins", csgoStats.Wins);
            writer.WriteString("Average Deaths", csgoStats.AverageDeaths);

            if (csgoStats.ExtensionData != null)
            {
                foreach (var kvp in csgoStats.ExtensionData)
                {
                    writer.WritePropertyName(kvp.Key);
                    kvp.Value.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
        else
        {
            throw new JsonException($"Unsupported type: {value.GetType()}");
        }
    }
}
