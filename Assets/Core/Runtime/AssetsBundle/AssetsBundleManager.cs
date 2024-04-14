using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic {
    public class AssetBundleManager {
        public static AssetBundleManager Instance {
            get {
                if(m_instance == null) {
                    m_instance = new AssetBundleManager();
                }
                return m_instance;
            }
        }

        private static AssetBundleManager m_instance;
        private Dictionary<string, AssetsBundleState> loadedBundles;
        private Dictionary<string, AssetBundleCreateRequest> loadingBundles;
        private List<string> unLoadBundle;

        private AssetBundleManager() {
            loadedBundles = new Dictionary<string, AssetsBundleState>(8);
            loadingBundles = new Dictionary<string, AssetBundleCreateRequest>(8);
            unLoadBundle = new List<string>();
        }

        public void Destory() {
            loadedBundles.Clear();
            loadingBundles.Clear();
            unLoadBundle.Clear();
            Resources.UnloadUnusedAssets();
        }

        public void Tick() {
            BundleTick();
        }

        public void LoadBundleAsync(string name) {
            loadingBundles.Add(name, AssetBundle.LoadFromFileAsync(Config.assetPath + name + Config.bundleExtend));
        }

        public AssetBundle LoadBundle(string name) {
            var ab = AssetBundle.LoadFromFile(Config.assetPath + name + Config.bundleExtend);
            AddAssetsBundle(name, ab);
            return ab;
        }

        public bool Contains(string name) {
            return loadedBundles.ContainsKey(name);
        }

        public bool TryGetBundle(string name,out AssetBundle ab) {
            if(loadedBundles.ContainsKey(name)) {
                ab = loadedBundles[name].ab;
                return true;
            }
            ab = null;
            return false;
        }

        public bool IsBundleLoading(string name) {
            return loadingBundles.ContainsKey(name);
        }

        public void AddReferenceCount(string name) {
            if(loadedBundles.TryGetValue(name,out var abState)) {
                abState.referenceCount++;
            }
        }

        public void RemoveReferenceCount(string name) {
            if(loadedBundles.TryGetValue(name, out var abState)) {
                abState.referenceCount = Mathf.Max(0, abState.referenceCount - 1);
            }
        }

        private void BundleTick() {
            // 检查 bundle 生命周期
            foreach(var key in loadedBundles.Keys) {
                var state = loadedBundles[key];
                if(state.referenceCount ==0 && state.lifeTime <= 1) {
                    state.ab.UnloadAsync(false);
                    unLoadBundle.Add(key);
                } else if(state.referenceCount == 0) {
                    loadedBundles[key].lifeTime -= 1;
                } else {
                    loadedBundles[key].lifeTime = Config.bundleLifeTime;
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
        }

        private bool AddAssetsBundle(string name, AssetBundle bundle) {
            if(loadedBundles.ContainsKey(name)) {
                return false;
            }

            loadedBundles.Add(name, new AssetsBundleState(bundle, Config.bundleLifeTime));
            return true;
        }
    }

    internal class AssetsBundleState {
        public AssetBundle ab;
        public List<string> deps;
        public int lifeTime;
        public int referenceCount;

        public AssetsBundleState(AssetBundle ab, int lifeTime) {
            deps = new List<string>();
            this.ab = ab;
            this.lifeTime = lifeTime;
            referenceCount = 1;
        }
    }
}
