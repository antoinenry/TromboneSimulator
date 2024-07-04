using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(VerticalLayoutGroup))]
public class VerticalScrollList : MonoBehaviour
{
    [Header("Components")]
    public List<GameObject> elements;
    public Button upButton;
    public Button downButton;
    [Header("Navigation")]
    public int maxDisplayCount = 5;
    public int displayOffset = 0;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            upButton?.onClick?.AddListener(ScrollUp);
            downButton?.onClick?.AddListener(ScrollDown);
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            upButton?.onClick?.RemoveListener(ScrollUp);
            downButton?.onClick?.RemoveListener(ScrollDown);
        }
    }

    private void Update()
    {
        if (RequireComponents() == false) return;
        int elementCount = elements != null ? elements.Count : 0;
        // Exception: no scrolling needed
        if (elementCount <= maxDisplayCount)
        {
            displayOffset = 0;
            upButton.gameObject.SetActive(false);
            downButton.gameObject.SetActive(false);
            for (int i = 0; i < elementCount; i++) elements[i]?.SetActive(true);
            return;
        }
        // Scrolling
        int maxOffset = elementCount - maxDisplayCount;
        displayOffset = Mathf.Clamp(displayOffset, 0, maxOffset);
        // Activate/deactivate scroll buttons
        bool canScrollDown = displayOffset > 0;
        if (upButton)
        {
            upButton.transform.SetSiblingIndex(0);
            upButton.gameObject.SetActive(canScrollDown);
        }
        bool canScrollUp = displayOffset < maxOffset;
        if (downButton)
        {
            downButton.transform.SetSiblingIndex(elementCount + 2);
            downButton.gameObject.SetActive(canScrollUp);
        }
        // Activate/deactivate displayed items and ensure display order is correct
        int firstActiveIndex = displayOffset + (canScrollDown ? 1 : 0);
        int lastActiveIndex = displayOffset - (canScrollUp ? 1 : 0) + maxDisplayCount;
        for (int i = 0; i < elementCount; i++)
        {
            if (elements[i] == null) continue;
            elements[i].transform.SetSiblingIndex(i + 1);
            elements[i].SetActive(i >= firstActiveIndex && i < lastActiveIndex);
        }
    }

    private bool RequireComponents()
    {
        if (elements == null || upButton == null || downButton == null) return false;
        return true;
    }

    private void ScrollUp() => displayOffset++;

    private void ScrollDown() => displayOffset--;

    public void Clear()
    {
        if (elements == null)
        {
            elements = new List<GameObject> ();
            return;
        }
        foreach (GameObject e in elements) DestroyImmediate(e);
        elements?.Clear();
    }

    public void InstantiateElement(GameObject model, int repeat = 1)
    {
        if (model == null) return;
        int count = Mathf.Max(repeat, 0);
        if (elements == null) elements = new(count);
        for (int i = 0; i < count; i++) elements.Add(Instantiate(model, transform));
    }

    public void Add(GameObject element)
    {
        if (element == null) return;
        if (elements == null) elements = new List<GameObject>();
        elements.Add(element);
        element.transform.SetParent(transform);
    }
}
