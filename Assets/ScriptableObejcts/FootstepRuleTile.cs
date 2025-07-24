using System;
using UnityEngine;

[CreateAssetMenu(menuName="Tiles/FootstepRuleTile")]
public class FootstepRuleTile:RuleTile<FootstepRuleTile.Neighbor>
{
    [Serializable]
    public class Neighbor : RuleTile.TilingRuleOutput.Neighbor{}

    public SFX[] footstepVariants;
}
