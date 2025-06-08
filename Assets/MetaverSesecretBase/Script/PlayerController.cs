using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody2D _rigidbody;
    private float _moveSpeed = 5f;
    private Vector2 _moveInput;

    private Vector2 _touchStartPos;
    private Vector2 _touchCurrentPos;
    private bool _isTouching = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsOwner) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        // PC：キーボード入力
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        _moveInput = new Vector2(moveX, moveY).normalized;

#else
        // モバイル：タッチスライド入力
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _touchStartPos = touch.position;
                    _isTouching = true;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_isTouching)
                    {
                        _touchCurrentPos = touch.position;
                        Vector2 delta = _touchCurrentPos - _touchStartPos;
                        _moveInput = delta.normalized;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _moveInput = Vector2.zero;
                    _isTouching = false;
                    break;
            }
        }
#endif
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        _rigidbody.linearVelocity = _moveInput * _moveSpeed;
    }
}
