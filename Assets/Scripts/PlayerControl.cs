using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 5f;
   [SerializeField] private GameObject playerSprite;
   [SerializeField] private GameObject rightArm;
   [SerializeField] private GameObject leftArm;
   
   private Camera _mainCamera;
   private Rigidbody2D _rigidbody;
   private Vector2 _playerInput;
   private bool _isFlipped = false;
   
   public event Action<bool> OnPlayerMove;
   public event Action<bool> OnPlayerShot;
   public event Action<bool> OnPlayerReload;
   
   private void Awake()
   {
      _rigidbody = GetComponent<Rigidbody2D>();
   }

   private void Start()
   {
      _mainCamera = Camera.main;
   }

   private void LateUpdate() //애니메이션 적용이후라 팔 움직임... -> AvatarMask로?
   {
      Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;
      
      Vector2 direction = mousePos - rightArm.transform.position;
      Debug.DrawRay(rightArm.transform.position, direction, Color.red);
      
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

      if (_isFlipped)
      {
         if (angle > 0)
         {
            angle = 180 - angle;
         }
         else
         {
            angle = -180 - angle;
         }
      }
      angle = Mathf.Clamp(angle, -60f, 60f);
      rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);
      //avatar mask, flip적용(좌측도)
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
         OnPlayerMove?.Invoke(false);
         return;
      }
      
      if(_playerInput.y != 0) return;
      playerScale.x = Mathf.Abs(playerScale.x) * _playerInput.x;
      _isFlipped = _playerInput.x < 0; //왼쪽입력 시 Flip
      playerSprite.transform.localScale = playerScale;
      OnPlayerMove?.Invoke(true);
   }
    
    
}
