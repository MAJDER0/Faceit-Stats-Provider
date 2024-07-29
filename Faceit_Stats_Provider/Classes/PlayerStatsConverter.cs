using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class PlayerStatsConverter : JsonConverter<AnalyzerMatchStats.Player_Stats>
{
    public override AnalyzerMatchStats.Player_Stats Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var playerStats = new AnalyzerMatchStats.Player_Stats();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return playerStats;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read(); // Move to the value

                switch (propertyName)
                {
                    case "K/D Ratio":
                    case "KDRatio":
                        playerStats.KDRatio = reader.GetString();
                        break;
                    case "K/R Ratio":
                    case "KRRatio":
                        playerStats.KRRatio = reader.GetString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AnalyzerMatchStats.Player_Stats value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("K/D Ratio", value.KDRatio);
        writer.WriteString("K/R Ratio", value.KRRatio);

        writer.WriteEndObject();
    }
}
