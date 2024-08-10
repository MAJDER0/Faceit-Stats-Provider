using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class LifetimeConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(AnalyzerPlayerStatsForCsgo.Lifetime) || typeToConvert == typeof(AnalyzerPlayerStats.Lifetime);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(AnalyzerPlayerStatsForCsgo.Lifetime))
        {
            var lifetime = new AnalyzerPlayerStatsForCsgo.Lifetime();
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
        else if (typeToConvert == typeof(AnalyzerPlayerStats.Lifetime))
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

        throw new JsonException($"Unsupported type: {typeToConvert}");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is AnalyzerPlayerStatsForCsgo.Lifetime csgoLifetime)
        {
            writer.WriteStartObject();

            writer.WriteString("Average K/D Ratio", csgoLifetime.AverageKDRatio);
            writer.WriteString("Win Rate %", csgoLifetime.WinRate);
            writer.WriteString("Wins", csgoLifetime.Wins);
            writer.WriteString("TotalHeadshots", csgoLifetime.TotalHeadshots);
            writer.WriteString("LongestWinStreak", csgoLifetime.LongestWinStreak);
            writer.WriteString("KDRatio", csgoLifetime.KDRatio);
            writer.WriteString("Matches", csgoLifetime.Matches);
            writer.WritePropertyName("RecentResults");
            JsonSerializer.Serialize(writer, csgoLifetime.RecentResults, options);
            writer.WriteString("AverageHeadshots", csgoLifetime.AverageHeadshots);
            writer.WriteString("CurrentWinStreak", csgoLifetime.CurrentWinStreak);

            if (csgoLifetime.ExtensionData != null)
            {
                foreach (var kvp in csgoLifetime.ExtensionData)
                {
                    writer.WritePropertyName(kvp.Key);
                    kvp.Value.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
        else if (value is AnalyzerPlayerStats.Lifetime lifetime)
        {
            writer.WriteStartObject();

            writer.WriteString("Average K/D Ratio", lifetime.AverageKDRatio);
            writer.WriteString("Win Rate %", lifetime.WinRate);
            writer.WriteString("Wins", lifetime.Wins);
            writer.WriteString("TotalHeadshots", lifetime.TotalHeadshots);
            writer.WriteString("LongestWinStreak", lifetime.LongestWinStreak);
            writer.WriteString("KDRatio", lifetime.KDRatio);
            writer.WriteString("Matches", lifetime.Matches);
            writer.WritePropertyName("RecentResults");
            JsonSerializer.Serialize(writer, lifetime.RecentResults, options);
            writer.WriteString("AverageHeadshots", lifetime.AverageHeadshots);
            writer.WriteString("CurrentWinStreak", lifetime.CurrentWinStreak);

            if (lifetime.ExtensionData != null)
            {
                foreach (var kvp in lifetime.ExtensionData)
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
