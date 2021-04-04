using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Numerics;
using System.Drawing;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Discord.Audio;

using MySql.Data.MySqlClient;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archon
{
    public class TemplatePost
    {
        public static void Main() { }

        public SocketCommandContext Context;

        public TemplatePost(SocketCommandContext context)
        { Context = context; }
        public async Task ReplyAsync(string text = "", Embed emb = null) 
        { await Context.Channel.SendMessageAsync(text, embed: emb); }

        public async Task Post()
        {
            //Code
        }
    }
}
