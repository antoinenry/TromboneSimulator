using System;
using System.Reflection;
using UnityEngine.Events;

public abstract partial class ObjectiveInstance
{
    #region Types and parameters
    static public Type GetType(string name) => Array.Find(GetAllTypes(), t => t.Name == name);

    static public Type[] GetAllTypes()
    {
        Type[] assemblyTypes = typeof(ObjectiveInstance).GetNestedTypes();
        return Array.FindAll(assemblyTypes, t => typeof(ObjectiveInstance).IsAssignableFrom(t));
    }
    
    static public string[] GetAllTypeNames() => Array.ConvertAll(GetAllTypes(), t => t.Name);

    static public FieldInfo[] GetParameterFields(Type type)
        => type != null && typeof(ObjectiveInstance).IsAssignableFrom(type) ? type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : new FieldInfo[0];

    static public string[] GetParameterNames(Type type)
        => Array.ConvertAll(GetParameterFields(type), f => f.Name);

    static public string[] GetParameterNames(string type)
        => GetParameterNames(GetType(type));

    static public string[] GetParameterNames<T>() where T : ObjectiveInstance
        => GetParameterNames(typeof(T));

    static public Type[] GetParameterTypes(Type type)
        => Array.ConvertAll(GetParameterFields(type), f => f.FieldType);

    static public Type[] GetParameterTypes(string type)
        => GetParameterTypes(GetType(type));

    static public Type[] GetParameterTypes<T>() where T : ObjectiveInstance
        => GetParameterTypes(typeof(T));
    #endregion

    #region Construction
    public ObjectiveInstance()
    {
        onComplete = new UnityEvent<ObjectiveInfo>();
    }

    public static ObjectiveInstance New<T>(ObjectiveInfo objectiveInfo) where T : ObjectiveInstance, new()
    {
        Type objectiveType = GetType(objectiveInfo.type);
        if (objectiveType != typeof(T)) return null;
        ObjectiveInstance o = new T();
        o.SetInfo(objectiveInfo);
        return o;
    }

    public static ObjectiveInstance NewObjective(ObjectiveInfo objectiveInfo)
    {
        Type objectiveType = GetType(objectiveInfo.type);
        if (objectiveType == null) return null;
        MethodInfo constructionMethod =  typeof(ObjectiveInstance).GetMethod("New", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(objectiveType);
        return constructionMethod.Invoke(null, new object[1] { objectiveInfo }) as ObjectiveInstance;
    }
    #endregion

    public string name;
    public bool isComplete;
    public UnityEvent<ObjectiveInfo> onComplete;

    public virtual ObjectiveInfo GetInfo()
    {
        Type thisType = GetType();
        return ObjectiveInfo.New(thisType, name);
    }

    public virtual void SetInfo(ObjectiveInfo value)
    {
        // Check objective type
        Type thisType = GetType();
        if (value.type != thisType.Name)
            throw new Exception("Type mismatch: " + thisType.Name + " / " + value.type);
        // Set name
        name = value.name;
        // Set parameters
        string[] thoseParameters = GetParameterNames(thisType);
        if (value.parameters == null || value.parameters.Length != thoseParameters.Length)
            throw new Exception("Parameter count mismatch (" + thisType.Name + ")");
    }

    public virtual void OnMusicPlayerUpdate() { }
    public virtual void OnMusicEnd() { }
    public virtual void OnPerformanceJudgeScore(float score) { }

    public void Complete(bool value = true)
    {
        if (isComplete != value)
        {
            isComplete = value;
            onComplete.Invoke(GetInfo());
        }
    }
}