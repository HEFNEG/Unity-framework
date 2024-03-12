using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIDrag : UIElement, IBeginDragHandler, IDragHandler, IEndDragHandler {
        public void OnBeginDrag(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIDragEvent(
                this,
                Config.UI_BEGIN_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));

        }

        public void OnDrag(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIDragEvent(
                this,
                Config.UI_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));
        }

        public void OnEndDrag(PointerEventData eventData) {
            AppBootstrap.ui.Dispatch(new UIDragEvent(
                this,
                Config.UI_END_DRAG_EVENT,
                eventData.position,
                eventData.pointerCurrentRaycast));
        }
    }
}
