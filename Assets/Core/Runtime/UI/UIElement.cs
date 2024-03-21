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

        private void Awake() {
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
            if(this.parent == null) {
                return;
            }
            this.parent?.RemoveChild(this);
        }

        private void OnTransformParentChanged() {
            var currentParent = RectTransform.parent;
            bool isFind = false;
            while(currentParent != null) {
                if(!false && currentParent.TryGetComponent<UIElement>(out var element)) {
                    element.AddChild(this);
                    parent = element;
                    isFind = true;
                }else if(currentParent.TryGetComponent<UIManager>(out var manager)) {
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
        public T Qurey<T>(string name = "") where T : UIElement {
            for(int i = 0; i < children.Count; i++) {
                var child = children[i];
                if(string.IsNullOrEmpty(name) && child.GetType() == typeof(T)) {
                    return child as T;
                } else if(name == child.name && child.GetType() == typeof(T)) {
                    return child as T;
                }
            }

            for(int i = 0; i < children.Count; i++) {
                return children[i].Qurey<T>(name);
            }

            return null;
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
