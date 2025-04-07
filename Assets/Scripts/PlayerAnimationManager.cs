using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    private SPUM_Prefabs _spumUnit;
    
    private bool _isPlaying = false;
    
    private void Awake()
    {
        _spumUnit = GetComponent<SPUM_Prefabs>();
    }

    private void Update()
    {
        if (!_isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerAnimator.SetBool("LongSpear_idle", true);
                _isPlaying = true;
            }
            
        }
    }
}
