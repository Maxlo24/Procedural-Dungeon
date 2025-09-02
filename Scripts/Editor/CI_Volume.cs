using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Volume))]
    public class CI_Volume : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            GUILayout.Space(10);
            if (GUILayout.Button("Toggle Volume View"))
            {
                ((Volume)target).ToggleGizmoToDraw();
            }
            if (GUILayout.Button("Toggle Link view"))
            {
                ((Volume)target).ToggleLinkView();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Toggle Voxel view"))
            {
                ((Volume)target).ToggleVoxelView();
            }
            if (GUILayout.Button("Recalculate Bounds"))
            {
                ((Volume)target).DetermineOutsideFacesAndRecalculateBounds();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Generate linkFaces"))
            {
                ((Volume)target).GenerateLinkFaces();
            }
            if (GUILayout.Button("Clear"))
            {
                ((Volume)target).ClearLinkFaces();
            }
        }
    }
}