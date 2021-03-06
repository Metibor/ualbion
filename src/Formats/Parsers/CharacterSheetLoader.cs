﻿using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.CharacterData)]
    class CharacterSheetLoader : IAssetLoader<CharacterSheet>
    {
        const int SpellSchoolCount = 7;
        const int MaxSpellsPerSchool = 30;

        public CharacterSheet Serdes(CharacterSheet sheet, ISerializer s, AssetKey key, AssetInfo config)
        {
            var initialOffset = s.Offset;
            sheet ??= new CharacterSheet(key);
            s.Begin();
            s.Check();
            sheet.Type = s.EnumU8(nameof(sheet.Type), sheet.Type);
            sheet.Gender = s.EnumU8(nameof(sheet.Gender), sheet.Gender);
            sheet.Race = s.EnumU8(nameof(sheet.Race), sheet.Race);
            sheet.Class = s.EnumU8(nameof(sheet.Class), sheet.Class);
            sheet.Magic.SpellClasses = s.EnumU8(nameof(sheet.Magic.SpellClasses), sheet.Magic.SpellClasses);
            sheet.Level = s.UInt8(nameof(sheet.Level), sheet.Level);
            sheet.Unknown6 = s.UInt8(nameof(sheet.Unknown6), sheet.Unknown6);
            sheet.Unknown7 = s.UInt8(nameof(sheet.Unknown7), sheet.Unknown7);
            sheet.Languages = s.EnumU8(nameof(sheet.Languages), sheet.Languages);
            s.Check();

            var spriteType =
                sheet.Type switch
                {
                    CharacterType.Party => AssetType.BigPartyGraphics,
                    CharacterType.Npc => AssetType.BigNpcGraphics,
                    CharacterType.Monster => AssetType.MonsterGraphics,
                    _ => throw new InvalidOperationException($"Unhandled character type {sheet.Type}")
                };

            byte spriteId = s.UInt8(nameof(sheet.SpriteId), (byte)sheet.SpriteId.Id);
            sheet.SpriteId = new AssetId(spriteType, spriteId);
            sheet.PortraitId = s.TransformEnumU8(nameof(sheet.PortraitId), sheet.PortraitId, Tweak<SmallPortraitId>.Instance); 
            sheet.Unknown11 = s.UInt8(nameof(sheet.Unknown11 ), sheet.Unknown11);
            sheet.Unknown12 = s.UInt8(nameof(sheet.Unknown12), sheet.Unknown12);
            sheet.Unknown13 = s.UInt8(nameof(sheet.Unknown13), sheet.Unknown13);
            sheet.Unknown14 = s.UInt8(nameof(sheet.Unknown14), sheet.Unknown14);
            sheet.Unknown15 = s.UInt8(nameof(sheet.Unknown15), sheet.Unknown15);
            sheet.Unknown16 = s.UInt8(nameof(sheet.Unknown16), sheet.Unknown16);
            sheet.Combat.ActionPoints = s.UInt8(nameof(sheet.Combat.ActionPoints), sheet.Combat.ActionPoints);
            sheet.EventSetId = s.TransformEnumU16(nameof(sheet.EventSetId), sheet.EventSetId, Tweak<EventSetId>.Instance);
            sheet.WordSetId = s.TransformEnumU16(nameof(sheet.WordSetId), sheet.WordSetId, Tweak<EventSetId>.Instance);
            sheet.Combat.TrainingPoints = s.UInt16(nameof(sheet.Combat.TrainingPoints), sheet.Combat.TrainingPoints);

            ushort gold = s.UInt16("Gold", sheet.Inventory?.Gold.Amount ?? 0);
            ushort rations = s.UInt16("Rations", sheet.Inventory?.Rations.Amount ?? 0);
            if (sheet.Inventory != null)
            {
                sheet.Inventory.Gold.Item = new Gold();
                sheet.Inventory.Rations.Item = new Rations();
                sheet.Inventory.Gold.Amount = gold;
                sheet.Inventory.Rations.Amount = rations;
            }

            sheet.Unknown1C = s.UInt16(nameof(sheet.Unknown1C), sheet.Unknown1C);
            s.Check();

            sheet.Combat.PhysicalConditions = s.EnumU8(nameof(sheet.Combat.PhysicalConditions), sheet.Combat.PhysicalConditions);
            sheet.Combat.MentalConditions = s.EnumU8(nameof(sheet.Combat.MentalConditions), sheet.Combat.MentalConditions);
            s.Check();

            sheet.Unknown20 = s.UInt16(nameof(sheet.Unknown20), sheet.Unknown20);
            sheet.Unknown22 = s.UInt16(nameof(sheet.Unknown22), sheet.Unknown22);
            sheet.Unknown24 = s.UInt16(nameof(sheet.Unknown24), sheet.Unknown24);
            sheet.Unknown26 = s.UInt16(nameof(sheet.Unknown26), sheet.Unknown26);
            sheet.Unknown28 = s.UInt16(nameof(sheet.Unknown28), sheet.Unknown28);
            sheet.Attributes.Strength = s.UInt16(nameof(sheet.Attributes.Strength), sheet.Attributes.Strength);
            sheet.Attributes.StrengthMax = s.UInt16(nameof(sheet.Attributes.StrengthMax), sheet.Attributes.StrengthMax);
            sheet.Unknown2E = s.UInt16(nameof(sheet.Unknown2E), sheet.Unknown2E);
            sheet.Unknown30 = s.UInt16(nameof(sheet.Unknown30), sheet.Unknown30);
            sheet.Attributes.Intelligence = s.UInt16(nameof(sheet.Attributes.Intelligence), sheet.Attributes.Intelligence);
            sheet.Attributes.IntelligenceMax = s.UInt16(nameof(sheet.Attributes.IntelligenceMax), sheet.Attributes.IntelligenceMax);
            sheet.Unknown36 = s.UInt16(nameof(sheet.Unknown36), sheet.Unknown36);
            sheet.Unknown38 = s.UInt16(nameof(sheet.Unknown38), sheet.Unknown38);
            sheet.Attributes.Dexterity = s.UInt16(nameof(sheet.Attributes.Dexterity), sheet.Attributes.Dexterity);
            sheet.Attributes.DexterityMax = s.UInt16(nameof(sheet.Attributes.DexterityMax), sheet.Attributes.DexterityMax);
            sheet.Unknown3E = s.UInt16(nameof(sheet.Unknown3E), sheet.Unknown3E);
            sheet.Unknown40 = s.UInt16(nameof(sheet.Unknown40), sheet.Unknown40);
            sheet.Attributes.Speed = s.UInt16(nameof(sheet.Attributes.Speed), sheet.Attributes.Speed);
            sheet.Attributes.SpeedMax = s.UInt16(nameof(sheet.Attributes.SpeedMax), sheet.Attributes.SpeedMax);
            sheet.Unknown46 = s.UInt16(nameof(sheet.Unknown46), sheet.Unknown46);
            sheet.Unknown48 = s.UInt16(nameof(sheet.Unknown48), sheet.Unknown48);
            sheet.Attributes.Stamina = s.UInt16(nameof(sheet.Attributes.Stamina), sheet.Attributes.Stamina);
            sheet.Attributes.StaminaMax = s.UInt16(nameof(sheet.Attributes.StaminaMax), sheet.Attributes.StaminaMax);
            sheet.Unknown4E = s.UInt16(nameof(sheet.Unknown4E), sheet.Unknown4E);
            sheet.Unknown50 = s.UInt16(nameof(sheet.Unknown50), sheet.Unknown50);
            sheet.Attributes.Luck = s.UInt16(nameof(sheet.Attributes.Luck), sheet.Attributes.Luck);
            sheet.Attributes.LuckMax = s.UInt16(nameof(sheet.Attributes.LuckMax), sheet.Attributes.LuckMax);
            sheet.Unknown56 = s.UInt16(nameof(sheet.Unknown56), sheet.Unknown56);
            sheet.Unknown58 = s.UInt16(nameof(sheet.Unknown58), sheet.Unknown58);
            sheet.Attributes.MagicResistance = s.UInt16(nameof(sheet.Attributes.MagicResistance), sheet.Attributes.MagicResistance);
            sheet.Attributes.MagicResistanceMax = s.UInt16(nameof(sheet.Attributes.MagicResistanceMax), sheet.Attributes.MagicResistanceMax);
            sheet.Unknown5E = s.UInt16(nameof(sheet.Unknown5E), sheet.Unknown5E);
            sheet.Unknown60 = s.UInt16(nameof(sheet.Unknown60), sheet.Unknown60);
            sheet.Attributes.MagicTalent = s.UInt16(nameof(sheet.Attributes.MagicTalent), sheet.Attributes.MagicTalent);
            sheet.Attributes.MagicTalentMax = s.UInt16(nameof(sheet.Attributes.MagicTalentMax), sheet.Attributes.MagicTalentMax);
            sheet.Unknown66 = s.UInt16(nameof(sheet.Unknown66), sheet.Unknown66);
            sheet.Unknown68 = s.UInt16(nameof(sheet.Unknown68), sheet.Unknown68);
            s.Check();

            sheet.Age = s.UInt16(nameof(sheet.Age), sheet.Age);
            sheet.Unknown6C = s.UInt8(nameof(sheet.Unknown6C), sheet.Unknown6C);
                s.RepeatU8("UnknownBlock6D", 0, 13);
            sheet.Skills.CloseCombat = s.UInt16(nameof(sheet.Skills.CloseCombat), sheet.Skills.CloseCombat);
            sheet.Skills.CloseCombatMax = s.UInt16(nameof(sheet.Skills.CloseCombatMax), sheet.Skills.CloseCombatMax);
            sheet.Unknown7E = s.UInt16(nameof(sheet.Unknown7E), sheet.Unknown7E);
            sheet.Unknown80 = s.UInt16(nameof(sheet.Unknown80), sheet.Unknown80);
            sheet.Skills.RangedCombat = s.UInt16(nameof(sheet.Skills.RangedCombat), sheet.Skills.RangedCombat);
            sheet.Skills.RangedCombatMax = s.UInt16(nameof(sheet.Skills.RangedCombatMax), sheet.Skills.RangedCombatMax);
            sheet.Unknown86 = s.UInt16(nameof(sheet.Unknown86), sheet.Unknown86);
            sheet.Unknown88 = s.UInt16(nameof(sheet.Unknown88), sheet.Unknown88);
            sheet.Skills.CriticalChance = s.UInt16(nameof(sheet.Skills.CriticalChance), sheet.Skills.CriticalChance);
            sheet.Skills.CriticalChanceMax = s.UInt16(nameof(sheet.Skills.CriticalChanceMax), sheet.Skills.CriticalChanceMax);
            sheet.Unknown8E = s.UInt16(nameof(sheet.Unknown8E), sheet.Unknown8E);
            sheet.Unknown90 = s.UInt16(nameof(sheet.Unknown90), sheet.Unknown90);
            sheet.Skills.LockPicking = s.UInt16(nameof(sheet.Skills.LockPicking), sheet.Skills.LockPicking);
            sheet.Skills.LockPickingMax = s.UInt16(nameof(sheet.Skills.LockPickingMax), sheet.Skills.LockPickingMax);
            sheet.UnknownBlock96 = s.List(nameof(sheet.UnknownBlock96), sheet.UnknownBlock96, 13, (_, x, s2) => s2.UInt32(null, x));
            sheet.Combat.LifePoints = s.UInt16(nameof(sheet.Combat.LifePoints), sheet.Combat.LifePoints);
            sheet.Combat.LifePointsMax = s.UInt16(nameof(sheet.Combat.LifePointsMax), sheet.Combat.LifePointsMax);
            sheet.UnknownCE = s.UInt16(nameof(sheet.UnknownCE), sheet.UnknownCE);
            sheet.Magic.SpellPoints = s.UInt16(nameof(sheet.Magic.SpellPoints), sheet.Magic.SpellPoints);
            sheet.Magic.SpellPointsMax = s.UInt16(nameof(sheet.Magic.SpellPointsMax), sheet.Magic.SpellPointsMax);
            sheet.Combat.Protection = s.UInt16(nameof(sheet.Combat.Protection), sheet.Combat.Protection);
            sheet.UnknownD6 = s.UInt16(nameof(sheet.UnknownD6), sheet.UnknownD6);
            sheet.Combat.Damage = s.UInt16(nameof(sheet.Combat.Damage), sheet.Combat.Damage);

            sheet.UnknownDA = s.UInt16(nameof(sheet.UnknownDA), sheet.UnknownDA);
            sheet.UnknownDC = s.UInt16(nameof(sheet.UnknownDC), sheet.UnknownDC);
            sheet.UnknownDE = s.UInt32(nameof(sheet.UnknownDE), sheet.UnknownDE);
            sheet.UnknownE2 = s.UInt16(nameof(sheet.UnknownE2), sheet.UnknownE2);
            sheet.UnknownE4 = s.UInt16(nameof(sheet.UnknownE4), sheet.UnknownE4);
            sheet.UnknownE6 = s.UInt32(nameof(sheet.UnknownE6), sheet.UnknownE6);
            sheet.UnknownEA = s.UInt32(nameof(sheet.UnknownEA), sheet.UnknownEA);

            sheet.Combat.ExperiencePoints = s.UInt32(nameof(sheet.Combat.ExperiencePoints), sheet.Combat.ExperiencePoints);
            // e.g. 98406 = 0x18066 => 6680 0100 in file
            s.Check();

            byte[] knownSpellBytes = null;
            byte[] spellStrengthBytes = null;
            if (s.Mode != SerializerMode.Reading)
            {
                var activeSpellIds = sheet.Magic.SpellStrengths.Keys;
                var knownSpells = new uint[SpellSchoolCount];
                var spellStrengths = new ushort[MaxSpellsPerSchool * SpellSchoolCount];
                foreach (var spellId in activeSpellIds)
                {
                    uint schoolId = (uint)spellId / 100;
                    int offset = (int)spellId % 100;
                    if (sheet.Magic.SpellStrengths[spellId].Item1)
                        knownSpells[schoolId] |= 1U << offset + 1;
                    spellStrengths[schoolId * MaxSpellsPerSchool + offset] = sheet.Magic.SpellStrengths[spellId].Item2;
                }

                knownSpellBytes = knownSpells.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
                spellStrengthBytes = spellStrengths.Select(BitConverter.GetBytes).SelectMany(x => x).ToArray();
            }

            knownSpellBytes = s.ByteArray("KnownSpells", knownSpellBytes, SpellSchoolCount * sizeof(uint));
            s.Check();

            sheet.UnknownFA = s.UInt16(nameof(sheet.UnknownFA), sheet.UnknownFA);
            sheet.UnknownFC = s.UInt16(nameof(sheet.UnknownFC), sheet.UnknownFC);
            s.Check();

            sheet.GermanName = s.FixedLengthString(nameof(sheet.GermanName), sheet.GermanName, 16); // 112
            sheet.EnglishName = s.FixedLengthString(nameof(sheet.EnglishName), sheet.EnglishName, 16);
            sheet.FrenchName = s.FixedLengthString(nameof(sheet.FrenchName), sheet.FrenchName, 16);

            s.Check();

            spellStrengthBytes = s.ByteArray("SpellStrength", spellStrengthBytes, MaxSpellsPerSchool * SpellSchoolCount * sizeof(ushort));

            if (s.Mode == SerializerMode.Reading)
            {
                for (int school = 0; school < SpellSchoolCount; school++)
                {
                    byte knownSpells = 0;
                    for (int offset = 0; offset < MaxSpellsPerSchool; offset++)
                    {
                        if (offset % 8 == 0)
                            knownSpells = knownSpellBytes[school * 4 + offset / 8];
                        int i = school * MaxSpellsPerSchool + offset;
                        bool isKnown = (knownSpells & (1 << (offset % 8))) != 0;
                        ushort spellStrength = BitConverter.ToUInt16(spellStrengthBytes, i * sizeof(ushort));
                        var spellId = (SpellId)(school * 100 + offset);

                        if (spellStrength > 0)
                            sheet.Magic.SpellStrengths[spellId] = (false, spellStrength);

                        if (isKnown)
                        {
                            SpellId correctedSpellId = spellId - 1;
                            if (!sheet.Magic.SpellStrengths.TryGetValue(correctedSpellId, out var current))
                                current = (false, 0);
                            sheet.Magic.SpellStrengths[correctedSpellId] = (true, current.Item2);
                        }
                    }
                }
            }

            if (sheet.Type != CharacterType.Party)
            {
                ApiUtil.Assert(s.Offset - initialOffset == 742, "Expected non-player character sheet to be 742 bytes");
                s.End();
                return sheet;
            }

            s.Meta(nameof(sheet.Inventory), sheet.Inventory, Inventory.SerdesCharacter);

            ApiUtil.Assert(s.Offset - initialOffset == 940, "Expected player character sheet to be 940 bytes");
            s.End();
            return sheet;
        }

        public object Load(BinaryReader br, long streamLength, AssetKey key, AssetInfo config)
        {
            var sheet = Serdes(null, new AlbionReader(br, streamLength), key, config);
            return sheet;
        }
    }
}
