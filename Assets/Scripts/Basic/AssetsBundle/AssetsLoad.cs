using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class AssetsLoad {
    private Dictionary<string, AssetsBundleState> loadedBundles;
    private Dictionary<string, AssetBundleCreateRequest> loadingBundles;
    private Dictionary<uint, AssetHandle> assetHandles;
    private uint nextId;

    public void Initialize() {
        loadedBundles = new Dictionary<string, AssetsBundleState>();
        loadingBundles = new Dictionary<string, AssetBundleCreateRequest>();
        assetHandles = new Dictionary<uint, AssetHandle>();
    }

    public void Destory() {
        loadedBundles.Clear();
        loadingBundles.Clear();
        assetHandles.Clear();
        Resources.UnloadUnusedAssets();
    }

    public void Tick() {
        // 检查 bundle 生命周期
        foreach(var key in loadedBundles.Keys) {
            var state = loadedBundles[key];
            if(state.lifeTime <= 1) {
                state.ab.UnloadAsync(false);
                loadedBundles.Remove(key);
            } else {
                state.lifeTime -= 1;
                loadedBundles[key] = state;
            }
        }
        // 检查 loading bundle 状态
        foreach(var key in loadingBundles.Keys) {
            var request = loadingBundles[key];
            if(request.isDone) {
                AddAssetsBundle(key, request.assetBundle);
                loadedBundles.Remove(key);
            }
        }

        // 检查 handle 状态
        foreach(var key in assetHandles.Keys) {
            var handle = assetHandles[key];
            if(handle.state == HandleState.None) {
                assetHandles.Remove(key);
            } else if(handle.state == HandleState.loadBundle && loadedBundles.TryGetValue(handle.bundleName, out var bundleState)) {
                handle.request = bundleState.ab.LoadAssetAsync(handle.assetName);
                handle.state = HandleState.LoadAsset;
            } else if(handle.state == HandleState.LoadAsset && handle.request != null && !handle.request.isDone) {
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
            ab = AssetBundle.LoadFromFile(AssetsConfig.path + bundleName + ".bundle");
            if(string.IsNullOrEmpty(ab.name)) {
                Debug.LogError(path + "bundle 加载失败");
                return default;
            }

            AddAssetsBundle(bundleName, ab);
        }

        var sp = ab.LoadAsset<T>(assetName);
        return sp;
    }

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
            handle.state = HandleState.loadBundle;
        } else {
            handle.state = HandleState.loadBundle;
            loadingBundles.Add(bundleName, AssetBundle.LoadFromFileAsync(AssetsConfig.path + bundleName + ".bundle"));
        }

        return handle;
    }

    internal bool AddAssetsBundle(string name, AssetBundle bundle) {
        if(loadedBundles.ContainsKey(name)) {
            return false;
        }

        loadedBundles.Add(name, new AssetsBundleState {
            ab = bundle,
            lifeTime = AssetsConfig.bundleLifeTime
        });
        return true;
    }

    internal void RefreshAssetsBundle(string name) {
        if(loadedBundles.ContainsKey(name)) {
            return;
        }
        loadedBundles[name].lifeTime = AssetsConfig.bundleLifeTime;
    }
}

public class AssetHandle {
    public uint id { get; }
    public string bundleName { get; }
    public string assetName { get; }
    public HandleState state { get; internal set; }
    public AssetBundleRequest request { get; internal set; }

    public bool isSuccessful { get { return request != null && request.isDone; } }

    public AssetHandle(uint id, string bundleName, string assetName) {
        this.id = id;
        this.bundleName = bundleName;
        this.assetName = assetName;
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
    loadBundle,
    LoadAsset,
}
internal class AssetsBundleState {
    public AssetBundle ab;
    public int lifeTime;
}


