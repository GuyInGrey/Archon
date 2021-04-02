﻿using System;

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
    }
}
