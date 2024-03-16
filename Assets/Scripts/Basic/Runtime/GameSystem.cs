
using Game.Basic.Console;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Basic {
    public partial class GameSystem : SystemBase {
        protected override void OnCreate() {
            base.OnCreate();
            AppBootstrap.asset.Initialize();
            AppBootstrap.ui.Initialize();

            ConsoleCommandBasic.RegisteredWaitHandle();
        }
        protected override void OnUpdate() {
            AppBootstrap.asset.Tick();
            AppBootstrap.ui.Tick();
            AppBootstrap.console.Tick();

            if(Input.GetKeyDown(KeyCode.F12)) {
                AppBootstrap.console.SwitchActive();
            }
        }
    }
}
