using Game.Basic.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Basic.Console {
    [RequireComponent(typeof(TMP_InputField))]
    public class ConsoleInputField : UIElement {
        private TMP_InputField inputField;
        public override void OnInitialize() {
            base.OnInitialize();
            inputField = GetComponent<TMP_InputField>();

            inputField.onValueChanged.AddListener(OnInputValueChange);
            inputField.onSubmit.AddListener(OnSumbit);
            inputField.caretColor = Color.green;
            inputField.caretWidth = 2;
            inputField.ActivateInputField();
        }

        public override void Tick() {
            base.Tick();
            inputField.ActivateInputField();
        }

        public void Submit() {
            OnSumbit(inputField.text);
        }

        private void OnInputValueChange(string newValue) {

        }

        private void OnSumbit(string newValue) {
            Console.Instance.InvokeCmd(newValue);
            inputField.text = string.Empty;
        }
    }
}