using System;
using UnityEngine;

public struct NotePlacementRating
{
    public Vector2[] coordinates;
    public float[] fit_direct;
    public float[] fit_reverse;

    public NotePlacementRating(Vector2[] c)
    {
        int coordinatesCount = c != null ? c.Length : 0;
        coordinates = new Vector2[coordinatesCount];
        fit_direct = new float[coordinatesCount];
        fit_reverse = new float[coordinatesCount];
        if (coordinatesCount > 0)
        {
            Array.Copy(c, coordinates, coordinatesCount);
            for (int i = 0; i < coordinatesCount; i++)
            {
                fit_direct[i] = (float)(coordinatesCount - i) / (float)coordinatesCount;
                fit_reverse[i] = (float)(coordinatesCount - i) / (float)coordinatesCount;
            }
        }
    }

    public NotePlacementRating(NotePlacementRating clone)
    {
        if (clone.coordinates != null)
        {
            int coordinatesCount = clone.coordinates.Length;
            coordinates = new Vector2[coordinatesCount];
            Array.Copy(clone.coordinates, coordinates, coordinatesCount);
            fit_direct = new float[coordinatesCount];
            Array.Copy(clone.fit_direct, fit_direct, coordinatesCount);
            fit_reverse = new float[coordinatesCount];
            Array.Copy(clone.fit_reverse, fit_reverse, coordinatesCount);
        }
        else
        {
            coordinates = null;
            fit_direct = null;
            fit_reverse = null;
        }
    }

    public int BestCoordinatesIndex
    {
        get
        {
            if (coordinates == null) return -1;
            int coordinatesCount = coordinates.Length;
            if (coordinatesCount == 0) return -1;
            int bestIndex = 0;
            float bestFit = float.NegativeInfinity;
            for (int i = 0, iend = coordinates.Length; i < iend; i++)
            {
                if (fit_direct[i] > bestFit)
                {
                    bestFit = fit_direct[i];
                    bestIndex = i;
                }
                if (fit_reverse[i] > bestFit)
                {
                    bestFit = fit_reverse[i];
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
    }
}