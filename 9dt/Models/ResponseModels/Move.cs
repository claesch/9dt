using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace _9dt.Models
{
    public class MoveResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MoveType Type { get; set; }
        public string Player{ get; set; }
        public int? Column { get; set; }
    }
}