using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private float cellSize = 50f;
    public float CellSize => cellSize;

}
