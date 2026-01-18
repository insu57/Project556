using System;
using UnityEngine;

public class EnemyMoveControl : MonoBehaviour
{
    private Rigidbody2D _rb;
    private IEnemyContext _enemy;
    private float _enemySpeed;
    private float _currentSpeed;
    private  LayerMask _groundMask;
    private bool _inChase;
    private bool _isGrounded;
    private bool _isJumping;
    [SerializeField] private float tileCheckDist = 1.5f;
    [SerializeField] private float wallCheckYOffset = 0.5f;
    [SerializeField] private float jumpVerticalForce = 6f;
    [SerializeField] private float jumpHorizontalForce = 3f;
    
    private void Awake()
    {
        TryGetComponent(out _rb);
    }
    
    //임시. 구현필요.
    public void Init(float speed, IEnemyContext enemy, LayerMask groundMask)
    {
        _enemySpeed = speed;
        _currentSpeed = 0;//순찰인 경우?
        _enemy = enemy;
        _groundMask = groundMask;
    }

    public void StartChase(bool inChase)
    {
        _inChase = inChase;
        if (inChase)
        {
            _currentSpeed = _enemySpeed;
        }
        else
        {
            _currentSpeed = 0;
        }
        //flip은?
    }

    private void GroundCheck() //플레이어 Ground 체크
    {
        const float groundCheckDistance = 0.5f;
        bool isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckDistance, _groundMask);
        //LayerMask 체크

        _isGrounded = isGrounded;
    }
    
    private void EnemyMove()
    {
        //벽, 바닥 체크(점프, 낙하) 너무 높으면 불가 -> 추적에서 경계(Idle?)
        //낙하관련 구현 필요(낙하 데미지(공통), 너무 높은 위치라면 우회 혹은 경계(Idle)
        if(!_isGrounded || _isJumping) return;
        
        int dir = _enemy.IsFlipped ? -1 : 1;
        _rb.linearVelocityX = dir * _currentSpeed;
    }

    private void TileCheck()
    {
        if(!_isGrounded || _isJumping) return;
        
        Vector2 facingDir = Vector2.right;
        if(_enemy.IsFlipped) facingDir = -facingDir;
        
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(0, wallCheckYOffset);
        bool wallCheck = Physics2D.Raycast(raycastOrigin, facingDir, tileCheckDist, _groundMask);
        if (wallCheck) //벽이 있다면...
        {
            Jump(facingDir);
        }
    }

    private void Jump(Vector2 facingDir)
    {
        _isJumping = true;
        Vector2 jumpVelocity = new Vector2(facingDir.x * jumpHorizontalForce, jumpVerticalForce);
        _rb.linearVelocity = jumpVelocity; 
    }
    
    private void FixedUpdate()
    {
        GroundCheck();

        if (_isJumping)
        {
            // y축 속도가 0 이하이고 땅에 닿아있으면 착지한 것으로 간주
            if (_isGrounded && _rb.linearVelocity.y <= 0.01f)
            {
                _isJumping = false;
                _rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // 지상에 있거나 자유 낙하 중인 상태.
            EnemyMove();
            TileCheck(); //점프 조건이 맞으면 _isJumping이 true
        }
    }

    private void OnDrawGizmos()
    {
        //WallCheck
        if(_enemy == null) return;
        Vector3 facingDir = Vector2.right;
        if(_enemy.IsFlipped) facingDir = -facingDir;
        Gizmos.color = Color.green;
        Vector3 raycastOrigin = transform.position + new Vector3(0, wallCheckYOffset, 0);
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + facingDir * tileCheckDist);
    }
}
