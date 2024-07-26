using Faceit_Stats_Provider.ModelsForAnalyzer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Faceit_Stats_Provider.Classes
{
    public class PlayerMatchStatsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<(string playerId, AnalyzerMatchStats.Rootobject)>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonArray = JArray.Load(reader);
            var result = new List<(string playerId, AnalyzerMatchStats.Rootobject)>();

            foreach (var item in jsonArray)
            {
                var playerId = item["playerId"].Value<string>();
                var matchStats = item["matchStats"].ToObject<AnalyzerMatchStats.Rootobject>(serializer);
                result.Add((playerId, matchStats));
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = (List<(string playerId, AnalyzerMatchStats.Rootobject)>)value;
            writer.WriteStartArray();

            foreach (var (playerId, matchStats) in list)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("playerId");
                writer.WriteValue(playerId);
                writer.WritePropertyName("matchStats");
                serializer.Serialize(writer, matchStats);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}
