using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Templar;
using Templar.Database;

namespace Archon
{
    public class PartyCmd : ModuleBase<SocketCommandContext>
    {
        private static Database<Party> Database = new("parties");

        //[Command("create")]
        //[Summary("Create a new party and associated channels.")]
        ////[Syntax("create [name]")]
        //public async Task Create([Remainder] string name)
        //{
        //    var everyone = Context.Client.Guilds.First().EveryoneRole;

        //    // Can't be in more than 2 parties at once
        //    var hosts = await Database.GetByProperty("Host", Context.User.Id);
        //    var guests = await Database.GetByProperty("Guests", Context.User.Id, DatabaseComparer.Contains);

        //    if (hosts.Count + guests.Count > 2 ||
        //        name.Length > 50 ||
        //        ModerationEvents.NewFilter().DetectAllProfanities(name).Count > 0)
        //    {
        //        await Context.ReactError(); return;
        //    }
        //    await Context.ReactOk();

        //    var party = new PartyInfo
        //    {
        //        Host = Context.User.Id,
        //        Guests = new ulong[0],
        //        CreationTime = DateTime.Now,
        //    };

        //    var channel = await Context.Guild.CreateTextChannelAsync(name.ToLower().Replace(" ", "-"), prop =>
        //    {
        //        prop.Topic = "Party Chat";
        //        prop.CategoryId = Category;
        //        prop.PermissionOverwrites = new List<Overwrite>()
        //        {
        //            new Overwrite(everyone.Id, PermissionTarget.Role,
        //                new OverwritePermissions(viewChannel: PermValue.Deny)),
        //            new Overwrite(Context.User.Id, PermissionTarget.User,
        //                new OverwritePermissions(viewChannel: PermValue.Allow)),
        //            new Overwrite(Context.Client.CurrentUser.Id, PermissionTarget.User,
        //                new OverwritePermissions(viewChannel: PermValue.Allow)),
        //        };
        //    });

        //    party.TextChannel = channel.Id;

        //    var vc = await Context.Guild.CreateVoiceChannelAsync(name, prop =>
        //    {
        //        prop.CategoryId = Category;
        //        prop.PermissionOverwrites = new List<Overwrite>()
        //        {
        //            new Overwrite(everyone.Id, PermissionTarget.Role,
        //                new OverwritePermissions(viewChannel: PermValue.Deny, connect: PermValue.Deny)),
        //            new Overwrite(Context.User.Id, PermissionTarget.User,
        //                new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow)),
        //            new Overwrite(BotManager.Client.CurrentUser.Id, PermissionTarget.User,
        //                new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow)),
        //        };
        //    });

        //    party.VoiceChannel = vc.Id;

        //    var all = PartyInfo.GetAll();
        //    all.Add(party);
        //    PartyInfo.SaveParties(all);
        //    await channel.SendMessageAsync($"Party `{name}` created, {Context.User.Mention}! It will be automatically deleted in 24 hours.");
        //}
    }
}
