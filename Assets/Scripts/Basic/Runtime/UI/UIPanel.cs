using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic {
    [RequireComponent(typeof(Canvas))]
    public class UIPanel : UIElement {
        private Canvas canvas;
        public int priority = 0;
        public bool isPop = false;

        public override void OnInitialize() {
            base.OnInitialize();
            canvas = GetComponent<Canvas>();
            canvas.sortingOrder = priority * 100;
        }

        public virtual void OnOpen() {

        }

        public virtual void OnClose() {

        }
    }
}
