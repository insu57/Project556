using System;
using UnityEngine;

public class EnemyMoveControl : MonoBehaviour
{
    private Rigidbody2D _rb;
    private IEnemyContext _enemy;
    private float _enemySpeed;
    private float _currentSpeed;
    private  LayerMask _groundMask;
    
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

    public void StartChase(bool inChase, float moveVector)
    {
        if (inChase)
        {
            _currentSpeed = _enemySpeed * moveVector;
        }
        else
        {
            _currentSpeed = 0;
        }
        //flip은?
    }

    private bool ColliderCheck() //플레이어 Ground 체크
    {
        const float groundCheckDistance = 0.25f;
        bool isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckDistance, _groundMask);
        //LayerMask 체크

        return isGrounded;
    }
    
    private void EnemyMove()
    {
        //벽, 바닥 체크(점프, 낙하) 너무 높으면 불가 -> 추적에서 경계로
        int dir = _enemy.IsFlipped ? -1 : 1;
        _rb.linearVelocityX = dir * _currentSpeed;
    }
    
    private void FixedUpdate()
    {
        //움직임... 단순 좌우 움직임이 아니라 추적은? 추가 작업 필요(이동AI)
        EnemyMove();
    }
}
