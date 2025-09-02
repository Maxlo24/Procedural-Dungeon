using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VolumeLinkFace))]
    public class CI_VolumeLinkFace : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            if (GUILayout.Button("Turn In Hard Link"))
            {
                ((VolumeLinkFace)target).TurnInHardLink();
            }
            if (GUILayout.Button("Turn In Soft Link"))
            {
                ((VolumeLinkFace)target).TurnInSoftLink();
            }
        }
    }
}