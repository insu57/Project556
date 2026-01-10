using System;
using UnityEngine;

public class EnemyMoveControl : MonoBehaviour
{
    private float _enemySpeed;
    private float _currentSpeed;
    
    //임시. 구현필요.
    public void Init(float speed)
    {
        _enemySpeed = speed;
        _currentSpeed = 0;//순찰인 경우?
    }

    public void StartChase(bool inChase, float direction)
    {
        if (inChase)
        {
            _currentSpeed = _enemySpeed * direction;
        }
        else
        {
            _currentSpeed = 0;
        }
        //flip은?
    }
    
    private void FixedUpdate()
    {
        //움직임... 단순 좌우 움직임이 아니라 추적은? 추가 작업 필요(이동AI)
        
    }
}
