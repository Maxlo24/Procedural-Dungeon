using UnityEditor;
using UnityEngine;

namespace Generator.Dungeon
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Room))]
    public class CI_Room : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10); 
            if (GUILayout.Button("Test Generation"))
            {
                ((Room)target).TestGeneration();
            }
            if (GUILayout.Button("Clear"))
            {
                ((Room)target).Clear();
            }
            if (GUILayout.Button("Add Monster Spawnpoint"))
            {
                ((Room)target).AddMonsterSpawnPoint();
            }

            GUILayout.Space(10); 
            if (GUILayout.Button("Generate Basic Tiles"))
            {
                ((Room)target).GenerateBasicTilesEditor();
            }
            if (GUILayout.Button("Basic Tiles as Tile Spawner"))
            {
                ((Room)target).TurnAllBasicTilesInTileSpawner();
            }
            if (GUILayout.Button("Clear Basic Tiles"))
            {
                ((Room)target).ClearBasicTiles();
            }


            GUILayout.Space(10);
            if (GUILayout.Button("Toggle Spawnpoint View"))
            {
                ((Room)target).ToggleSpawnPointView();
            }
        }
    }
}