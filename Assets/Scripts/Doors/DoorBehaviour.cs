using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class DoorBehaviour : MonoBehaviour
{
    public Collider2D mainCharacterCollider;
    public Collider2D doormatCollider;
    public Sprite normalDoorSprite;
    public Sprite highlightedDoorSprite;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private bool _downKeyLock;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _downKeyLock = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mainCharacterCollider.IsTouching(doormatCollider))
        {
            _downKeyLock = Input.GetKey(KeyCode.DownArrow);
        }
        
        if (mainCharacterCollider.IsTouching(doormatCollider))
        {
            if (!_downKeyLock && Input.GetKey(KeyCode.DownArrow))
            {
                DontDestroyOnLoad(mainCharacterCollider.gameObject);
                mainCharacterCollider.transform.position =
                    new Vector3(0f, 0f, mainCharacterCollider.transform.position.z);
                SceneManager.LoadSceneAsync("Main Character's House", LoadSceneMode.Single);
            }

            if (_downKeyLock && !Input.GetKey(KeyCode.DownArrow)) _downKeyLock = false;
        }

        _spriteRenderer.sprite = _collider.IsTouching(mainCharacterCollider) ? highlightedDoorSprite : normalDoorSprite;
    }
}
