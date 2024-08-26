using UnityEngine;

[ExecuteAlways]
public class CopySpriteRenderer : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer source;
    public SpriteRenderer destination;
    [Header("Values")]
    public bool copySprite = true;
    public bool copySize = true;

    void Update()
    {
        if (source == null || destination == null) return;
        if (copySprite) destination.sprite = source.sprite;
        if (copySize) destination.size = source.size;
    }
}
