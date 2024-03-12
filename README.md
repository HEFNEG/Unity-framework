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
> 不完善，没有对数据变化的监听

基础脚本
1. UIPanel - 必须挂在 UGUI 的 Canvas 组件上
2. UIElement - 基础UI元素，通过继承派生出其他的ui
3. UIEvent - UI事件的基类

### UI 加载
1.可以先准备一份文件，以 toml 为例 。也可以用其他格式。
~~~ toml
[[ui]]
name = "ui/test"
path = "arts/prefab.bundle/ui/uitest.prefab"
~~~

2.通过 UIManager.Register(string name,string path) 进行注册。
~~~C#
AppBootstrap.ui.Register(name,path);
~~~

通过 UIManager.Open(string name)，打开一个挂载了UIPanel脚本的Canvas，这个ui会自动挂载到UIManager下面。</p>
也可以用 UIManager.Close(string name)关闭。</p>
用 UIManager.Load(string name),加载一个有UIElement脚本的ui组件，需要自行决定实例化到哪里。

~~~C#
// open canvas
AppBootstrap.ui.Open("ui/test");
// close canvas
AppBootstrap.ui.Close("ui/test");
// load ui
UIElementHandle = AppBootstrap.ui.Load("ui/test");
~~~

### UI 查找
可以调用 UIManager 和 UIElement 的 Query 函数
~~~C#
var panel = AppBootstrap.ui.Query<UIPanel>("ui/test");
var element = panel.Query<UIElement>();
~~~

### UI事件派发
UIEvent的结构如下
~~~C#
public class UIEvent {
    public readonly UIElement sender;  // 触发者
    public string senderName => sender.name;
    public string eventName { get; private set; } // 事件名称

    public UIEvent(UIElement element, string eventName) {
        this.sender = element;
        this.eventName = eventName;
    }
}
~~~
#### 触发
在作为触发器的ui上挂载触发脚本，例如 UIClick。这个事件会逐级向上传递，但不包括它自己。</p>
现在包含 UIClick 在内，有6种脚本
~~~C#
public class UIClick : UIElement, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData) {
        AppBootstrap.ui.Dispatch(new UIClickEvent(
            this,
            Config.UI_CLICK_EVENT, // 事件名称，已经事先定义好
            eventData.button,
            eventData.clickCount));
    }
}
~~~

#### 监听
~~~C#
public class UITest : UIPanel {
    public override void OnShow() {
        base.OnShow();
        AddListener(EventHandle); // 添加要执行的事件
        Debug.Log("UI Show");
    }

    public override void OnHide() {
        base.OnShow();
        RemoveListener(EventHandle);// 移除要执行的事件
        // RemoveAllListener();
        Debug.Log("UI Hide");
    }

    public void EventHandle(UIEvent uiEvent) {
        Debug.Log(uiEvent.eventName); // 输出事件的名称
    }
}
~~~