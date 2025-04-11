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
   [SerializeField] private GameObject bullet;
   
   
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
      RotateArm();
   }

   private void FixedUpdate()
   {
      PlayerMovement();
   }

   private void RotateArm()
   {
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
      //avatar mask?
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
         Transform muzzleTransform = weapon.GetComponentInChildren<PlayerWeapon>().MuzzleTransform;
         
         GameObject bulletPrefab = Instantiate(bullet, muzzleTransform.transform.position, Quaternion.identity);
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
      if (value.isPressed)
      {
         _rigidbody.AddForce(new Vector2(0f, jumpSpeed), ForceMode2D.Impulse);
      }
      //점프 개선
      // 벽 충돌 개선
   }

   private void OnReload(InputValue value)
   {
      
   }
    
}
