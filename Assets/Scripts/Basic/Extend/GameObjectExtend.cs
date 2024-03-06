using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameObjectExtend 
{
    public static void SetActiveEx(this GameObject gameObject,bool isActive) {
        if(gameObject == null || gameObject.activeSelf == isActive) {
            return;
        }
        gameObject.SetActive(isActive);
    }
}
