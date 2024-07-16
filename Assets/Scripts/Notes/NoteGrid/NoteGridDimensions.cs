using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct NoteGridDimensions : IEquatable<NoteGridDimensions>
{
    public float[] lineTones;
    [Min(0)] public int columns;
    public float tonePerColumn;

    public int LineCount
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

    public override bool Equals(object obj) => Equals((NoteGridDimensions)obj);

    public override int GetHashCode() => base.GetHashCode();

    public bool Equals(NoteGridDimensions other)
    {
        if (columns != other.columns || tonePerColumn != other.tonePerColumn) return false;
        if (lineTones == other.lineTones) return true;       
        if (lineTones == null && other.lineTones != null) return false;
        if (lineTones != null && other.lineTones == null) return false;
        int l = lineTones.Length;
        if (l != other.lineTones.Length) return false;
        for (int i = 0; i < l; i++) if (lineTones[i] !=  other.lineTones[i]) return false;
        return true;
    }

    public static bool operator ==(NoteGridDimensions left, NoteGridDimensions right) => left.Equals(right);
    public static bool operator !=(NoteGridDimensions left, NoteGridDimensions right) => !left.Equals(right);

    public Vector2[] ToneToCoordinates(float tone) => ToneToCoordinates(tone, 0, LineCount);

    public Vector2[] ToneToCoordinates(float tone, int yMin, int yMax)
    {
        // Find all possible coordinate (slide, pressure) for tone
        List<Vector2> coordinates = new List<Vector2>();
        float lastDeltaTone = LineCount > 1 ? lineTones[LineCount - 1] - lineTones[LineCount - 2] : 0;
        for (int y = yMin; y < yMax; y++)
        {
            float vTone;
            if (y < LineCount) vTone = lineTones[y];
            else if (lastDeltaTone > 0f) vTone = lineTones[LineCount - 1] + lastDeltaTone * (y - LineCount + 1);
            else break;
            if (vTone == tone)
            {
                coordinates.Add(new(0f, y));
                continue;
            }
            if (vTone > tone)
            {
                // Deduce x from y
                if (tonePerColumn != 0f)
                {
                    float x = (vTone - tone) / tonePerColumn;
                    if (x >= 0 && x < columns) coordinates.Add(new(x, y));
                    else break;
                }
            }
        }
        return coordinates.ToArray();
    }

    public Vector2 ToneToCoordinate(float tone) => ToneToCoordinate(tone, 0, LineCount);

    public Vector2 ToneToCoordinate(float tone, int yMin, int yMax)
    {
        Vector2[] possibleCoordinates = ToneToCoordinates(tone, yMin, yMax);
        if (possibleCoordinates == null || possibleCoordinates.Length == 0) return new Vector2(float.NaN, float.NaN);
        else return possibleCoordinates[0];
    }

    public bool Contains(Vector2 coordinate) => Contains(coordinate, 0, LineCount);

    public bool Contains(Vector2 coordinate, int yMin, int yMax)
    {
        if (lineTones == null || float.IsNaN(coordinate.x) || float.IsNaN(coordinate.y)) return false;
        else return coordinate.x >= 0f && coordinate.x < columns && coordinate.y >= yMin && coordinate.y < yMax;
    }
}