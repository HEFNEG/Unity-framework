using System.IO;

using Unity.Entities;

using UnityEngine;
using UnityEngine.InputSystem;

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
        asset.Initialize();

        var uiObject = new GameObject("UIManager");
        ui = uiObject.AddComponent<UIManager>();

        world = new World(defaultWorldName);
        Debug.Log(world.IsCreated);
        World.DefaultGameObjectInjectionWorld = world;
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, typeof(GameSystem));

        // 初始化另一个 world
        // DefaultWorldInitialization.Initialize("game world");
        return true;
    }
}