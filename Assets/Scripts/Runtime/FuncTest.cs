using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuncTest : MonoBehaviour {
    private AssetHandle handle;
    // Start is called before the first frame update
    void Start() {
        var prefab = AppBootstrap.asset.Load<GameObject>("arts/prefab.bundle/cube.prefab");
        Instantiate(prefab);

        handle = AppBootstrap.asset.LoadAsync("arts/prefab.bundle/cube.prefab");
    }

    // Update is called once per frame
    void Update() {
        if(handle!=null && handle.isSuccessful) {
            Instantiate(handle.GetAsset<GameObject>());
        }
    }
}
