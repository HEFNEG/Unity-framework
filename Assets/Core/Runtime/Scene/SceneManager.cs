using Game.Basic.Console;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Game.Basic {
    public class SceneManager {
        public static SceneManager Instance {
            get {
                if(m_instance == null) {
                    m_instance = new SceneManager();
                    m_instance.handles = new List<SceneLoadHandle>(2);
                }
                return m_instance;
            }
        }
        private static SceneManager m_instance;
        private List<SceneLoadHandle> handles;

        public bool LoadScene(string path) {
            string[] splits = path.Split(Config.bundleExtend + "/");
            if(splits.Length != 2) {
                Debug.LogError("scene path error");
                return false;
            }
            AssetBundleManager.Instance.LoadBundleAsync(splits[0]);
            handles.Add(new SceneLoadHandle {
                sceneName = splits[1],
                bundleName = splits[0],
                state = SceneLoadState.LoadBundle
            });
            return true;

        }

        public void UnLoadScene(string scene) {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }

        public void Tick() {
            for(int i = handles.Count - 1; i >= 0; i--) {
                var handle = handles[i];
                switch(handle.state) {
                    case SceneLoadState.LoadBundle:
                        if(AssetBundleManager.Instance.Contains(handle.bundleName)) {
                            handle.state = SceneLoadState.LoadScene;
                            handle.operation = USceneManager.LoadSceneAsync(handle.sceneName, LoadSceneMode.Additive);
                            handle.operation.allowSceneActivation = true;
                            AssetBundleManager.Instance.AddReferenceCount(handle.bundleName);
                            handles[i] = handle;
                        }

                        break;
                    case SceneLoadState.LoadScene:
                        if(handle.operation != null && handle.operation.isDone) {
                            handle.state = SceneLoadState.None;
                            USceneManager.SetActiveScene(USceneManager.GetSceneByName(handle.sceneName));
                            handles[i] = handle;
                        }
                        break;
                    case SceneLoadState.None:
                        handles.RemoveAt(i);
                        break;

                }
            }
        }
    }

    public struct SceneLoadHandle {
        public string sceneName;
        public string bundleName;
        public SceneLoadState state;
        public AsyncOperation operation;
    }

    public enum SceneLoadState {
        None,
        LoadBundle,
        LoadScene
    }
}
