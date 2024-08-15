using TMPro;
using UnityEngine;
using System;

public class GameObjectTextDisplay : MonoBehaviour
{
    [Serializable]
    public struct GameObjectText
    {
        public GameObject targetObject;
        public string text;
    }

    public TextMeshProUGUI descriptionField;
    public string defaultText = "";
    public GameObjectText[] text;

    private GameObject currentTarget;

    private void OnEnable()
    {
        HideDescription();
    }

    public void ShowDescription(GameObject target)
    {
        currentTarget = target;
        if (descriptionField == null || text == null) return;
        descriptionField.text = Array.Find(text, t => t.targetObject == target).text;
    }

    public void HideDescription(GameObject target)
    {
        if (currentTarget == target) HideDescription();
    }

    public void HideDescription()
    {
        if (descriptionField == null || text == null) return;
        descriptionField.text = defaultText;
    }
}
