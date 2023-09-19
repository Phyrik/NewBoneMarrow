using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBehaviour : MonoBehaviour
{
    public Collider2D doormatCollider;
    public Sprite normalDoorSprite;
    public Sprite highlightedDoorSprite;
    public string sceneToGoTo;
    public Vector2 locationToSpawnAt;
    private Collider2D _collider;
    private bool _downKeyLock;
    private SpriteRenderer _spriteRenderer;
    private GameObject _mainCharacter;
    private Collider2D _mainCharacterCollider;

    // Start is called before the first frame update
    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _downKeyLock = false;
        _mainCharacter = GameObject.Find("Main Character");
        _mainCharacterCollider = _mainCharacter.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_mainCharacterCollider.IsTouching(doormatCollider))
        {
            _downKeyLock = Input.GetKey(KeyCode.DownArrow);
        }

        if (_mainCharacterCollider.IsTouching(doormatCollider))
        {
            if (!_downKeyLock && Input.GetKey(KeyCode.DownArrow))
            {
                _mainCharacter.transform.position = new Vector3(locationToSpawnAt.x, locationToSpawnAt.y,
                    _mainCharacterCollider.transform.position.z);
                _mainCharacter.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                SceneManager.LoadSceneAsync(sceneToGoTo, LoadSceneMode.Single);
            }

            if (_downKeyLock && !Input.GetKey(KeyCode.DownArrow))
            {
                _downKeyLock = false;
            }
        }

        _spriteRenderer.sprite = _collider.IsTouching(_mainCharacterCollider) ? highlightedDoorSprite : normalDoorSprite;
    }
}