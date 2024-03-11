﻿using TMPro;
using UnityEngine;

namespace Game.Basic {
    public class NumberItem : ScrollItem {
        public override void Show<T>(T data) {
            var textField = GetComponentInChildren<TMP_Text>();
            textField.text = data.ToString();
        }
    }
}
