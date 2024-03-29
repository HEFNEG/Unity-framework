using Game.Basic;
using Game.Basic.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public class FuncTest : MonoBehaviour {
    private AssetHandle handle;
    List<IntPtr> list;
    Entity entity;
    byte va = 0;

    private void Awake() {
        list = new List<IntPtr>();
    }

    // Start is called before the first frame update
    void Start() {
        /*var prefab = AppBootstrap.asset.Load<GameObject>("arts/prefab.bundle/cube.prefab");
        Instantiate(prefab);*/

        handle = AppBootstrap.asset.LoadAsync("arts/prefab.bundle/cube.prefab");
        entity = GetEntity();

        System.Type type = typeof(Entity);
        var fields = type.GetFields();
        var method = type.GetMethod("GetId");
        type.GetField("id").SetValue(entity, 1);
        type.GetField("id").SetValue(entity, 2);

        AppBootstrap.ui.Open("ui/test");
    }

    public void AddListener<T>(Action<T> func) where T:struct {
        var hashcode = Hash128.Compute(typeof(T).FullName);
    }

    // Update is called once per frame
    void Update() {
        if(handle != null && handle.isSuccessful) {
            Instantiate(handle.GetAsset<GameObject>());
        }
        unsafe {
            foreach(var item in list) {
                var address = item.ToPointer();
                var gchandle = GCHandle.FromIntPtr(new IntPtr(address));
                var temp = (Entity)gchandle.Target;
                temp.id++;
                gchandle.Target = temp;
                // Debug.Log($"{entity.id} - {temp.id}");
            }
        }
        
    }

    Entity GetEntity() {
        Entity entity = new Entity() { id = 99 };
        
        unsafe {
            var gcHandle = GCHandle.Alloc(entity);
            var address = GCHandle.ToIntPtr(gcHandle).ToPointer();
            gcHandle = GCHandle.FromIntPtr(new IntPtr(address));
            // Debug.Log(((Entity)gcHandle.Target).id);

            ref Entity ptr = ref UnsafeUtility.AsRef<Entity>(&entity);
            list.Add(GCHandle.ToIntPtr(gcHandle));
            return ptr;
        }
    }

    struct Entity {
        public int id;
        public int id2;

        public void GetId(int arg) {
            
        }
    }
}
