﻿using System.IO;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class EncounterEvent : IMapEvent
    {
        public static EncounterEvent Serdes(EncounterEvent e, ISerializer s)
        {
            e ??= new EncounterEvent();
            e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            return new EventNode(id, new EncounterEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            });
        }

        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"encounter_event ({Unk1} {Unk2} {Unk3} {Unk4} {Unk5} {Unk6} {Unk8})";
        public MapEventType EventType => MapEventType.Encounter;
    }
}
