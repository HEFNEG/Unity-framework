
using Game.Basic.Console;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Basic {
    public partial class GameSystem : SystemBase {
        protected override void OnCreate() {
            base.OnCreate();
            AppBootstrap.asset.Initialize();
            string line = System.IO.File.ReadAllText(Config.assetPath + "config/ui.toml");
            AppBootstrap.ui.Initialize(line);
            AppBootstrap.ui.Open("ui/test");

            AppBootstrap.eventMgr.Initialize();

            ConsoleCommandBasic.RegisteredWaitHandle();
        }
        protected override void OnUpdate() {
            AppBootstrap.asset.Tick();
            AppBootstrap.ui.Tick();
            AppBootstrap.console.Tick();
            AppBootstrap.eventMgr.Tick();

            if(Input.GetKeyDown(KeyCode.F12)) {
                AppBootstrap.console.SwitchActive();
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            AppBootstrap.asset.Destory();
            AppBootstrap.eventMgr.Destory();
        }
    }
}
