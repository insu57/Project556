using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName="Tiles/FootstepTile")]
public class FootstepTile : Tile
{
    public SFX[] footstepVariants; //SFX -> AudioManager -> AudioClip
}
