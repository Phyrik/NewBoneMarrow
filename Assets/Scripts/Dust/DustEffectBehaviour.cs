using UnityEngine;
using UnityEngine.SceneManagement;

public class DustEffectBehaviour : MonoBehaviour
{
    public float secondsBetweenSpriteChanges;
    public Sprite[] jumpDustEffectSprites;
    private int _currentSpriteIndex;
    private float _secondsSinceLastSpriteChange;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = jumpDustEffectSprites[0];
        _currentSpriteIndex = 0;

        float scaleSize = MainCharacterBackgroundBehaviour.SceneSizeDictionary[SceneManager.GetActiveScene().name];
        float timesBiggerThanMainCharacter = 5f;
        transform.localScale = new Vector3(scaleSize * timesBiggerThanMainCharacter, scaleSize * timesBiggerThanMainCharacter, transform.localScale.z);
    }

    // Update is called once per frame
    private void Update()
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