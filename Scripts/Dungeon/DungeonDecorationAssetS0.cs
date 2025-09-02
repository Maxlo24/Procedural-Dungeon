using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonDecorationPreset", menuName = "ScriptableObjects/Dungeon/DecorationPreset", order = 4)]
    public class DungeonDecorationAssetSO : ScriptableObject
    {
        public List<Decoration> decorations = new List<Decoration>();
    }
}