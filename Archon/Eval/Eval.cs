using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

//using ForsetiFramework.Utility;

using Microsoft.CSharp;

using Templar;

namespace Archon
{
    public partial class BotAdmin : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<ulong, (SocketCommandContext, string)> History = new();

        [Command("eval")]
        [RequireOwner, Typing]
        public async Task Evaluate([Remainder] string code)
        {
            var lines = code.Split('\n').ToList();
            lines.RemoveAll(l => l.StartsWith("```"));
            lines = lines.Select(s => "            " + s).ToList();
            code = string.Join("\n", lines);
            var get = code.Contains("return");

            var template = File.ReadAllText((Debugger.IsAttached ? "" : "bin/") + "Eval/" + (get ? "TemplateGet" : "TemplatePost") + ".cs");
            code = template.Replace("//Code", code);

            await ExecuteCode(Context, code);
        }

        [Event(Events.ReactionAdded)]
        public static async Task Reaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.UserId != 126481324017057792) { return; }
            if (!History.ContainsKey(arg1.Id)) { return; }
            (var Context, var code) = History[arg1.Id];

            if (arg3.Emote.Name == "🔄")
            {
                await ExecuteCode(Context, code);
            }
            else if (arg3.Emote.Name == "🖥️")
            {
                code = string.Join("\n", code.Split('\n').ToList().Where(c => !c.StartsWith("using ") || c.Trim() == ""));
                var codeParts = code.SplitWithLength(1990).Select(c => $"```cs\n{c}\n```").ToList();
                codeParts.ForEach(c => Context.Channel.SendMessageAsync(c));
            }
        }

        public static async Task ExecuteCode(SocketCommandContext Context, string code)
        {
            var get = code.Contains("return");
            var obj = (Context, code);

            var timeStarted = DateTime.UtcNow;
            var compiled = Compiler.Compile(code, out var diagnostics, out var success);
            var compileTime = (DateTime.UtcNow - timeStarted).TotalMilliseconds;

            if (!success)
            {
                var e = Bot.CreateEmbed()
                    .WithTitle("Compilation Failed")
                    .WithColor(Color.Red);

                foreach (var diag in diagnostics)
                {
                    var title = diag.WarningLevel > 0 ? "Warning" : "Error";
                    e = e.AddField(title, "```\n" + diag + "\n```");
                }

                var msg = await Context.Channel.SendMessageAsync(embed: e.Build());
                await msg.AddReactionsAsync(new[] { "🔄", "🖥" }.Select(s => new Emoji(s)).ToArray());
                History.Add(msg.Id, obj);
                return;
            }

            var asm = Compiler.GetAssembly(compiled);

            try
            {
                var type = asm.GetType(get ? "Archon.TemplateGet" : "Archon.TemplatePost");
                var m = type.GetMethod(get ? "Get" : "Post");

                var con = type.GetConstructor(new[] { typeof(SocketCommandContext) });
                var instance = con.Invoke(new object[] { Context });

                timeStarted = DateTime.UtcNow;
                var result = m?.Invoke(instance, null);

                if (result is Task<object> task)
                {
                    result = task.GetAwaiter().GetResult();
                }
                else if (result is Task task2)
                {
                    result = null;
                    task2.GetAwaiter().GetResult();
                }
                var timeTook = (DateTime.UtcNow - timeStarted).TotalMilliseconds;

                var resultString = result is null ? "No Result" : result.ToString().Trim();
                resultString = resultString.Length > 2030 ? resultString.Substring(0, 2030) : resultString;
                resultString = resultString.Length == 0 ? "`Empty Result`" : resultString;

                var e = Bot.CreateEmbed().WithColor(Color.Green)
                    .AddField("Running Time", $"{timeTook}ms", true)
                    .AddField("Compilation Time", $"{compileTime}ms", true)
                    .WithDescription($"```\n{resultString}\n```")
                    .WithTitle("Runtime Results");
                var msg = await Context.Channel.SendMessageAsync(embed: e.Build());
                await msg.AddReactionsAsync(new[] { "🔄", "🖥️" }.Select(s => new Emoji(s)).ToArray());
                History.Add(msg.Id, obj);
            }
            catch (Exception err)
            {
                var e = Bot.CreateEmbed();
                var errr = err.ToString();
                errr = errr.Length > 250 ? errr.Substring(0, 250) : errr;

                var stackParts = err.StackTrace.SplitWithLength(1000);
                stackParts.ForEach(s => e = e.AddField("Stack Trace", $"```\n{s}\n```"));

                e = e.WithColor(Color.Red)
                    .AddField("Error", errr)
                    .WithTitle("Runtime Error");
                var msg = await Context.Channel.SendMessageAsync(embed: e.Build());
                await msg.AddReactionsAsync(new[] { "🔄", "🖥️" }.Select(s => new Emoji(s)).ToArray());
                History.Add(msg.Id, obj);
            }
        }
    }
}
