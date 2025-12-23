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
    //무기 - 피해량, 방어관통력, 치명확률\
    [SerializeField] private float detectRange;
    [SerializeField] private float viewRange;
    [SerializeField] private float shotRange;
    public string EnemyName => enemyName;
    public float HealthAmount => healthAmount;
    public bool IsHuman => isHuman;
    public int ArmorAmount => armorAmount;
}
