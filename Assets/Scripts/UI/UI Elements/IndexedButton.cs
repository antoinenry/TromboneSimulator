using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[ExecuteAlways]
public class IndexedButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;
    public int index;
    public string text;

    public UnityEvent<int> onClick;

    private void Reset()
    {
        button = GetComponent<Button>();
        if (button != null) buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (Application.isPlaying && button != null) button.onClick.AddListener(Click);
    }

    private void OnDisable()
    {
        if (Application.isPlaying && button != null) button.onClick.RemoveListener(Click);
    }

    private void Update()
    {
        if (buttonText != null) buttonText.text = text;
    }

    private void Click()
    {
        onClick.Invoke(index);
    }
}
