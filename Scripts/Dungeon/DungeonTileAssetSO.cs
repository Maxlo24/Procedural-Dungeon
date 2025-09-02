using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonTilePreset", menuName = "ScriptableObjects/Dungeon/TilePreset", order = 2)]
    public class DungeonTileAssetSO : ScriptableObject
    {
        public List<Tile> wallTiles = new List<Tile>();
        public List<Tile> ceilingTiles = new List<Tile>();
        public List<Tile> floorTiles = new List<Tile>();
        public List<Tile> oppeningTiles = new List<Tile>();
        public List<Tile> doorTiles = new List<Tile>();
    }
}