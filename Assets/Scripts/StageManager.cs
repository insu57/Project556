using System;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Tilemap[] tilemaps; //Stage의 TileMaps
    
    private void Start()
    {
        AudioManager.Instance.PlayBGM(BGM.ForestEnvironment);
    }
    
    public SFX GetTileFootstepSFX(Vector3 playerPos) //해당 TileMap의 FootStep SFX
    {
        
        foreach (var tilemap in tilemaps)
        {
            var tileCell = tilemap.WorldToCell(playerPos);
            if(!tilemap.HasTile(tileCell)) 
            {
                continue;//타일이 없음
            }      
            var baseTile = tilemap.GetTile(tileCell);
            if (baseTile is not FootstepRuleTile ruleTile) continue;
            var sfxArray = ruleTile.footstepVariants;
            var result = sfxArray[Random.Range(0, sfxArray.Length)];
            return result;
        }
        return SFX.None;
    }
}
