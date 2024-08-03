using System;
using System.Reflection;

[Serializable]
public struct ObjectiveInfo : IEquatable<ObjectiveInfo>
{
    public string name;
    public string type;
    public string[] parameters;

    public static ObjectiveInfo New<T>() where T : ObjectiveInstance
    {
        ObjectiveInfo o = new ObjectiveInfo();
        o.type = typeof(T).Name;
        Type[] parameterTypes = o.GetParameterTypes();
        o.parameters = Array.ConvertAll(parameterTypes, t => "");
        return o;
    }

    public static ObjectiveInfo New(Type objectiveType, string name)
    {
        ObjectiveInfo o = new ObjectiveInfo();
        o.type = typeof(ObjectiveInstance).IsAssignableFrom(objectiveType) ? objectiveType.Name : null;
        Type[] parameterTypes = o.GetParameterTypes();
        o.parameters = Array.ConvertAll(parameterTypes, t => "");
        o.name = name;
        return o;
    }

    public static ObjectiveInfo New(string objectiveType, string name)
        => New(ObjectiveInstance.GetType(objectiveType), name);

    public string LongName
    {
        get
        {
            Type objectiveType = ObjectiveInstance.GetType(type);
            MethodInfo getMethod = objectiveType?.GetMethod("GetDisplayName", BindingFlags.Static | BindingFlags.Public);
            if (getMethod == null) return name;
            object[] methodParameters = new object[1] { this };
            object result = getMethod.Invoke(null, methodParameters);
            if (result == null || result.GetType() != typeof(string)) return name;
            return (string)result;
        }
    }

    public bool IsValid()
    {
        if (type == null) return false;
        Type getType = ObjectiveInstance.GetType(type);
        if (getType == null) return false;
        string[] correctParameters = ObjectiveInstance.GetParameterNames(getType);
        return parameters != null && correctParameters.Length == parameters.Length;
    }

    public string[] GetParameterNames() => ObjectiveInstance.GetParameterNames(type);
    public Type[] GetParameterTypes() => ObjectiveInstance.GetParameterTypes(type);

    public override bool Equals(object obj) => Equals((ObjectiveInfo)obj);

    public override int GetHashCode() => base.GetHashCode();

    public bool Equals(ObjectiveInfo other)
    {
        if (type != other.type) return false;
        if (parameters == null) return other.parameters == null;
        if (other.parameters == null) return false;
        if (other.parameters.Length != parameters.Length) return false;
        for (int i = 0, iend = parameters.Length; i < iend; i++) if (other.parameters[i] != parameters[i]) return false;
        return true;
    }

    public static bool operator ==(ObjectiveInfo left, ObjectiveInfo right) => left.Equals(right);
    public static bool operator !=(ObjectiveInfo left, ObjectiveInfo right) => !left.Equals(right);
}