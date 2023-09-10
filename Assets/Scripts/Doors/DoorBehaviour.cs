using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBehaviour : MonoBehaviour
{
    public Collider2D mainCharacterCollider;
    public Collider2D doormatCollider;
    public Sprite normalDoorSprite;
    public Sprite highlightedDoorSprite;
    private Collider2D _collider;
    private bool _downKeyLock;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _downKeyLock = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!mainCharacterCollider.IsTouching(doormatCollider))
        {
            _downKeyLock = Input.GetKey(KeyCode.DownArrow);
        }

        if (mainCharacterCollider.IsTouching(doormatCollider))
        {
            if (!_downKeyLock && Input.GetKey(KeyCode.DownArrow))
            {
                mainCharacterCollider.transform.position =
                    new Vector3(0f, 0f, mainCharacterCollider.transform.position.z);
                SceneManager.LoadSceneAsync("Main Character's House", LoadSceneMode.Single);
            }

            if (_downKeyLock && !Input.GetKey(KeyCode.DownArrow))
            {
                _downKeyLock = false;
            }
        }

        _spriteRenderer.sprite = _collider.IsTouching(mainCharacterCollider) ? highlightedDoorSprite : normalDoorSprite;
    }
}