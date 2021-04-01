using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Templar;
using Templar.Database;

namespace Archon
{
    class Program
    {
        static async Task Main()
        {
            var config = JObject.Parse(File.ReadAllText(@"..\config.json"));
            var webhook = config["webhook"].Value<string>();
            var database = config["databaseConnection"].Value<string>();
            var token = config["token"].Value<string>();
            var prefix = Debugger.IsAttached ? "$" : "!";

            var bot = new Bot(webhook, database, prefix)
            {
                RunCommand = (async (msg) =>
                {
                    return await UtilityCmd.RunTag(msg, prefix);
                })
            };

            await bot.Start(token);
        }
    }
}
