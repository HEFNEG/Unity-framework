using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cooper.UI {
    public class NumberItem : ScrollItem {
        public override void Show<T>(T data) {
            var textField = GetComponentInChildren<TMP_Text>();
            textField.text = data.ToString();
        }
    }
}
