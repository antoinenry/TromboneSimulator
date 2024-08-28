using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SwooshDisplay : MonoBehaviour
{
    public TextMeshProUGUI textPrefab;
    public Transform swooshFrom;
    public Transform spawnPoint;
    public Transform swooshTo;
    public float swooshSpeed = 10f;
    public float fadeSpeed = 1f;
    public Color color = Color.white;

    private Queue<TextMeshProUGUI> textInstances;
    private TextMeshProUGUI freshText;

    public string FreshText => freshText != null ? freshText.text : null; 

    private void Awake()
    {
        textInstances = new Queue<TextMeshProUGUI>();
    }

    private void Start()
    {
        Clear();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        float move = deltaTime * swooshSpeed;
        float fade = deltaTime * fadeSpeed;
        if (textInstances.Count > 0)
        {
            // Destroy old text
            TextMeshProUGUI oldText = textInstances.Peek();
            if (Vector2.Distance(oldText.rectTransform.position, swooshTo.position) <= float.Epsilon)
            {
                textInstances.Dequeue();
                Destroy(oldText.gameObject);
            }
            // Move texts
            foreach (TextMeshProUGUI d in textInstances)
            {
                Vector2 position = d.rectTransform.position;
                d.rectTransform.position = Vector2.MoveTowards(position, swooshTo.position, move);
                Color color = d.color;
                color.a -= fade;
                d.color = color;
            }
        }
        // Bring fresh text up
        if (freshText != null)
        {
            Vector2 position = freshText.rectTransform.position;
            freshText.rectTransform.position = Vector2.MoveTowards(position, spawnPoint.position, move);
        }
    }

    public void SetTextContent(string text)
    {
        if (freshText == null)
        {
            freshText = Instantiate(textPrefab, swooshFrom);
            freshText.transform.SetParent(transform, true);
        }
        freshText.text = text;
        freshText.color = color;
    }

    public void SetTextColor(Color color)
    {
        if (freshText == null) return;
        freshText.color = color;
    }

    public void FreeText()
    {
        if (freshText != null && textInstances != null)
        {
            textInstances.Enqueue(freshText);
            freshText = null;
        }
    }

    public void Clear()
    {
        if (freshText != null) Destroy(freshText.gameObject);
        freshText = null;
        if (textInstances != null)
            foreach (TextMeshProUGUI d in textInstances)
                if (d != null) Destroy(d.gameObject);
        textInstances = new Queue<TextMeshProUGUI>();
    }
}
