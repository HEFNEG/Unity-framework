using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomlet;
using Tomlet.Models;
using UnityEngine;

public class UIManager : MonoBehaviour {
    private List<UIElement> uiPanel;
    private Dictionary<string, string> uiAssetPaths;

    private void Awake() {
        uiPanel = new List<UIElement>();
        DontDestroyOnLoad(this);
    }

    private void OnDestroy() {
        uiPanel.Clear();
    }

    public void Initialize() {
        uiAssetPaths = new Dictionary<string, string>(64);
        string line = File.ReadAllText(Config.assetPath + "config/ui.toml");
        var parse = new TomlParser();
        var tomlDocument = parse.Parse(line);
        var registerNames = tomlDocument.GetArray("ui");
        for(int i = 0; i < registerNames.Count; i++) {
            var ui = registerNames[0] as TomlTable;
            uiAssetPaths.Add(ui.GetString("key"), ui.GetString("value"));
        }
    }

    public void Tick() {
        for(int i = 0; i < uiPanel.Count; i++) {
            uiPanel[i].Tick();
        }
    }

    public UIElement Load(string path) {
        
        return null;
    }

    public void Open(string name) {

    }

    public void Close(string name) {

    }


    public T Qurey<T>(string name = "") where T : UIElement {
        for(int i = 0; i < uiPanel.Count; i++) {
            var child = uiPanel[i];
            if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                return (T)child;
            } else if(name == child.name && child.GetType() == typeof(T)) {
                return (T)child;
            }
        }

        for(int i = 0; i < uiPanel.Count; i++) {
            return uiPanel[i].Qurey<T>(name);
        }

        return null;
    }
}
