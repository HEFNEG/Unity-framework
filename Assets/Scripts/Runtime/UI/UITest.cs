using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITest : UIElement {
    public override void OnShow() {
        base.OnShow();
        Debug.Log("UI Show");
    }

    public override void OnHide() {
        base.OnShow();
        Debug.Log(" UI Hide");
    }
}

