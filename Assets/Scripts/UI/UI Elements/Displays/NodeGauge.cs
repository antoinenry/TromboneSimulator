using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class NodeGauge : MonoBehaviour
{
    [Header("Components")]
    public GridLayoutGroup layout;
    public Image nodeModel;
    [Header("Look")]
    public Vector2 nodeSize = new Vector2(2, 6);
    public float overflowWidth = 50f;
    public Color[] valueColors = new Color[2] { Color.gray, Color.white };
    [Header("Input")]
    [SerializeField] private int[] values = new int[3] { 1, 0, 0 };

    private Image[] nodeInstances;
    private int instanceCount;

    public int NodeCount
    {
        get => values != null ? values.Length : 0;
        set
        {
            int count = Mathf.Max(0, value);
            if (values == null) values = new int[count];
            else Array.Resize(ref values, count);
        }
    }

    private void Update()
    {
        if (nodeModel == null || layout == null) instanceCount = -1;
        if (instanceCount != NodeCount) ResetNodeInstances();
        UpdateNodeColors();
        UpdateGaugeSize();
    }

    public void ResetNodeInstances()
    {
        // Ensure model in in layout
        if (layout == null || nodeModel == null) return;
        nodeModel.transform.SetParent(layout.transform);
        // Destroy all instances exept model
        nodeInstances = layout.GetComponentsInChildren<Image>(true);
        foreach(Image instance in nodeInstances)
        {
            if (instance == nodeModel) continue;
            else DestroyImmediate(instance.gameObject);
        }
        // Set new instances
        instanceCount = NodeCount;
        nodeInstances = new Image[instanceCount];
        if (instanceCount == 0)
        {
            nodeModel.gameObject.SetActive(false);
            return;
        }
        // Use model as first instance
        nodeInstances[0] = nodeModel;
        nodeModel.gameObject.SetActive(true);
        // Create other instances based on model
        for (int i = 1; i < instanceCount; i++)
        {
            nodeInstances[i] = Instantiate(nodeModel, layout.transform);
            nodeInstances[i].gameObject.SetActive(true);
        }
    }

    public void UpdateNodeColors()
    {
        int colorCount = valueColors != null ? valueColors.Length : 0;
        if (colorCount == 0) return;
        instanceCount = nodeInstances != null ? nodeInstances.Length : 0;
        for (int i = 0, iend = NodeCount; i < iend; i++)
        {
            if (i >= instanceCount) break;
            if (nodeInstances[i] == null) continue;
            int colorIndex = Mathf.Clamp(values[i], 0, colorCount - 1);
            nodeInstances[i].color = valueColors[colorIndex];
        }
    }

    public void UpdateGaugeSize()
    {
        RectTransform gaugeRect = layout?.GetComponent<RectTransform>();
        if (gaugeRect == null) return;
        Vector2 size = gaugeRect.sizeDelta;
        size.x = (layout.cellSize.x + layout.spacing.x) * NodeCount;
        size.y = layout.cellSize.y;
        if (overflowWidth > 0)
        {
            int overflow = Mathf.CeilToInt(size.x / overflowWidth);
            if (overflow > 0)
            {
                layout.constraintCount = overflow;
                Vector2 cellSize = nodeSize;
                cellSize.y /= overflow;
                layout.cellSize = cellSize;
                size.x /= overflow;
            }
        }
        else
        {
            layout.constraintCount = 1;
            layout.cellSize = nodeSize;
        }
        gaugeRect.sizeDelta = size;
    }

    public void SetNodeAt(int index, int value)
    {
        if (index < 0 || index >= NodeCount) return;
        values[index] = value;
        UpdateNodeColors();
    }

    public void SetAllNodes(int value)
    {
        for (int i = 0, iend = NodeCount; i < iend; i++) values[i] = value;
        UpdateNodeColors();
    }
}
