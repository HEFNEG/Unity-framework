using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;
using System.IO;

public static class AssetsBundleBuild {
    private readonly static string[] buildTypes = { "bundle", "file" };

    [MenuItem("Assets/Develop Tools/Build/clear cache")]
    public static void ClearCache() {
        Directory.Delete(Application.dataPath + "/../Library/Bee", true);
        Directory.Delete(Application.dataPath + "/../Library/BuildPlayerData", true);
    }

    [MenuItem("Assets/Develop Tools/Build/Build", priority = 1)]
    public static void BuildAsset() {
        Dictionary<string, string> pkgs = new Dictionary<string, string>();
        Queue<string> iterDirectory = new Queue<string>();
        List<AssetBundleBuild> bundleBuilds = new List<AssetBundleBuild>();

        if(!Directory.Exists(Config.assetPath)) {
            Directory.CreateDirectory(Config.assetPath);
        }

        var selections = Selection.GetFiltered<Object>(SelectionMode.Assets);
        foreach(var obj in selections) {
            var path = AssetDatabase.GetAssetPath(obj);
            iterDirectory.Enqueue(path);
            while(iterDirectory.Count > 0) {
                var currentPath = iterDirectory.Dequeue();
                var pkg = Path.Combine(currentPath, Config.pkgFile).Replace('\\', '/');
                if(File.Exists(pkg)) {
                    pkgs.Add(currentPath.Replace('\\','/'), pkg);
                }
                foreach(var str in Directory.GetDirectories(currentPath)) {
                    iterDirectory.Enqueue(str);
                }
            }
            foreach(var pair in pkgs) {
                Build(pair.Key, pair.Value, ref bundleBuilds);
            }

            BuildPipeline.BuildAssetBundles(Config.assetPath, bundleBuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            pkgs.Clear();
            bundleBuilds.Clear();
        }

        File.Delete(Path.Combine(Config.assetPath, "StreamingAssets").Replace('\\', '/'));
        System.GC.Collect();
    }

    public static void Build(string path, string pkg, ref List<AssetBundleBuild> bundleBuilds) {
        if(string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path)) {
            return;
        }

        // 加载 .pkg
        var pkgJson = JsonMapper.ToObject(File.ReadAllText(pkg));
        var buildType = pkgJson["type"].ToString();
        var buildTarget = pkgJson["target"];
        Debug.Log(pkg);

        List<string> bundleFiles = new List<string>();
        List<string> addressName = new List<string>();
        Queue<string> directorys = new Queue<string>();
        directorys.Enqueue(path);
        if(buildType == buildTypes[0]) {
            bundleFiles.Add(pkg);
            addressName.Add(pkg.Replace(path + "/", ""));
        }

        while(directorys.Count > 0) {
            var currentPath = directorys.Dequeue();

            for(int i = 0; i < buildTarget.Count; i++) {
                var files = Directory.GetFiles(currentPath, "*" + buildTarget[i]);
                for(int j = 0; j < files.Length; j++) {
                    bundleFiles.Add(files[j].Replace('\\', '/'));
                    addressName.Add(bundleFiles[^1].Replace(path + "/", ""));
                }
            }

            var children = Directory.GetDirectories(currentPath);
            for(int i = 0; i < children.Length; i++) {
                if(!File.Exists(Path.Combine(children[i], Config.pkgFile).Replace('\\', '/'))) {
                    directorys.Enqueue(children[i]);
                }
            }
        }

        if(buildType == buildTypes[0]) {
            // build bundle
            AssetBundleBuild bundleBuild = new AssetBundleBuild();
            bundleBuild.assetBundleName = path.Substring(path.IndexOf('/') + 1);
            bundleBuild.assetBundleVariant = "bundle";
            bundleBuild.addressableNames = addressName.ToArray();
            bundleBuild.assetNames = bundleFiles.ToArray();
            bundleBuilds.Add(bundleBuild);
        }else if(buildType == buildTypes[1]) {
            for(int i = 0; i < bundleFiles.Count; i++) {
                var file = bundleFiles[i];
                var targetFile = Config.assetPath + file.Replace("Assets/", "");
                var diret = Path.GetDirectoryName(targetFile).ToLower();
                if(!Directory.Exists(diret)) {
                    Directory.CreateDirectory(diret);
                }
                File.Copy(file, Config.assetPath + file.Replace("Assets/", ""),true);
            }
        }
    }

}
