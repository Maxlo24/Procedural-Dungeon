using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tile))]
    public class CI_Tile : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Turn Into Tile Spawner"))
            {
                ((Tile)target).TurnIntoTileSpawner();
            }
            if (GUILayout.Button("Turn Into Tile Spawner With This Style"))
            {
                ((Tile)target).TurnIntoTileSpawner();
            }
        }
    }
}