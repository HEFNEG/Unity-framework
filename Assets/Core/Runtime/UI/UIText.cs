using System;
using TMPro;
using UnityEngine;

namespace Game.Basic.UI {
    public class UIText : UIElement {
        [SerializeField] private TMP_Text m_text;

        public string text {
            get {
                return m_text.text;
            }
            set {
                m_text.text = value;
            }
        }
        private void Awake() {
            if(m_text == null) {
                m_text = GetComponent<TMP_Text>();
            }
        }
    }
}
