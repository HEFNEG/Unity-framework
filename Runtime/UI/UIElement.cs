using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic.UI {
    public class UIElement : MonoBehaviour {
        public RectTransform RectTransform => transform as RectTransform;
        public UIElement parent { get; private set; }
        private List<UIElement> children = new List<UIElement>();
        private event Action<UIEvent> eventCallBack;
        protected UIManager uiMananger;
        private bool isValid = false;

        private void Awake() {
            OnTransformParentChanged();
        }

        private void OnEnable() {
            if(isValid) {
                OnShow();
            }
        }

        private void Start() {
            OnInitialize();
            OnShow();
            isValid = true;
        }

        private void OnDisable() {
            OnHide();
        }

        private void OnBeforeTransformParentChanged() {
            if(this.parent == null) {
                return;
            }
            this.parent?.RemoveChild(this);
        }

        private void OnTransformParentChanged() {
            var currentParent = RectTransform.parent;
            bool isFind = false;
            while(currentParent != null) {
                if(!isFind && currentParent.TryGetComponent<UIElement>(out var element)) {
                    element.AddChild(this);
                    parent = element;
                    isFind = true;
                } else if(currentParent.TryGetComponent<UIManager>(out var manager)) {
                    uiMananger = manager;
                    break;
                }
                currentParent = currentParent.parent;
            }
        }
        private void AddChild(UIElement element) {
            children.Add(element);
        }

        private void RemoveChild(UIElement element) {
            children.Remove(element);
        }

        private void TickChild() {
            for(int i = 0; i < children.Count; i++) {
                if(children[i].isActiveAndEnabled) {
                    children[i].Tick();
                }
            }
        }

        public T Query<T>(string name = "") where T : UIElement {
            for(int i = 0; i < children.Count; i++) {
                var child = children[i];
                if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                    return child as T;
                } else if(name == child.name && child.GetType() == typeof(T)) {
                    return child as T;
                }
            }

            for(int i = 0; i < children.Count; i++) {
                return children[i].Query<T>(name);
            }

            return null;
        }

        public void QueryAll<T>(List<T> list, string name = "") where T : UIElement {
            for(int i = 0; i < children.Count; i++) {
                var child = children[i];
                if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                    list.Add(child as T);
                } else if(name == child.name && child.GetType() == typeof(T)) {
                    list.Add(child as T);
                }
            }

            for(int i = 0; i < children.Count; i++) {
                children[i].QueryAll<T>(list, name);
            }
        }

        public void AddListener(Action<UIEvent> action) {
            eventCallBack += action;
        }

        public void RemoveListener(Action<UIEvent> action) {
            eventCallBack -= action;
        }

        public void RemoveAllListener() {
            eventCallBack = null;
        }

        public void InvokeCallBack(UIEvent uiEvent) {
            eventCallBack?.Invoke(uiEvent);
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
}
