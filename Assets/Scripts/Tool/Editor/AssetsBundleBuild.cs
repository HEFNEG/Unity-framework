using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;
using System.IO;

public class AssetsBundleBuild {
    [MenuItem("Assets/Build/clear cache")]
    public static void ClearCache() {
        Directory.Delete(Application.dataPath + "/../Library/Bee",true);
        Directory.Delete(Application.dataPath + "/../Library/BuildPlayerData",true);
    }

    [MenuItem("Assets/Build/Build", priority = 1)]
    public static void BuildAsset() {
        Dictionary<string, string> pkgs = new Dictionary<string, string>();
        Queue<string> iterDirectory = new Queue<string>();
        List<AssetBundleBuild> bundleBuilds = new List<AssetBundleBuild>();

        if(!Directory.Exists(AssetsConfig.path)) {
            Directory.CreateDirectory(AssetsConfig.path);
        }

        var selections = Selection.GetFiltered<Object>(SelectionMode.Assets);
        foreach(var obj in selections) {
            var path = AssetDatabase.GetAssetPath(obj);
            iterDirectory.Enqueue(path);
            while(iterDirectory.Count > 0) {
                var currentPath = iterDirectory.Dequeue();
                var pkg = Path.Combine(currentPath, ".pkg").Replace('\\', '/');
                if(File.Exists(pkg)) {
                    pkgs.Add(currentPath, pkg);
                }
                foreach(var str in Directory.GetDirectories(currentPath)) {
                    iterDirectory.Enqueue(str);
                }
            }
            foreach(var pair in pkgs) {
                Build(pair.Key, pair.Value,ref bundleBuilds);
            }

            BuildPipeline.BuildAssetBundles(AssetsConfig.path, bundleBuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            pkgs.Clear();
            bundleBuilds.Clear();
        }
        File.Delete(Path.Combine(AssetsConfig.path, "StreamingAssets").Replace('\\', '/'));
    }

    public static void Build(string path, string pkg ,ref List<AssetBundleBuild> bundleBuilds) {
        if(string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path)) {
            return;
        }

        // ╪сть .pkg
        var pkgJson = JsonMapper.ToObject(File.ReadAllText(pkg));
        var buildTarget = pkgJson["target"];
        Debug.Log(pkg);

        List<string> bundleFiles = new List<string>();
        Queue<string> directorys = new Queue<string>();
        directorys.Enqueue(path);
        while(directorys.Count > 0) {
            var currentPath = directorys.Dequeue();

            for(int i = 0; i < buildTarget.Count; i++) {
                var files = Directory.GetFiles(currentPath, "*" + buildTarget[i]);
                for(int j = 0; j < files.Length; j++) {
                    bundleFiles.Add(files[j].Replace('\\', '/'));
                }
            }

            var children = Directory.GetDirectories(currentPath);
            for(int i = 0; i < children.Length; i++) {
                if(!File.Exists(Path.Combine(children[i], ".pkg").Replace('\\', '/'))) {
                    directorys.Enqueue(children[i]);
                }
            }
        }

        // build bundle
        AssetBundleBuild bundleBuild = new AssetBundleBuild();
        bundleBuild.assetBundleName = path.Substring(path.IndexOf('/') + 1);
        bundleBuild.assetBundleVariant = "bundle";
        bundleBuild.assetNames = bundleFiles.ToArray();
        bundleBuilds.Add(bundleBuild);
    }
}
