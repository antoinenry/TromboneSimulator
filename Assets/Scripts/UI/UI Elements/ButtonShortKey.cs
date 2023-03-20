using UnityEngine;
using UnityEngine.UI;

public class ButtonShortKey : MonoBehaviour
{
    public KeyCode[] keys = new KeyCode[] { KeyCode.Space, KeyCode.Escape };

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (button != null && button.enabled == true && button.onClick != null)
        {
            foreach (KeyCode k in keys)
                if (Input.GetKeyDown(k)) button.onClick.Invoke();
        }
    }
}