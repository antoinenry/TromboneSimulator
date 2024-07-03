using System;

[Serializable]
public struct SerializableObjectiveInfo
{
    public string type;
    public string[] parameters;

    public static SerializableObjectiveInfo New<T>() where T : Objective
    {
        SerializableObjectiveInfo o = new SerializableObjectiveInfo();
        o.type = typeof(T).Name;
        Type[] parameterTypes = o.GetParameterTypes();
        o.parameters = Array.ConvertAll(parameterTypes, t => "");
        return o;
    }

    public static SerializableObjectiveInfo New(Type objectiveType)
    {
        SerializableObjectiveInfo o = new SerializableObjectiveInfo();
        o.type = typeof(Objective).IsAssignableFrom(objectiveType) ? objectiveType.Name : null;
        Type[] parameterTypes = o.GetParameterTypes();
        o.parameters = Array.ConvertAll(parameterTypes, t => "");
        return o;
    }

    public static SerializableObjectiveInfo New(string objectiveType)
        => New(Objective.GetType(objectiveType));

    public bool IsValid()
    {
        if (type == null) return false;
        Type getType = Objective.GetType(type);
        if (getType == null) return false;
        string[] correctParameters = Objective.GetParameterNames(getType);
        return parameters != null && correctParameters.Length == parameters.Length;
    }

    public string[] GetParameterNames() => Objective.GetParameterNames(type);
    public Type[] GetParameterTypes() => Objective.GetParameterTypes(type);
}