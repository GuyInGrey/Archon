using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Templar.Database;

namespace Archon
{
    [Serializable]
    public class ServerConfig : IDatabaseModal
    {
        public ulong ServerID;
        public string Config;

        public JObject GetConfigObject() =>
            JObject.Parse(Config);

        public void SetConfigObject(JObject config) => 
            Config = JsonConvert.SerializeObject(config);
    }
}
