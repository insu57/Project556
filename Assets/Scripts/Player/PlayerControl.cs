using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Player
{
   public class PlayerControl : MonoBehaviour
   {
      [ShowInInspector] private float _currentMoveSpeed;
      public float MoveSpeed { set; get; }
      public float SprintSpeedMultiplier { set; get; }
      public float JumpSpeed { set; get; } 
   
      [SerializeField] private GameObject rightArm;
      [SerializeField] private GameObject leftArm;
      private Vector3 _baseRArmPosition;
      private float _shootAngle;
      
      private PlayerInput _playerInput;
      private InputAction _moveAction; //개선방안?
      private InputAction _sprintAction;
      private InputAction _jumpAction;
      private InputAction _shootAction;
      private InputAction _reloadAction;
      private InputAction _toggleFireModeAction;
      private InputAction _interactAction;
      private InputAction _openUIAction;
      private InputAction _openSettingAction;
      private InputAction _scrollWheelAction;
      private InputAction _changeWeaponAction;
      private InputAction _quickSlotAction;

      private Camera _mainCamera;
      private Rigidbody2D _rigidbody;
      private Collider2D[] _colliders;
   
      private LayerMask _groundMask;
      private LayerMask _climbMask;
   
      private Vector2 _playerMoveVector;
      [SerializeField, Space] private float airDeceleration = 5f; //공중 감속수치
      public bool IsUnarmed { set; get; }
      public bool IsOneHanded { set; get; }
      public FireMode CurrentFireMode { set; get; }
      private bool _inShooting = false;
      private bool _canShoot = true;
      private bool _isFlipped = false;
      private bool _isGrounded = false;
      private bool _canClimb = false;
      private bool _canRotateArm = true;
      
      private float _burstFiringTime;
      
      private UIControl _uiControl;
      
      public event Action<bool> OnPlayerMove;
      public event Action<bool> OnPlayerSprint;
      public event Action OnPlayerReload;
      public event Action OnFieldInteract;
      public event Action<float> OnScrollInteractMenu;
      public event Action<EquipWeaponIdx> OnChangeWeaponAction;
      public event Action<QuickSlotIdx> OnQuickSlotAction;
      public event Action<bool, float> OnShootAction; //isFlipped, shootAngle
      public event Action OnReloadEndAction;
      public event Action OnToggleFireModeAction;
      
      private void Awake()
      {
         TryGetComponent(out _rigidbody);
      
         _playerInput = GetComponent<PlayerInput>(); //PlayerInput - Player Action Map
         _playerInput.actions.Disable();
         var playerMap = _playerInput.actions.FindActionMap("Player");
         _moveAction = playerMap.FindAction("Move");
         _sprintAction = playerMap.FindAction("Sprint");
         _jumpAction = playerMap.FindAction("Jump");
         _shootAction = playerMap.FindAction("Shoot");
         _reloadAction = playerMap.FindAction("Reload");
         _toggleFireModeAction = playerMap.FindAction("ToggleFireMode");
         _interactAction = playerMap.FindAction("Interact");
         _openUIAction = playerMap.FindAction("OpenUI");
         _openSettingAction = playerMap.FindAction("OpenSetting");
         _scrollWheelAction = playerMap.FindAction("ScrollWheel");
         _changeWeaponAction = playerMap.FindAction("ChangeWeapon");
         _quickSlotAction = playerMap.FindAction("QuickSlot");
         
         _uiControl = FindAnyObjectByType<UIControl>(); //UIControl
         _uiControl.Init(this); //초기화
      }

      private void Start()
      {
         _mainCamera = Camera.main;
         _baseRArmPosition = rightArm.transform.localPosition; 
         _colliders = GetComponents<Collider2D>();
         _groundMask = LayerMask.GetMask("Ground");
         _climbMask = LayerMask.GetMask("Climbing");
         
         _playerInput.SwitchCurrentActionMap("Player");
         
         _currentMoveSpeed = MoveSpeed;
      }

      private void OnEnable()
      {
         _moveAction.performed += OnMove; //InputAction 이벤트처리
         _moveAction.canceled += OnMove;
         _sprintAction.performed += OnSprint;
         _sprintAction.canceled += OnSprint;
         _jumpAction.performed += OnJump;
         _shootAction.started += OnShoot;
         _shootAction.canceled += OnShoot;
         _reloadAction.performed += OnReload;
         _toggleFireModeAction.performed += OnToggleFireMode;
         _interactAction.performed += OnInteract;
         _openUIAction.performed += OnOpenUI;
         _openSettingAction.performed += OnOpenSetting;
         _scrollWheelAction.performed += OnScrollWheel;
         _changeWeaponAction.performed += OnChangeWeapon;
         _quickSlotAction.performed += OnQuickSlot;
      }

      private void OnDisable()
      {
         _moveAction.performed -= OnMove;
         _moveAction.canceled -= OnMove;
         _sprintAction.performed -= OnSprint;
         _sprintAction.canceled -= OnSprint;
         _jumpAction.performed -= OnJump;
         _shootAction.started -= OnShoot;
         _shootAction.canceled -= OnShoot;
         _reloadAction.performed -= OnReload;
         _toggleFireModeAction.performed -= OnToggleFireMode;
         _interactAction.performed -= OnInteract;
         _openUIAction.performed -= OnOpenUI;
         _openSettingAction.performed -= OnOpenSetting;
         _scrollWheelAction.performed -= OnScrollWheel;
         _changeWeaponAction.performed -= OnChangeWeapon;
         _quickSlotAction.performed -= OnQuickSlot;
      }

      private void Update()
      {
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
      
      private bool ColliderCheck() //FixedUpdate가 아니라 다른곳에서?
      {
         const float groundCheckDistance = 0.25f;
         bool isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckDistance, _groundMask);
         
         Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, Color.blue);
         Debug.DrawRay(transform.position, Vector2.left * groundCheckDistance, Color.blue);
         Debug.DrawRay(transform.position, Vector2.right * groundCheckDistance, Color.blue);
         _canClimb = _colliders.Any(col => col.IsTouchingLayers(_climbMask));

         return isGrounded;
      }

      private void RotateArm()
      {
         if(IsUnarmed) return;//비무장 제한...
         //장전 등 몇몇 행동에서는 안움직여야한다.
         if (!_canRotateArm) return;

         //Debug.Log("RotateArm");
         
         Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition); //마우스위치
         mousePos.z = 0;

         Vector2 direction; //방향 계산
         if (IsOneHanded)
         {
            direction = mousePos - rightArm.transform.position;
            Debug.DrawRay(rightArm.transform.position, direction, Color.red); //조준선
         }
         else //TwoHanded
         {
            direction = mousePos - leftArm.transform.position;
            Debug.DrawRay(leftArm.transform.position, direction, Color.red);
         }
 
      
         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //팔 - 마우스 각도 계산

         if (_isFlipped) //좌측 flip에 맞게 계산
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

         if (IsOneHanded)
         {
            angle = Mathf.Clamp(angle, -60f, 60f); //각도제한
      
            _shootAngle = angle;
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);
         }
         else //TwoHanded
         {
            angle = Mathf.Clamp(angle, -40f, 40f); //85f center -125~-45
            _shootAngle = angle;
         
            float t = Mathf.InverseLerp(-40f, 40f, -angle); //현재 조준각도의 보간치 
            float targetAngle = Mathf.Lerp(-90, 70, t); //조준각도의 보간치에 맞추어 RightArm 회전보간

            float angleThreshold = 37f;  //해당 각도보다 크면(임계치) rightArm Position 또한 변경         
            if (targetAngle > angleThreshold)
            {
               float pt = Mathf.InverseLerp(angleThreshold, 70, targetAngle ); //임계치-최댓값에 현재 각도 보간치
               rightArm.transform.localPosition = Vector3.Lerp(_baseRArmPosition, new Vector3(-0.272f, -0.072f, 0f), pt);
               //보간치에 따라 position 벡터 계산
               //기본 (-0.161, 0.037) -> (-0.2, -0.25) -> (-0.272 -0.072) 개선?
               //무기마다 세부설정 -> Animation Curve? struct로 저장해서 불러오기
            }
            else
            {
               rightArm.transform.localPosition = _baseRArmPosition;
            }
         
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, targetAngle); 
            leftArm.transform.localRotation = Quaternion.Euler(0, 0, -angle - 85f);   
            //SPUM캐릭터에서 scale.-x로 Flip시킨 것을 디폴트로 만듦. -> 그래서 캐릭터 좌우가 캐릭터 기준이 아닌 사용자/제작자 시점이 기준
            //조작하는 방향대로 설정과 기존 캐릭터를 기준으로...
         }
      
      }
   
      private void PlayerMovement() //플레이어 이동
      {
         var isGrounded = ColliderCheck();
         
         if(!isGrounded && _isGrounded) OnPlayerMove?.Invoke(false); //점프 시 전환(점프 애니메이션으로 바꾸어도 됨)
         if(isGrounded && !_isGrounded && _playerMoveVector.x != 0) OnPlayerMove?.Invoke(true); //착지 시 전환
         
         _isGrounded = isGrounded;
         if (_isGrounded)
         {
            _rigidbody.linearVelocityX = _playerMoveVector.x * _currentMoveSpeed;
         }
         
         if (_canClimb)
         {
            _rigidbody.linearVelocityY = _playerMoveVector.y * _currentMoveSpeed; 
            _rigidbody.gravityScale = 0;
         }
         else
         {
            _rigidbody.gravityScale = 1;
         }
      }

      public void BlockControl(bool isBlock)
      {
         _canRotateArm = !isBlock; //팔 회전 - block상태면 false
         if (isBlock) //ActionMap 교체로 캐릭터 컨트롤 제한
         {
            _playerInput.SwitchCurrentActionMap("UI");
         }
         else
         {
            _playerInput.SwitchCurrentActionMap("Player");
         }
      }

      private void OnInteract(InputAction.CallbackContext context)
      {
         OnFieldInteract?.Invoke();
      }
   
      private void OnOpenUI(InputAction.CallbackContext context) //플레이어 정보(인벤토리 창), Default Tab키
      {
         BlockControl(true); //컨트롤 입력 제한
         _uiControl.OnOpenPlayerUI();
      }

      private void OnOpenSetting(InputAction.CallbackContext context)
      {
         BlockControl(true);
         _uiControl.OnOpenSettingsUI();
      }

      private void OnScrollWheel(InputAction.CallbackContext context)
      {
         var delta = context.ReadValue<Vector2>(); //마우스 휠 스크롤
         
         OnScrollInteractMenu?.Invoke(delta.y);
      }

      private void OnChangeWeapon(InputAction.CallbackContext context)
      {
         if(context.control is not KeyControl key) return;
         
         switch (key.keyCode)
         {
            case Key.Digit1:OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Primary); return;
            case Key.Digit2:OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Secondary); return;
            case Key.Digit3: OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Unarmed); return;
            default: return;
         }
      }

      private void OnQuickSlot(InputAction.CallbackContext context)
      {
         if(context.control is not KeyControl key) return;
         var idx = key.keyCode - Key.Digit1 + 1; //개선?
         OnQuickSlotAction?.Invoke((QuickSlotIdx)idx);
      }
   
      private void OnMove(InputAction.CallbackContext context) //플레이어 이동 입력
      {
         _playerMoveVector = context.ReadValue<Vector2>();//Vector2 wasd 
      
         Vector3 playerScale = transform.localScale;
      
         if (_playerMoveVector.x == 0 ) //x축 이동입력이 없을 때
         {
            OnPlayerMove?.Invoke(false);
         }
         else
         {
            playerScale.x = Mathf.Abs(playerScale.x) * Mathf.Sign(_playerMoveVector.x);
            //입력에 따라 flip결정
            _isFlipped = _playerMoveVector.x < 0; //왼쪽입력 시 Flip
            transform.localScale = playerScale;
            OnPlayerMove?.Invoke(true);
         }
      }

      private void OnSprint(InputAction.CallbackContext context) //달리기 로직 변경?
      {
         if (!_isGrounded) return;
         if (context.performed)
         {
            _currentMoveSpeed = MoveSpeed * SprintSpeedMultiplier;
            OnPlayerSprint?.Invoke(true);
         }
         else if (context.canceled)
         {
            _currentMoveSpeed = MoveSpeed;
            OnPlayerSprint?.Invoke(false);
         }
      }
      
      private void OnShoot(InputAction.CallbackContext ctx)
      {
         if(IsUnarmed) return;
         if(!_canShoot) return;
         
         switch (CurrentFireMode)
         {
            case FireMode.SemiAuto: //단발
            {
               if (ctx.started)
               {
                  OnShootAction?.Invoke(_isFlipped, _shootAngle);
               }
               break;
            }
            case FireMode._2Burst:
            {
               break;
            }
            case FireMode._3Burst:
            {
               if (ctx.started)
               {
                  OnShootAction?.Invoke(_isFlipped, _shootAngle);
               }
               break;
            }
               case FireMode.FullAuto:
            {
               if (ctx.started) //좌클릭 홀드일 때 연사, 때면 사격종료
               {
                  _inShooting = true;
               }
               else if (ctx.canceled)
               {
                  _inShooting = false;
               }

               break;
            }
         }
      }

      private void Shoot() //사격(연사) 메서드
      {
         if(IsUnarmed) return;
         if(CurrentFireMode != FireMode.FullAuto) return;  //단발인 경우 return
         if(!_canShoot) return; //사격 불가 시 return
         if(!_inShooting) return; //사격 중 인지 check
         OnShootAction?.Invoke(_isFlipped, _shootAngle);
      }
   
      private void OnJump(InputAction.CallbackContext ctx) //점프입력 Space키
      {
         if (ctx.performed && _isGrounded) //땅에 있을 때
         {
            _rigidbody.AddForce(new Vector2(0, JumpSpeed), ForceMode2D.Impulse); //단순하게 위로 -> moveVector...
         }
      }

      private void OnReload(InputAction.CallbackContext ctx) //재장전 입력 R키
      {
         if(IsUnarmed) return;
         _canRotateArm = false; //팔회전 불가
         _canShoot = false; //사격 불가
         _inShooting = false; //사격 중인 경우 중단
         OnPlayerReload?.Invoke();//장전 이벤트 전달
      }

      public void OnReloadEnd()//장전 종료시 호출
      {
         _canRotateArm = true;
         _canShoot = true;
         OnReloadEndAction?.Invoke();//장전 애니메이션 종료시 장전 매커니즘 작동
      }

      private void OnToggleFireMode(InputAction.CallbackContext ctx)
      {
         if(IsUnarmed) return;
         OnToggleFireModeAction?.Invoke();
      }
   
   }
}
