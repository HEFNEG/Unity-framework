
using Unity.Entities;

public partial class GameSystem : SystemBase {
    protected override void OnUpdate() {
        AppBootstrap.asset.Tick();
        AppBootstrap.ui.Tick();
    }
}
