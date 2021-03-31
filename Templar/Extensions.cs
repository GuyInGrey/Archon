﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Templar
{
    public static class Extensions
    {
        public static bool HasAttribute<T>(this MemberInfo e) where T : Attribute =>
            !(e.GetCustomAttribute<T>() is null);

        public static bool HasAttribute<T>(this MemberInfo e, out T att) where T : Attribute
        {
            att = e.GetCustomAttribute<T>();
            return !(att is null);
        }

        public static async Task ReactOk(this ICommandContext c) =>
            await c.Message.AddReactionAsync(new Emoji("👌"));

        public static async Task ReactError(this ICommandContext c) =>
            await c.Message.AddReactionAsync(new Emoji("❌"));

        public static async Task ReactQuestion(this ICommandContext c) =>
            await c.Message.AddReactionAsync(new Emoji("❓"));

        public static (string left, string right) SplitAt(this string s, int index) =>
            index == -1 ? (s, "") : (s.Substring(0, index), s[index..]);
    }
}