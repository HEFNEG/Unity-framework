using System;
using UnityEngine;

namespace Game.Basic {
    public interface IEventListenHandle {

    }
    public static class EventExtend {
        public static void AddEventListener<T>(this IEventListenHandle listener, EventManager eventMgr, Action<EventHandle> action) where T : struct {
            eventMgr.Register(listener, Hash128.Compute(typeof(T).FullName), action);
        }

        public static void RemoveEventListener<T>(this IEventListenHandle listener, EventManager eventMgr) where T : struct {
            eventMgr.UnRegister(listener, Hash128.Compute(typeof(T).FullName));
        }
    }
}
