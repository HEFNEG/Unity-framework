using UnityEngine.InputSystem;

namespace Game.Basic {
    public class InputManager {
        InputActionAsset inputAsset;

        public void Initialize(string inputs) {
            inputAsset = InputActionAsset.FromJson(inputs);
            inputAsset.Enable();
        }

        public void SetEnable(string mapName,bool isActive) {
            var actionMap = inputAsset.FindActionMap(mapName);
            if(actionMap != null) {
                if(isActive) {
                    actionMap.Enable();
                } else {
                    actionMap.Disable();
                }
            }
        }

        public InputAction GetAction(string mapName,string actionName) {
            var actionMap = inputAsset.FindActionMap(mapName);
            if(actionMap != null) {
                return actionMap.FindAction(actionName);
            }
            return null;
        }

        public InputAction GetAction(string actionName) {
            return inputAsset.FindAction(actionName);
        }
    }
}