using Game.Basic.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Basic.Console {
    public class Console : UIElement {
        [SerializeField] private LoopScrollView scrollView;
        [SerializeField] private ConsoleInputField inputField;
        private List<string> messages = new List<string>(32);
        private Dictionary<string, Action<string[]>> functions = new Dictionary<string, Action<string[]>>(16);
        public static Console Instance { get; private set; }

        public override void OnInitialize() {
            base.OnInitialize();
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public override void Tick() {
            base.Tick();
        }

        public void Output(string value, Message type = Message.Normal) {
            string outputValue = string.Empty;
            switch(type) {
                case Message.Normal:
                    outputValue = $"<color=green>Log</color>:{value}";
                    Debug.Log(outputValue);
                    break;
                case Message.Warning:
                    outputValue = $"<color=yellow>Warning</color>:{value}";
                    Debug.LogWarning(outputValue);
                    break;
                case Message.Error:
                    outputValue = $"<color=red>Error</color>:{value}";
                    Debug.LogError(outputValue);
                    break;

            }
            messages.Add(outputValue);
            scrollView.SetData(messages);
            scrollView.ToLast();
        }

        public void InvokeCmd(string value) {
            var cmdLines = value.Split(' ', 10, options: StringSplitOptions.RemoveEmptyEntries);
            if(cmdLines.Length == 0) {
                return;
            }

            var command = cmdLines[0];
            var args = new string[cmdLines.Length - 1];
            if(functions.TryGetValue(command, out var func)) {
                if(cmdLines.Length > 1) {
                    cmdLines.CopyTo(args, 1);
                }
                func.Invoke(args);
            }

            Output(value);
        }

        public void RegisterCmd(string command, Action<string[]> func) {
            if(functions.ContainsKey(command)) {
                Output($"{command} has be register", Message.Warning);
            } else {
                functions.Add(command, func);
            }
        }

        public void SwitchActive() {
            gameObject.SetActiveEx(!gameObject.activeSelf);
        }

        public void ClearConsole() {
            messages.Clear();
            scrollView.SetData(messages);
        }
    }

    public enum Message : byte {
        Normal,
        Warning,
        Error,
    }
}
