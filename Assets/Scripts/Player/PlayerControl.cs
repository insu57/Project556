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
   private Vector3 _baseRArmPosition;
   private float _shootAngle;
   
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
      _baseRArmPosition = rightArm.transform.localPosition;
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
      if (!_canRotateArm) return;

      bool isOneHanded = _playerManager.CheckIsOneHanded();

      Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;

      Vector2 direction;
      if (isOneHanded)
      {
         direction = mousePos - rightArm.transform.position;
         Debug.DrawRay(rightArm.transform.position, direction, Color.red); //조준선
      }
      else //TwoHanded
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
      else //TwoHanded
      {
         angle = Mathf.Clamp(angle, -40f, 40f); //85f center -125~-45
         _shootAngle = angle;
         
         float t = Mathf.InverseLerp(-40f, 40f, -angle);
         float targetAngle = Mathf.Lerp(-90, 70, t); //RightArm 회전보간

         float positionThreshold = 37f;
         
         if (targetAngle > positionThreshold)
         {
            float pt = Mathf.InverseLerp(positionThreshold, 70, targetAngle );
            rightArm.transform.localPosition = Vector3.Lerp(_baseRArmPosition, new Vector3(-0.272f, -0.072f, 0f), pt);
         }
         else
         {
            rightArm.transform.localPosition = _baseRArmPosition;
         }
         
         //기본 -0.161(x), 0.037 -> -0.2~-0.25 -> -0.272 -0.072 //무기마다 세부설정 -> Animation Curve? sturct로 저장해서 불러오기
         rightArm.transform.localRotation = Quaternion.Euler(0, 0, targetAngle); //가중치 필요...  0~-80, 0~70 //35부터
         leftArm.transform.localRotation = Quaternion.Euler(0, 0, -angle - 85f);   //-70부터...
         //SPUM캐릭터에서 scale.-x로 Flip시킨 것을 디폴트로 만듦. -> 그래서 캐릭터 좌우가 캐릭터 기준이 아닌 사용자/제작자 시점이 기준
         //조작하는 방향대로 설정과 기존 캐릭터를 기준으로...
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
