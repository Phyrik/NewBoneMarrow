using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class MainCharacterMovementBehaviour : MonoBehaviour
{
    public static Dictionary<string, MainCharacterMovementBehaviour> Instances { get; private set; }

    public float acceleration;
    public float maxSpeed;
    public float groundFrictionCoefficient;
    public float airFrictionCoefficient;
    public float jumpSpeed;
    public Sprite standingSprite;
    public Sprite flyingSprite;
    public Sprite[] walkingSprites;
    public EdgeCollider2D feetCollider;
    public GameObject jumpDustEffectPrefabGameObject;
    private Rigidbody2D _rigidbody;
    private int _currentWalkingSpriteIndex;
    private float _secondsSinceLastSpriteChange;
    private SpriteRenderer _spriteRenderer;
    private bool _doubleJumpUsed;
    private bool _jumpButtonReset;

    // Start is called before the first frame update
    void Start()
    {
        // singleton per scene
        Instances ??= new Dictionary<string, MainCharacterMovementBehaviour>();

        if (Instances.ContainsKey(SceneManager.GetActiveScene().name) &&
            Instances[SceneManager.GetActiveScene().name] != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instances[SceneManager.GetActiveScene().name] = this;
        }
    
        // set defaults for this object's attributes
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentWalkingSpriteIndex = 0;
        _secondsSinceLastSpriteChange = 0f;
        _doubleJumpUsed = false;
        _jumpButtonReset = true;

        if (LoadAndSaveManagerBehaviour.NeedsLoaded)
        {
            LoadAndSaveManagerBehaviour.NeedsLoaded = false;

            // load stuff
            _doubleJumpUsed = LoadAndSaveManagerBehaviour.CurrentSaveData.DoubleJumpUsed;
            transform.position = new Vector3(LoadAndSaveManagerBehaviour.CurrentSaveData.MainCharacterPosX,
                LoadAndSaveManagerBehaviour.CurrentSaveData.MainCharacterPosY, transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool movingHorizontally = CheckAndApplyHorizontalMovement();
        bool movingVertically = CheckAndApplyVerticalMovement();
        
        if (!movingHorizontally)
        {
            ApplyFriction();

            // reset times and sprites
            _secondsSinceLastSpriteChange = 0f;
            _spriteRenderer.sprite = standingSprite;
        }

        // tick clocks
        _secondsSinceLastSpriteChange += Time.deltaTime;
    }

    private bool CheckAndApplyHorizontalMovement()
    {
        bool leftOrRightPressedThisFrame = false;
        
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            // get new direction
            Direction direction = Input.GetKey(KeyCode.RightArrow) ? Direction.Right : Direction.Left;
            
            // move in direction
            _spriteRenderer.flipX = direction == Direction.Left;
            _rigidbody.velocity = new Vector2(direction == Direction.Left // if direction is left...
                    ? Math.Clamp(_rigidbody.velocity.x - acceleration, -maxSpeed,
                        float.PositiveInfinity) // ...then apply negative acceleration with max negative speed clamp
                    : Math.Clamp(_rigidbody.velocity.x + acceleration, float.NegativeInfinity,
                        maxSpeed), // ...otherwise apply positive acceleration with max positive speed clamp
                _rigidbody.velocity.y);
            // cycle walking sprites
            if (_secondsSinceLastSpriteChange > 0.2f)
            {
                _secondsSinceLastSpriteChange = 0f;
                _currentWalkingSpriteIndex = (_currentWalkingSpriteIndex + 1) % walkingSprites.Length;
            }

            // apply current walking sprite
            _spriteRenderer.sprite = walkingSprites[_currentWalkingSpriteIndex];
            
            // use normal sprite if in air
            if (!TouchingGround())
            {
                _spriteRenderer.sprite = flyingSprite;
            }

            leftOrRightPressedThisFrame = true;
        }

        return leftOrRightPressedThisFrame;
    }

    private bool CheckAndApplyVerticalMovement()
    {
        bool jumpPressedThisFrame = false;
        
        // jump pressed
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // jump if on ground or double jump available
            if ((TouchingGround() || !_doubleJumpUsed) && _jumpButtonReset)
            {
                _jumpButtonReset = false;
                if (!TouchingGround()) _doubleJumpUsed = true;
                _rigidbody.position = new Vector2(_rigidbody.position.x, _rigidbody.position.y + 0.02f);
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpSpeed);
                Instantiate(jumpDustEffectPrefabGameObject, gameObject.transform.position, Quaternion.identity);
            }
            
            jumpPressedThisFrame = true;
        }
        else
        {
            _jumpButtonReset = true;
        }
        
        // reset double jump
        if (TouchingGround())
        {
            _doubleJumpUsed = false;
        }

        return jumpPressedThisFrame;
    }

    private void ApplyFriction()
    {
        float friction = TouchingGround() ? groundFrictionCoefficient : airFrictionCoefficient;
        float newXVel = _rigidbody.velocity.x;
        if (newXVel < friction * Time.deltaTime && newXVel > -friction * Time.deltaTime)
        {
            newXVel = 0f;
        }
        else if (newXVel > 0f)
        {
            newXVel -= friction * Time.deltaTime;
        }
        else if (newXVel < 0f)
        {
            newXVel += friction * Time.deltaTime;
        }

        _rigidbody.velocity = new Vector2(newXVel, _rigidbody.velocity.y);
    }

    private bool TouchingGround()
    {
        return feetCollider.IsTouchingLayers();
    }
}

internal enum Direction
{
    Left,
    Right
}
