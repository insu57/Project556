using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyName;
    [SerializeField] private float healthAmount;
    [SerializeField] private bool isHuman;
    [SerializeField] private int armorAmount;
    //방어도 - 체력
    //무기 - 피해량, 방어관통력, 치명확률

    public string EnemyName => enemyName;
    public float HealthAmount => healthAmount;
    public bool IsHuman => isHuman;
    public int ArmorAmount => armorAmount;
}
