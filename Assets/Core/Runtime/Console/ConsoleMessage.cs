using Game.Basic.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Basic.Console {
    [RequireComponent(typeof(TMP_Text))]
    public class ConsoleMessage : ScrollItem {
        private TMP_Text text;
        public override void OnInitialize() {
            base.OnInitialize();
            text = GetComponent<TMP_Text>();
        }

        public override void Show<T>(T data){
            base.Show(data);
            text.text = data.ToString();
        }
    }
}
