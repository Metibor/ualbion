﻿using System.Diagnostics;
using System.IO;
using System.Text;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class AutomapInfo
    {
        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public string Name { get; private set; } // name length = 15

        public static AutomapInfo Serdes(int _, AutomapInfo existing, ISerializer s)
        {
            var info = existing ?? new AutomapInfo();
            info.X = s.UInt8(nameof(X), info.X); // 0
            info.Y = s.UInt8(nameof(Y), info.Y); // 1
            info.Unk2 = s.UInt8(nameof(Unk2), info.Unk2); // 2
            info.Unk3 = s.UInt8(nameof(Unk3), info.Unk3); // 3
            info.Name = s.FixedLengthString(nameof(Name), info.Name, 15); // 4
            return info;
        }

        public static AutomapInfo Load(BinaryReader br)
        {
            var i = new AutomapInfo
            {
                X = br.ReadByte(),
                Y = br.ReadByte(),
                Unk2 = br.ReadByte(),
                Unk3 = br.ReadByte()
            };
            var nameBytes = br.ReadBytes(15);

            bool done = false; // Verify that strings contain no embedded nulls.
            int length = 0;
            foreach (var t in nameBytes)
            {
                if(done)
                    Debug.Assert(t == 0);
                else if (t == 0)
                    done = true;
                else
                    length++;
            }

            i.Name = Encoding.GetEncoding(850).GetString(nameBytes, 0, length);
            return i;
        }
    }
}