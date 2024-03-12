using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Basic.UI {
    public class UIEvent {
        public readonly UIElement sender;
        public string senderName => sender.name;
        public string eventName { get; private set; }

        public UIEvent(UIElement element, string eventName) {
            this.sender = element;
            this.eventName = eventName;
        }
    }

    public class UIClickEvent : UIEvent {
        public readonly PointerEventData.InputButton button;
        public readonly int clickCount;

        public UIClickEvent(UIElement element, string eventName, PointerEventData.InputButton button, int clickCount) : base(element, eventName) {
            this.button = button;
            this.clickCount = clickCount;
        }
    }

    public class UIPointerEvent : UIEvent {
        public readonly Vector2 position;

        public UIPointerEvent(UIElement element, string eventName, Vector2 position) : base(element, eventName) {
            this.position = position;
        }
    }

    public class UIScrollEvent : UIEvent {
        public readonly Vector2 scroll;

        public UIScrollEvent(UIElement element, string eventName, Vector2 scroll) : base(element, eventName) {
            this.scroll = scroll;
        }
    }

    public class UIDropEvent : UIEvent {
        public readonly Vector2 position;

        public UIDropEvent(UIElement element, string eventName,Vector2 position) : base(element, eventName) {
            this.position = position;
        }
    }

    public class UIDragEvent : UIEvent {
        public readonly Vector2 position;
        public readonly RaycastResult raycastResult;

        public UIDragEvent(UIElement element, string eventName, Vector2 position,RaycastResult raycastResult) : base(element, eventName) {
            this.position = position;
            this.raycastResult = raycastResult;
        }
    }
}
