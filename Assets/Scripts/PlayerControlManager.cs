using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlManager : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 5f;
   [SerializeField] private GameObject playerSprite;
   private Rigidbody2D _rigidbody;
   private Vector2 _playerInput;

   private PlayerAnimationManager _playerAnimationManager;
   
   private void Awake()
   {
      _rigidbody = GetComponent<Rigidbody2D>();
      _playerAnimationManager = GetComponent<PlayerAnimationManager>();
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
      
      Vector3 playerScale = playerSprite.transform.localScale;

      if (_playerInput.x == 0)
      {
         _playerAnimationManager.IsWalk = false;
         return;
      }
      playerScale.x = Mathf.Abs(playerScale.x) * _playerInput.x * -1;// 기본 좌측(InputX == -1) 우측 반전(Input == 1)
      playerSprite.transform.localScale = playerScale;
      _playerAnimationManager.IsWalk = true;
   }
    
    
}
