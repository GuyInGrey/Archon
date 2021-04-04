using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Templar;
using Extensions = Templar.Extensions;

namespace Archon.Modules
{
    [Group("elite")]
    public class EliteDangerous : ModuleBase<SocketCommandContext>
    {
        public static string TempFolder => Path.Combine(Directory.GetCurrentDirectory(), @"temp2/");

        public static string DumpFileUrl => "https://www.edsm.net/dump/systemsPopulated.json.gz";
        public static string GalnetUrl => "https://www.alpha-orbital.com/galnet-feed";
        public static string EDSMUrl => "https://www.edsm.net/";
        public static string FleetCarrierUrl => "https://us-central1-canonn-api-236217.cloudfunctions.net/postFleetCarriers?serial=";

        public static ITextChannel Channel => Bot.Instance.Client.GetChannel(827930180340416512) as ITextChannel;

        [Event(Events.Ready)]
        public static async Task RegisterCmds()
        {
            foreach (var cmd in await Bot.Instance.Client.Rest.GetGuildApplicationCommands(769057370646511628))
            {
                await cmd.DeleteAsync();
            }


            var props = new SlashCommandCreationProperties()
            {
                Name = "elite",
                Description = "Commands related to Elite: Dangerous.",
                Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "status",
                        Description = "Get Elite's server status.",
                        Type = ApplicationCommandOptionType.SubCommand,
                        Required = false,
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "carrier",
                        Description = "Get a fleet carrier's info.",
                        Type = ApplicationCommandOptionType.SubCommand,
                        Required = false,
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "carrierid",
                                Description = "The ID of the carrier to get info about.",
                                Required = false,
                                Type = ApplicationCommandOptionType.String,
                            }
                        }
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "system",
                        Description = "System commands.",
                        Type = ApplicationCommandOptionType.SubCommandGroup,
                        Required = false,
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "info",
                                Description = "Get info about a system.",
                                Type = ApplicationCommandOptionType.SubCommand,
                                Required = false,
                                Options = new List<ApplicationCommandOptionProperties>()
                                {
                                    new ApplicationCommandOptionProperties()
                                    {
                                        Name = "system",
                                        Required = true,
                                        Type = ApplicationCommandOptionType.String,
                                        Description = "The system to get info about.",
                                    },
                                }
                            },
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "distance",
                                Description = "Get the distance between two systems.",
                                Type = ApplicationCommandOptionType.SubCommand,
                                Required = false,
                                Options = new List<ApplicationCommandOptionProperties>()
                                {
                                    new ApplicationCommandOptionProperties()
                                    {
                                        Name = "system1",
                                        Required = true,
                                        Type = ApplicationCommandOptionType.String,
                                        Description = "The first system.",
                                    },
                                    new ApplicationCommandOptionProperties()
                                    {
                                        Name = "system2",
                                        Required = true,
                                        Type = ApplicationCommandOptionType.String,
                                        Description = "The second system.",
                                    }
                                }
                            },
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "bodies",
                                Description = "Get celestial bodies in a system.",
                                Type = ApplicationCommandOptionType.SubCommand,
                                Required = false,
                                Options = new List<ApplicationCommandOptionProperties>()
                                {
                                    new ApplicationCommandOptionProperties()
                                    {
                                        Name = "system",
                                        Required = true,
                                        Type = ApplicationCommandOptionType.String,
                                        Description = "The system to get the bodies of.",
                                    }
                                },
                            },
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "factions",
                                Description = "Get factions in a system.",
                                Type = ApplicationCommandOptionType.SubCommand,
                                Required = false,
                                Options = new List<ApplicationCommandOptionProperties>()
                                {
                                    new ApplicationCommandOptionProperties()
                                    {
                                        Name = "system",
                                        Required = true,
                                        Type = ApplicationCommandOptionType.String,
                                        Description = "The system to get the factions of.",
                                    }
                                },
                            },
                        },
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Name = "faction",
                        Description = "Get info and states of a faction.",
                        Type = ApplicationCommandOptionType.SubCommand,
                        Required = false,
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "faction",
                                Required = true,
                                Type = ApplicationCommandOptionType.String,
                                Description = "The faction to get info and states of.",
                            }
                        }
                    },
                },
            };

            Console.WriteLine(props.ToString());
            await Bot.Instance.Client.Rest.CreateGuildCommand(props, 769057370646511628);
        }

        [Interaction("elite")]
        public static async Task Interaction(SocketInteraction i)
        {
            await i.FollowupAsync("```js\n" + JsonConvert.SerializeObject(i.Data, Formatting.Indented) + "\n```");
        }

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
            Console.WriteLine("Beginning Faction Search");
            var populatedSystemsPath = TempFolder + Path.GetFileNameWithoutExtension(DumpFileUrl);
            var systemsWeAreIn = new List<(string, JObject)>(); // (system, faction)

            var factionRegex = new Regex("\"factions\":(\\[(?:(?:{[^}]+}),)+{[^}]+}\\])");
            var systemNameRegex = new Regex("\"name\":\"([^\"]+)\"");

            using var lineReader = new StreamReader(File.OpenRead(populatedSystemsPath), Encoding.UTF8, true, 4096);
            var line = "";
            while (!((line = lineReader.ReadLine()) is null))
            {
                //line = line.Trim();
                //if (line.EndsWith(",")) { line = line[0..^1]; }
                //if (line.Length < 5) { continue; }
                //if (!line.Contains("factions")) { continue; }

                //var match = factionRegex.Match(line);
                //if (match.Groups.Count < 2) { continue; }

                //var system = systemNameRegex.Match(line).Groups[1].Value;

                //Console.WriteLine(line + "\n\n-\n\n");

                //var factions = JArray.Parse(match.Groups[1].Value);
                //foreach (JObject faction in factions)
                //{
                //    if (faction["name"].Value<string>() == "LDS Enterprises")
                //    {
                //        systemsWeAreIn.Add((system, faction));
                //    }
                //}


                //Console.WriteLine($"\n\nCount: {match.Groups.Count}\n{match.Groups[1].Value}\n=");

                //var system = JObject.Parse(line);
                //if (!system.ContainsKey("factions")) { continue; }
                //var factions = system["factions"] as JArray;
                //foreach (JObject faction in factions)
                //{
                //    if (faction["name"].Value<string>() == "LDS Enterprises")
                //    {
                //        systemsWeAreIn.Add((system, faction));
                //    }
                //}
            }
            Console.WriteLine("Finished Faction Search");

            var topic = "\"Fly Dangerously, CMDRs, but never without a rebuy\" [LDS Enterprises Presence] \n(";
            topic += string.Join(") \n(", systemsWeAreIn.Select(s =>
            {
                (var system, var faction) = s;

                var state = faction["state"].Value<string>();
                state = state == "None" ? "No State" : state;

                var influencePer = faction["influence"].Value<double>() * 100;
                var influence = influencePer.ToString("0.0") + "%";

                var happiness = faction["happiness"];

                return $"[{system}] {influence}, {state}, {happiness}";
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
                .AddField("Services", servicesString);

            await ReplyAsync(embed: e.Build());
        }

        [Command("distance")]
        public async Task Distance(string system1, string system2)
        {
            var (x, y, z) = GetCoords(system1);
            if (float.IsNaN(x))
            {
                await ReplyAsync($"`{system1}` is not a known system.");
                return;
            }
            var sys2 = GetCoords(system2);
            if (float.IsNaN(sys2.x))
            {
                await ReplyAsync($"`{system2}` is not a known system.");
                return;
            }

            var distance = (float)Math.Sqrt(
                Math.Pow(x - sys2.x, 2) +
                Math.Pow(y - sys2.y, 2) +
                Math.Pow(z - sys2.z, 2));

            var e = Bot.CreateEmbed()
                .WithTitle("Distance")
                .WithDescription($"{system1} - {system2}\n{distance:0.00} ly");

            await ReplyAsync(embed: e.Build());
        }

        public static (float x, float y, float z) GetCoords(string systemName)
        {
            systemName = HttpUtility.UrlEncode(systemName);
            var url = EDSMUrl + $"api-v1/system?showCoordinates=1&systemName={systemName}";
            var content = Extensions.DownloadString(url);

            if (content.Contains("["))
            {
                return (float.NaN, float.NaN, float.NaN);
            }

            var obj = JObject.Parse(content);
            var coords = obj["coords"] as JObject;
            return (coords["x"].Value<float>(), coords["y"].Value<float>(), coords["z"].Value<float>());
        }
    }
}
