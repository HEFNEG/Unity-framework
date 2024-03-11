
using Unity.Entities;

namespace Game.Basic {
    public partial class GameSystem : SystemBase {
        protected override void OnCreate() {
            base.OnCreate();
            AppBootstrap.asset.Initialize();
            AppBootstrap.ui.Initialize();
        }
        protected override void OnUpdate() {
            AppBootstrap.asset.Tick();
            AppBootstrap.ui.Tick();
        }
    }
}
