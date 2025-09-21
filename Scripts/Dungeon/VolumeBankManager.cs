using System.Collections.Generic;
using System.Linq;

namespace Generator.Dungeon
{
    public class VolumeBankManager
    {
        private Dictionary<VolumeType, List<Volume>> m_volumebank;
        private DungeonAssetSO m_dungeonAsset;
        private float m_dungeonOrigine; 
        private Dictionary<VolumeType, int> m_roomTypeFrequency;

        public VolumeBankManager(float _proceduralDungeonOrigine) 
        {
            m_dungeonOrigine = _proceduralDungeonOrigine;
        }

        public void GenerateVolumeBank(DungeonAssetSO _dungeonAsset)
        {
            m_dungeonAsset = _dungeonAsset;
            m_roomTypeFrequency = new Dictionary<VolumeType, int>
            {
                { VolumeType.Basic, _dungeonAsset.basicRoomFrequency },
                { VolumeType.Hall, _dungeonAsset.hallRoomFrequency },
                { VolumeType.Stairs, _dungeonAsset.stairsRoomFrequency },
                { VolumeType.Special, _dungeonAsset.specialRoomFrequency }
            };

            m_volumebank = new Dictionary<VolumeType, List<Volume>>();
            List<Room> _rooms = new List<Room>(_dungeonAsset.roomPreset.fillRooms);
  
            foreach (Room _room in _rooms)
            {
                Volume _volume = _room.GetComponent<Volume>();

                if (m_volumebank.ContainsKey(_volume.Type))
                {
                    m_volumebank[_volume.Type].Add(_volume);
                }
                else
                {
                    m_volumebank[_volume.Type] = new List<Volume> { _volume };
                }
            }
        }

        public Queue<Volume> GetShuffledVolumesOrderedByType(DRandom _random)
        {
            Dictionary<VolumeType, int> _volumeProbabilityDic = new Dictionary<VolumeType, int>(m_roomTypeFrequency);
            List<VolumeType> _volumeTypeOrder = _volumeProbabilityDic.WeightedShuffle(_random.random);
            List<Volume> _volumePool = new List<Volume>();

            foreach (VolumeType volumeType in _volumeTypeOrder)
                _volumePool = _volumePool.Concat(GetShuffledVolumes(m_volumebank[volumeType], _random).ToList()).ToList();

            return new Queue<Volume>(_volumePool);
        }

        public Queue<Volume> GetShuffledVolumesOfOneRandomType(DRandom _random)
        {
            Dictionary<VolumeType, int> _volumeProbabilityDic = new Dictionary<VolumeType, int>(m_roomTypeFrequency);
            List<VolumeType> _volumeTypeOrder = _volumeProbabilityDic.WeightedShuffle(_random.random);
            return GetShuffledVolumes(m_volumebank[_volumeTypeOrder[0]], _random);
        }


        public Queue<Volume> GetShuffledVolumes(List<Volume> _volumePool, DRandom _random)
        {
            List<Volume> _volumes = new List<Volume>(_volumePool);
            _volumes.Shuffle(_random.random);
            Queue<Volume> _possibleRooms = new Queue<Volume>(_volumes);

            return _possibleRooms;
        }

        public List<Volume> GetVolumeOfType(VolumeType _type)
        {
            return m_volumebank[_type];
        }
    }
}