using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "FireModeImage", menuName = "Scriptable Objects/FireModeImage")]
public class FireModeImage : ScriptableObject
{
    [Serializable]
    private struct FireModeSpriteData
    {
        public FireMode fireMode;
        public Sprite fireModeImage;
    }
    
    [SerializeField] private AmmoCategory ammoCategory;

    [SerializeField] private List<FireModeSpriteData> fireModeImageMaps = new()
    {
        new FireModeSpriteData() {fireMode = FireMode.SemiAuto, fireModeImage = null},
        new FireModeSpriteData() {fireMode = FireMode.FullAuto, fireModeImage = null},
        new FireModeSpriteData() {fireMode = FireMode._2Burst, fireModeImage = null},
        new FireModeSpriteData() {fireMode = FireMode._3Burst, fireModeImage = null},
    };
    
    public AmmoCategory AmmoCategory => ammoCategory;
    public Dictionary<FireMode, Sprite> FireModeSpriteDict => FireModeSpriteMapping(fireModeImageMaps);

    private Dictionary<FireMode, Sprite> FireModeSpriteMapping(List<FireModeSpriteData> fireModeSprites)
    {
        var result = new Dictionary<FireMode, Sprite>();
        foreach (var fireModeSpriteData in fireModeSprites)
        {
            result.Add(fireModeSpriteData.fireMode, fireModeSpriteData.fireModeImage);
        }
        return result;
    }
}
