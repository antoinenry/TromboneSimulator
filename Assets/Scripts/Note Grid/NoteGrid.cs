using UnityEngine;
using System;

[ExecuteAlways]
public class NoteGrid : MonoBehaviour
{
    [Serializable] public struct Margin { public float left, right, bottom, top; }

    [Header("Components")]
    public TromboneCore trombone;
    public SpriteRenderer verticalLines;
    public SpriteRenderer horizontalLines;
    [Header("Dimensions")]
    public float timeScale = 1f;
    public NoteGridDimensions dimensions = NoteGridDimensions.DefaultTromboneGrid;
    [Header("Aspect")]
    public Vector2 gridSize;
    public Margin gridMargin;
    public bool cameraConstrained;
    public Vector2Int cellSize;
    public Vector2Int cellSpacing;
    public bool flattenX = false;
    public bool flattenY = false;

    private void Update()
    {
        if (Application.isPlaying == false)
            UpdateGrid();
    }

    private void Start()
    {
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        // Synchronize with trombone
        if (trombone != null)
        {
            // Cell count
            dimensions.columns = Mathf.CeilToInt(trombone.slideTones) + 1;
            dimensions.lineTones = trombone.PressureTones;
            // Dimensions
            if (trombone.tromboneDisplay != null)
            {
                // Set trombone movement from cell size
                trombone.tromboneDisplay.toneWidth = (cellSize.x + cellSpacing.x) * dimensions.tonePerColumn;
                trombone.tromboneDisplay.stepHeight = cellSize.y + cellSpacing.y;
                // Set grid position to match the tip of the trombone's slide
                transform.position = trombone.transform.position                                    // trombone base position
                    + trombone.tromboneDisplay.SlideBumperMinX * Vector3.right                       // tip of the slide
                    + (Vector3)((Vector2)cellSize / 2f)                                             // center of the cell
                    + cellSize.x * Vector3.left;                                                    // first column
                // Special "flat" modes
                flattenX = !trombone.tromboneDisplay.enableSlideMovement;
                flattenY = !trombone.tromboneDisplay.enablePressureMovement;
            }
        }
        // Get screen size from camera
        if (cameraConstrained == true && Camera.main != null) 
            gridSize = new Vector2(Camera.main.aspect, 1f) * 2f * Camera.main.orthographicSize;
        // Place horizontal lines
        if (horizontalLines != null)
        {
            // Flat mode: no horizontal lines
            if (flattenY == true)
                horizontalLines.enabled = false;
            // Normal mode
            else
            {
                horizontalLines.enabled = true;
                // Get cell borders from sprite
                float spriteBorder = horizontalLines.sprite.border.y + horizontalLines.sprite.border.w;
                // Keep lines on left side of the screen
                horizontalLines.transform.position = Vector3.Scale(horizontalLines.transform.position, new Vector3(0f, 1f, 1f));
                // Set sprite size
                horizontalLines.size = new Vector2()
                {
                    // Horizontal lines take the whole screen width
                    x = gridSize.x,
                    // Sprite height depends on cell count and size
                    y = spriteBorder + (cellSize.y + cellSpacing.y) * dimensions.LineCount
                };
                // Apply margins
                horizontalLines.transform.position += gridMargin.left * Vector3.right;
                horizontalLines.size -= (gridMargin.left + gridMargin.right) * Vector2.right;
            }
        }
        // Place vertical lines
        if (verticalLines != null && verticalLines.sprite != null)
        {
            // Flat mode: no vertical lines
            if (flattenX == true)
                verticalLines.enabled = false;
            // Normal mode
            else
            {
                verticalLines.enabled = true;
                // Get cell borders from sprite
                float spriteBorder = verticalLines.sprite.border.x + verticalLines.sprite.border.z;
                // Keep lines on bottom side of the screen
                verticalLines.transform.position = Vector3.Scale(verticalLines.transform.position, new Vector3(1f, 0f, 1f));
                // Set sprite size
                verticalLines.size = new Vector2()
                {
                    // Sprite width depends on cell count and size
                    x = spriteBorder + (cellSize.x + cellSpacing.x) * dimensions.columns,
                    // Vertical lines take the whole screen height
                    y = gridSize.y
                };
                // Apply margins
                verticalLines.transform.position += gridMargin.bottom * Vector3.up;
                verticalLines.size -= (gridMargin.bottom + gridMargin.top) * Vector2.up;

            }
        }
    }

    public Vector2 CoordinatesToLocalPosition(Vector2 coord)
    {
        // Get position from coordinates and cell size, with spacing
        Vector2 pos = Vector2.Scale(coord, cellSize + cellSpacing);
        // "Flat" modes
        if (flattenX) pos.x = 0f;
        if (flattenY) pos.y = 0f;
        return pos;
    }

    public Vector2 WorldPositionToCoordinates(Vector2 position)
    {
        Vector2 completeCellSize = cellSize + cellSpacing;
        return new Vector2()
        {
            x = completeCellSize.x != 0f ? position.x / completeCellSize.x : float.NaN,
            y = completeCellSize.y != 0f ? position.y / completeCellSize.y : float.NaN
        };
    }
}
