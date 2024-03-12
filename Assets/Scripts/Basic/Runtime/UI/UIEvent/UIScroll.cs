using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIScroll : UIElement,IScrollHandler {
        public void OnScroll(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIScrollEvent(
                this,
                Config.UI_SCROLL_EVENT,
                eventData.scrollDelta));
        }
    }
}