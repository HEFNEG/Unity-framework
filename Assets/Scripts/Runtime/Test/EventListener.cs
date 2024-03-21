using UnityEngine;
using Game.Basic;

public class EventListener : MonoBehaviour, IEventListenHandle {
    // Start is called before the first frame update
    void Start() {
        this.AddEventListener<int>(AppBootstrap.eventMgr, DebugLog);
    }


    public void DebugLog(EventHandle handle) {
        Debug.Log($"event number : {handle.GetEvent<int>()}");
    }
}
