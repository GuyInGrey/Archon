using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Newtonsoft.Json.Linq;

using Templar;
using Extensions = Templar.Extensions;

namespace Archon.Modules
{
    [Group("elite")]
    public class EliteDangerous : ModuleBase<SocketCommandContext>
    {
        public static string TempFolder => Path.Combine(Directory.GetCurrentDirectory(), @"temp\");

        public static string DumpFileUrl => "https://www.edsm.net/dump/systemsPopulated.json.gz";
        public static string GalnetUrl => "https://www.alpha-orbital.com/galnet-feed";
        public static string EDSMUrl => "https://www.edsm.net/";
        public static string FleetCarrierUrl => "https://us-central1-canonn-api-236217.cloudfunctions.net/postFleetCarriers?serial=";

        public static ITextChannel Channel => Bot.Instance.Client.GetChannel(827930180340416512) as ITextChannel;

        //[Clockwork(1000 * 60 * 60 /* 60 minutes */)]
        [Event(Events.Ready)]
        public static async Task Update()
        {
            if (!Directory.Exists(TempFolder)) { Directory.CreateDirectory(TempFolder); }
            UpdateDumpFile();
            PostGalnetIfUpdate();
            await UpdateChannelDescription();
        }

        public static void UpdateDumpFile()
        {
            var localFile = TempFolder + Path.GetFileName(DumpFileUrl);
            var fileNeedsUpdate =
                !File.Exists(localFile) ||
                new FileInfo(localFile).Length !=
                Extensions.GetRemoteFileSize(DumpFileUrl);

            if (!fileNeedsUpdate) { return; }

            if (File.Exists(localFile)) { File.Delete(localFile); }
            Extensions.DownloadFile(DumpFileUrl, TempFolder);

            var decompressed = TempFolder + Path.GetFileNameWithoutExtension(localFile);
            if (File.Exists(decompressed)) { File.Delete(decompressed); }
            Extensions.DecompressGZip(localFile, decompressed);
        }

        public static void PostGalnetIfUpdate()
        {
            var galnet = JArray.Parse(Extensions.DownloadString(GalnetUrl));
            var posted = TempFolder + "posted.tmp";

            var needsUpdate =
                !File.Exists(posted) ||
                File.ReadAllText(posted) != galnet[0]["title"].Value<string>();

            if (!needsUpdate) { return; }

            if (File.Exists(posted)) { File.Delete(posted); }
            File.WriteAllText(posted, galnet[0]["title"].Value<string>());

            var content = galnet[0]["content"].Value<string>();
            content = content.Replace("<br /><br />", "\n\n");

            var e = Bot.CreateEmbed()
                .WithTitle(galnet[0]["title"].Value<string>())
                .WithDescription(content)
                .WithFooter(galnet[0]["date"].Value<string>())
                .WithThumbnailUrl("https://i.imgur.com/Ud8MOzY.png")
                .WithAuthor("Galnet Article", "https://i.imgur.com/99ZDAiT.png")
                .WithUrl("https://community.elitedangerous.com/");

            Channel.SendMessageAsync(embed: e.Build());
        }

        public static async Task UpdateChannelDescription()
        {
            var populatedSystemsPath = TempFolder + Path.GetFileNameWithoutExtension(DumpFileUrl);
            var systemsWeAreIn = new List<(JObject, JObject)>(); // (system, faction)

            using var lineReader = new StreamReader(File.OpenRead(populatedSystemsPath), Encoding.UTF8, true, 4096);
            var line = "";
            while (!((line = lineReader.ReadLine()) is null))
            {
                line = line.Trim();
                if (line.EndsWith(",")) { line = line[0..^1]; }
                if (line.Length < 5) { continue; }

                var system = JObject.Parse(line);
                if (!system.ContainsKey("factions")) { continue; }
                var factions = system["factions"] as JArray;
                foreach (JObject faction in factions)
                {
                    if (faction["name"].Value<string>() == "LDS Enterprises")
                    {
                        systemsWeAreIn.Add((system, faction));
                    }
                }
            }

            var topic = "\"Fly Dangerously, CMDRs, but never without a rebuy\" [LDS Enterprises Presence] \n(";
            topic += string.Join(") \n(", systemsWeAreIn.Select(s =>
            {
                (var system, var faction) = s;

                var name = system["name"].Value<string>();

                var state = faction["state"].Value<string>();
                state = state == "None" ? "No State" : state;

                var influencePer = faction["influence"].Value<double>() * 100;
                var influence = influencePer.ToString("0.0") + "%";

                var happiness = faction["happiness"];

                return $"[{name}] {influence}, {state}, {happiness}";
            })) + ");";

            try // Rate limit catcher
            {
                await Channel.ModifyAsync(c =>
                {
                    c.Topic = topic;
                });
            } catch { }
        }

        [Command("status")]
        public async Task Status()
        {
            var status = JObject.Parse(Extensions.DownloadString(EDSMUrl + "api-status-v1/elite-server"));
            var time = DateTime.UtcNow.AddYears(1286);

            var e = Bot.CreateEmbed()
                .WithFooter(time.ToString())
                .WithTitle("Elite Server Status")
                .WithDescription(status["message"].Value<string>());
            await ReplyAsync(embed: e.Build());
        }

        [Command("carrier")]
        public async Task Carrier(string carrierID = "QNQ-WTK")
        {
            carrierID = carrierID.ToUpper();
            if (!Regex.IsMatch(carrierID, "[A-Z0-9]{3}-[A-Z0-9]{3}"))
            {
                await ReplyAsync("That is not a valid carrier ID.");
                return;
            }

            var carrierArray = JArray.Parse(Extensions.DownloadString(FleetCarrierUrl + carrierID));
            if (carrierArray.Count == 0)
            {
                await ReplyAsync("I couldn't find a carrier with that ID.");
                return;
            }

            var carrier = carrierArray[0] as JObject;
            var services = JArray.Parse(carrier["services"].Value<string>());
            var servicesString = string.Join(", ", services.Select(s =>
            {
                var serv = s.Value<string>();
                serv = char.ToUpper(serv[0]) + serv[1..];
                return serv;
            }));
            var e = Bot.CreateEmbed()
                .WithTitle(carrier["name"].Value<string>())
                .AddField("Location", carrier["current_system"].Value<string>())
                .AddField("Previous Location", carrier["previous_system"].Value<string>())
                .AddField("Services", servicesString);

            await ReplyAsync(embed: e.Build());
        }
    }
}
