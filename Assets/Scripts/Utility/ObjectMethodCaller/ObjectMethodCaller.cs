using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class ObjectMethodCaller
{
    public string[] methods;

    public ObjectMethodCaller(params string[] methodNames)
    {
        methods = methodNames;
    }

    static public void CallMethod(UnityEngine.Object methodOwner, string methodName)
    {
        Type methodOwnerType = methodOwner.GetType();
        MethodInfo method = methodOwnerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null) method.Invoke(methodOwner, null);
        else Debug.LogError(string.Format("Unable to find method {0} in {1}. Implement those or change method names in Debug inspector.", methodName, methodOwnerType));
    }
}