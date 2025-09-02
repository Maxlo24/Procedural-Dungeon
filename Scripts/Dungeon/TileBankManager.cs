using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generator.Dungeon
{
    public class TileBankManager
    {
        private Dictionary<RoomType, Dictionary<TileType, List<Tile>>> m_tileByRoomByType;

        public void SortTiles(List<DungeonTileAssetSO> _tilesPresets)
        {
            m_tileByRoomByType = new Dictionary<RoomType, Dictionary<TileType, List<Tile>>>();

            foreach (RoomType _roomType in System.Enum.GetValues(typeof(RoomType)))
            {
                m_tileByRoomByType[_roomType] = new Dictionary<TileType, List<Tile>>();

                foreach (TileType _tileType in System.Enum.GetValues(typeof(TileType)))
                {
                    m_tileByRoomByType[_roomType][_tileType] = new List<Tile>();
                }
            }

            List<Tile> _tiles = new List<Tile>();
            foreach (DungeonTileAssetSO _preset in _tilesPresets)
            {
                _tiles = _tiles.Concat(_preset.wallTiles).ToList();
                _tiles = _tiles.Concat(_preset.ceilingTiles).ToList();
                _tiles = _tiles.Concat(_preset.floorTiles).ToList();
                _tiles = _tiles.Concat(_preset.oppeningTiles).ToList();
                _tiles = _tiles.Concat(_preset.doorTiles).ToList();

            }

            foreach (Tile _tile in _tiles)
            {
                foreach (RoomType _validRoom in _tile.Rooms)
                {
                    m_tileByRoomByType[_validRoom][_tile.Type].Add(_tile);
                }
            }
        }

        public List<Tile> GetTilesWithStyles(RoomType _roomType, TileType _tileType, List<TileStyle> _tileStyles)
        {
            List<Tile> _out = new List<Tile>();
            foreach (Tile _tile in m_tileByRoomByType[_roomType][_tileType])
            {
                foreach(TileStyle _style in _tileStyles)
                {
                    if(_tile.Style == _style)
                    {
                        _out.Add(_tile);
                    }
                }
            }
            return _out;
        }

        public List<Tile> GetTileByRoomAndTileType(RoomType _roomType, TileType _tileType)
        {
            return m_tileByRoomByType[_roomType][_tileType];
        }
    }
}