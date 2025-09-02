using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoomRenderer))]
    public class CI_RoomRenderer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            if (GUILayout.Button("Make Visible"))
            {
                ((RoomRenderer)target).SwitchRoomVisibility(true);
            }
            if (GUILayout.Button("Make Invisible"))
            {
                ((RoomRenderer)target).SwitchRoomVisibility(false);
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Find All MeshRenderer"))
            {
                ((RoomRenderer)target).FindAllMeshRenderers();
            }
        }
    }
}