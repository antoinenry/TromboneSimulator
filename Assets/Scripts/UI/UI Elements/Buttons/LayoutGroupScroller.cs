using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutGroup))]
public class LayoutGroupScroller : MonoBehaviour
{
    [Header("Components")]
    public List<GameObject> elements;
    public Button scrollPreviousButton;
    public Button scrollNextButton;
    [Header("Navigation")]
    public int maxDisplayCount = 5;
    public int scrollStep = 1;
    public int displayOffset = 0;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            scrollPreviousButton?.onClick?.AddListener(ScrollPrevious);
            scrollNextButton?.onClick?.AddListener(ScrollNext);
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            scrollPreviousButton?.onClick?.RemoveListener(ScrollPrevious);
            scrollNextButton?.onClick?.RemoveListener(ScrollNext);
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
            scrollPreviousButton.gameObject.SetActive(false);
            scrollNextButton.gameObject.SetActive(false);
            for (int i = 0; i < elementCount; i++) elements[i]?.SetActive(true);
            return;
        }
        // Scrolling
        int maxOffset = elementCount - maxDisplayCount;
        displayOffset = Mathf.Clamp(displayOffset, 0, maxOffset);
        // Activate/deactivate scroll buttons
        bool canScrollDown = displayOffset > 0;
        if (scrollPreviousButton)
        {
            scrollPreviousButton.transform.SetSiblingIndex(0);
            scrollPreviousButton.gameObject.SetActive(canScrollDown);
        }
        bool canScrollUp = displayOffset < maxOffset;
        if (scrollNextButton)
        {
            scrollNextButton.transform.SetSiblingIndex(elementCount + 2);
            scrollNextButton.gameObject.SetActive(canScrollUp);
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
        if (elements == null || scrollPreviousButton == null || scrollNextButton == null) return false;
        return true;
    }

    private void ScrollNext() => displayOffset += scrollStep;

    private void ScrollPrevious() => displayOffset -= scrollStep;

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
