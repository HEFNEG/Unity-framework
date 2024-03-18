using System;
using UnityEngine;

namespace Game.Basic {
    public interface IEventListenHandle {

    }
    public static class EventExtend {
        public static void AddEventListener<T>(this IEventListenHandle listener, Action<EventHandle> action) where T : struct {
            AppBootstrap.eventMgr.Register(listener, Hash128.Compute(typeof(T).FullName), action);
        }

        public static void RemoveEventListener<T>(this IEventListenHandle listener) where T : struct {
            AppBootstrap.eventMgr.UnRegister(listener, Hash128.Compute(typeof(T).FullName));
        }
    }
}
