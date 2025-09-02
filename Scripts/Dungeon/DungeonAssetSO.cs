using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonPreset", menuName = "ScriptableObjects/Dungeon/DungeonPreset", order = 1)]
    public class DungeonAssetSO : ScriptableObject
    {
        public DungeonRoomAssetSO roomPreset;
        public List<DungeonDecorationAssetSO> decorationPresets = new List<DungeonDecorationAssetSO>();
        public List<DungeonTileAssetSO> tilesPresets;
        
        public int basicRoomFrequency = 80;
        public int hallRoomFrequency = 25;
        public int stairsRoomFrequency = 25;
        public int specialRoomFrequency = 25;
    }
}