using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VolumeGenerator))]
    public class CI_VolumeGenerator : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Voxel Grid"))
            {
                ((VolumeGenerator)target).GenerateVoxelGrid();
            }
            if (GUILayout.Button("Assign Voxels To Volume"))
            {
                ((VolumeGenerator)target).AssignVoxelsToVolume();
            }
            if (GUILayout.Button("Clear Voxels"))
            {
                ((VolumeGenerator)target).Clear();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Done"))
            {
                ((VolumeGenerator)target).Done();
            }
        }
    }
}