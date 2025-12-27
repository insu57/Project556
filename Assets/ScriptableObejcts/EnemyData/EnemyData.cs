using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyName;
    [SerializeField] private float healthAmount; //체력량
    [SerializeField] private bool isHuman; //인간형 인지
    [SerializeField] private int armorAmount; //방어도
    //방어도 - 체력
    //무기 - 피해량, 방어관통력, 치명확률
    
    [SerializeField] private float detectRange = 2f; //감지범위
    [SerializeField] private float viewRange = 4f; //시야범위
    [SerializeField] private float viewAngle = 90; //시야각도
    [SerializeField] private float chaseRange = 2f; //추격범위?
    [SerializeField] private float moveSpeed = 2f;
    public string EnemyName => enemyName;
    public float HealthAmount => healthAmount;
    public bool IsHuman => isHuman;
    public int ArmorAmount => armorAmount;
    
    public float DetectRange => detectRange;
    public float ViewRange => viewRange;
    public float ViewAngle => viewAngle;
    public float ChaseRange => chaseRange;
    public float MoveSpeed => moveSpeed;
}
