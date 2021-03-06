﻿using System;
using System.IO;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.State
{
    public class GameState : ServiceComponent<IGameState>, IGameState
    {
        SavedGame _game;
        Party _party;

        public DateTime Time => SavedGame.Epoch + _game.ElapsedTime;
        IParty IGameState.Party => _party;
        public ICharacterSheet GetNpc(NpcCharacterId id) => _game != null && _game.NpcStats.TryGetValue(id, out var sheet) ? sheet : null;
        public ICharacterSheet GetPartyMember(PartyCharacterId id) => _game != null && _game.PartyMembers.TryGetValue(id, out var member) ? member : null;
        public short GetTicker(TickerId id) => _game != null && _game.Tickers.TryGetValue(id, out var value) ? value : (short)0;
        public bool GetSwitch(SwitchId id) => _game != null && _game.Switches.TryGetValue(id, out var value) && value;

        public MapChangeList TemporaryMapChanges => _game.TemporaryMapChanges;
        public MapChangeList PermanentMapChanges => _game.PermanentMapChanges;
        public MapDataId MapId => _game?.MapId ?? 0;

        public GameState()
        {
            On<NewGameEvent>(e => NewGame(e.MapId, e.X, e.Y));
            On<LoadGameEvent>(e => LoadGame(e.Id));
            On<SaveGameEvent>(e => SaveGame(e.Id, e.Name));
            On<FastClockEvent>(e => TickCount += e.Frames);
            On<SetTimeEvent>(e =>
            {
                if (_game != null)
                    _game.ElapsedTime = e.Time - SavedGame.Epoch;
            });
            On<LoadMapEvent>(e =>
            {
                if (_game != null)
                    _game.MapId = e.MapId;
            });
            On<SetTemporarySwitchEvent>(e => _game.Switches[e.SwitchId] = e.Operation switch
            {
                SetTemporarySwitchEvent.SwitchOperation.Reset => false,
                SetTemporarySwitchEvent.SwitchOperation.Set => true,
                SetTemporarySwitchEvent.SwitchOperation.Toggle => _game.Switches.ContainsKey(e.SwitchId) ? !_game.Switches[e.SwitchId] : true,
                _ => false
            });
            On<SetTickerEvent>(e =>
            {
                _game.Tickers.TryGetValue(e.TickerId, out var curValue);
                _game.Tickers[e.TickerId] = (byte)e.Operation.Apply(curValue, e.Amount, 0, 255);
            });
            On<ChangeTimeEvent>(e => { });

            AttachChild(new InventoryManager(GetInventory));
        }

        IInventory IGameState.GetInventory(InventoryId id)
        {
            if (id.Type == InventoryType.Player)
            {
                var player = _party[(PartyCharacterId)id.Id];
                if (player != null)
                    return player.Apparent.Inventory;
            }

            return GetInventory(id);
        }

        Inventory GetInventory(InventoryId id)
        {
            if (_game == null)
                return null;

            Inventory inventory;
            switch(id.Type)
            {
                case InventoryType.Player:
                    inventory = _game.PartyMembers.TryGetValue((PartyCharacterId)id.Id, out var member) ? member.Inventory : null;
                    break;
                case InventoryType.Chest: _game.Chests.TryGetValue((ChestId)id.Id, out inventory); break;
                case InventoryType.Merchant: _game.Merchants.TryGetValue((MerchantId)id.Id, out inventory); break;
                default:
                    throw new InvalidOperationException($"Unexpected inventory type requested: \"{id.Type}\"");
            }

            return inventory;
        }

        public int TickCount { get; private set; }
        public bool Loaded => _game != null;

        void NewGame(MapDataId mapId, ushort x, ushort y)
        {
            var assets = Resolve<IAssetManager>();
            _game = new SavedGame
            {
                MapId = mapId,
                PartyX = x,
                PartyY = y,
                ActiveMembers = { [0] = PartyCharacterId.Tom }
            };

            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                _game.PartyMembers.Add(charId, assets.LoadPartyMember(charId));

            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                _game.NpcStats.Add(charId, assets.LoadNpc(charId));

            foreach(ChestId id in Enum.GetValues(typeof(ChestId)))
                _game.Chests.Add(id, assets.LoadChest(id));

            foreach(MerchantId id in Enum.GetValues(typeof(MerchantId)))
                _game.Merchants.Add(id, assets.LoadMerchant(id));

            InitialiseGame();
        }

        void LoadGame(ushort id)
        {
            _game = Resolve<IAssetManager>().LoadSavedGame(id);
            InitialiseGame();
        }

        void SaveGame(ushort id, string name)
        {
            if (_game == null)
                return;

            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            _game.Name = name;

            for (int i = 0; i < SavedGame.MaxPartySize; i++)
                _game.ActiveMembers[i] = _party.StatusBarOrder.Count > i
                    ? _party.StatusBarOrder[i].Id
                    : (PartyCharacterId?)null;

            var key = new AssetKey(AssetType.SavedGame, id);
            var generalConfig = Resolve<IAssetManager>().LoadGeneralConfig();
            var filename = Path.Combine(generalConfig.BasePath, generalConfig.SavePath, $"SAVE.{key.Id:D3}");

            using var stream = File.Open(filename, FileMode.Create);
            using var bw = new BinaryWriter(stream);
            loader.Serdes(_game, new AlbionWriter(bw), key, null);
        }

        void InitialiseGame()
        {
            _party?.Remove();
            _party = AttachChild(new Party(_game.PartyMembers, _game.ActiveMembers, GetInventory));
            Raise(new LoadMapEvent(_game.MapId));
            Raise(new StartClockEvent());
            Raise(new SetContextEvent(ContextType.Leader, AssetType.PartyMember, (int)_party.Leader));
            Raise(new PartyChangedEvent());
            Raise(new PartyJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new CameraJumpEvent(_game.PartyX, _game.PartyY));
            Raise(new PlayerEnteredTileEvent(_game.PartyX, _game.PartyY));
        }
    }
}
