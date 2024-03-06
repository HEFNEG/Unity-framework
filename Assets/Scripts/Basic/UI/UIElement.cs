using System.Collections.Generic;
using UnityEngine;

public class UIElement : MonoBehaviour {
    public RectTransform RectTransform => transform as RectTransform;
    private List<UIElement> children;

    private void Awake() {
        children = new List<UIElement>();
        OnTransformParentChanged();
        OnInitialize();
    }

    private void OnEnable() {
        OnShow();
    }

    private void OnDisable() {
        OnHide();
    }

    private void OnBeforeTransformParentChanged() {
        if(RectTransform.parent == null) {
            return;
        }
        var parent =  RectTransform.parent.GetComponent<UIElement>();
        parent?.RemoveChild(this);
    }

    private void OnTransformParentChanged() {
        if(RectTransform.parent == null) {
            return;
        }
        var parent = RectTransform.parent.GetComponent<UIElement>();
        parent?.AddChild(this);
    }

    private void AddChild(UIElement element) {
        children.Add(element);
    }

    private void RemoveChild(UIElement element) {
        children.Remove(element);
    }

    private void TickChild() {
        for(int i = 0; i < children.Count; i++) {
            children[i].Tick();
        }
    }

    public T Qurey<T>(string name = "") where T : UIElement {
        for(int i = 0; i < children.Count; i++) {
            var child = children[i];
            if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                return (T)child;
            }else if(name == child.name && child.GetType() == typeof(T)) {
                return (T)child;
            }
        }

        for(int i = 0; i < children.Count; i++) {
            return children[i].Qurey<T>(name);
        }

        return null;
    }

    public virtual void OnInitialize() {

    }

    public virtual void OnShow() {

    }

    public virtual void OnHide() {

    }

    public virtual void Tick() {
        TickChild();
    }
}
