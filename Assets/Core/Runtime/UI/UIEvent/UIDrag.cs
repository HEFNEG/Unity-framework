using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIDrag : UIElement, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public void OnBeginDrag(PointerEventData eventData) {
            uiMananger.Dispatch(new UIDragEvent(
                this,
                Config.UI_BEGIN_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));

        }

        public void OnDrag(PointerEventData eventData) {
            uiMananger.Dispatch(new UIDragEvent(
                this,
                Config.UI_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));
        }

        public void OnEndDrag(PointerEventData eventData) {
            uiMananger.Dispatch(new UIDragEvent(
                this,
                Config.UI_END_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));
        }
    }
}
