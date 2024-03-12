using Game.Basic.UI;
using UnityEngine;

public class UITest : UIPanel {
    public override void OnShow() {
        base.OnShow();
        AddListener(EventHandle);
        Debug.Log("UI Show");
    }

    public override void OnHide() {
        base.OnShow();
        RemoveListener(EventHandle);
        Debug.Log(" UI Hide");
    }

    public void EventHandle(UIEvent uiEvent) {
        Debug.Log(uiEvent.eventName);
    }
}

