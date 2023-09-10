using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCharacterMovementBehaviour : MonoBehaviour
{
    public static Dictionary<string, MainCharacterMovementBehaviour> Instances { get; private set; }

    public float acceleration;
    public float maxSpeed;
    public float dashSpeed;
    public float groundFrictionCoefficient;
    public float airFrictionCoefficient;
    public float jumpSpeed;
    public float secondsBetweenSpriteChanges;
    public float secondsToDashFor;
    public Sprite standingSprite;
    public Sprite flyingSprite;
    public Sprite dashSprite;
    public Sprite[] walkingSprites;
    public EdgeCollider2D feetCollider;
    public GameObject jumpDustEffectPrefabGameObject;
    public PreventMovementCollider[] preventMovementColliders;
    public float floatingPointTolerance;
    private Rigidbody2D _rigidbody;
    private int _currentWalkingSpriteIndex;
    private float _secondsSinceLastSpriteChange;
    private SpriteRenderer _spriteRenderer;
    private bool _doubleJumpUsedThisAirtime;
    private bool _jumpButtonReset;
    private bool _dashing;
    private bool _dashUsedThisAirtime;
    private bool _dashKeyLock;
    private float _secondsSinceDashingStarted;
    private Direction? _dashDirection;

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
        _doubleJumpUsedThisAirtime = false;
        _jumpButtonReset = true;
        _secondsSinceDashingStarted = 0f;
        _dashDirection = null;
        _dashKeyLock = false;

        if (LoadAndSaveManagerBehaviour.NeedsLoaded)
        {
            LoadAndSaveManagerBehaviour.NeedsLoaded = false;

            // load stuff
            _doubleJumpUsedThisAirtime = LoadAndSaveManagerBehaviour.CurrentSaveData.DoubleJumpUsed;
            transform.position = new Vector3(LoadAndSaveManagerBehaviour.CurrentSaveData.MainCharacterPosX,
                LoadAndSaveManagerBehaviour.CurrentSaveData.MainCharacterPosY, transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckAndApplyHorizontalMovement();
        CheckAndApplyVerticalMovement();
        
        ApplyFriction();

        // tick clocks
        _secondsSinceLastSpriteChange += Time.deltaTime;
    }

    private void CheckAndApplyHorizontalMovement()
    {
        // get input info
        bool leftOrRightPressedThisFrame = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow);
        bool leftAndRightPressedThisFrame = Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow);
        
        // dash key lock
        if (IsTouchingGround())
        {
            _dashKeyLock = Input.GetKey(KeyCode.Space);
        }
        
        // get new direction info and velocity info
        Direction? accDirection = Input.GetKey(KeyCode.RightArrow) ? Direction.Right : Input.GetKey(KeyCode.LeftArrow) ? Direction.Left : null;
        Direction velDirection = _rigidbody.velocity.x > 0f ? Direction.Right : Direction.Left;
        bool stationary = _rigidbody.velocity.x > -floatingPointTolerance && _rigidbody.velocity.x < floatingPointTolerance;

        // reset dash once on ground
        if (IsTouchingGround())
        {
            _dashUsedThisAirtime = false;
        }
        
        if (!_dashing)
        {
            // allows only one dash per airtime
            if (!IsTouchingGround())
            {
                // if dashing set dashing to true
                if (Input.GetKey(KeyCode.Space) && !_dashUsedThisAirtime && !_dashKeyLock)
                {
                    _dashDirection = accDirection ?? velDirection;
                    _dashing = true;
                }

                if (_dashKeyLock && !Input.GetKey(KeyCode.Space)) _dashKeyLock = false;
            }
            // else do normal horizontal stuff
            if (!_dashing)
            {
                if (leftOrRightPressedThisFrame && !leftAndRightPressedThisFrame)
                {
                    // move in direction
                    _rigidbody.velocity = new Vector2(accDirection.Value == Direction.Left // if direction is left...
                            ? IsLeftDisabled() // ...and left is not disabled...
                                ? _rigidbody.velocity.x
                                : Math.Clamp(_rigidbody.velocity.x - acceleration * Time.deltaTime, -maxSpeed,
                                    float
                                        .PositiveInfinity) // ...then apply negative acceleration with max negative speed clamp.
                            // Otherwise if direction is right...
                            : IsRightDisabled() // ...and right is not disabled...
                                ? _rigidbody.velocity.x
                                : Math.Clamp(_rigidbody.velocity.x + acceleration * Time.deltaTime,
                                    float.NegativeInfinity,
                                    maxSpeed), // ...then apply positive acceleration with max positive speed clamp
                        _rigidbody.velocity.y);
                }

                if (!stationary)
                {
                    transform.localScale = velDirection == Direction.Left
                        ? new Vector3(-Math.Abs(transform.localScale.x), transform.localScale.y,
                            transform.localPosition.z)
                        : new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y,
                            transform.localPosition.z);

                    // cycle walking sprites
                    if (_secondsSinceLastSpriteChange > secondsBetweenSpriteChanges)
                    {
                        _secondsSinceLastSpriteChange = 0f;
                        _currentWalkingSpriteIndex = (_currentWalkingSpriteIndex + 1) % walkingSprites.Length;
                    }

                    // apply current walking sprite and enable waving ears
                    _spriteRenderer.sprite = walkingSprites[_currentWalkingSpriteIndex];

                    // use normal sprite if in air and disable waving ears
                    if (!IsTouchingGround())
                    {
                        _spriteRenderer.sprite = flyingSprite;
                    }
                }
                else
                {
                    // reset times and sprites
                    _secondsSinceLastSpriteChange = 0f;
                    _spriteRenderer.sprite = IsTouchingGround() ? standingSprite : flyingSprite;
                }
            }
        }
        // if dashing all other horizontal physics will be ignored
        else
        {
            if (_secondsSinceDashingStarted < secondsToDashFor)
            {
                _spriteRenderer.sprite = dashSprite;
                _rigidbody.velocity =
                    new Vector2(
                        _dashDirection == null ? _spriteRenderer.flipX ? -dashSpeed : dashSpeed :
                        _dashDirection.Value == Direction.Left ? -dashSpeed : dashSpeed, 0f);
                _secondsSinceDashingStarted += Time.deltaTime;
            }
            else
            {
                _dashing = false;
                _dashUsedThisAirtime = true;
                _dashDirection = null;
                _secondsSinceDashingStarted = 0f;
            }
        }
    }

    private void CheckAndApplyVerticalMovement()
    {
        // jump pressed
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // jump if on ground or double jump available
            if ((IsTouchingGround() || !_doubleJumpUsedThisAirtime) && _jumpButtonReset)
            {
                _jumpButtonReset = false;
                if (!IsTouchingGround()) _doubleJumpUsedThisAirtime = true;
                _rigidbody.position = new Vector2(_rigidbody.position.x, _rigidbody.position.y + 0.02f);
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpSpeed);
                Instantiate(jumpDustEffectPrefabGameObject, gameObject.transform.position, Quaternion.identity);
            }
        }
        else
        {
            _jumpButtonReset = true;
        }
        
        // reset double jump
        if (IsTouchingGround())
        {
            _doubleJumpUsedThisAirtime = false;
        }
    }

    private void ApplyFriction()
    {
        float friction = IsTouchingGround() ? groundFrictionCoefficient : airFrictionCoefficient;
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

    private bool IsTouchingGround()
    {
        return feetCollider.IsTouchingLayers();
    }

    private bool IsLeftDisabled()
    {
        return preventMovementColliders
            .Where(pmc => pmc.sceneName == SceneManager.GetActiveScene().name)
            .Where(pmc => pmc.disableLeft)
            .Any(pmc => _rigidbody.IsTouching(pmc.collider));
    }

    private bool IsRightDisabled()
    {
        return preventMovementColliders
            .Where(pmc => pmc.sceneName == SceneManager.GetActiveScene().name)
            .Where(pmc => pmc.disableRight)
            .Any(pmc => _rigidbody.IsTouching(pmc.collider));
    }

    [Serializable]
    public class PreventMovementCollider
    {
        public Collider2D collider;
        public bool disableLeft;
        public bool disableRight;
        public string sceneName;
    }
}

internal enum Direction
{
    Left,
    Right
}
