using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

public class MeshSaveEditor : MonoBehaviour
{
    [MenuItem("CONTEXT/MeshFilter/Save Mesh")]
    public static void SaveMesh(MenuCommand menuCommand) {
        var meshFilter = menuCommand.context as MeshFilter;
        var mesh = meshFilter.sharedMesh;
        var path = EditorUtility.SaveFilePanel("save mesh", "Assets/", "sliceMesh", "mesh");
        if (string.IsNullOrEmpty(path)) {
            return;
        }
        var saveMesh = Instantiate(mesh);
        path = FileUtil.GetProjectRelativePath(path);
        AssetDatabase.CreateAsset(saveMesh,path);
        AssetDatabase.SaveAssets();
    }
}
