using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class SeparateModelEditor {
    [MenuItem("Model/DeleteNoActive")]
    public static void Separate() {
        /*if(Selection.count <= 0) {
            return;
        }
       var gameObjects = Selection.gameObjects;
        foreach(var model in gameObjects) {
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(model, AssetDatabase.GetAssetPath(Selection.activeInstanceID), InteractionMode.AutomatedAction);
            var children = model.GetComponentsInChildren<Transform>(true);
            foreach(var child in children) {
                if(!child.gameObject.activeSelf) {
                    PrefabUtility.ApplyRemovedGameObject(model, child.gameObject, InteractionMode.AutomatedAction);
                }
            }

        }*/
    }
}
