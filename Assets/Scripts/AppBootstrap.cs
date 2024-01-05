using System.IO;

using Unity.Entities;

using UnityEngine;
using UnityEngine.InputSystem;

public class AppBootstrap : ICustomBootstrap {

    public static World world { get; private set; }
    public static InputActionAsset input { get; private set; }

    public bool Initialize(string defaultWorldName) {
        Debug.Log("zzz");
        world = new World("default world");
        World.DefaultGameObjectInjectionWorld = world;
        string inputJson = string.Empty;
#if UNITY_EDITOR
        inputJson = File.ReadAllText(Application.dataPath + "/config/input.inputactions");
#endif
        input = InputActionAsset.FromJson(inputJson);
        input.Enable();
        return true;
    }

}