﻿using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Inventory, FileFormat.MerchantInventory)]
    class ChestLoader : IAssetLoader<Chest>
    {
        public Chest Serdes(Chest chest, ISerializer s, string name, AssetInfo config)
        {
            chest ??= new Chest();

            var start = s.Offset;
            for (int i = 0; i < Chest.SlotCount; i++)
                chest.Slots[i] = s.Meta($"Slot{i}", chest.Slots[i], ItemSlotLoader.Serdes);

            if (config.Format == FileFormat.MerchantInventory)
                return chest;

            chest.Gold = s.UInt16("Gold", chest.Gold);
            chest.Rations = s.UInt16("Rations", chest.Rations);
            return chest;
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config) => Serdes(null, new GenericBinaryReader(br, streamLength), name, config);
    }
}
