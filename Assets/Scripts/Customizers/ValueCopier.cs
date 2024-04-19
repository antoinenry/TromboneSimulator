using System;
using System.Reflection;

public static class ValueCopier
{
    public static void CopyValuesByName(Type valueContainer, object source, object destination)
    {
        if (destination == null || source == null || valueContainer == null) return;
        FieldInfo[] customsFields = valueContainer.GetFields(BindingFlags.Instance | BindingFlags.Public);
        Type sourceType = source.GetType();
        Type destinationType = destination.GetType();
        foreach (FieldInfo f in customsFields)
        {
            string fieldName = f.Name;
            FieldInfo sourceField = sourceType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (sourceField == null) continue;
            FieldInfo destinationField = destinationType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (destinationField == null) continue;
            object sourceValue = sourceField.GetValue(source);
            destinationField.SetValue(destination, sourceValue);
        }
    }

    public static void CopyValuesByName<ValuesContainer>(object source, object destination)
        => CopyValuesByName(typeof(ValuesContainer), source, destination);
}