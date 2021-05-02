using discordbot.DAL.Infrastructure.Interfaces;

using Newtonsoft.Json;

namespace discordbot.DAL.Entities
{
    class Token : TValue
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }

        [JsonIgnore]
        public string Key => Name;

        [JsonIgnore]
        public object Value => this;
    }
}
