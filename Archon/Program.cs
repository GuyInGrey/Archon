using System.IO;

using Newtonsoft.Json.Linq;

using Templar;

namespace Archon
{
    class Program
    {
        static void Main()
        {
            var config = JObject.Parse(File.ReadAllText(@"..\config.json"));

            var bot = new Bot(config["webhook"].Value<string>());
            bot.RunCommand = ((con) =>
            {
                return true;
            });

            bot.Start(config["token"].Value<string>()).GetAwaiter().GetResult();
        }
    }
}
