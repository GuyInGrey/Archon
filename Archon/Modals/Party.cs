using System;
using System.Linq;

using Templar.Database;

namespace Archon
{
    [Serializable]
    public class Party : IDatabaseModal
    {
        public ulong Host;
        public string Guests;
        public ulong TextChannel;
        public ulong VoiceChannel;
        public DateTime CreationTime;

        public ulong[] GetGuests() =>
            Guests is null || Guests == "" ? 
            Array.Empty<ulong>() : 
            Guests.Split(",").Select(s => ulong.Parse(s)).ToArray();

        public void SetGuests(ulong[] guests) =>
            Guests = string.Join(",", guests);
    }
}
