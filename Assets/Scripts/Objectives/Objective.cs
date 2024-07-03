using System;
using System.Reflection;
using UnityEngine.Events;

public abstract partial class Objective
{
    #region Types and parameters
    static public Type GetType(string name) => Array.Find(GetAllTypes(), t => t.Name == name);

    static public Type[] GetAllTypes()
    {
        Type[] assemblyTypes = typeof(Objective).GetNestedTypes();
        return Array.FindAll(assemblyTypes, t => typeof(Objective).IsAssignableFrom(t));
    }
    
    static public string[] GetAllTypeNames() => Array.ConvertAll(GetAllTypes(), t => t.Name);

    static public string[] GetParameterNames(Type type)
    {
        if (type != null && typeof(Objective).IsAssignableFrom(type))
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            return Array.ConvertAll(fields, f => f.Name);
        }
        else
            return null;
    }

    static public string[] GetParameterNames(string type)
        => GetParameterNames(GetType(type));

    static public string[] GetParameterNames<T>() where T : Objective
        => GetParameterNames(typeof(T));

    static public Type[] GetParameterTypes(Type type)
    {
        if (type != null && typeof(Objective).IsAssignableFrom(type))
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            return Array.ConvertAll(fields, f => f.FieldType);
        }
        else
            return null;
    }

    static public Type[] GetParameterTypes(string type)
        => GetParameterTypes(GetType(type));

    static public Type[] GetParameterTypes<T>() where T : Objective
        => GetParameterTypes(typeof(T));
    #endregion

    #region Construction
    public static Objective New<T>(SerializableObjectiveInfo objectiveInfo) where T : Objective, new()
    {
        Type objectiveType = GetType(objectiveInfo.type);
        if (objectiveType != typeof(T)) return null;
        Objective o = new T();
        o.SetInfo(objectiveInfo);
        return new T();
    }

    public static Objective NewObjective(SerializableObjectiveInfo objectiveInfo)
    {
        Type objectiveType = GetType(objectiveInfo.type);
        if (objectiveType == null) return null;
        MethodInfo constructionMethod =  typeof(Objective).GetMethod("New", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(objectiveType);
        return constructionMethod.Invoke(null, new object[1] { objectiveInfo }) as Objective;
    }
    #endregion

    public UnityEvent onComplete;

    public virtual SerializableObjectiveInfo GetInfo()
    {
        Type thisType = GetType();
        return SerializableObjectiveInfo.New(thisType);
    }

    public virtual void SetInfo(SerializableObjectiveInfo value)
    {
        Type thisType = GetType();
        if (value.type != thisType.Name)
            throw new Exception("Type mismatch: " + thisType.Name + " / " + value.type);
        string[] thoseParameters = GetParameterNames(thisType);
        if (value.parameters == null || value.parameters.Length != thoseParameters.Length)
            throw new Exception("Parameter count mismatch (" + thisType.Name + ")");
    }

    public virtual void OnMusicEnd() { }
    public virtual void OnPerformanceJudgeScore(float score) { }
}