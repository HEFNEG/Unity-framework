using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour{
    private List<UIElement> uiElements;

    private void Awake() {
        uiElements = new List<UIElement>();
        DontDestroyOnLoad(this);
    }

    private void OnDestroy() {
        uiElements.Clear();
    }

    public void Tick() {
        for(int i = 0; i < uiElements.Count; i++) {
            uiElements[i].Tick();
        }
    }

    public UIElement Load(string path) {
        
        return null;
    }

    public T Qurey<T>(string name = "") where T : UIElement {
        for(int i = 0; i < uiElements.Count; i++) {
            var child = uiElements[i];
            if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                return (T)child;
            } else if(name == child.name && child.GetType() == typeof(T)) {
                return (T)child;
            }
        }

        for(int i = 0; i < uiElements.Count; i++) {
            return uiElements[i].Qurey<T>(name);
        }

        return null;
    }
}
