using System.Collections.Generic;
using System.Linq;

namespace Generator.Dungeon
{
    public class DecorationBankManager
    {
        private Dictionary<RoomType, Dictionary<SurfaceTag, List<Decoration>>> m_decorationByRoomSurface;

        public void SortDecorations(List<DungeonDecorationAssetSO> _decorationsPresets)
        {

            m_decorationByRoomSurface = new Dictionary<RoomType, Dictionary<SurfaceTag, List<Decoration>>>();


            foreach(RoomType _roomType in System.Enum.GetValues(typeof(RoomType)))
            {
                m_decorationByRoomSurface[_roomType] = new Dictionary<SurfaceTag, List<Decoration>>();

                foreach (SurfaceTag _surfaceType in System.Enum.GetValues(typeof(SurfaceTag)))
                {
                    m_decorationByRoomSurface[_roomType][_surfaceType] = new  List<Decoration>();
                }
            }

            List<Decoration> _decorations = new List<Decoration>();
            foreach(DungeonDecorationAssetSO _preset in _decorationsPresets)
            {
                _decorations = _decorations.Concat(_preset.decorations).ToList();
            }

            foreach (Decoration _deco in _decorations)
            {
                foreach(RoomType _validRoom in _deco.Rooms)
                {

                    if (_deco.CombinationRequired)
                    {
                        if (!m_decorationByRoomSurface[_validRoom].Keys.Contains(_deco.SurfaceTag))
                        {
                            m_decorationByRoomSurface[_validRoom][_deco.SurfaceTag] = new List<Decoration>();
                        }

                        m_decorationByRoomSurface[_validRoom][_deco.SurfaceTag].Add(_deco);
                    }
                    else
                    {
                        foreach(SurfaceTag _validSurface in _deco.Surfaces)
                        {
                            m_decorationByRoomSurface[_validRoom][_validSurface].Add(_deco);
                        }
                    }
                }
            }
        }

        public List<Decoration> GetDecorationsWithTag(DecorationTag _tag, RoomType _roomType, SurfaceTag _surfaceTag)
        {
            List<Decoration> _validDeco = new List<Decoration>();

            foreach (Decoration _deco in m_decorationByRoomSurface[_roomType][_surfaceTag])
            {
                if(_deco.Tag == _tag)
                {
                    _validDeco.Add(_deco);
                }
            }
            return _validDeco;
        }

        public List<Decoration> GetDecorationsWithTags(List<DecorationTag> _tags, RoomType _roomType, SurfaceTag _surfaceTag)
        {
            List<Decoration> _out = new List<Decoration>();
            foreach (DecorationTag _tag in _tags)
            {
                _out = _out.Concat(GetDecorationsWithTag(_tag, _roomType, _surfaceTag)).ToList();
            }
            return _out;
        }

        public List<Decoration> GetDecorationByRoomAndSurfaceType(RoomType _roomType, SurfaceTag _surfaceTag)
        {
            return m_decorationByRoomSurface[_roomType][_surfaceTag];
        }
    }
}