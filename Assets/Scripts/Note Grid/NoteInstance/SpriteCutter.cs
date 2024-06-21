using System.Collections.Generic;
using UnityEngine;

public class SpriteCutter : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float spriteEndLength;
    public float minSliceLength;
    public bool horizontalSlice;
    public bool roundValues = true;

    private List<SpriteRenderer> sliceRenderers;

    private void Awake()
    {
        sliceRenderers = new List<SpriteRenderer>() { spriteRenderer };
    }

    public void SetTotalLength(float length)
    {
        if (roundValues) length = Mathf.Ceil(length);
        if (sliceRenderers == null || sliceRenderers.Count == 0)
        {
            if (spriteRenderer != null) SetSpriteSlice(spriteRenderer, 0f, length);
            return;
        }
        // Find last slice
        SpriteRenderer lastSlice = null;
        float lastSliceEnd = 0f;
        foreach (SpriteRenderer slice in sliceRenderers)
        {
            float thisSliceEnd = horizontalSlice ? slice.transform.localPosition.x + slice.size.x : slice.transform.localPosition.y + slice.size.y;
            if (thisSliceEnd > lastSliceEnd)
            {
                lastSlice = slice;
                lastSliceEnd = thisSliceEnd;
            }
        }
        // No last slice found or current length is already correct
        if (lastSlice == null || lastSliceEnd == length) return;
        // Augment length
        if (lastSliceEnd < length)
        {
            float lastSliceStart = horizontalSlice ? lastSlice.transform.localPosition.x : lastSlice.transform.localPosition.y;
            SetSpriteSlice(lastSlice, lastSliceStart, length);
        }
        // Reduce length
        else if (lastSliceEnd > length)
        {
            Cut(length, float.PositiveInfinity);
        }
    }

    public void Cut(float cutStart, float cutEnd)
    {
        if (roundValues)
        {
            cutStart = Mathf.Ceil(cutStart);
            cutEnd = Mathf.Ceil(cutEnd);
        }
        // Check parameters are valid
        if (spriteRenderer == null) return;
        cutStart = Mathf.Max(cutStart, 0f);
        cutEnd = Mathf.Max(cutEnd, 0f);
        if (cutStart == cutEnd) return;
        if (cutStart > cutEnd)
        {
            // Swap start and end
            float s = cutEnd;
            cutEnd = cutStart;
            cutStart = s;
        }
        cutStart = Mathf.Max(cutStart, 0f);
        // Re evaluate slices
        bool slicesRemoved = false;
        for (int i = 0, sliceCount = sliceRenderers.Count; i < sliceCount; i++)
        {
            SpriteRenderer sliceRenderer = sliceRenderers[i];
            if (sliceRenderer == null)
            {
                slicesRemoved = true;
                continue;
            }
            float sliceStart = horizontalSlice ? sliceRenderer.transform.localPosition.x : sliceRenderer.transform.localPosition.y;
            float sliceEnd = sliceStart - spriteEndLength + (horizontalSlice ? sliceRenderer.size.x : sliceRenderer.size.y);
            // If the slice is out of the cut, it is not affected
            if (sliceEnd <= cutStart || sliceStart >= cutEnd)
                continue;
            // If the slice is completly inside the cut, it is entirely removed
            if (sliceStart >= cutStart && sliceEnd <= cutEnd)
            {
                // Keep the original sprite renderer object
                if (sliceRenderer == spriteRenderer)
                {
                    SetSpriteSlice(sliceRenderer, 0f, 0f);
                    sliceRenderers[i] = null;
                }
                // Destroy if not the original
                else
                    DestroyImmediate(sliceRenderer.gameObject);
                slicesRemoved = true;
                continue;
            }
            // If the cut is completly inside the slice, it splits the slice in two
            if (sliceStart < cutStart && sliceEnd > cutEnd)
            {
                // Respect minimum slice length
                if (sliceEnd - sliceStart <= 2f * minSliceLength) break;
                cutStart = Mathf.Max(cutStart, sliceStart + minSliceLength);
                cutEnd = Mathf.Max(cutEnd, cutStart);
                cutEnd = Mathf.Min(cutEnd, sliceEnd - minSliceLength);
                // Create other half
                SpriteRenderer newSlice = Instantiate(spriteRenderer, transform);
                sliceRenderers.Add(newSlice);
                // Resize two halves
                SetSpriteSlice(sliceRenderer, sliceStart, cutStart);
                SetSpriteSlice(newSlice, cutEnd, sliceEnd);
                // Other slices won't be affected, we can get out now
                break;
            }
            // If the cut eats only the end of the slice, resize the slice
            if (sliceStart < cutStart)
            {
                // Respect minimum slice length
                if (sliceEnd - sliceStart <= minSliceLength) continue;
                // Reduce slice length from the end
                SetSpriteSlice(sliceRenderer, sliceStart, cutStart);
                continue;
            }
            // If the cut eats only the start of the slice, resize the slice and we can get out here
            else
            {
                // Respect minimum slice length, with special case for last slice
                if (sliceEnd - sliceStart <= minSliceLength)
                {
                    if (i == sliceCount - 1) cutEnd = sliceEnd;
                    else break;
                }
                // Reduce slice length from the start
                SetSpriteSlice(sliceRenderer, cutEnd, sliceEnd);
                break;
            }
        }

        // Cleanup
        if (slicesRemoved) sliceRenderers.RemoveAll(s => s == null);
    }

    private void SetSpriteSlice(SpriteRenderer sliceRenderer, float start, float end)
    {
        if (roundValues)
        {
            start = Mathf.Ceil(start);
            end = Mathf.Ceil(end);
        }
        if (sliceRenderer == null) return;
        Vector3 pos = sliceRenderer.transform.localPosition;
        Vector2 size = sliceRenderer.size;
        if (horizontalSlice)
        {
            pos.x = start;
            size.x = end - start;
            if (size.x > 0f) size.x += spriteEndLength;
        }
        else
        {
            pos.y = start;
            size.y = end - start;
            if (size.y > 0f) size.y += spriteEndLength;
        }
        sliceRenderer.transform.localPosition = pos;
        sliceRenderer.size = size;
    }

    public float[] GetSlices()
    {
        int sliceCount = sliceRenderers.Count;
        float[] slices = new float[sliceCount * 2];
        if (horizontalSlice)
        {
            for (int i = 0; i < sliceCount; i++)
            {
                slices[2 * i] = sliceRenderers[i].transform.localPosition.x;
                slices[2 * i + 1] = slices[2 * i] + sliceRenderers[i].size.x;
            }
        }
        else
        {
            for (int i = 0; i < sliceCount; i++)
            {
                slices[2 * i] = sliceRenderers[i].transform.localPosition.y;
                slices[2 * i + 1] = slices[2 * i] + sliceRenderers[i].size.y;
            }
        }
        if (roundValues)
        {
            for (int i = 0; i < 2 * sliceCount; i++)
            {
                slices[i] = Mathf.Ceil(slices[i]);
            }
        }
        return slices;
    }

    public void SetColor(Color c)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = c;
        if (sliceRenderers != null)
            foreach (SpriteRenderer s in sliceRenderers)
                if (s != null)
                    s.color = c;
    }

    public void SetColorAtIndex(Color c, int at)
    {
        int sliceCount = sliceRenderers != null ? sliceRenderers.Count : 0;
        if (at >= 0 && at < sliceCount) sliceRenderers[at].color = c;
    }

    public void SetColorFromToPosition(Color c, float fromPosition, float toPosition, bool exclusive = false, bool strict = false)
    {
        if (Application.isPlaying == false)
        {
            if (spriteRenderer != null) spriteRenderer.color = c;
        }
        else if (sliceRenderers != null)
        {
            if (roundValues)
            {
                fromPosition = Mathf.Ceil(fromPosition);
                toPosition = Mathf.Ceil(toPosition);
            }
            FloatSegment segmentToColor = new FloatSegment(fromPosition, toPosition);
            foreach (SpriteRenderer s in sliceRenderers)
            {
                if (s != null)
                {
                    float sliceStart = horizontalSlice ? s.transform.localPosition.x : s.transform.localPosition.y;
                    float sliceEnd = sliceStart + (horizontalSlice ? s.size.x : s.size.y) - spriteEndLength;
                    FloatSegment sliceSegment = new FloatSegment(sliceStart, sliceEnd);
                    if (exclusive)
                    {
                        if (segmentToColor.Contains(sliceSegment, strict))
                            s.color = c;
                    }
                    else
                    {
                        if (segmentToColor.Crosses(sliceSegment, strict))
                            s.color = c;
                    }
                }
            }
        }
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
        if (sliceRenderers != null)
            foreach (SpriteRenderer s in sliceRenderers)
                if (s != null)
                    s.enabled = visible;
    }

    public void SetFlipX(bool flip)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = flip;
        if (sliceRenderers != null)
            foreach (SpriteRenderer s in sliceRenderers)
                if (s != null)
                    s.flipX = flip;
    }

    public void SetFlipY(bool flip)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipY = flip;
        if (sliceRenderers != null)
            foreach (SpriteRenderer s in sliceRenderers)
                if (s != null)
                    s.flipY = flip;
    }
}
