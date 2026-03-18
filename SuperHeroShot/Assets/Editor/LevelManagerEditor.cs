using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace rescueforce
{
    [CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelManager lm = (LevelManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Bake Grid"))
        {
            Undo.RecordObject(lm, "Bake Grid");
            lm.BakeGrid();
            EditorUtility.SetDirty(lm);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(lm.gameObject.scene);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn All"))
        {
            lm.SpawnAll();
        }
        if (GUILayout.Button("Clear Positions"))
        {
            Undo.RecordObject(lm, "Clear Spawn Positions");
            lm.ClearSpawnPositions();
            EditorUtility.SetDirty(lm);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(lm.gameObject.scene);
        }
        if (GUILayout.Button("Clear Enemies"))
        {
            lm.ClearAllEnemies();
        }
        EditorGUILayout.EndHorizontal();
    }
}

}

