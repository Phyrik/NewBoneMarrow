using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDustEffectBehaviour : MonoBehaviour
{
    public float secondsBetweenSpriteChanges;
    public Sprite[] jumpDustEffectSprites;
    private SpriteRenderer _spriteRenderer;
    private float _secondsSinceLastSpriteChange;
    private int _currentSpriteIndex;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = jumpDustEffectSprites[0];
        _currentSpriteIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _secondsSinceLastSpriteChange += Time.deltaTime;

        if (_secondsSinceLastSpriteChange > secondsBetweenSpriteChanges)
        {
            _currentSpriteIndex++;
            if (_currentSpriteIndex > jumpDustEffectSprites.Length - 1)
            {
                Destroy(gameObject);
                return;
            }
            _spriteRenderer.sprite = jumpDustEffectSprites[_currentSpriteIndex];
            _secondsSinceLastSpriteChange = 0f;
        }
    }
}
