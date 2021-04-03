using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
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

        public static List<string> SplitWithLength(this string s, int length)
        {
            var toReturn = new List<string>();

            while (s.Length > length)
            {
                var index = s.Substring(0, length).LastIndexOf("\n");
                if (index == -1)
                {
                    s = s.Insert(length - 2, "\n");
                    continue;
                }
                else if (index == 0)
                {
                    s = s.Insert(length - 2, "\n");
                    continue;
                }
                toReturn.Add(s.Substring(0, index));
                s = s[index..];
            }
            toReturn.Add(s);
            return toReturn;
        }

        public static long GetRemoteFileSize(string url)
        {
            var webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            using var webResponse = webRequest.GetResponse();
            return long.Parse(webResponse.Headers.Get("Content-Length"));
        }

        public static void DownloadFile(string url, string dir)
        {
            using var client = new WebClient();
            client.DownloadFile(url, Path.Combine(dir, Path.GetFileName(url)));
        }

        public static string DownloadString(string url)
        {
            using var client = new WebClient();
            return client.DownloadString(new Uri(url));
        }

        public static void DecompressGZip(string file, string newFile)
        {
            var fileToDecompress = new FileInfo(file);
            using var originalFileStream = fileToDecompress.OpenRead();
            var currentFileName = fileToDecompress.FullName;
            using var decompressedFileStream = File.Create(newFile);
            using var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(decompressedFileStream);
        }
    }
}
