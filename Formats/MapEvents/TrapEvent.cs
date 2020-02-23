﻿using System.Diagnostics;
using System.IO;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class TrapEvent : IMapEvent
    {
        public static TrapEvent Serdes(TrapEvent e, ISerializer s)
        {
            e ??= new TrapEvent();
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
            var trapEvent = new TrapEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };

            Debug.Assert(trapEvent.Unk4 == 0);
            Debug.Assert(trapEvent.Unk8 == 0);

            return new EventNode(id, trapEvent);
        }

        public byte Unk1 { get; private set; } // Observed values: 1,6,7,11,255
        public byte Unk2 { get; private set; } // 2,3 (2 only seen once)
        public byte Unk3 { get; private set; } // 0,1,2
        public byte Unk5 { get; private set; } // [0..12]
        public ushort Unk6 { get; private set; } // [0..10000], mostly 6, 0 or multiples of 5. Damage?

        byte Unk4 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"trap ({Unk1} {Unk2} {Unk3} {Unk5} {Unk6})";
        public MapEventType EventType => MapEventType.Trap;
    }
}
