using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public static class VoxelGrid
    {
        public static float VOXEL_SIZE = 3f;
        public static bool DRAW_GRID = false;
        public static bool DRAW_FACES = false;
        public static bool DRAW_LINK = false;
        public static bool DRAW_SPAWNPOINTS = false;

        public static List<Vector3> ConnectLink(List<Vector3> voxels, VolumeHardLink pivotLink, VolumeHardLink linkToConnectTo)
        {
            if (pivotLink == null || linkToConnectTo == null)
            {
                Debug.LogWarning("VoxelGrid.ConnectLink called with a null link! Aborting connection.");
                return voxels; // or return new List<Vector3>() to fail gracefully
            }

            Vector3 rotation = new Vector3(0, (linkToConnectTo.transform.eulerAngles.y - pivotLink.transform.eulerAngles.y) + 180f, 0);
            Vector3 translation = linkToConnectTo.transform.position - pivotLink.transform.position;

            List<Vector3> rotatedVoxels = RotateVoxelsAroundPoint(voxels, pivotLink.transform.position, rotation);
            List<Vector3> translatedVoxels = TranslateVoxels(rotatedVoxels, translation);

            return translatedVoxels;
        }
        public static List<Vector3> TranslateVoxels(List<Vector3> voxels, Vector3 translation)
        {
            List<Vector3> translatedVoxels = new List<Vector3>();

            foreach (var voxel in voxels)
                translatedVoxels.Add(voxel + translation);

            return translatedVoxels;
        }

        public static List<Vector3> RotateVoxelsAroundPoint(List<Vector3> voxels, Vector3 pivot, Vector3 angles)
        {
            List<Vector3> rotatedVoxels = new List<Vector3>();

            foreach (var voxel in voxels)
                rotatedVoxels.Add(RotatePointAroundPivot(voxel, pivot, angles));

            return rotatedVoxels;
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot; // get point direction relative to pivot
            direction = Quaternion.Euler(angles) * direction; // rotate it
            point = direction + pivot; // calculate rotated point
            return point; // return it
        }

        public static GameObject CheckContainerExistance(Transform _object,String _containerName)
        {
            GameObject _container;
            Transform _containerT = _object.Find(_containerName);
            if (_containerT == null)
            {
                _container = new GameObject(_containerName);
                _container.transform.parent = _object.transform;
            }
            else
            {
                _container = _containerT.gameObject;
            }
            return _container;
        }

        public static bool CheckIfFaceIsWall(Vector3 _direction)
        {
            return _direction == Vector3.forward || _direction == Vector3.back || _direction == Vector3.right || _direction == Vector3.left;
        }


        public static Vector3 RoundVec3(Vector3 _vec)
        {
            return new Vector3(Mathf.RoundToInt(_vec.x * 10) * 0.1f, Mathf.RoundToInt(_vec.y * 10) * 0.1f, Mathf.RoundToInt(_vec.z * 10) * 0.1f);
        }

        public static List<T> TypeFlagToList<T>(T _selectedRooms) where T : Enum
        {
            List<T> _outList = new List<T>();
            foreach (T flag in Enum.GetValues(typeof(T)))
            {
                if (_selectedRooms.HasFlag(flag) && Convert.ToInt32(flag) != 0)
                {
                    _outList.Add(flag);
                }
            }
            return _outList;
        }

    }
}