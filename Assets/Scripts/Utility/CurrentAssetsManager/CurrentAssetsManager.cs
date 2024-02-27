using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility/CurrentAssetsManager", fileName = "CurrentAssets")]
public class CurrentAssetsManager : ScriptableObject
{
    #region STATIC
    static public UnityEngine.Object GetCurrent(Type objectType)
    {
        SetSingleInstance();
        if (_instance == null || _instance.current == null) return null;
        return _instance.current.Find(o => TypeMatch(o, objectType));
    }

    static public T GetCurrent<T>() where T : UnityEngine.Object => GetCurrent(typeof(T)) as T;

    static public void SetCurrent<T>(T c) where T : UnityEngine.Object
    {
        SetSingleInstance();
        if (_instance == null) return;
        if (_instance.current == null) _instance.current = new List<UnityEngine.Object>();
        int typeIndex = _instance.current.FindIndex(o => TypeMatch(o, c.GetType()));
        if (c)
        {
            if (typeIndex == -1) _instance.current.Add(c);
            else _instance.current[typeIndex] = c;
        }
        else if(typeIndex != -1)
        {
            _instance.current.RemoveAt(typeIndex);
        }
    }

    static public void ClearCurrent(Type objectType)
    {
        SetSingleInstance();
        if (_instance == null || _instance.current == null) return;
        _instance.current.RemoveAll(o => TypeMatch(o, objectType));
    }

    static public void ClearCurrent<T>() where T : UnityEngine.Object => ClearCurrent(typeof(T));

    static public void RemoveCurrent<T>(T c) where T : UnityEngine.Object
    {
        SetSingleInstance();
        if (_instance == null || _instance.current == null) return;
        _instance.current.RemoveAll(o => o == c);
    }

    static public bool IsCurrent<T>(T c) where T : UnityEngine.Object
    {
        SetSingleInstance();
        if (_instance == null || _instance.current == null) return false;
        return _instance.current.Contains(c);
    }

    static private CurrentAssetsManager _instance;

    static private void SetSingleInstance()
    {
        if (_instance != null) return;
        // Find all CurrentManager assets in Resources folders
        Resources.LoadAll<CurrentAssetsManager>("");
        CurrentAssetsManager[] all = Resources.FindObjectsOfTypeAll<CurrentAssetsManager>();
        Resources.UnloadUnusedAssets();
        // There must be only ONE
        if (all == null || all.Length == 0)
        {
            Debug.LogWarning("No CurrentManager asset found. You must create one in a Resource folder.");
            _instance = null;
        }
        else
        {
            if (all.Length > 1) Debug.LogWarning("Multiple CurrentManager assets found. Using " + all[0]);
            _instance = all[0];
        }
    }

    static private bool TypeMatch(UnityEngine.Object o, Type t) => o != null && o.GetType() == t;
    #endregion

    #region INSTANCE
    [SerializeField] private List<UnityEngine.Object> current;
    #endregion
}
