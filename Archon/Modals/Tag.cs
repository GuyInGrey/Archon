using Templar.Database;

namespace Archon
{
    public class Tag : IDatabaseModal
    {
        public string Name;
        public string Content;
        public string Attachments;

        public string[] GetAttachments()
        {
            if (Attachments is null || Attachments == "")
            {
                return new string[0];
            }

            return Attachments.Split(",");
        }

        public void SetAttachments(string[] args)
        {
            Attachments = string.Join(",", args);
        }
    }
}
