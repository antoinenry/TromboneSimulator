using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class TromboneControlWiring : MonoBehaviour
{
    [Serializable]
    public class Wire
    {
        public string valueType;
        public MonoBehaviour from;
        public MonoBehaviour to;

        public Wire(string valueTypeName, ITromboneOutput fromOutput, ITromboneInput toInput)
        {
            valueType = valueTypeName;
            if (fromOutput is MonoBehaviour) from = fromOutput as MonoBehaviour;
            if (toInput is MonoBehaviour) to = toInput as MonoBehaviour;
        }

        public void TransmitValue()
        {
            if (from == null || to == null || valueType == null)
            {
                Debug.LogError("Null wiring reference(s).");
                return;
            }
            switch (valueType)
            {
                case "Grab":
                    (to as ITromboneGrabInput).Grab = (from as ITromboneGrabOutput).Grab;
                    break;
                case "Blow":
                    (to as ITromboneBlowInput).Blow = (from as ITromboneBlowOutput).Blow;
                    break;
                case "SlideTone":
                    (to as ITromboneSlideToneInput).SlideTone = (from as ITromboneSlideToneOutput).SlideTone;
                    break;
                case "PressureLevel":
                    (to as ITrombonePressureLevelInput).PressureLevel = (from as ITrombonePressureLevelOutput).PressureLevel;
                    break;
                case "PressureTone":
                    (to as ITrombonePressureToneInput).PressureTone = (from as ITrombonePressureToneOutput).PressureTone;
                    break;
            }
        }
    }

    static public string GetValueTypeName(Type interfaceType)
    {
        if (interfaceType == null) return null;
        string name = interfaceType.Name;
        name = name.Replace("ITrombone", "");
        name = name.Replace("Input", "");
        name = name.Replace("Output", "");
        return name;
    }

    static public Type GetValueInterfaceType(Type parentInterfaceType, string valueTypeName)
    {
        if (parentInterfaceType == null && valueTypeName == null) return null;
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] allAssemblyTypes = assembly.GetTypes();         
        return Array.Find(allAssemblyTypes, t => t != null && t.IsInterface && t.GetInterface(parentInterfaceType.Name) != null && t.Name.Contains(valueTypeName));
    }

    static public bool CanWire(string valueTypeName, ITromboneOutput from, ITromboneInput to)
    {
        if (valueTypeName == null || from == null || to == null) return false;
        // Check that input and output are different
        if (from == to) return false;
        // Check that output implements necessary interface
        Type outputInterfaceType = GetValueInterfaceType(typeof(ITromboneOutput), valueTypeName);
        Type[] fromInterfaces =  from.GetType().GetInterfaces();
        if (Array.IndexOf(fromInterfaces, outputInterfaceType) == -1) return false;
        // Check that input implements necessary interface
        Type inputInterfaceType = GetValueInterfaceType(typeof(ITromboneInput), valueTypeName);
        Type[] toInterfaces = to.GetType().GetInterfaces();
        if (Array.IndexOf(toInterfaces, inputInterfaceType) == -1) return false;
        // Check out
        return true;
    }

    public List<Wire> wiringSequence;

    public int WireCount => wiringSequence != null ? wiringSequence.Count : 0;
    public ITromboneInput[] InputComponents => GetComponentsInChildren<ITromboneInput>(true);
    public ITromboneOutput[] OutputComponents => GetComponentsInChildren<ITromboneOutput>(true);
    public string[] InputValueNames
    {
        get
        {
            ITromboneInput[] inputComponents = InputComponents;
            List<string> names = new List<string>();
            foreach (ITromboneInput component in inputComponents)
            {
                Type[] componentInterfaces = component.GetType().GetInterfaces();
                foreach (Type i in componentInterfaces)
                {
                    string valueTypeName = GetValueTypeName(i);
                    if (valueTypeName != null && valueTypeName != "" && names.Contains(valueTypeName) == false) names.Add(valueTypeName);
                }
            }            
            return names.OrderBy(n => n).ToArray();
        }
    }

    public void Update()
    {
        if (wiringSequence != null)
        {
            foreach (Wire w in wiringSequence)
                if (w != null) (w.to as ITromboneInput).ClearInputs();
            foreach (Wire w in wiringSequence)
                if (w != null) w.TransmitValue();
        }
    }

    public ITromboneOutput GetWireStart(string valueType, ITromboneInput wireTo)
    {
        if (wiringSequence != null)
        {
            Wire getWire = wiringSequence.Find(w => w.valueType == valueType && w.to as ITromboneInput == wireTo);
            if (getWire != null) return getWire.from as ITromboneOutput;
        }
        return null;
    }

    public void SetWire(string valueType, ITromboneOutput wireFrom, ITromboneInput wireTo)
    {
        if (wiringSequence == null) wiringSequence = new List<Wire>();
        Wire getWire = wiringSequence.Find(w => w.valueType == valueType && w.to as ITromboneInput == wireTo);
        if (getWire != null)
        {
            if (getWire.to != null) (getWire.to as ITromboneInput).ClearInputs();
            if (wireFrom != null) getWire.from = wireFrom as MonoBehaviour;
            else wiringSequence.Remove(getWire);
        }
        else if (wireFrom != null && valueType != null)
        {
            getWire = new Wire(valueType, wireFrom, wireTo);
            wiringSequence.Add(getWire);
        }        
    }
}