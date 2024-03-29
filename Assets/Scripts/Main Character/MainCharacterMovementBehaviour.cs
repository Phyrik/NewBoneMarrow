using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainCharacterMovementBehaviour : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float dashSpeed;
    public float groundFrictionCoefficient;
    public float airFrictionCoefficient;
    public float dashedAirFrictionCoefficient;
    public float jumpSpeed;
    public float secondsBetweenSpriteChanges;
    public float secondsToDashFor;
    public Sprite standingSprite;
    public Sprite flyingSprite;
    public Sprite dashSprite;
    public Sprite[] walkingSprites;
    public EdgeCollider2D feetCollider;
    public GameObject dustEffectPrefabGameObject;
    public float dustCloseness;
    public PreventMovementCollider[] preventMovementColliders;
    public float floatingPointTolerance;
    private int _currentWalkingSpriteIndex;
    private Direction? _dashDirection;
    private bool _dashing;
    private bool _dashKeyLock;
    private bool _dashUsedThisAirtime;
    private bool _doubleJumpUsedThisAirtime;
    private bool _jumpButtonReset;
    private bool _justDashed;
    private Rigidbody2D _rigidbody;
    private float _secondsSinceDashingStarted;
    private float _secondsSinceLastSpriteChange;
    private SpriteRenderer _spriteRenderer;
    public static Dictionary<string, MainCharacterMovementBehaviour> Instances { get; private set; }

    // Start is called before the first frame update
    private void Start()
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
    private void Update()
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
        Direction? accDirection = Input.GetKey(KeyCode.RightArrow) ? Direction.Right :
            Input.GetKey(KeyCode.LeftArrow) ? Direction.Left : null;
        Direction velDirection = IsFloatEqualTo(_rigidbody.velocity.x, 0f)
            ?
            _spriteRenderer.flipX ? Direction.Left : Direction.Right
            : IsFloatGreaterThan(_rigidbody.velocity.x, 0f)
                ? Direction.Right
                : IsFloatLessThan(_rigidbody.velocity.x, 0f)
                    ? Direction.Left
                    : throw new Exception();
        bool stationary = IsFloatEqualTo(_rigidbody.velocity.x, 0f);

        // reset dash once on ground
        if (IsTouchingGround())
        {
            _dashUsedThisAirtime = false;
        }
        
        // check if not just dashed
        if (!_dashUsedThisAirtime || Input.GetKey(KeyCode.Space))
        {
            _justDashed = false;
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
                    _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0f);
                    Instantiate(dustEffectPrefabGameObject,
                        new Vector3(transform.position.x + (_dashDirection == Direction.Left ? -dustCloseness : dustCloseness),
                            transform.position.y, transform.position.z),
                        Quaternion.AngleAxis(90f, _dashDirection == Direction.Right ? Vector3.forward : Vector3.back));
                }

                if (_dashKeyLock && !Input.GetKey(KeyCode.Space))
                {
                    _dashKeyLock = false;
                }
            }

            // else do normal horizontal stuff
            if (!_dashing)
            {
                if (leftOrRightPressedThisFrame && !leftAndRightPressedThisFrame && (!_justDashed ||
                        (velDirection == Direction.Right
                            ? Input.GetKey(KeyCode.LeftArrow)
                            : Input.GetKey(KeyCode.RightArrow))))
                {
                    // move in direction
                    _rigidbody.velocity = new Vector2(
                        accDirection.Value == Direction.Left
                            ?
                            IsLeftDisabled()
                                ? _rigidbody.velocity.x
                                : Math.Clamp(
                                    _rigidbody.velocity.x - acceleration * transform.localScale.x * 5f * Time.deltaTime,
                                    -maxSpeed * transform.localScale.x * 5f, float.PositiveInfinity)
                            : IsRightDisabled()
                                ? _rigidbody.velocity.x
                                : Math.Clamp(
                                    _rigidbody.velocity.x + acceleration * transform.localScale.x * 5f * Time.deltaTime,
                                    float.NegativeInfinity, maxSpeed * transform.localScale.x * 5f),
                        _rigidbody.velocity.y);
                }

                if (!stationary)
                {
                    _spriteRenderer.flipX = velDirection == Direction.Left;
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
                _spriteRenderer.flipX = _dashDirection.Value == Direction.Left;
                _rigidbody.velocity =
                    new Vector2(
                        (_dashDirection.Value == Direction.Left ? -dashSpeed : dashSpeed) * transform.localScale.x * 5f,
                        Math.Clamp(_rigidbody.velocity.y, 0f, float.PositiveInfinity));
                _secondsSinceDashingStarted += Time.deltaTime;
            }
            else
            {
                _dashing = false;
                _dashUsedThisAirtime = true;
                _dashDirection = null;
                _secondsSinceDashingStarted = 0f;
                _justDashed = true;
            }
        }
    }

    private void CheckAndApplyVerticalMovement()
    {
        // reset double jump
        if (IsTouchingGround())
        {
            _doubleJumpUsedThisAirtime = false;
        }
        
        // jump pressed
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // jump if on ground or double jump available
            if ((IsTouchingGround() || !_doubleJumpUsedThisAirtime) && _jumpButtonReset)
            {
                _jumpButtonReset = false;
                if (!IsTouchingGround())
                {
                    _doubleJumpUsedThisAirtime = true;
                }

                _rigidbody.position = new Vector2(_rigidbody.position.x, _rigidbody.position.y + 0.02f);
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpSpeed * transform.localScale.x * 5f);
                Instantiate(dustEffectPrefabGameObject, transform.position, Quaternion.identity);
            }
        }
        else
        {
            _jumpButtonReset = true;
        }
    }

    private void ApplyFriction()
    {
        float friction = (_justDashed ? dashedAirFrictionCoefficient :
            IsTouchingGround() ? groundFrictionCoefficient : airFrictionCoefficient) * transform.localScale.x * 5f;
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
        return feetCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("Ground"));
    }

    private bool IsLeftDisabled()
    {
        return preventMovementColliders.Where(pmc => pmc.DisableLeft).Any(pmc => _rigidbody.IsTouching(pmc.Collider));
    }

    private bool IsRightDisabled()
    {
        return preventMovementColliders.Where(pmc => pmc.DisableRight).Any(pmc => _rigidbody.IsTouching(pmc.Collider));
    }

    public class PreventMovementCollider
    {
        private string GameObjectName;
        public bool DisableLeft;
        public bool DisableRight;
        
        public PreventMovementCollider(string gameObjectName, bool disableLeft, bool disableRight)
        {
            GameObjectName = gameObjectName;
            DisableLeft = disableLeft;
            DisableRight = disableRight;
        }
        
        public Collider2D Collider
        {
            get { return GameObject.Find(GameObjectName).GetComponent<Collider2D>(); }
        }
    }
    
    private bool IsFloatEqualTo(float floatValue, float comparisonValue)
    {
        return floatValue > comparisonValue - floatingPointTolerance &&
               floatValue < comparisonValue + floatingPointTolerance;
    }

    private bool IsFloatGreaterThan(float floatValue, float comparisonValue)
    {
        return floatValue > comparisonValue - floatingPointTolerance;
    }

    private bool IsFloatLessThan(float floatValue, float comparisonValue)
    {
        return floatValue < comparisonValue + floatingPointTolerance;
    }
}

internal enum Direction
{
    Left,
    Right
}