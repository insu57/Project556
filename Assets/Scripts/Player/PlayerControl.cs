using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
   [SerializeField] private float moveSpeed = 5f;
   [SerializeField] private GameObject playerSprite;
   [SerializeField] private GameObject rightArm;
   [SerializeField] private GameObject leftArm;
   [SerializeField] private float jumpSpeed = 5f;
   //임시?
   [SerializeField] private GameObject weapon;
   [SerializeField] private float bulletSpeed = 10f;
   [SerializeField] private float fireRate = 0.5f;
   private float _shootAngle;
   [SerializeField] private GameObject bullet; //탄종별로 바꾸기
   
   private PlayerManager _playerManager;
   private Camera _mainCamera;
   private Rigidbody2D _rigidbody;
   private Vector2 _playerInput;
   private bool _isFlipped = false;
   private bool _isGrounded = false;
   private bool _canRotateArm = true;
   
   public event Action<bool> OnPlayerMove;
   public event Action<bool> OnPlayerShot;
   public event Action<bool> OnPlayerReload;
   
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
      _isGrounded = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Ground"));
      Debug.DrawRay(transform.position, Vector2.down * 0.2f, Color.blue);
      Debug.DrawRay(transform.position, Vector2.left * 0.2f, Color.blue);
      Debug.DrawRay(transform.position, Vector2.right * 0.2f, Color.blue);
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

   private void RotateArm()
   {
      //장전 등 몇몇 행동에서는 안움직여야한다.
      if(!_canRotateArm) return;
      
      Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;
      
      Vector2 direction = mousePos - rightArm.transform.position;
      Debug.DrawRay(rightArm.transform.position, direction, Color.red);
      
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
      angle = Mathf.Clamp(angle, -60f, 60f);
      
      _shootAngle = angle;
      rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);
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

   private void OnShoot(InputValue value)
   {
      if (value.isPressed)
      {
         //bullet Rotation 조정, ShootDirection 수정
         Transform muzzleTransform = _playerManager.CurrentWeapon.MuzzleTransform;
            
         GameObject bulletPrefab = Instantiate(bullet, muzzleTransform.transform.position, Quaternion.identity);
         Destroy(bulletPrefab, 2f); //임시 -> ObjectPooling으로
         Rigidbody2D bulletRB = bulletPrefab.GetComponent<Rigidbody2D>();
         if (_isFlipped)
         {
            //Flip이면 x반대방향으로
            Vector2 direction = 
               new Vector2(-Mathf.Cos(_shootAngle*Mathf.Deg2Rad), Mathf.Sin(_shootAngle*Mathf.Deg2Rad));
            bulletPrefab.transform.localRotation = Quaternion.Euler(0, 0, 180 - _shootAngle);
            bulletRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
         }
         else
         {
            Vector2 direction = 
               new Vector2(Mathf.Cos(_shootAngle*Mathf.Deg2Rad), Mathf.Sin(_shootAngle*Mathf.Deg2Rad));
            bulletPrefab.transform.localRotation = Quaternion.Euler(0, 0, _shootAngle);
            bulletRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
         }
        
      }
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
      //Reload
   }
    
}
