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

        public string[] GetAttachments()
        {
            if (Attachments is null || Attachments == "")
            {
                return Array.Empty<string>();
            }

            return Attachments.Split(",");
        }

        public void SetAttachments(string[] args)
        {
            Attachments = string.Join(",", args);
        }
    }
}
