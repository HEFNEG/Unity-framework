using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIClick : UIElement, IPointerClickHandler {
        public void OnPointerClick(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIClickEvent(
                this,
                Config.UI_CLICK_EVENT,
                eventData.button,
                eventData.clickCount));
        }
    }
}