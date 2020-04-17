﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Map;
using UAlbion.Game.Events;
using Object = UAlbion.Formats.Assets.Labyrinth.Object;

namespace UAlbion.Game.Entities.Map3D
{
    public class Map : Component, IMap
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Map, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
            H<Map, MapInitEvent>((x, e) => x.FireEventChains(TriggerType.MapInit, true)),
            H<Map, SlowClockEvent>((x, e) => x.FireEventChains(TriggerType.EveryStep, false)),
            H<Map, HourElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryHour, false)),
            H<Map, DayElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryDay, false))
            // H<Map3D, UnloadMapEvent>((x, e) => x.Unload()),
        );

        readonly MapData3D _mapData;
        LabyrinthData _labyrinthData;
        float _backgroundRed;
        float _backgroundGreen;
        float _backgroundBlue;

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
            // TODO
        }

        public Map(MapDataId mapId, MapData3D mapData) : base(Handlers)
        {
            MapId = mapId;
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        public override string ToString() => $"Map3D:{MapId} {LogicalSize.X}x{LogicalSize.Y} TileSize: {TileSize}";
        public MapDataId MapId { get; }
        public MapType MapType => MapType.ThreeD;
        public IMapData MapData => _mapData;
        public Vector2 LogicalSize { get; private set; }
        public Vector3 TileSize { get; private set; }
        public float BaseCameraHeight => (_labyrinthData?.CameraHeight ?? 0) != 0 ? _labyrinthData.CameraHeight * 8 : TileSize.Y / 2;

        public override void Subscribed()
        {
            if (_labyrinthData != null)
                return;

            var assets = Resolve<IAssetManager>();
            _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);

            if (_labyrinthData == null)
            {
                TileSize = Vector3.One * 512;
                return;
            }

            TileSize = new Vector3(_labyrinthData.EffectiveWallWidth, _labyrinthData.WallHeight, _labyrinthData.EffectiveWallWidth);
            AttachChild(new MapRenderable3D(MapId, _mapData, _labyrinthData, TileSize));
            AttachChild(new ScriptManager(_mapData.Id));

            if (_labyrinthData.BackgroundId.HasValue)
                AttachChild(new Skybox(_labyrinthData.BackgroundId.Value));

            var palette = assets.LoadPalette(_mapData.PaletteId);
            uint backgroundColour = palette.GetPaletteAtTime(0)[_labyrinthData.BackgroundColour];
            _backgroundRed = (backgroundColour & 0xff) / 255.0f;
            _backgroundGreen = (backgroundColour & 0xff00 >> 8) / 255.0f;
            _backgroundBlue = (backgroundColour & 0xff0000 >> 16) / 255.0f;

            //if(_labyrinthData.CameraHeight != 0)
            //    Debugger.Break();

            //if(_labyrinthData.Unk12 != 0) // 7=1|2|4 (Jirinaar), 54=32|16|4|2, 156=128|16|8|2 (Tall town)
            //    Debugger.Break();

            var maxObjectHeightRaw = _labyrinthData.ObjectGroups.Max(x => x.SubObjects.Max(y => (int?)y.Y));
            float objectYScaling = TileSize.Y / _labyrinthData.WallHeight;
            if (maxObjectHeightRaw > _labyrinthData.WallHeight * 1.5f)
                objectYScaling /= 2; // TODO: Figure out the proper way to handle this.

            Raise(new LogEvent(LogEvent.Level.Info, $"WallHeight: {_labyrinthData.WallHeight} MaxObj: {maxObjectHeightRaw} EffWallWidth: {_labyrinthData.EffectiveWallWidth}"));

            foreach (var npc in _mapData.Npcs.Values)
            {
                var objectData = _labyrinthData.ObjectGroups[npc.ObjectNumber - 1];
                foreach (var subObject in objectData.SubObjects)
                {
                    // TODO: Build proper NPC objects with AI, sound effects etc
                    var sprite = BuildMapObject(npc.Waypoints[0].X, npc.Waypoints[0].Y, subObject, objectYScaling);
                    if (sprite == null)
                        continue;

                    AttachChild(sprite);
                }
            }

            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    var contents = _mapData.Contents[y * _mapData.Width + x];
                    if (contents == 0 || contents >= _labyrinthData.ObjectGroups.Count)
                        continue;

                    var objectInfo = _labyrinthData.ObjectGroups[contents - 1];
                    foreach (var subObject in objectInfo.SubObjects)
                    {
                        var sprite = BuildMapObject(x, y, subObject, objectYScaling);
                        if (sprite == null)
                            continue;

                        AttachChild(sprite);
                    }
                }
            }

            Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue));
        }

        IEnumerable<MapEventZone> GetZonesOfType(TriggerType triggerType)
        {
            var matchingKeys = _mapData.ZoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
            return matchingKeys.SelectMany(x => _mapData.ZoneTypeLookup[x]);
        }

        void FireEventChains(TriggerType type, bool log)
        {
            var zones = GetZonesOfType(type);
            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Warning));

            foreach (var zone in zones)
                Raise(new TriggerChainEvent(zone.Chain, zone.Node, type, zone.X, zone.Y));

            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Info));
        }

        MapObject BuildMapObject(int tileX, int tileY, SubObject subObject, float objectYScaling)
        {
            var definition = _labyrinthData.Objects[subObject.ObjectInfoNumber];
            if (definition.TextureNumber == null)
                return null;

            bool onFloor = (definition.Properties & Object.ObjectFlags.FloorObject) != 0;

            // We should probably be offsetting the main tilemap by half a tile to centre the objects
            // rather than fiddling with the object positions... will need to reevaluate when working on
            // collision detection, path-finding etc.
            var objectBias = new Vector3(-1.0f, 0, -1.0f) / 2;
                /*
                (MapId == MapDataId.Jirinaar3D || MapId == MapDataId.AltesFormergebäude || MapId == MapDataId.FormergebäudeNachKampfGegenArgim)
                    ? new Vector3(-1.0f, 0, -1.0f) / 2
                    : new Vector3(-1.0f, 0, -1.0f); // / 2;
                */

            var tilePosition = (new Vector3(tileX, 0, tileY) + objectBias) * TileSize;
            var offset = new Vector3(
                subObject.X,
                subObject.Y * objectYScaling,
                subObject.Z);

            var smidgeon = onFloor
                ? new Vector3(0,offset.Y < float.Epsilon ? 0.5f : -0.5f, 0)
                : Vector3.Zero;

            Vector3 position = (tilePosition + offset + smidgeon) / TileSize;

            return new MapObject(
                definition.TextureNumber.Value,
                position,
                new Vector2(definition.MapWidth, definition.MapHeight),
                (definition.Properties & Object.ObjectFlags.FloorObject) != 0
            );
        }
    }
}
