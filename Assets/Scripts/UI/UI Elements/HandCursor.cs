using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class HandCursor : MonoBehaviour
{
    [Flags]
    public enum CursorState { Visible = 1, PointAtUI = 2, Click = 4, SecondaryClick = 8, Trombone = 16 }
    [Serializable]
    public struct HandSprites
    {
        public Sprite idle;
        public Sprite pointing;
        public Sprite clicking;
        public Sprite tromboning;

        public Sprite GetSprite(CursorState cursor)
        {
            if (cursor.HasFlag(CursorState.Visible))
            {
                if (cursor.HasFlag(CursorState.PointAtUI))
                {
                    if (cursor.HasFlag(CursorState.Click))
                        return clicking;
                    else
                        return pointing;
                }
                else if (cursor.HasFlag(CursorState.Trombone))
                {
                    return tromboning;
                }
                else
                    return idle;
            }
            return null;
        }
    }

    [Header("Components")]
    public SpriteRenderer handRenderer;
    public GraphicRaycaster menuUIRaycaster;
    public GraphicRaycaster gameUIRaycaster;
    [Header("Control")]
    public CursorState cursorState;
    public int clickButtonNumber = 0;
    public int secondaryClickButtonNumber = 1;
    public string xAxisName = "Mouse X";
    public string yAxisName = "Mouse Y";
    [Header("Look")]
    public bool roundDisplayPosition;
    public HandSprites sprites;
    public bool showRealCursor;
    public string defaultSortingLayer;
    public string tromboneSortingLayer;

    public bool MainClick { get; private set; }
    public bool SecondaryClick { get; private set; }
    public Vector2 HandPosition
    {
        get => roundDisplayPosition ? new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y)) : transform.position;
        set => transform.position = roundDisplayPosition ? new Vector2(Mathf.Round(value.x), Mathf.Round(value.y)) : value;
    }
    public Vector2 Move { get; private set; }

    private Camera cam;
    private PointerEventData pointerEventData;

    private void Awake()
    {
        pointerEventData = new PointerEventData(FindObjectOfType<EventSystem>(true));
        cam = Camera.main;
    }

    private void Update()
    {
        // Hide/show default cursor
        Cursor.visible = showRealCursor;
        // Cursor movement
        Move = new Vector2(Input.GetAxis(xAxisName), Input.GetAxis(yAxisName)) / cam.scaledPixelHeight;
        if (cursorState.HasFlag(CursorState.Trombone))
        {
            // When grabbing trombone, cursor is locked in the center of the screen
            //Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            // Follow mouse position
            HandPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        // Cursor click
        MainClick = Input.GetMouseButton(clickButtonNumber);
        SecondaryClick = Input.GetMouseButton(secondaryClickButtonNumber);
        // Hand animation
        if (handRenderer != null)
        {
            GetCurrentState(ref cursorState);
            handRenderer.sprite = sprites.GetSprite(cursorState);
            // Sorting layer changes when grabbing trombone
            handRenderer.sortingLayerName = cursorState.HasFlag(CursorState.Trombone) ? tromboneSortingLayer : defaultSortingLayer;
        }
    }

    private void GetCurrentState(ref CursorState state)
    {
        // Raycast from cursor position
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        // In menus
        if (MenuUI.VisibleMenuCount > 0)
            menuUIRaycaster.Raycast(pointerEventData, results);
        // In game
        else
            gameUIRaycaster.Raycast(pointerEventData, results);
        // Process raycast hits
        state &= ~CursorState.PointAtUI;
        state &= ~CursorState.Click;
        foreach (RaycastResult r in results)
        {
            Selectable selectableTarget = r.gameObject.GetComponent<Selectable>();
            if (selectableTarget != null && selectableTarget.interactable == true)
            {
                state |= CursorState.PointAtUI;
                if (MainClick) state |= CursorState.Click;
                return;
            }
        }
    }
}