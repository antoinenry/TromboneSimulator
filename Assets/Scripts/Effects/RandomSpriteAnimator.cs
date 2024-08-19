using System.Collections;
using UnityEngine;

public class RandomSpriteAnimator : MonoBehaviour
{
    public Sprite[] sprites;
    public float frameDuration = .1f;
    public bool avoidRepetition = true;

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
            yield return new WaitForSeconds(frameDuration);
        }   
    }
}
