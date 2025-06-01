using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;
    private Rigidbody2D _rigidbody;

    private float _moveSpeed = 5f;

    private Vector2 _moveInput;

    void Start()
    {
        _rigidbody = _player.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        _moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        _rigidbody.linearVelocity = _moveInput * _moveSpeed;
    }
}
