using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Game.Basic.UI {
    public class UIDrop : UIElement, IDropHandler {
        public void OnDrop(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIDropEvent(
                this,
                Config.UI_DROP_EVENT,
                eventData.position));
        }
    }
}
