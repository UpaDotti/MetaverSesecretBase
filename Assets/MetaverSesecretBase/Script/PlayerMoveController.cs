using Unity.Netcode;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    private GameObject _player;
    private Rigidbody2D _rigidbody;
    private float _moveSpeed = 5f;

    private bool _isStart = false;
    private Vector2 _moveInput;
    private Vector2 _touchStartPos;
    private Vector2 _touchCurrentPos;
    private bool _isTouching = false;



    public void StartMove(GameObject player)
    {
        _player = player;
        _rigidbody = player.GetComponent<Rigidbody2D>();
        _isStart = true;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // PC�F�L�[�{�[�h����
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        _moveInput = new Vector2(moveX, moveY).normalized;

#else
        // ���o�C���F�^�b�`�X���C�h����
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
        if (!_isStart) return;
        _rigidbody.linearVelocity = _moveInput * _moveSpeed;
    }
}
