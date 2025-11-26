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
      public float MoveSpeed { set; get; } //이동 속도
      public float SprintSpeedMultiplier { set; get; } //달리기 배수
      public float JumpForce { set; get; }  //점프 운동량  
      //PlayerData에서 할당(개선 필요)
      [SerializeField, Range(0f, 1f)] private float airDragMultiplier = 0.95f;

      [SerializeField] private GameObject playerSprite;
      [SerializeField] private Transform eyesPos;
      [SerializeField] private GameObject rightArm;
      [SerializeField] private GameObject leftArm;
      private Vector3 _baseRArmPosition;
      public float ShootAngle { private set; get; }

      private PlayerInput _playerInput; 
      private InputAction _moveAction; //플레이어 InputAction...   개선방안?
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
   
      private Vector2 _playerMoveVector; //이동 벡터
      
      public bool IsUnarmed { set; get; } //비무장 체크
      public bool IsOneHanded { set; get; }//한손무기 체크
      public FireMode CurrentFireMode { set; get; } //현재 발사모드
      public bool InShooting { set; get; } //사격 중 인지
      private bool _inReloading; //장전 중 인지
      private bool _canShoot  = true; //사격 가능 여부
      public bool IsFlipped { private set; get; }
      //플레이어 스프라이트 flip여부
      private bool _isGrounded = false; //ground위 인지 체크
      private bool _canClimb = false; //Climb 가능 여부
      private bool _canRotateArm = true; //팔 회전 여부
      private bool _inJumping = false;
      
      private UIControl _uiControl;
      
      public event Action<bool> OnPlayerMove; //이동 이벤트(bool IsMove)
      public event Action<bool> OnPlayerSprint; //달리기 이벤트(bool IsSprint)
      public event Action OnPlayerReload; //장전 이벤트
      public event Action OnFieldInteract; //필드 상호작용
      public event Action<float> OnScrollInteractMenu; //필드 상호작용 메뉴 스크롤(float 스크롤 방향)
      public event Action<EquipWeaponIdx> OnChangeWeaponAction;  //무기교체(장비무기 인덱스)
      public event Action<QuickSlotIdx> OnQuickSlotAction; //퀵슬롯 사용(퀵슬롯 인덱스)
      public event Action<bool, float> OnShootAction; //사격 이벤트 (isFlipped, shootAngle)
      public event Action<int, bool, float> OnBurstShootAction; //점사 이벤트 (burstCount, isFlipped, shootAngle)
      public event Action OnReloadEndAction; //장전 끝
      public event Action OnToggleFireModeAction; //사격방식 변경
      public event Action<bool> OnPlayerFlip; //isFlipped 
      
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
         
         var fov = GetComponentInChildren<FieldOfView>();
         fov.Init(this);
      }

      private void Start()
      {
         _mainCamera = Camera.main;
         _baseRArmPosition = rightArm.transform.localPosition; 
         _colliders = GetComponents<Collider2D>();
         _groundMask = LayerMask.GetMask("Ground"); //타일 layer
         _climbMask = LayerMask.GetMask("Climbing");
         
         _playerInput.SwitchCurrentActionMap("Player");
         
         _currentMoveSpeed = MoveSpeed; //현재 이동속도 초기화
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
         Shoot(); //총기 연사 처리
      }

      private void LateUpdate() 
      {
         //애니메이션 이후 처리
         RotateArm(); //팔 회전
      }

      private void FixedUpdate()
      {
         //물리 기반 처리
         PlayerMovement(); //플레이어 이동(Rigidbody 기반)
      }
      
      private void RotateArm()
      {
         float angle;
         Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition); //마우스위치
         mousePos.z = 0;
         Vector2 direction; //방향 계산
         
         if(IsUnarmed)
         {
            direction = mousePos - eyesPos.position;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (IsFlipped) //좌측 flip에 맞게 계산
            {
               if (angle > 0) //각도 부호에 따라
               {
                  angle = 180 - angle;
               }
               else
               {
                  angle = -180 - angle;
               }
            }
            angle = Mathf.Clamp(angle, -60f, 60f);//시야 각도(기본 각도 + 아이템 등 효과)로 수정 필요
            ShootAngle =  angle;
            return;//비무장 제한...
         }
         //장전 등 행동에서는 안움직여야한다.
         if (!_canRotateArm) return; //팔 이동 제한 체크
         
         if (IsOneHanded)
         {
            direction = mousePos - rightArm.transform.position; //마우스 - 팔 벡터 
            Debug.DrawRay(rightArm.transform.position, direction, Color.red); //조준선
         }
         else //TwoHanded
         {
            direction = mousePos - leftArm.transform.position;
            Debug.DrawRay(leftArm.transform.position, direction, Color.red);
         }
      
         angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //팔 - 마우스 각도 계산

         if (IsFlipped) //좌측 flip에 맞게 계산
         {
            if (angle > 0) //각도 부호에 따라
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
      
            ShootAngle = angle; //발사 각도 할당
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);//팔 각도 할당
         }
         else //TwoHanded
         {
            angle = Mathf.Clamp(angle, -40f, 40f); //각도 제한  85f center -125~-45
            ShootAngle = angle;
         
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
         
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, targetAngle); //팔 각도 회전
            leftArm.transform.localRotation = Quaternion.Euler(0, 0, -angle - 85f);   
            //캐릭터 좌우가 캐릭터 기준이 아닌 사용자/제작자 시점이 기준(SPUM 에셋 이슈)
         }
      }
   
      private bool ColliderCheck() //플레이어 Ground 체크
      {
         const float groundCheckDistance = 0.25f;
         bool isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckDistance, _groundMask);
         //LayerMask 체크
         
         Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, Color.blue);
         Debug.DrawRay(transform.position, Vector2.left * groundCheckDistance, Color.blue);
         Debug.DrawRay(transform.position, Vector2.right * groundCheckDistance, Color.blue);
         _canClimb = _colliders.Any(col => col.IsTouchingLayers(_climbMask)); //Climb타일 체크

         return isGrounded;
      }
      
      private void PlayerMovement() //플레이어 이동
      {
         var isGrounded = ColliderCheck();
         
         if(!isGrounded && _isGrounded) OnPlayerMove?.Invoke(false); //점프 시 전환(점프 애니메이션으로 바꾸어도 됨)
         if(isGrounded && !_isGrounded && _playerMoveVector.x != 0) OnPlayerMove?.Invoke(true); //착지 시 전환
         
         _isGrounded = isGrounded;
         
         if (_canClimb)
         {
            _rigidbody.linearVelocityX = _playerMoveVector.x * _currentMoveSpeed; 
            _rigidbody.linearVelocityY = _playerMoveVector.y * _currentMoveSpeed; 
            _rigidbody.gravityScale = 0; //하락 방지
         }
         else
         {
            _rigidbody.gravityScale = 1;
            
            if (_isGrounded) //Ground라면
            {
               if (_rigidbody.linearVelocityY <= 0f) //착지 시(양수라면 점프하는 순간)
               {
                  _inJumping =  false; //점프 끝
               }
               _rigidbody.linearVelocityX = _playerMoveVector.x * _currentMoveSpeed; //rigidbody기반 이동
            }
            else
            {
               if (!_inJumping) //점프가 아닐 때(추락 중)
               {
                  _rigidbody.linearVelocityX *= airDragMultiplier; //공중 속도 감소
               }
            }
         }
      }

      public void BlockControl(bool isBlock) //조작 제한
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

      private void OnInteract(InputAction.CallbackContext context) //상호작용 F
      {
         OnFieldInteract?.Invoke(); //이벤트
      }
   
      private void OnOpenUI(InputAction.CallbackContext context) //플레이어 정보(인벤토리 창), Default Tab키
      {
         BlockControl(true); //컨트롤 입력 제한
         _uiControl.OnOpenPlayerUI(); //플레이어 창 활성화
      }

      public void OnOpenCrate() //상자 열 때(플레이어 창 활성화)
      {
         BlockControl(true);
         _uiControl.OnOpenPlayerUI();
      }

      private void OnOpenSetting(InputAction.CallbackContext context) //설정창, ESC
      {
         BlockControl(true);
         _uiControl.OnOpenSettingsUI();
      }

      private void OnScrollWheel(InputAction.CallbackContext context) 
      {
         var delta = context.ReadValue<Vector2>(); //마우스 휠 스크롤
         
         OnScrollInteractMenu?.Invoke(delta.y); //상호작용 메뉴 스크롤
      }

      private void OnChangeWeapon(InputAction.CallbackContext context) //무기 교체
      {
         if(InShooting) return; 
         if(_inReloading) return; //사격, 장전 중 불가
         if(context.control is not KeyControl key) return;
         
         switch (key.keyCode)
         {
            case Key.Digit1:OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Primary); return; //1~3번
            case Key.Digit2:OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Secondary); return;
            case Key.Digit3: OnChangeWeaponAction?.Invoke(EquipWeaponIdx.Unarmed); return;
            default: return;
         }
      }

      private void OnQuickSlot(InputAction.CallbackContext context) //퀵슬롯
      {
         if(context.control is not KeyControl key) return;
         var idx = key.keyCode - Key.Digit1 + 1; //개선?
         OnQuickSlotAction?.Invoke((QuickSlotIdx)idx); //퀵슬롯 이벤트
      }
   
      private void OnMove(InputAction.CallbackContext context) //플레이어 이동 입력
      {
         _playerMoveVector = context.ReadValue<Vector2>();//Vector2 wasd 
      
         //전체 x 스프라이트만
         
         Vector3 playerScale = playerSprite.transform.localScale;
         
         if (_playerMoveVector.x == 0 ) //x축 이동입력이 없을 때
         {
            OnPlayerMove?.Invoke(false); 
         }
         else
         {
            playerScale.x = Mathf.Abs(playerScale.x) * Mathf.Sign(_playerMoveVector.x); //이동키에 따라 방향전환
            //입력에 따라 flip결정
            bool currentPlayerFlipped = _playerMoveVector.x < 0;
            if (currentPlayerFlipped != IsFlipped)
            {
               OnPlayerFlip?.Invoke(currentPlayerFlipped); //flip상태 변경
            }
            IsFlipped = currentPlayerFlipped;
            playerSprite.transform.localScale = playerScale;
            OnPlayerMove?.Invoke(true);
         }
      }

      private void OnSprint(InputAction.CallbackContext context) //달리기(누를 때만)
      {
         if (!_isGrounded) return;//땅 위에서만 
         if (context.performed)
         {
            _currentMoveSpeed = MoveSpeed * SprintSpeedMultiplier; //배수따라
            OnPlayerSprint?.Invoke(true);
         }
         else if (context.canceled)
         {
            _currentMoveSpeed = MoveSpeed; //원래대로
            OnPlayerSprint?.Invoke(false);
         }
      }
      
      private void OnShoot(InputAction.CallbackContext ctx) //사격
      {
         if(IsUnarmed) return; //비무장일 시 return
         
         if (_inReloading) //장전 중단(한발씩 장전시)
         {
            _inReloading = false;
         }
         
         if(!_canShoot) return; //사격 불가 체크
         
         switch (CurrentFireMode)
         {
            case FireMode.SemiAuto: //단발
            {
               if (ctx.started)
               {
                  OnShootAction?.Invoke(IsFlipped, ShootAngle); //사격 이벤트
               }
               break;
            }
            case FireMode._2Burst:
            case FireMode._3Burst:
            {
               if(InShooting) return;
               int burstCount = 0;
               if(CurrentFireMode == FireMode._2Burst) burstCount = 2;
               else if(CurrentFireMode == FireMode._3Burst) burstCount = 3;
               
               if(burstCount == 0) return;
               
               if (ctx.started)
               {
                  OnBurstShootAction?.Invoke(burstCount, IsFlipped, ShootAngle); //점사 이벤트(발사모드 따라)
               }
               break;
            }
               case FireMode.FullAuto:
            {
               if (ctx.started) //좌클릭 홀드일 때 연사, 때면 사격종료
               {
                  InShooting = true;
               }
               else if (ctx.canceled)
               {
                  InShooting = false;
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
         if(!InShooting) return; //사격 중 인지 check
         OnShootAction?.Invoke(IsFlipped, ShootAngle);
      }
   
      private void OnJump(InputAction.CallbackContext ctx) //점프입력 Space키
      {
         if (ctx.performed && _isGrounded) //땅에 있을 때
         {
            _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse); //Rigidbody기반
            _inJumping =  true;
         }
      }

      private void OnReload(InputAction.CallbackContext ctx) //재장전 입력 R키
      {
         if(IsUnarmed) return; //비무장 체크
         if(_inReloading) return; //장전 중 인지 체크
         InShooting = false; //사격 중인 경우 중단
         OnPlayerReload?.Invoke();//장전 이벤트 전달
      }

      public void SetReloadState(bool inReloading)
      {
         _canRotateArm = !inReloading; //장전 중 팔회전 불가
         _canShoot = !inReloading; //장전 중 사격 불가
         _inReloading = inReloading; //현재 장전 중 인지
      }

      public void OnReloadOneRoundEnd() //한발씩 장전 종료시
      {
         if (!_inReloading) //장전을 중단했다면
         {
            SetReloadState(false); //장전 State Exit
         }
         else
         {
            OnReloadEndAction?.Invoke(); //장전 이벤트
         }
      }
      
      public void OnReloadEnd()//장전 종료시 호출
      {
         _canRotateArm = true;
         _canShoot = true;
         _inReloading = false;
         OnReloadEndAction?.Invoke();//장전 애니메이션 종료시 장전 매커니즘 작동
      }

      private void OnToggleFireMode(InputAction.CallbackContext ctx)
      {
         if(IsUnarmed) return;
         if(InShooting) return;
         OnToggleFireModeAction?.Invoke(); //발사모드 전환 이벤트
      }
   
   }
}
