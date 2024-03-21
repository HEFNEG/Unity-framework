using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.Basic {
    public class EventManager {
        private Dictionary<Hash128, List<EventListener>> callback;
        private List<EventHandle> broadcastEvents;
        private List<NotifyEvent> notifyEvents;

        public void Initialize() {
            callback = new Dictionary<Hash128, List<EventListener>>(16);
            broadcastEvents = new List<EventHandle>();
            notifyEvents = new List<NotifyEvent>();
        }

        public void Destory() {
            callback.Clear();
            broadcastEvents.Clear();
            notifyEvents.Clear();
        }

        public void Tick() {
            BroadcastTick();
            NotifyTick();
        }

        public void Register(IEventListenHandle listener, Hash128 hashcode, Action<EventHandle> action) {
            if(!callback.ContainsKey(hashcode)) {
                callback.Add(hashcode, new List<EventListener>(4));
            }
            callback[hashcode].Add(new EventListener { listener = listener, action = action });
        }

        public void UnRegister(IEventListenHandle listener, Hash128 hashcode) {
            if(callback.TryGetValue(hashcode, out var list)) {
                for(int i = 0; i < list.Count; i++) {
                    if(list[i].listener == listener) {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void Broadcast<T>(T message) where T : struct {
            Hash128 hash = Hash128.Compute(typeof(T).FullName);
            IntPtr ptr = GCHandle.ToIntPtr(GCHandle.Alloc(message));
            broadcastEvents.Add(new EventHandle { hashcode = hash, intPtr = ptr });
        }

        public void Notify<T>(IEventListenHandle listener, T message) where T : struct {
            if(listener == null) {
                return;
            }

            Hash128 hash = Hash128.Compute(typeof(T).FullName);
            IntPtr ptr = GCHandle.ToIntPtr(GCHandle.Alloc(message));
            notifyEvents.Add(new NotifyEvent {
                listener = listener,
                eventHandle = new EventHandle { hashcode = hash, intPtr = ptr }
            });
        }

        private void BroadcastTick() {
            for(int i = 0; i < broadcastEvents.Count; i++) {
                var bradcastHandle = broadcastEvents[i];
                if(callback.TryGetValue(bradcastHandle.hashcode, out var listeners)) {
                    for(int j = listeners.Count - 1; j >= 0; j--) {
                        if(listeners[j].listener != null) {
                            listeners[j].action.Invoke(bradcastHandle);
                        } else {
                            listeners.RemoveAt(j);
                        }
                    }
                }
                GCHandle.FromIntPtr(bradcastHandle.intPtr).Free();
            }
            broadcastEvents.Clear();
        }

        private void NotifyTick() {
            for(int i = 0; i < notifyEvents.Count; i++) {
                var notifyHandle = notifyEvents[i];
                if(notifyHandle.listener != null && callback.TryGetValue(notifyHandle.eventHandle.hashcode, out var listeners)) {
                    for(int j = listeners.Count - 1; j >= 0; j--) {
                        if(listeners[j].listener == notifyHandle.listener) {
                            listeners[j].action.Invoke(notifyHandle.eventHandle);
                            break;
                        }
                    }
                }
                GCHandle.FromIntPtr(notifyHandle.eventHandle.intPtr).Free();
            }
            notifyEvents.Clear();
        }
    }

    public struct EventHandle {
        public Hash128 hashcode;
        public IntPtr intPtr;

        public T GetEvent<T>() where T : struct {
            return (T)(GCHandle.FromIntPtr(intPtr).Target);
        }
    }

    public struct NotifyEvent {
        public IEventListenHandle listener;
        public EventHandle eventHandle;
    }

    public struct EventListener {
        public IEventListenHandle listener;
        public Action<EventHandle> action;
    }

}
