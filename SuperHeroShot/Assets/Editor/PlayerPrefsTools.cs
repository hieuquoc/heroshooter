using UnityEngine;
using UnityEditor;

public static class PlayerPrefsTools
{
    [MenuItem("Tools/Clear PlayerPrefs %#k")]
    private static void ClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "This will delete all PlayerPrefs. Continue?", "Clear", "Cancel"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Clear PlayerPrefs", "PlayerPrefs cleared.", "OK");
        }
    }

    [MenuItem("Tools/Clear PlayerPrefs", true)]
    private static bool ClearPlayerPrefsValidate()
    {
        return !EditorApplication.isPlaying;
    }
}
