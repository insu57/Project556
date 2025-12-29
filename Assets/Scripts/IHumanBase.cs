using UnityEngine;

public interface IHumanType
{
    //ReloadAnim
    public float PlayReloadSFX();
    public bool CheckWeaponHasNotDetachMag();
    public void OnReloadOneRoundEnd();
    public void OnReloadEnd();
    //MoveAnim
    public float GetSprintSpeedMultiplier();
    public float LastFootstepTime{ set; get;}
    public AudioSource OneShotSource { get; }
    public Vector3 GetPosition();
    //LoadAmmoAnim
    public float PlayLoadAmmoSFX();
}
