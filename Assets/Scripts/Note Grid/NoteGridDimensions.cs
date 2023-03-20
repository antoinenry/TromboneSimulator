using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct NoteGridDimensions
{
    public float[] lineTones;
    [Min(0)] public int columns;
    public float tonePerColumn;

    public int lines
    {
        get => lineTones != null ? lineTones.Length : 0;
        set
        {
            int lineCount = Mathf.Max(0, value);
            if (lineTones != null) Array.Resize(ref lineTones, value);
            else lineTones = new float[lineCount];
        }
    }

    static public NoteGridDimensions DefaultTromboneGrid => new NoteGridDimensions()
    {
        lineTones = new float[7] { 46, 53, 58, 62, 65, 68, 70 },
        columns = 7,
        tonePerColumn = 1f
    };

    public Vector2[] ToneToCoordinates(float tone)
    {
        // Find all possible coordinate (slide, pressure) for tone
        List<Vector2> coordinates = new List<Vector2>();
        for (int y = 0, ymax = lineTones.Length; y < ymax; y++)
        {
            float vTone = lineTones[y];
            if (vTone >= tone)
            {
                Vector2 coord;
                coord.y = y;
                // Deduce x from y
                if (tonePerColumn != 0f)
                {
                    coord.x = (vTone - tone) / tonePerColumn;
                    if (coord.x >= 0 && coord.x < columns)
                        coordinates.Add(coord);
                }
                else if(vTone == tone)
                {
                    coord.x = 0f;
                    coordinates.Add(coord);
                }
            }
        }
        return coordinates.ToArray();
    }

    public Vector2 ToneToCoordinate(float tone)
    {
        Vector2[] possibleCoordinates = ToneToCoordinates(tone);
        if (possibleCoordinates == null || possibleCoordinates.Length == 0) return new Vector2(float.NaN, float.NaN);
        else return possibleCoordinates[0];
    }

    public bool Contains(Vector2 coordinate)
    {
        if (lineTones == null || float.IsNaN(coordinate.x) || float.IsNaN(coordinate.y)) return false;
        else return coordinate.x >= 0f && coordinate.x < columns && coordinate.y >= 0f && coordinate.y < lineTones.Length;
    }
}