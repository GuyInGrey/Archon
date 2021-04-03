using System;

using Templar.Database;

namespace Archon
{
    [Serializable]
    public class Tag : IDatabaseModal
    {
        public string Name;
        public string Content;
        public string Attachments;

        public string[] GetAttachments() =>
            Attachments is null || Attachments == "" ?
            Array.Empty<string>() :
            Attachments.Split(",");

        public void SetAttachments(string[] args) =>
            Attachments = string.Join(",", args);
    }
}
