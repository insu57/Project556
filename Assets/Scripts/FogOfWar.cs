using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _spriteRenderer.enabled = true;
    }
}
