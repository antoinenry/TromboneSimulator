using UnityEngine;
using UnityEngine.UI;

public class TromboneActivatedButton : MonoBehaviour
{
    private Button button;
    private float buttonHeight;
    private TromboneDisplay trombone;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonHeight = button.transform.GetComponent<RectTransform>().rect.height;
    }

    private void OnEnable()
    {
        trombone = FindObjectOfType<TromboneDisplay>();        
    }

    private void Update()
    {
        if (trombone != null && trombone.body != null && trombone.Grab == true)
        {
            float buttonY = transform.position.y;
            float tromboneY = trombone.body.transform.position.y;
            if (Mathf.Abs(buttonY - tromboneY) <= buttonHeight)
            {
                button.Select();
                if (Input.GetMouseButtonDown(0)) button.onClick.Invoke();
            }
        }
    }
}