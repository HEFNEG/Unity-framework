using Game.Basic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSender : MonoBehaviour
{
    int count = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        while(count-- >= 0) {
            AppBootstrap.eventMgr.Broadcast(count);
        }
    }
}
