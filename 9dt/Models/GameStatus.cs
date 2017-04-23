using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace _9dt.Models
{
    public class GameStatus
    {
        public string[] Players { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public GameState State { get; set; }
        public string Winner { get; set; }
    }
}