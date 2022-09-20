using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MainCharacterMovementBehaviour : MonoBehaviour
{
    public float speed;
    public float dragCoefficient;
    public float jumpSpeed;
    public Sprite standingSprite;
    public Sprite[] walkingSprites;
    public BoxCollider2D bodyCollider;
    public EdgeCollider2D feetCollider;
    private Rigidbody2D _rigidbody;
    private int _currentWalkingSpriteIndex;
    private float _secondsSinceLastSpriteChange;
    private SpriteRenderer _spriteRenderer;
    private float _secondsSinceLastJump;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentWalkingSpriteIndex = 0;
        _secondsSinceLastSpriteChange = 0f;
        _secondsSinceLastJump = 0f;
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
        _secondsSinceLastJump += Time.deltaTime;
    }

    private bool CheckAndApplyHorizontalMovement()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            // get new direction
            Direction direction = Input.GetKey(KeyCode.RightArrow) ? Direction.Right : Direction.Left;
            
            // move in direction
            _spriteRenderer.flipX = direction == Direction.Left;
            _rigidbody.velocity = new Vector2(direction == Direction.Left ? -speed : speed, _rigidbody.velocity.y);
            // cycle walking sprites
            if (_secondsSinceLastSpriteChange > 0.2f)
            {
                _secondsSinceLastSpriteChange = 0f;
                _currentWalkingSpriteIndex = (_currentWalkingSpriteIndex + 1) % walkingSprites.Length;
            }

            // apply current walking sprite
            _spriteRenderer.sprite = walkingSprites[_currentWalkingSpriteIndex];

            return true;
        }

        return false;
    }

    private bool CheckAndApplyVerticalMovement()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (TouchingGround() && _secondsSinceLastJump > 0.1f)
            {
                _secondsSinceLastJump = 0f;
                _rigidbody.position = new Vector2(_rigidbody.position.x, _rigidbody.position.y + 0.02f);
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.y + jumpSpeed);
            }
            
            return true;
        }

        return false;
    }

    private void ApplyFriction()
    {
        float newXVel = _rigidbody.velocity.x;
        if (newXVel < dragCoefficient * Time.deltaTime && newXVel > -dragCoefficient * Time.deltaTime)
        {
            newXVel = 0f;
        }
        else if (newXVel > 0f)
        {
            newXVel -= dragCoefficient * Time.deltaTime;
        }
        else if (newXVel < 0f)
        {
            newXVel += dragCoefficient * Time.deltaTime;
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
