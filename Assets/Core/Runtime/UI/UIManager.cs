using System.Collections.Generic;
using System.IO;
using Tomlet;
using Tomlet.Models;
using UnityEngine;

namespace Game.Basic.UI {

    public class UIManager : MonoBehaviour {
        private List<UIPanel> uiPanels;
        private List<UIElementHandle> uihandles;
        private List<UIPanel> popPanels;
        private List<UIEvent> uiEvents;
        private Dictionary<string, string> uiAssetPaths;

        private void Awake() {
            uiPanels = new List<UIPanel>(16);
            uihandles = new List<UIElementHandle>(8);
            popPanels = new List<UIPanel>(8);
            uiEvents = new List<UIEvent>(8);
            DontDestroyOnLoad(this);
        }

        private void OnDestroy() {
            uiPanels.Clear();
            uihandles.Clear();
            popPanels.Clear();
            uiEvents.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiToml">
        /// ui.toml
        /// -----------------------------------------
        /// [[ui]]
        /// name = "ui/test"
        /// path = "ui.bundle/uitest.prefab"
        /// 
        /// [[ui]]
        /// name = "ui/test2"
        /// path = "ui.bundle/uitest.prefab2"
        /// 
        /// .....
        /// </param>
        public void Initialize(string uiToml = "") {
            uiAssetPaths = new Dictionary<string, string>(64);
            if(string.IsNullOrEmpty(uiToml)) {
                return;
            }
            var parse = new TomlParser();
            var tomlDocument = parse.Parse(uiToml);
            var registerNames = tomlDocument.GetArray("ui");
            for(int i = 0; i < registerNames.Count; i++) {
                var ui = registerNames[0] as TomlTable;
                RegisterUI(ui.GetString("name"), ui.GetString("path"));
            }
        }

        public void RegisterUI(string name, string path) {
            uiAssetPaths.Add(name, path);
        }

        public void Tick() {
            for(int i = 0; i < uiPanels.Count; i++) {
                uiPanels[i].Tick();
            }

            TickHandle();
            TickEvent();
        }

        public UIElementHandle Load(string name) {
            if(uiAssetPaths.TryGetValue(name, out var path)) {
                return new UIElementHandle {
                    assetHandle = AssetsLoad.Instance.LoadAsync(path),
                };
            }

            return default;
        }

        public void Open(string name) {
            for(int i = 0; i < uiPanels.Count; i++) {
                var panel = uiPanels[i];
                if(panel.name == name) {
                    panel.gameObject.SetActiveEx(true);
                    if(panel.isPop) {
                        popPanels.Add(panel);
                    }
                    return;
                }
            }
            if(uiAssetPaths.TryGetValue(name, out var path)) {
                uihandles.Add(new UIElementHandle {
                    name = name,
                    assetHandle = AssetsLoad.Instance.LoadAsync(path),

                });
            }
        }

        public void Close(string name) {
            for(int i = 0; i < uiPanels.Count; i++) {
                var panel = uiPanels[i];
                if(panel.name == name) {
                    panel.gameObject.SetActiveEx(false);
                    popPanels.Remove(panel);
                    return;
                }
            }
        }

        public T Qurey<T>(string name = "") where T : UIElement {
            for(int i = 0; i < uiPanels.Count; i++) {
                var child = uiPanels[i];
                if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                    return child as T;
                } else if(name == child.name && child.GetType() == typeof(T)) {
                    return child as T;
                }
            }

            for(int i = 0; i < uiPanels.Count; i++) {
                return uiPanels[i].Qurey<T>(name);
            }

            return null;
        }

        public void Dispatch(UIEvent uiEvent) {
            uiEvents.Add(uiEvent);
        }

        private void TickHandle() {
            for(int i = 0; i < uihandles.Count; i++) {
                var handle = uihandles[i];
                if(handle.assetHandle.isSuccessful) {
                    var panel = Instantiate(handle.assetHandle.GetAsset<GameObject>()).GetComponent<UIPanel>();
                    panel.name = handle.name;
                    panel.transform.SetParent(this.transform);
                    uiPanels.Add(panel);
                    if(panel.isPop) {
                        popPanels.Add(panel);
                    }

                    uihandles.Remove(handle);
                }
            }
        }

        private void TickEvent() {
            for(int i = 0; i < uiEvents.Count; i++) {
                var sender = uiEvents[i].sender;
                var target = sender.parent;
                while(target != null) {
                    target.InvokeCallBack(uiEvents[i]);
                    target = target.parent;
                }
            }
            uiEvents.Clear();
        }
    }

    public struct UIElementHandle {
        public string name;
        public AssetHandle assetHandle;
    }
}
