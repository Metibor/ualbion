﻿using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("npc_off")]
    public class NpcOffEvent : Event, INpcEvent
    {
        public NpcOffEvent(int npcId) { NpcId = npcId; }
        [EventPart("npcId")] public int NpcId { get; }
    }
}