using System.Collections;
using UnityEngine;

public class RandomSpriteAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    public float frameDuration = .1f;
    public bool avoidRepetition = true;
    public bool randomSize = false;
    public Vector2 minSize;
    public Vector2 maxSize;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    private void OnEnable() => StartCoroutine(AnimateCoroutine());
    private void OnDisable() => StopCoroutine(AnimateCoroutine());

    private IEnumerator AnimateCoroutine()
    {
        int spriteIndex = 0;
        int spriteCount = 0;
        while (enabled && spriteRenderer != null)
        {
            // Swap sprite
            spriteCount = sprites != null ? sprites.Length : 0;
            if (avoidRepetition && spriteCount > 1)
            {
                int randomIndex = Random.Range(0, spriteCount - 1);
                if (randomIndex == spriteIndex) randomIndex++;
                spriteIndex = randomIndex;
            }
            else
            {
                spriteIndex = Random.Range(0, spriteCount);
            }
            spriteRenderer.sprite = sprites[spriteIndex];
            // Change size
            if (randomSize)
            {
                spriteRenderer.size = new Vector2()
                {
                    x = Random.Range(minSize.x, minSize.y),
                    y = Random.Range(maxSize.x, maxSize.y)
                };
            }
            yield return new WaitForSeconds(frameDuration);
        }   
    }
}
