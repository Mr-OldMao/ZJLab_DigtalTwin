# API手册

[TOC]





# ReadMe

API手册、开发文档、版本记录



# Example

范例

### AssetsUnityPackage

测试某些案例所需要的资源，如资源加载、音频播放

## ExampleTest.unity

测试场景



# Framework

框架主体



## 0GameLaunch





## 1Manager

管理器

### AudioManager.cs

### HotUpdateManager

### MainManager.cs

### ResManager

### ResManager

资源加载管理器，对ResLoader进行封装

核心API

```c#
/// <summary>
/// 同步加载资源
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
/// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
/// <param name="loadModel">加载方式</param>
/// <param name="goCloneReturn">T:GameObject 是否自动克隆并返回</param>
/// <returns></returns>
public static T LoadSync<T>(string resPath, LoadMode resType = LoadMode.Default, bool goCloneReturn = true) 
	where T : UnityEngine.Object
        
/// <summary>
/// 异步加载资源
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="resPath">资源路径，具体格式根据加载方式loadModel而定</param>
/// <param name="callback">完成回调</param>
/// <param name="loadModel">资源加载方式</param>
public static void LoadAsync<T>(string resPath, Action<T> callback, LoadMode resType = LoadMode.Default) 
	where T : UnityEngine.Object
        
/// <summary>
/// 卸载指定资源
/// </summary>
/// <param name="resPath">资源路径</param>
public static void UnLoadAssets(string resPath, LoadMode loadMode = (LoadMode)(-1))
```



### UIManager.cs

UI管理器

功能：UI资源的加载、卸载、面板层级配置、屏幕适配

核心API

```c#
    /// <summary>
    /// 显示UI窗体
    /// </summary>
    /// <typeparam name="T">UIFormBase的派生类</typeparam>
    /// <returns></returns>
    public T Show<T>() where T : UIFormBase

    /// <summary>
    /// 显示UI窗体
    /// </summary>
    /// <typeparam name="T">UIFormBase的派生类</typeparam>
    /// <param name="uiFormName">UI窗体路径："Assets/xxx/xx.prefab"</param>
    /// <returns></returns>
    public T Show<T>(string uiFormName, UILayerType uILayerType = UILayerType.Common) where T : UIFormBase
    
    /// <summary>
    /// 隐藏UI窗体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Hide<T>() where T : UIFormBase

    /// <summary>
    /// 获取UI窗体实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public GameObject GetUIFormEntity<T>() where T : UIFormBase
        
    /// <summary>
    /// 获取UI窗体的逻辑脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetUIFormLogicScript<T>() where T : UIFormBase
```



## 2Utility

### HotUpdate

热更新路径配置

HotUpdateConfig.cs

版本信息数据，热更文件信息的读写

VersionData.cs

### IO



### WindowsInteraction

OpenDialogFile.cs

PC平台，非编辑器下控制Windows窗体交互，对硬盘文件夹选择读取操作

```c#
// 打开文件目录
FolderBrowserHelper.OpenFolder(@"E:\temp");
// 选择JPG/PNG图片文件并返回选择文件路径
FolderBrowserHelper.SelectFile(_ => Debug.Log(_), FolderBrowserHelper.IMAGEFILTER);
// 选择文件目录并返回选择文件夹路径
Debug.Log(FolderBrowserHelper.GetPathFromWindowsExplorer());
```



数据读写接口

AbFileIO.cs

文本文件读写

FileIOTxt.cs

核心API:

```c#
//初始化
AbFileIO io =new FileIOTxt("根目录","文件名(带后缀)")
//读
io.Read();
//写
io.Write("文本内容");
```



### Json

Json序列化、反序列化插件LitJson源代码



### MsgEvent

消息系统

脚本：MsgEvent.cs

标题：消息系统 

功能：基于事件的消息系统

目的：降低不同模块之间的相互调用的耦合性 

具体功能：消息注册(四种)：1.不带参、不带返回值 2.带参、不带返回值 3.不带参、带返回值 4.带参、带返回值 

​					消息发送(四种)：1.不带参、不带返回值 2.带参、不带返回值 3.不带参、带返回值 4.带参、带返回值

​					消息注销(九种)：1.注销指定类型指定消息 2.注销指定类型所有消息 3.注销所有类型所有消息

核心API：

```c#
//注册消息
        //注册不带参不带返回值消息
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, () => { Debug.Log("无参、无返回、lamdba"); });
        //注册带参不带返回值消息
        MsgEvent.RegisterMsgEvent(MsgEventName.Test, (object obj) => { Debug.Log("有参、无返回、lamdba obj：" + obj); });
		//注册不带参带返回值消息
		MsgEvent.RegisterMsgEvent(MsgEventName.Test, () =>
        {
            Debug.Log("无参、有返回、lamdba");
            int value = 123;
            return value;
        });
		//注册无参无返回值消息
		MsgEvent.RegisterMsgEvent(MsgEventName.Test, (object friendID) =>
        {
            Debug.Log("有参、有返回、lamdba");
            string res = (string)friendID + "1111";
            return res;
        });

//注销消息
		//注销指定类型指定消息，仅针对非lamdba事件注册的消息使用
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, action);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, actionParam);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, func);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, funcParam);
        //注销指定类型所有消息，可用于注销使用Lamda和非Lamdba表达式注册过的事件消息
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamNoReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamNoReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.NoParamHasReturn);
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test, MsgEventType.HasParamHasReturn);
        //注销所有类型所有消息，可用于注销使用Lamda和非Lamdba表达式注册过的事件消息
        MsgEvent.UnregisterMsgEvent(MsgEventName.Test);

//发送消息
        //发送无参无返回值消息
        MsgEvent.SendMsg(MsgEventName.Test);
        //发送有参无返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, "测试发送有参无返回值消息");
        //发送无参有返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, (obj) => { Debug.Log("测试发送无参有返回值 res：" + obj); });
        //发送有参有返回值消息
        MsgEvent.SendMsg(MsgEventName.Test, "1001", (obj) => { Debug.Log("测试发送有参有返回值 res：" + obj); });
```



### Network

网络通信

脚本：NetworkHttp.cs

标题：网络通信基于Http应用层协议(高层协议) 

功能：发送HTTP请求，获取请求结果

核心API

```
NetworkHttp.GetInstance.SendRequest(RequestType.Get, "URL接口地址", new Dictionary<string, string>()
            {
            	{ "参数key","参数value"},
                { "platform","3d"},//接口调用来源（pc,ios,android,3d）
                { "mobile",phoneNum},//手机号码
                { "type","1"},//验证码类型（1短信2语音）
            }, (string json) =>
            {
                Debug.Log("服务器返回 json：" + json);
            }, {"Authorization", "TokenValue" }, "Body-raw参数 一般传入json数据", (p, k) =>
            {
                Debug.Log($"请求失败 错误信息：{p}，请求失败啊的接口地址：{k}");
            });
```



脚本：SocketClient.cs

todo

脚本：SocketServer.cs

todo



### Pool

对象池

核心脚本：Pool.cs

核心功能：获取对象、回收对象、获取正在使用的对象实例

核心API：

```c#
//初始化		
            GameObject objRes = null;
            Transform objResParent = null;
            Pool<GameObject> pool = new Pool<GameObject>(() =>
            {
                //回调 获取对象-创建新对象后回调
                GameObject newObj = UnityEngine.Object.Instantiate<GameObject>(objRes);
                newObj.transform.SetParent(objResParent);
                newObj.gameObject.SetActive(false);
                return newObj;
            }, (GameObject cube) =>
            {
                //获取对象后回调
                //cube.gameObject.SetActive(true);
            }, (GameObject cube) =>
            {
                //回收对象时回调
                //cube.gameObject.SetActive(false);
            }, 10);//预先创建对象的个数

//分配一个对象
            GameObject obj = m_PoolObj.Allocate();
//回收一个指定对象
            bool recycleObjSuc = m_PoolObj.Recycle(obj);
//回收所有正在使用的对象
            bool recycleObjsSuc = m_PoolObj.RecycleAll();
//获取所有正在使用的对象
            List<GameObject> objs = m_PoolObj.GetUsingObjs();
//回收指定正在使用的对象通过场景游戏对象名
            GameObject findObj = m_PoolObj.GetUsingObjsByObjName("sceneGameObjectName");
```



### RefCounter

引用计数器

AbRefCounter.cs 



### ResLoader

核心脚本：核心实现类封装**LoadResource.cs**   核心实现类ResLoader.cs 资源加载器

核心功能：对四种资源(Editor自定义路径资源、Resource资源、ab包、ab包中具体)的同步加载、异步加载、资源卸载

具体实现类：

ResEditor.cs	Editor自定义路径资源同步加载、异步加载、卸载

ResResources.cs	Resource资源同步加载、异步加载、卸载

ResAssetBundlePack.cs	AB包的同步加载、异步加载、卸载

ResAssetBundleAsset.cs	AB包中的具体资源同步加载、异步加载、卸载

AbRes.cs		资源实现类的抽象基类

ABSetting.cs	AB自动化标记、打包设置

测试类： ExampleTest.unity	TestResLoader.cs

核心API：

```c#
//同步加载	<泛型类型>(资源路径,资源类型)
LoadResource.LoadSync<T>("资源路径", LoadMode.Default);
//异步加载
LoadResource.LoadAsync<T>("资源路径", (T t) =>
	{
		//todo
	}, ResType.Null);
//显示当前资源信息
ResLoader.ShowResLogInfo();
//卸载指定资源
ResLoader.UnLoadAssets("资源路径", LoadMode.ResResources);

/// <summary>
/// 资源种类
/// </summary>
public enum LoadMode
{
    /// <summary>
    /// 根据GameLaunch工程模式 编辑器模式为ABSetting.resTypeDefaultEditor ，打包模式ABSetting.resTypeDefaultNotEditor
    /// </summary>
    Default = 0,
    /// <summary>
    /// 编辑器模式下资源类型  path格式：Assets/AssetsRes/ABRes/Prefab/Cube3.prefab
    /// </summary>
    ResEditor,
    /// <summary>
    /// Resources目录下资源   path格式：(Resources/) + xxx/xxx/xx
    /// </summary>
    ResResources,
    /// <summary>
    /// ab包 .manifest资源     path格式：ab标签名称   (.manifest=>AssetBundleManifest=>Name)
    /// </summary>
    ResAssetBundlePack,
    /// <summary>
    /// ab包中的资源     path格式：ab标签名称   (.manifest=>AssetBundleManifest=>Name)
    /// </summary>
    ResAssetBundleAsset
}
```







### Singleton

单例

SingletonByMono.cs

### Tool

工具类

UnityTool.cs



ImageAutoChangeByText.cs UIImage自适应Text文本长度

LoadUIHintWindows.cs	自动加载提示框UI窗体

ScreenShotSavePhoto.cs	截图保存到手机本地相册

DownloadAsset.cs	根据URL下载资源



### UI

IUIFormBase.cs	UI窗体基类接口

UIFormBase.cs	UI窗体基类

UIFormConfig.cs	UI窗体配置表

UILayerType.cs	UI层级



## 3Extension

UnityEngine静态类扩展

TransformExtension.cs

StringExtension.cs



## 4Editor

Unity编辑器扩展



### AssetBundleTool

#### ABBuild.cs

一键打包生成AssetBundle

#### ABTag.cs

AB资源标签一键标记，自动对资源进行AB标记



### Attribute

自定义特性



### AutoCreateAssetFile

脚本：AutoCreateAssetFile.cs

标题：编辑器工具，自动生成脚本对应的.asset文件

功能：右键脚本（脚本需要继承ScriptableObject）自动生成.asset文件 



### AutoCreateScript

#### EditorSceneObjMapEntity

Scene GameObject => EntityClass 场景游戏对象转实体类

核心脚本：EditorSceneObjMapEntity.cs 

目的：解决手动获取场景对象实例需要繁琐且重复的操作

功能：脚本自动化生成工具，根据场景中某游戏对象下的所有子对象，创建实体并与之对应，自动映射与关联

调用：选中菜单MFramework/脚本自动化工具，拖入根节点，设置脚本路径，点击生成，可自动获取根节点其下字段、属性、映射并生成脚本到指定路径

规则：场景游戏对象会被直接引用成为字段名，所以命名要规范，并且不要使用创建场景对象时引擎默认的名称会被屏蔽，可自动屏蔽命名不规范的游戏对象，也可也自定义屏蔽某些对象



#### EditorJsonMapEntity

Json =》 Object，Json反序列化实体类自动生成(基于Newtonsoft.Json) 脚本自动化生成工具

核心脚本：EditorJsonMapEntity.cs

目的：为解决根据Json数据进行反序列化，需要手动创建对应的对象实体进行数据缓存的问题

功能：输入Json格式的字符串，指定实体类的路径，自动生成Json反序列化所需的实体类

调用：Unity菜单栏  MFramework/脚本自动化工具/2.Json自动映射实体类



### BuildLayout

#### LayoutWindow

核心脚本：EditorCreateAssetsDirectory.cs

标题：PC平台程序窗体布局，Unity发布PC 设置窗口 无边框（显示win任务栏），或全屏无边框



### BuildTool

Unity自动打包

#### autoBuildSummary.md

自动化打包总结，自动化打包.bat命令，可用于jenkins网页自动化打包

#### BuildConfigSetting

标题：自动化打包参数配置设置窗体 配合一键打包使用
功能：配置一键打包的设置参数， 配合一键打包使用
编辑器菜单：一键打包 快捷键Shift+B  MFreamwork/BuildTool/BuildTargetPlatformBundle  
           		   参数配置 快捷键Shift+S  MFreamwork/BuildTool/BuildConfigSetting

#### BuildTool

标题：自动打包工具
功能：根据参数配置一键打包当前平台(Android,PC)的项目,
编辑器菜单：一键打包 快捷键Shift+B  MFreamwork/BuildTool/BuildTargetPlatformBundle  
		              参数配置 快捷键Shift+S  MFreamwork/BuildTool/BuildConfigSetting

### IO

#### EditorCreateAssetsDirectory

核心脚本：EditorCreateAssetsDirectory.cs

标题：编辑器工具类 自动创建Unity工程目录

目的：初始化项目时可避免频繁创建文件夹操作



### Renderer

#### MergeMesh.cs

合并模型网格 子对象mesh

用法：在编辑器模式下，在子模型的父对象上挂载此脚本，并右键选择合并网格即可得到合并后的网格







## Launch

框架启动入口



## Plugin

框架所用到的dll



## Resources

框架资源









