﻿using Game.Basic.UI;
using System.IO;
using Unity.Entities;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace Game.Basic {
    public class AppBootstrap : ICustomBootstrap {

        public static World world { get; private set; }
        public static Game.Basic.InputManager inputMgr { get; private set; }
        public static AssetsLoad asset { get; private set; }

        public static UIManager ui { get; private set; }

        public static Console.Console console { get; private set; }

        public static EventManager eventMgr { get; private set; }

        public bool Initialize(string defaultWorldName) {
            string inputJson = string.Empty;

            inputMgr = new InputManager();
            inputMgr.Initialize(System.IO.File.ReadAllText(Config.assetPath + "config/input.inputactions"));

            asset = AssetsLoad.Instance;

            var consolePanel = GameObject.Instantiate(Resources.Load<GameObject>("Console/Console"));
            console = consolePanel.GetComponent<Console.Console>();
            consolePanel.SetActiveEx(false);

            var uiObject = new GameObject("UIManager");
            ui = uiObject.AddComponent<UIManager>();

            eventMgr = new EventManager();

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