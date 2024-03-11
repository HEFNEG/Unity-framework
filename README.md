# Unity-Basic
> 基于 Unity 2022.3.8;未经严格测试，存在未知 bug </p>
> 用了一点点 DOTS 来初始化,需要安装 Entities 包;也可以将这部分搬到 MonoBehavior

由于坑会越挖越大，先保证最基础的功能
## Asset 打包和加载
### 打包
1. 在需要打包的目录添加一份pkg.json文件，格式如下
~~~ json
{
  "type": "bundle", // 打包类型，bundle 或 file
  "target": [       // 目标文件后缀
    ".prefab",
    ".png"
  ],
  "deps": [
    "arts/model"    // 依赖，仅bundle有效
  ]
}
~~~
2. 选择需要打包的文件夹，右键选择 Build/Build。会根据 pkg.json 将文件夹的资源打成一个包。如果子文件夹也有 pkg.json,那么这个子文件夹会单独打包

### 加载

可以同步或异步加载，目前只能加载 AB 包里的资源</p>
涉及到异步，是潜在bug最多的地方
~~~C#
var handle = AppBootstrap.asset.LoadAsync("arts/prefab.bundle/cube.prefab"); 

var gameObject = AppBootstrap.asset.Load<GameObject>("arts/prefab.bundle/cube.prefab");
~~~

## UI 管理
> 施工中