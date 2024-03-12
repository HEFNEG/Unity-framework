using Game.Basic.UI;
using System.IO;

using Unity.Entities;

using UnityEngine;
using UnityEngine.InputSystem;
namespace Game.Basic {
    public class AppBootstrap : ICustomBootstrap {

        public static World world { get; private set; }
        public static InputActionAsset input { get; private set; }
        public static AssetsLoad asset { get; private set; }

        public static UIManager ui { get; private set; }

        public bool Initialize(string defaultWorldName) {
            string inputJson = string.Empty;
#if UNITY_EDITOR
            inputJson = File.ReadAllText(Application.dataPath + "/config/input.inputactions");
#endif
            input = InputActionAsset.FromJson(inputJson);
            input.Enable();

            asset = new AssetsLoad();

            var uiObject = new GameObject("UIManager");
            ui = uiObject.AddComponent<UIManager>();

            world = new World(defaultWorldName);
            Debug.Log(world.IsCreated);
            World.DefaultGameObjectInjectionWorld = world;
            var simlution = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
            simlution.AddSystemToUpdateList(world.CreateSystemManaged<GameSystem>());
            simlution.SortSystems();
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, typeof(GameSystem));

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            // 初始化另一个 world
            // DefaultWorldInitialization.Initialize("game world");
            return true;
        }
    }
}