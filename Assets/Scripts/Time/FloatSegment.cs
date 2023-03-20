using System;
using UnityEngine;

[Serializable]
public struct FloatSegment
{
    public float start;
    public float end;

    public FloatSegment(float segmentStart, float segmentEnd)
    {
        start = segmentStart;
        end = segmentEnd;
    }

    public bool IsNaN => float.IsNaN(start) || float.IsNaN(end);
    public bool IsReverse => IsNaN ? false : start > end;
    public float Length => IsNaN ? 0f : Mathf.Abs(start - end);

    public void InvertDirection()
    {
        float f = start;
        start = end;
        end = f;
    }

    public bool Contains(float t, bool strict = false)
    {
        if (strict)
        {
            if (t < end && t > start) return true;
            if (t < start && t > end) return true;
        }
        else
        {
            if (t <= end && t >= start) return true;
            if (t <= start && t >= end) return true;
        }
        return false;
    }

    public bool Contains(FloatSegment s, bool strict = false)
    {
        return Contains(s.start, strict) && Contains(s.end, strict);
    }

    public bool Crosses(FloatSegment s, bool strict = false)
    {
        return Contains(s.start, strict) || Contains(s.end, strict) || s.Contains(start, strict) || s.Contains(end, strict);
    }


    public FloatSegment[] Join(FloatSegment s)
    {
        if (this.IsReverse != s.IsReverse)
        {
            Debug.LogError("Trying to find junction between two opposites segments.");
            return null;
        }
        // Segments are separate: return both
        else if (this.Crosses(s) == false)
        {
            return new FloatSegment[2] { this, s };
        }
        // One segment contains the other: return container
        else if (this.Contains(s))
        {
            return new FloatSegment[1] { this };
        }
        else if (s.Contains(this))
        {
            return new FloatSegment[1] { s };
        }
        // Segments are touching: return junction as one segment
        else
        {
            FloatSegment junction;
            if (this.IsReverse == false)
            {
                junction.start = Mathf.Min(this.start, s.start);
                junction.end = Mathf.Max(this.end, s.end);
            }
            else
            {
                junction.start = Mathf.Max(this.start, s.start);
                junction.end = Mathf.Min(this.end, s.end);
            }
            return new FloatSegment[1] { junction };
        }
    }

    public FloatSegment[] Remove(FloatSegment s)
    {
        // If thif segment is entirely inside the cut part: return no segment
        if (s.Contains(this))
        {
            return new FloatSegment[0];
        }
        // Removed segment direction doesn't matter, make it the same as this
        if (s.IsReverse != this.IsReverse) s.InvertDirection();
        // Remove something only if there's an intersection
        FloatSegment intersection = Intersection(this, s);
        if (intersection.Length > 0)
        {
            // Cut part is strictly inside this segment : returns two segments, "around" intersection
            if (this.Contains(intersection, true))
            {
                return new FloatSegment[2]
                { 
                    new FloatSegment(start, intersection.start),
                    new FloatSegment(intersection.end, end)
                };
            }
            // Cut part is partially inside: returns one segment, before or after intersection
            else
            {
                if (this.end == intersection.end)
                    return new FloatSegment[1] { new FloatSegment(start, intersection.start) };
                else if (this.start == intersection.start)
                    return new FloatSegment[1] { new FloatSegment(intersection.end, end) };
                else
                {
                    // Hmmmm...
                    Debug.LogError("Remove segment error.");
                    return null;
                }
            }
        }
        // Nothing to remove
        else
            return new FloatSegment[1] { this };
    }

    static public FloatSegment NaN => new FloatSegment(float.NaN, float.NaN);

    static public FloatSegment Intersection(FloatSegment a, FloatSegment b)
    {
        FloatSegment intersection;
        if (a.IsReverse != b.IsReverse)
        {
            Debug.LogError("Trying to find intersection between two opposites segments.");
            intersection = NaN;
        }
        else if (a.Crosses(b) == false)
        {
            intersection = NaN;
        }
        else
        {
            if (a.Contains(b.start)) intersection.start = b.start;
            else intersection.start = a.start;
            if (a.Contains(b.end)) intersection.end = b.end;
            else intersection.end = a.end;
        }
        return intersection;
    }
}