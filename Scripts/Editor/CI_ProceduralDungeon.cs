using Generator.Dungeon;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(ProceduralDungeon))]
public class CI_ProceduralDungeon : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate"))
        {
            ((ProceduralDungeon)target).GenerateDungeon();
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Seed"))
        {
            ((ProceduralDungeon)target).SetRandomSeed();
        }
        if (GUILayout.Button("Clear"))
        {
            ((ProceduralDungeon)target).Clear();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Toggle Grid View"))
        {
            ((ProceduralDungeon)target).ToggleGridView();
        }
    }
}