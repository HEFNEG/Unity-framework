using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetsLoad {
    private Dictionary<string, AssetsBundleState> loadedBundles;
    private Dictionary<string, AssetBundleCreateRequest> loadingBundles;
    private List<AssetHandle> assetHandles;
    private List<string> unLoadBundle;
    private uint nextId;

    public void Initialize() {
        loadedBundles = new Dictionary<string, AssetsBundleState>();
        loadingBundles = new Dictionary<string, AssetBundleCreateRequest>();
        assetHandles = new List<AssetHandle>(16);
        unLoadBundle = new List<string>(8);
    }

    public void Destory() {
        loadedBundles.Clear();
        loadingBundles.Clear();
        assetHandles.Clear();
        unLoadBundle.Clear();
        Resources.UnloadUnusedAssets();
    }

    public void Tick() {
        // 检查 bundle 生命周期
        foreach(var key in loadedBundles.Keys) {
            var state = loadedBundles[key];
            if(state.lifeTime <= 1) {
                state.ab.UnloadAsync(false);
                unLoadBundle.Add(key);
            } else {
                loadedBundles[key].lifeTime -= 1 ;
            }
        }
        for(int i = 0; i < unLoadBundle.Count; i++) {
            loadedBundles.Remove(unLoadBundle[i]);
        }
        unLoadBundle.Clear();
        //Debug.Log($"loaded bundle count :{loadedBundles.Count}");

        // 检查 loading bundle 状态
        foreach(var key in loadingBundles.Keys) {
            var request = loadingBundles[key];
            if(request.isDone) {
                AddAssetsBundle(key, request.assetBundle);
                unLoadBundle.Add(key);
            }
        }
        for(int i = 0; i < unLoadBundle.Count; i++) {
            loadingBundles.Remove(unLoadBundle[i]);
        }
        unLoadBundle.Clear();

        // 检查 handle 状态
        for(int i= assetHandles.Count-1; i>=0;i--) {
            var handle = assetHandles[i];
            if(handle.state == HandleState.None) {
                assetHandles.RemoveAt(i);
            } else if(handle.state == HandleState.LoadBundle && loadedBundles.TryGetValue(handle.bundleName, out var bundleState)) {
                // had loaded bundle
                LoadDepend(bundleState, true);
                handle.request = bundleState.ab.LoadAssetAsync(handle.assetName);
                handle.state = HandleState.LoadDeps;
            } else if(handle.state == HandleState.LoadDeps) {
                // try load dependent
                RefreshAssetsBundle(handle.bundleName);
                for(int j = 0; j < handle.deps.Count; j++) {
                    if(!loadedBundles.ContainsKey(handle.deps[j])) continue;
                    handle.request = loadedBundles[handle.bundleName].ab.LoadAssetAsync(handle.assetName);
                    handle.state = HandleState.LoadAsset;
                }
            } else if(handle.state == HandleState.LoadAsset && handle.request != null && !handle.request.isDone) {
                // loading asset
                RefreshAssetsBundle(handle.bundleName);
            }
        }
    }

    public T Load<T>(string path) where T : Object {
        var pathArg = path.Split(".bundle/");
        if(pathArg.Length != 2) {
            Debug.LogError(path + "路径不符合要求");
            return default;
        }

        string bundleName = pathArg[0];
        string assetName = pathArg[1];
        // 加载 AB 包
        AssetBundle ab;
        if(loadedBundles.ContainsKey(bundleName)) {
            RefreshAssetsBundle(bundleName);
            ab = loadedBundles[bundleName].ab;
        } else {
            ab = AssetBundle.LoadFromFile(Config.assetPath + bundleName + ".bundle");
            if(string.IsNullOrEmpty(ab.name)) {
                Debug.LogError(path + "bundle 加载失败");
                return default;
            }

            AddAssetsBundle(bundleName, ab);
            // Load or Refresh dependent
            LoadDepend(loadedBundles[bundleName]);
        }

        var sp = ab.LoadAsset<T>(assetName);
        var text = ab.LoadAsset<TextAsset>(Config.pkgFile);
        return sp;
    }

    /// <summary>
    /// 异步加载不支持加载依赖
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public AssetHandle LoadAsync(string path) {
        var pathArg = path.Split(".bundle/");
        if(pathArg.Length != 2) {
            Debug.LogError(path + "路径不符合要求");
            return default;
        }

        string bundleName = pathArg[0];
        string assetName = pathArg[1];

        uint id = nextId++;
        var handle = new AssetHandle(id, bundleName, assetName);
        if(loadedBundles.ContainsKey(bundleName)) {
            handle.state = HandleState.LoadAsset;
            handle.request = loadedBundles[bundleName].ab.LoadAssetAsync(assetName);
        } else if(loadingBundles.ContainsKey(bundleName)) {
            handle.state = HandleState.LoadBundle;
        } else {
            handle.state = HandleState.LoadBundle;
            loadingBundles.Add(bundleName, AssetBundle.LoadFromFileAsync(Config.assetPath + bundleName + ".bundle"));
        }

        return handle;
    }

    internal void LoadDepend(AssetsBundleState bundleState , bool isAsync = false) {
        var dependents = JsonMapper.ToObject(bundleState.ab.LoadAsset<TextAsset>(Config.pkgFile).text)["deps"];
        for(int i = 0; i < dependents.Count; i++) {
            var name = dependents[i].ValueAsString();
            bundleState.deps.Add(name);
            if(!isAsync && !loadedBundles.ContainsKey(name)) {
                AddAssetsBundle(name, AssetBundle.LoadFromFile(name));
            } else if(isAsync && !loadingBundles.ContainsKey(name)) {
                loadingBundles.Add(name, AssetBundle.LoadFromFileAsync(name));
            } else {
                RefreshAssetsBundle(name);
            }
        }
    }

    internal bool AddAssetsBundle(string name, AssetBundle bundle) {
        if(loadedBundles.ContainsKey(name)) {
            return false;
        }

        loadedBundles.Add(name, new AssetsBundleState(bundle, Config.bundleLifeTime));
        return true;
    }

    internal void RefreshAssetsBundle(string name) {
        if(loadedBundles.ContainsKey(name)) {
            return;
        }
        loadedBundles[name].lifeTime = Config.bundleLifeTime;
    }
}

public class AssetHandle {
    public uint id { get; }
    public string bundleName { get; }
    public string assetName { get; }
    public HandleState state { get; internal set; }
    public AssetBundleRequest request { get; internal set; }
    public List<string> deps;

    public bool isSuccessful { get { return request != null && request.isDone; } }

    public AssetHandle(uint id, string bundleName, string assetName) {
        this.id = id;
        this.bundleName = bundleName;
        this.assetName = assetName;
        deps = new List<string>();
    }

    public T GetAsset<T>() where T : Object {
        if(request == null) {
            return null;
        }

        T asset = request.asset as T;
        state = HandleState.None;
        request = default;
        return asset;
    }
}


public enum HandleState {
    None,
    LoadBundle,
    LoadDeps,
    LoadAsset,
}
internal class AssetsBundleState {
    public AssetBundle ab;
    public List<string> deps;
    public int lifeTime;
    public AssetsBundleState(AssetBundle ab, int lifeTime) {
        deps = new List<string>();
        this.ab = ab;
        this.lifeTime = lifeTime;
    }
}


