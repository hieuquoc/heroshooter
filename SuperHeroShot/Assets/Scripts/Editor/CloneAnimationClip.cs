using UnityEngine;
using UnityEditor;
using System.IO;

public class CloneAnimationClip
{
    [MenuItem("Assets/Clone Animation Clip (Editable)", false, 20)]
    static void Clone()
    {
        foreach (var obj in Selection.objects)
        {
            var src = obj as AnimationClip;
            if (src == null) continue;

            // Instantiate a fully independent copy
            var clone = Object.Instantiate(src);
            clone.name = src.name + "_Clone";

            // Save into Assets/AnimationsCloned/
            string dir = "Assets/AnimationsCloned";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{clone.name}.anim");
            AssetDatabase.CreateAsset(clone, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(clone);
            Debug.Log($"[CloneAnimationClip] Saved editable clip → {path}");
        }
    }

    [MenuItem("Assets/Clone Animation Clip (Editable)", true)]
    static bool Validate() =>
        Selection.activeObject is AnimationClip;
}
