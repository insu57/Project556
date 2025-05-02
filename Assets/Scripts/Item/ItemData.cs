using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    
    public int ItemWidth => itemWidth;
    public int ItemHeight => itemHeight; //SO에서 정보 가져오기
    
    public List<int> SlotIdxList { private set; get; } //삭제?

    private void Start()
    {
        SlotIdxList = new List<int>();
    }
}
