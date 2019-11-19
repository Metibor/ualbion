﻿using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : MapEvent
    {
        public OpenChestEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            LockStrength = br.ReadByte(); // 1
            KeyItemId = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            ClosedMessageId = br.ReadByte(); // 4
            OpenedMessageId = br.ReadByte(); // 5
            ChestId = br.ReadUInt16(); // 6
            TrapEvent = br.ReadUInt16(); // 8
        }

        public byte LockStrength { get; set; }
        public byte KeyItemId { get; set; }
        public byte Unk3 { get; set; }
        public byte ClosedMessageId { get; set; }
        public byte OpenedMessageId { get; set; }
        public ushort ChestId { get; set; }
        public ushort TrapEvent { get; set; }
        public override string ToString() => $"open_chest {ChestId} Trap:{TrapEvent} Key:{KeyItemId} Lock:{LockStrength} Opened:{OpenedMessageId} Closed:{ClosedMessageId} ({Unk3})";
    }
}