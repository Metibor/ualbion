﻿using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.EventSet)]
    public class EventSetLoader : IAssetLoader<EventSet>
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config) => EventSet.Serdes(null, new GenericBinaryReader(br, streamLength));
        public EventSet Serdes(EventSet existing, ISerializer s, string name, AssetInfo config) => EventSet.Serdes(existing, s);
    }
}