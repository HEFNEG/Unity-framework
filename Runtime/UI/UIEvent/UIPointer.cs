using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIPointer : UIElement, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler{
        public void OnPointerDown(PointerEventData eventData) {
            uiMananger.Dispatch(new UIPointerEvent(
                this,
                Config.UI_POINT_DOWN_EVENT,
                eventData.position
                ));
        }

        public void OnPointerUp(PointerEventData eventData) {
            uiMananger.Dispatch(new UIPointerEvent(
                this,
                Config.UI_POINT_UP_EVENT,
                eventData.position
                ));
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            uiMananger.Dispatch(new UIPointerEvent(
                this,
                Config.UI_POINT_ENTER_EVENT,
                eventData.position
                ));
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            uiMananger.Dispatch(new UIPointerEvent(
                this,
                Config.UI_POINT_EXIT_EVENT,
                eventData.position
                ));
        }
    }
}
