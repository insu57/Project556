using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlManager : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 5f;
   [SerializeField] private GameObject playerSprite;
   private Rigidbody2D _rigidbody;
   private Vector2 _playerInput;
   
   
   private void Awake()
   {
      _rigidbody = GetComponent<Rigidbody2D>();
   }

   private void FixedUpdate()
   {
      PlayerMovement();
   }

   private void PlayerMovement()
   {
      _rigidbody.linearVelocityX = _playerInput.x * moveSpeed;
   }
   
   private void OnMove(InputValue value)
   {
      _playerInput = value.Get<Vector2>();
      Debug.Log(_playerInput);
      Vector3 playerScale = playerSprite.transform.localScale;
      if (_playerInput.x != 0)
      {
         playerScale.x = Mathf.Abs(playerScale.x) * _playerInput.x * -1;
         playerSprite.transform.localScale = playerScale;
      }

   }
    
    
}
