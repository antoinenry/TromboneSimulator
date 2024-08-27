using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectDispatch
{
    [System.Serializable]
    public struct DispatchInfo
    {
        public Transform dispatched;
        public Transform destination;
        public bool worldPositionStays;

        public Transform Origin {  get; private set; }

        public DispatchInfo(Transform dispatched, Transform destination, bool worldPositionStays = true)
        {
            this.dispatched = dispatched;
            this.destination = destination;
            this.worldPositionStays = worldPositionStays;
            Origin = null;
            TrySetOrigin();
        }

        public bool TrySetOrigin()
        {
            if (dispatched == null) return false;
            Origin = dispatched.parent;
            return true;
        }
    }

    private List<DispatchInfo> dispatchedList;

    public GameObjectDispatch()
    {
        dispatchedList = new List<DispatchInfo>();
    }

    private void OnDestroy()
    {
        DestroyDispatched();
    }

    public void DestroyDispatched()
    {
        if (dispatchedList == null) return;
        foreach (DispatchInfo info in dispatchedList)
            if (info.dispatched != null)
                Object.DestroyImmediate(info.dispatched.gameObject);
    }

    public void Dispatch(Transform dispatched, Transform destination, bool worldPositionStays = true)
    {
        if (dispatched == null) return;
        // Keep track of dispatch for cancellation and/or destruction
        if (dispatchedList == null) dispatchedList = new List<DispatchInfo>();
        if (dispatchedList.FindIndex(info => info.dispatched == dispatched) == -1)
        {
            DispatchInfo info = new(dispatched, destination, worldPositionStays);
            dispatchedList.Add(info);
        }
        // Dispatch
        dispatched.SetParent(destination, worldPositionStays);
    }

    public void CancelDispatch(Transform dispatched)
    {
        if (dispatchedList == null) return;
        DispatchInfo info = dispatchedList.Find(i => i.dispatched == dispatched);
        if (info.dispatched == null) return;
        info.dispatched.SetParent(info.Origin, info.worldPositionStays);
        dispatchedList?.RemoveAll(i => i.dispatched == info.dispatched);
    } 
}