using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CustomEditor(typeof(DecorationSpawner))]
    public class CI_DecorationSpawner : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Add spawnPoint"))
            {
                ((DecorationSpawner)target).AddSpawnPoint();
            }
            if (GUILayout.Button("Toggle spawnpoint view"))
            {
                ((DecorationSpawner)target).ToggleSpawnPointView();
            }
        }
    }
}