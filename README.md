# Unity-Basic
> 基于 Unity 2022.3.8;未经严格测试，存在未知 bug </p>

- 尽可能保持各个模块的独立，需要手动初始化每个模块。并调用<font color=#FFAA00>Tick()</font>函数进行每帧更新(有的话)
- 上班，后面不定期更新一下
- 简单直接为主

## Menu
- [Asset Load](#asset-打包和加载) - 资源打包和加载
- [UIManager](#ui-管理) - UI管理
- [Console](#游戏内console) - 游戏内控制台
- [EventSystem](#事件系统-event-system) - 简易事件系统
- [Scene](#scene-场景加载) - 场景加载
- [Animation-Playable](#动画-基于playable) - 基于Playable的动画机

## <font color=00AAFF>Asset 打包和加载</font>
### 打包
1. 在需要打包的目录添加一份pkg.json文件，格式如下
现在会自动生成依赖，但是要全部资源一起打包才能得到全部的依赖
~~~ json
{
  "type": "bundle", // 打包类型，bundle 或 file
  "target": [       // 目标文件后缀
    ".prefab",
    ".png"
  ],
  "deps": [
    "arts/model"    // 废弃字段
  ]
}
~~~
2. 选择需要打包的文件夹，右键选择 Build/Build。如果文件夹下有 pkg.json， 会将文件夹的资源打成一个包。如果子文件夹也有 pkg.json,那么这个子文件夹会单独打包

### 加载

可以同步或异步加载，目前接口只能加载 AB 包里的资源</p>
涉及到异步，是潜在bug最多的地方
~~~C#
var handle = AssetLoad.Instance.LoadAsync("arts/prefab.bundle/cube.prefab"); 

var gameObject =  AssetLoad.Instance.Load<GameObject>("arts/prefab.bundle/cube.prefab");
~~~

## <font color=00AAFF>UI 管理</font>
> 不完善，没有对数据变化的监听

基础脚本
1. UIPanel - 必须挂在 UGUI 的 Canvas 组件上
2. UIElement - 基础UI元素，通过继承派生出其他的ui
3. UIEvent - UI事件的基类，目前提供以下类型，可自行拓展
    - UIClick
    - UIDrop
    - UIDrag
    - UIScroll
    - UIPointer

### UI 加载
1.可以先准备一份文件，以 toml 为例 。也可以用其他格式。
~~~ toml
[[ui]]
name = "ui/test"
path = "arts/prefab.bundle/ui/uitest.prefab"
~~~
初始化UIManager时传入这份文件,进行注册
~~~C#
    var ui = new UIManager();
    ui.Initialize(str);
~~~

2.自己通过 UIManager.Register(string name,string path) 进行注册。
~~~C#
public void RegisterUI(string name, string path);
~~~

通过 UIManager.Open(string name)，打开一个挂载了UIPanel脚本的Canvas，这个ui会自动挂载到UIManager下面。</p>
也可以用 UIManager.Close(string name)关闭。</p>

~~~C#
public void Initialize(string uiToml = "");

public void Tick();

// AppBootstrap.ui.Open("ui/test");
// open canvas
public void Open(string name);

// close canvas
public void Close(string name);

// load UIElement
public UIElementHandle Load(string name);
~~~

### UI 查找
可以调用 UIManager 和 UIElement 的 Query 函数
~~~C#
public T Query<T>(string name = "");

public void QueryAll<T>(List<T> list, string name = "");
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
### UI事件触发
在作为触发器的ui上挂载触发脚本，例如 UIClick。这个事件会逐级向上传递，但不包括它自己。</p>
现在包含 UIClick 在内，有5种
~~~C#
public class UIClick : UIElement, IPointerClickHandler {
    public void OnPointerClick(PointerEventData eventData) {
        uiManager.Dispatch(new UIClickEvent(
            this,
            Config.UI_CLICK_EVENT, // 事件名称，已经事先定义好
            eventData.button,
            eventData.clickCount));
    }
}
~~~

### UI事件监听
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

## <font color=00AAFF>游戏内Console</font>

按 F12 打开和关闭，移动端展暂时无法打开
### 相关API
> 注册新的指令，以及在 console 中打印信息
~~~ C#
    // 加载Console
    var consolePanel = GameObject.Instantiate(Resources.Load<GameObject>("Console/Console"));
    
    consolePanel.RegisterCmd(string command, Action<string[]> func)

    consolePanel.Output(string value, Message type = Message.Normal)
    
~~~

## <font color=00AAFF>事件系统 Event System</font>

### 监听
需要继承 IEventListenHandle，并声明要监听的数据类型</p>
~~~C#
public class EventListener : MonoBehaviour, IEventListenHandle {
    // Start is called before the first frame update
    void Start() {
        // listen
        this.AddEventListener<int>(eventMgr, DebugLog);
    }


    public void DebugLog(EventHandle handle) {
        Debug.Log($"event number : {handle.GetEvent<int>()}");
    }
}
~~~

### API
~~~C#
    var eventMrg = new EventManager();
    eventMgr.Initilize();

    // 直接进行广播即可
    eventMgr.Broadcast(value);

    eventMgr.Notify(listener,value);

    // 记得每帧调用 Tick
    eventMgr.Tick();
~~~

## <font color=00AAFF>Scene 场景加载</font>
~~~C#
//输入资源路径
SceneManager.Instance.Load("scene.bundle/a.scene");

//由于是异步加载，需要每帧更新
SceneManager.Instance.Tick();
~~~

## <font color=00AAFF>动画-基于Playable</font>
> 能用

首先需要一份配置文本
[anim.json](./anim.json)
- clips:声明每一个动画
- layers：声明不同的layer，以进行动画混合。可以创建单个clip节点或者blendTree节点</p>
blenderTree 为 2D 空间下的，实际上会把position-位置转化为angle-角度，用angle控制混合，用position只是为了直观。

### API
~~~C#
// 进行初始化，config为配置文件的资源路径
public void Initialized(Animator animator, string config);

// 每帧更新接口
public void Tick();

// 播放某个动画(clip or blendTree)
public void Play(string clipName);

// 播放某个动画(clip or blendTree),但是有过渡
public void CrossFade(string clipName, float crossTime = 0.3f);

// 设置 blendTree 中的变量，来控制动画的融合
public void SetValue(string para, Vector2 value)
~~~
目前不支持指定layer，找到第一个name符合的就结束搜索(可以支持，但是没写)</p>
不支持动画事件(设计里是可以的，但是没写)