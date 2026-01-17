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
        //벽, 바닥 체크(점프, 낙하) 너무 높으면 불가 -> 추적에서 경계로
        if(!_isGrounded) return;
        
        int dir = _enemy.IsFlipped ? -1 : 1;
        _rb.linearVelocityX = dir * _currentSpeed;
    }

    private void TileCheck()
    {
        if(!_isGrounded) return;
        
        Vector2 facingDir = Vector2.right;
        if(_enemy.IsFlipped) facingDir = -facingDir;
        
        Vector2 raycastOrigin = (Vector2)transform.position + new Vector2(0, wallCheckYOffset);
        bool wallCheck = Physics2D.Raycast(raycastOrigin, facingDir, tileCheckDist, _groundMask);
        if (wallCheck)
        {
            Jump(facingDir);
        }
    }

    private void Jump(Vector2 facingDir)
    {
        Debug.Log("Jump");
        _rb.linearVelocity = Vector2.zero;
        
        Vector2 jumpForce = new Vector2(facingDir.x * jumpHorizontalForce, jumpVerticalForce);
        _rb.AddForce(jumpForce, ForceMode2D.Impulse);
    }
    
    private void FixedUpdate()
    {
        //움직임... 단순 좌우 움직임이 아니라 추적은? 추가 작업 필요(이동AI)
        GroundCheck();
        EnemyMove();
        TileCheck();
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
