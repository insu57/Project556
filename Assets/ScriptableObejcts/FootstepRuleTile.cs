using System;
using UnityEngine;

[CreateAssetMenu(menuName="Tiles/FootstepRuleTile")]
public class FootstepRuleTile:RuleTile<FootstepRuleTile.Neighbor> //타일맵 데이터
{
    [Serializable] public class Neighbor : RuleTile.TilingRuleOutput.Neighbor{}
    public SFX[] footstepVariants; //타일별 발소리
}
