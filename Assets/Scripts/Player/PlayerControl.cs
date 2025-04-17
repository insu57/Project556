using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 5f;
   [SerializeField] private float jumpSpeed = 5f;
   
   [SerializeField] private GameObject rightArm;
   [SerializeField] private GameObject leftArm;
   private float _shootAngle;
   
   //임시?
   [SerializeField] private float bulletSpeed = 10f;
   //[SerializeField] private float fireRate = 0.5f;
  
   [SerializeField] private GameObject bullet; //탄종별로 바꾸기, 추후 수정
   
   private PlayerManager _playerManager;
   private Camera _mainCamera;
   private Rigidbody2D _rigidbody;
   private Vector2 _playerInput;
   private bool _inShooting = false;
   private bool _canShoot = true;
   private bool _isFlipped = false;
   private bool _isGrounded = false;
   private bool _canRotateArm = true;
   
   public event Action<bool> OnPlayerMove;
   //public event Action<bool> OnPlayerShoot;
   public event Action OnPlayerReload;
   
   private void Awake()
   {
      _playerManager = GetComponent<PlayerManager>();
      _rigidbody = GetComponent<Rigidbody2D>();
   }

   private void Start()
   {
      _mainCamera = Camera.main;
   }
   
   private void Update()
   {
      GroundCheck();
      Shoot();
   }

   private void LateUpdate() 
   {
      //애니메이션 이후 처리
      RotateArm();
   }

   private void FixedUpdate()
   {
      //물리 기반 처리
      PlayerMovement();
   }

   private void GroundCheck()
   {
      float groundCheckDistance = 0.15f;
      _isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckDistance, LayerMask.GetMask("Ground"));
      Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, Color.blue);
      Debug.DrawRay(transform.position, Vector2.left * groundCheckDistance, Color.blue);
      Debug.DrawRay(transform.position, Vector2.right * groundCheckDistance, Color.blue);
   }

   private void RotateArm()
   {
      //장전 등 몇몇 행동에서는 안움직여야한다.
      //한손 - 두손 따라 수정...
      if (!_canRotateArm) return;

      bool isOneHanded = _playerManager.CheckIsOneHanded();

      Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;

      Vector2 direction;

      if (isOneHanded)
      {
         direction = mousePos - rightArm.transform.position;
         Debug.DrawRay(rightArm.transform.position, direction, Color.red);
      }
      else
      {
         direction = mousePos - leftArm.transform.position;
         Debug.DrawRay(leftArm.transform.position, direction, Color.red);
      }
 
      
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

      if (_isFlipped) //좌측
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

      if (isOneHanded)
      {
         angle = Mathf.Clamp(angle, -60f, 60f);
      
         _shootAngle = angle;
         rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);
      }
      else
      {
         Debug.Log(angle);
         angle = Mathf.Clamp(angle, -60f, 60f); //85f center
         _shootAngle = angle;
         rightArm.transform.localRotation = Quaternion.Euler(0, 0, -(angle-45f)); //이게 문제임
         leftArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);
      }
      
   }
   
   private void PlayerMovement()
   {
      _rigidbody.linearVelocityX = _playerInput.x * moveSpeed;
   }
   
   private void OnMove(InputValue value)
   {
      _playerInput = value.Get<Vector2>();
      
      Vector3 playerScale = transform.localScale;

      if (_playerInput.x == 0)
      {
         OnPlayerMove?.Invoke(false);
         return;
      }
      
      if(_playerInput.y != 0) return;
      playerScale.x = Mathf.Abs(playerScale.x) * _playerInput.x;
      _isFlipped = _playerInput.x < 0; //왼쪽입력 시 Flip
      transform.localScale = playerScale;
      OnPlayerMove?.Invoke(true);
   }

   private void OnShoot(InputValue value)
   {
      //WeaponData를 어떤식으로 연결???
      _inShooting = value.isPressed;
      if (!_playerManager.CheckIsAutomatic())
      {
         _playerManager.Shoot(_isFlipped, _shootAngle);
      }
   }

   private void Shoot()
   {
      if(!_playerManager.CheckIsAutomatic()) return;
      if(!_inShooting) return;
      _playerManager.Shoot(_isFlipped, _shootAngle);
   }
   
   private void OnJump(InputValue value)
   {
      if (value.isPressed && _isGrounded)
      {
         _rigidbody.AddForce(new Vector2(0f, jumpSpeed), ForceMode2D.Impulse);
      }
   }

   private void OnReload(InputValue value)
   {
      _canRotateArm = false;
      OnPlayerReload?.Invoke();
   }

   public void OnReloadEnd()
   {
      _canRotateArm = true;
      _playerManager.Reload();
   }
   
   public void CanRotateArm()
   {
      _canRotateArm = true;
   }
    
}
