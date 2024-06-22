using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GetAnimationClip : Editor {
    [MenuItem("Assets/Develop Tools/Separate Anim", priority = 1)]
    public static void SeparateAnimByModel() {
        string[] strs = Selection.assetGUIDs;

        if(strs.Length > 0) {
            int gameNum = strs.Length;
            string animFolder = EditorUtility.OpenFolderPanel("select folder", "", "");
            animFolder = Path.GetRelativePath(Application.dataPath + "/..", animFolder);
            for(int i = 0; i < gameNum; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(strs[i]);
                //Debug.Log(assetPath); //具体到fbx的路径
                
                // 获取assetPath下所有资源
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                bool isCreate = false;
                List<Object> animation_clip_list = new List<Object>();
                foreach(Object item in assets) {
                    if(typeof(AnimationClip) == item?.GetType())//找到fbx里面的动画
                    {
                        Debug.Log("找到动画片段：" + item);
                        if(!item.name.StartsWith("__preview")) {
                            animation_clip_list.Add(item);
                        }
                    }
                }
                foreach(AnimationClip animation_clip in animation_clip_list) {
                    Object new_animation_clip = new AnimationClip();
                    EditorUtility.CopySerialized(animation_clip, new_animation_clip);
                    string animation_path = Path.Combine(animFolder, new_animation_clip.name + ".anim").Replace('\\','/');
                    Debug.Log(animation_path);
                    AssetDatabase.CreateAsset(new_animation_clip, animation_path);

                    isCreate = true;
                }
                AssetDatabase.Refresh();
                if(isCreate)
                    Debug.Log("自动创建动画片段成功：" + animFolder);
                else
                    Debug.Log("未自动创建动画片段。");
            }
        } else {
            Debug.LogError("请选中需要一键提取动画片段的模型");
        }
    }
}
