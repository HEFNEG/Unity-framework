using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LitJson;
using System.IO;
using Game.Basic;

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
        List<JsonData> bundlePkgs = new List<JsonData>();

        if(!Directory.Exists(Config.assetPath)) {
            Directory.CreateDirectory(Config.assetPath);
        }

        var selections = Selection.GetFiltered<Object>(SelectionMode.Assets);
        // 遍历所有pkg文件
        foreach(var obj in selections) {
            var path = AssetDatabase.GetAssetPath(obj);
            iterDirectory.Enqueue(path);
            while(iterDirectory.Count > 0) {
                var currentPath = iterDirectory.Dequeue();
                var pkg = Path.Combine(currentPath, Config.pkgFile).Replace('\\', '/');
                if(File.Exists(pkg)) {
                    pkgs.Add(currentPath.Replace('\\', '/'), pkg);
                    // 添加依赖项
                    var pkgJson = JsonMapper.ToObject(File.ReadAllText(pkg));
                    var buildDeps = pkgJson["deps"];
                    for(int i = 0; buildDeps!=null && buildDeps.IsArray && i< buildDeps.Count; i++) {
                        string depPath = "Assets/" + buildDeps[i].ToString();
                        string pkgPath = Path.Combine(depPath, Config.pkgFile).Replace('\\', '/');
                        if(File.Exists(pkgPath) && !pkgs.ContainsKey(depPath)) {
                            pkgs.Add(depPath,pkgPath);
                        }
                    }
                }
                foreach(var str in Directory.GetDirectories(currentPath)) {
                    iterDirectory.Enqueue(str);
                }
            }
        }
        foreach(var pair in pkgs) {
            Build(pair.Key, pair.Value, ref bundleBuilds, ref bundlePkgs);
        }

        var manifest = BuildPipeline.BuildAssetBundles(Config.assetPath, bundleBuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        for(int i = 0; i < bundleBuilds.Count; i++) {
            AssetsBundleInfo info = new AssetsBundleInfo();
            var bundleName = bundleBuilds[i].assetBundleName;
            var allDeps = manifest.GetAllDependencies(bundleName + Config.bundleExtend);
            for(int j = 0; j < allDeps.Length; j++) {
                info.deps.Add(allDeps[j].ToLower().Replace(Config.assetPath, "").Replace(Config.bundleExtend, ""));
            }
            var allAssetsName = bundleBuilds[i].assetNames;
            for(int j = 0; j <allAssetsName.Length; j++) {
                info.allAssets.Add(allAssetsName[j].Replace(bundleName + "/","").Replace("Assets/",""));
            }

            string jsonInfo = JsonMapper.ToJson(info);
            File.WriteAllText(Config.assetPath + bundleName + Config.depExtend,jsonInfo);
        }
        

        pkgs.Clear();
        bundleBuilds.Clear();
        bundlePkgs.Clear();
        AssetDatabase.Refresh();

        var manifestFile =  Directory.GetFiles(Config.assetPath, "*" + Config.manifestExtend,SearchOption.AllDirectories);
        for(int i = 0; i < manifestFile.Length; i++) {
            File.Delete(manifestFile[i]);
        }
        File.Delete(Path.Combine(Config.assetPath, "StreamingAssets").Replace('\\', '/'));
        System.GC.Collect();
    }

    public static void Build(string path, string pkg, ref List<AssetBundleBuild> bundleBuilds, ref List<JsonData> bundlePkgs) {
        if(string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path)) {
            return;
        }

        // 加载 .pkg
        var pkgJson = JsonMapper.ToObject(File.ReadAllText(pkg));
        var buildType = pkgJson["type"].ToString();
        var buildTarget = pkgJson["target"];
        var buildDeps = pkgJson["deps"];
        Debug.Log(pkg);

        List<string> bundleFiles = new List<string>();
        List<string> addressName = new List<string>();
        Queue<string> directorys = new Queue<string>();
        directorys.Enqueue(path);

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

        if(bundleFiles.Count==0) {
            return;
        }
        
        if(buildType == buildTypes[0]) {
            // build bundle
            AssetBundleBuild bundleBuild = new AssetBundleBuild();
            bundleBuild.assetBundleName = path.Substring(path.IndexOf('/') + 1);
            bundleBuild.assetBundleVariant = "bundle";
            bundleBuild.addressableNames = addressName.ToArray();
            bundleBuild.assetNames = bundleFiles.ToArray();
            bundleBuilds.Add(bundleBuild);
            bundlePkgs.Add(pkgJson);

        } else if(buildType == buildTypes[1]) {
            for(int i = 0; i < bundleFiles.Count; i++) {
                var file = bundleFiles[i];
                var targetFile = Config.assetPath + file.Replace("Assets/", "");
                var diret = Path.GetDirectoryName(targetFile).ToLower();
                if(!Directory.Exists(diret)) {
                    Directory.CreateDirectory(diret);
                }
                File.Copy(file, Config.assetPath + file.Replace("Assets/", ""), true);
            }
        }
    }

}
