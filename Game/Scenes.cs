﻿using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game
{
    public class Scenes
    {
        public static Scene Create2DScene(Assets assets, EventExchange allScenesExchange)
        {
            // TODO: Build scenes from config
            var id = SceneId.World2D;
            var camera = new OrthographicCamera();
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(SpriteRenderer),
            };

            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var scene = new Scene((int)id, camera, renderers, sceneExchange);
            allScenesExchange.Attach(scene);
            var cameraMotion = new CameraMotion2D(camera);
            sceneExchange
                .Attach(camera)
                .Attach(cameraMotion);
            return scene;
        }

        public static Scene Create3DScene(Assets assets, EventExchange allScenesExchange)
        {
            var id = SceneId.World3D;
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(ExtrudedTileMapRenderer),
                typeof(SpriteRenderer),
            };

            var camera = new PerspectiveCamera();
            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var scene = new Scene((int)SceneId.World3D, camera, renderers, sceneExchange);
            allScenesExchange.Attach(scene);
            var cameraMotion = new CameraMotion3D(camera);
            sceneExchange
                .Attach(camera)
                .Attach(cameraMotion);
            return scene;
        }
    }
}
