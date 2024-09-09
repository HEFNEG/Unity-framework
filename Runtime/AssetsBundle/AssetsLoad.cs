using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Game.Basic.Console;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Game.Basic {
    public class AssetsLoad {
        public static AssetsLoad Instance {
            get {
                if(m_asset == null) {
                    m_asset = new AssetsLoad();
                    m_asset.Initialize();
                }
                return m_asset;
            }
        }

        private static AssetsLoad m_asset;
        private List<AssetHandle> assetHandles;
        private List<string> unLoadBundle;
        private AssetBundleManager ABManager;
        private uint nextId;

        private BundleLetterTree m_letterTree = new BundleLetterTree();

        private AssetsLoad() { }

        public void Initialize() {
            assetHandles = new List<AssetHandle>(16);
            unLoadBundle = new List<string>(8);
            m_asset.ABManager = AssetBundleManager.Instance;
            string[] allBundlePath = Directory.GetFiles(Config.assetPath, "*" + Config.bundleExtend,SearchOption.AllDirectories);
            for(int i = 0; i < allBundlePath.Length; i++) {
                var path = allBundlePath[i].Replace(Config.bundleExtend, "")
                    .Replace(Config.assetPath, "")
                    .Replace('\\', '/');
                m_letterTree.Insert(path);
            }
        }

        public void Destory() {
            assetHandles.Clear();
            unLoadBundle.Clear();
            Resources.UnloadUnusedAssets();
        }

        public void Tick() {
            HandleTick();
        }

        public T Load<T>(string path) where T : Object {
            /*var pathArg = path.Split(Config.bundleExtend + "/");
            if(pathArg.Length != 2) {
                Debug.LogError(path + "路径不符合要求");
                return default;
            }

            string bundleName = pathArg[0];
            string assetName = pathArg[1];*/
            if(!m_letterTree.TryFind(path, out var bundleName, out var assetName)) {
                Console.Console.Instance.Output($"{path} is Error,请检查资源路径",Message.Error);
                return null;
            }
            
            // 加载 AB 包
            AssetBundle ab = null;
            if(!ABManager.TryGetBundle(bundleName, out ab)) {
                ab = ABManager.LoadBundle(bundleName);
                if(string.IsNullOrEmpty(ab.name)) {
                    Debug.LogError(path + "bundle 加载失败");
                    return default;
                }
                LoadDepend(new AssetHandle(0, bundleName, string.Empty));
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
        public void LoadAsync(string path,Action<AssetHandle> OnSuccess = null) {
            /*var pathArg = path.Split(Config.bundleExtend + "/");
            if(pathArg.Length != 2) {
                Debug.LogError(path + "路径不符合要求");
                return default;
            }

            string bundleName = pathArg[0];
            string assetName = pathArg[1];*/

            if(!m_letterTree.TryFind(path, out var bundleName, out var assetName)) {
                Console.Console.Instance.Output($"{path} is Error,请检查资源路径",Message.Error);
                return ;
            }
            
            uint id = nextId++;
            var handle = new AssetHandle(id, bundleName, assetName,OnSuccess);
            assetHandles.Add(handle);
            if(ABManager.TryGetBundle(bundleName, out var ab) || ABManager.IsBundleLoading(bundleName)) {
                handle.state = HandleState.LoadBundle;
            } else {
                handle.state = HandleState.LoadBundle;
                ABManager.LoadBundleAsync(bundleName);
            }
        }

        private void HandleTick() {
            for(int i = assetHandles.Count - 1; i >= 0; i--) {
                var handle = assetHandles[i];
                if(handle.isSuccessful) {
                    handle.CheckSuccess();
                }
                
                if(handle.state == HandleState.None) {
                    assetHandles.RemoveAt(i);
                } else if(handle.state == HandleState.LoadBundle && ABManager.TryGetBundle(handle.bundleName, out var ab)) {
                    // had loaded bundle
                    ABManager.AddReferenceCount(handle.bundleName);
                    LoadDepend(handle, true);
                    handle.state = HandleState.LoadDeps;
                } else if(handle.state == HandleState.LoadDeps) {
                    // try load dependent
                    bool isReady = true;
                    for(int j = 0; j < handle.deps.Count; j++) {
                        if(!ABManager.Contains(handle.deps[j])) {
                            isReady = false;
                            break;
                        }
                    }
                    if(isReady && ABManager.TryGetBundle(handle.bundleName, out var ab2)) {
                        handle.request = ab2.LoadAssetAsync(handle.assetName);
                        handle.state = HandleState.LoadAsset;
                    }
                }
            }
        }

        private void LoadDepend(AssetHandle assetHandle, bool isAsync = false) {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(assetHandle.bundleName);
            while(queue.Count > 0) {
                JsonData jsonData = JsonMapper.ToObject(System.IO.File.ReadAllText(Config.assetPath + queue.Dequeue() + Config.depExtend));
                var deps = jsonData["deps"];
                for(int i = 0; i < deps.Count; i++) {
                    string depBundle = deps[i].ToString();
                    if(!string.IsNullOrEmpty(depBundle)) {
                        if(isAsync) {
                            assetHandle.deps.Add(depBundle);
                            ABManager.AddReferenceCount(depBundle);
                        }

                        if(ABManager.Contains(depBundle) || ABManager.IsBundleLoading(depBundle)) {
                            continue;
                        }

                        if(!isAsync) {
                            ABManager.LoadBundle(depBundle);
                        } else {
                            ABManager.LoadBundleAsync(depBundle);
                        }
                        queue.Enqueue(depBundle);
                    }
                }
            }
        }


    }

    public class AssetHandle {
        public uint id { get; }
        public string bundleName { get; }
        public string assetName { get; }
        public HandleState state { get; internal set; }
        public AssetBundleRequest request { get; internal set; }
        
        public List<string> deps;
        
        private  Action<AssetHandle> m_OnSuccess;
        public bool isSuccessful { get { return request != null && request.isDone; } }

        public AssetHandle(uint id, string bundleName, string assetName,Action<AssetHandle> OnSuccess = null) {
            this.id = id;
            this.bundleName = bundleName;
            this.assetName = assetName;
            deps = new List<string>();
            m_OnSuccess = OnSuccess;
        }

        public T GetAsset<T>() where T : Object {
            if(request == null) {
                return null;
            }

            T asset = request.asset as T;
            state = HandleState.None;
            request = default;
            RemoveReference();
            return asset;
        }

        public void CheckSuccess() {
            if(isSuccessful && m_OnSuccess!=null) {
                m_OnSuccess(this);
                state = HandleState.None;
                request = default;
                RemoveReference();
            }
        }
        
        private void RemoveReference() {
            var abManger = AssetBundleManager.Instance;
            abManger.RemoveReferenceCount(bundleName);
            for(int i = 0; i < deps.Count; i++) {
                abManger.RemoveReferenceCount(deps[i]);
            }
        }
    }

    public enum HandleState {
        None,
        LoadBundle,
        LoadDeps,
        LoadAsset,
    }
}