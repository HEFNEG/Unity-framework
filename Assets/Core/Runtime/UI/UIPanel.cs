using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic.UI {
    [RequireComponent(typeof(Canvas))]
    public class UIPanel : UIElement {
        private Canvas canvas;
        public int priority = 0;
        public bool isPop = false;
        public int sortingOrder {
            get {
                if(canvas == null) {
                    canvas = GetComponent<Canvas>();
                }
                return canvas.sortingOrder;
            }
        }

        public override void OnInitialize() {
            base.OnInitialize();
            canvas = GetComponent<Canvas>();
        }

        public void SetPriority(int priority) {
            canvas = GetComponent<Canvas>();
            canvas.sortingOrder = priority;
        }
    }
}