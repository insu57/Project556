using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage;

    private AmmoType _ammoType;
    
    //Bullet 종류별...
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }
    }
}
