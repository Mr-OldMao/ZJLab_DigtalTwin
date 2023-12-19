# 仿真程序手册



## 核心配置文件

路径：../StreamingAssets/Config.txt

```json
{
    "CoreConfig": {
        "SceneID": "", 	//唯一标识当前仿真程序，为空则从Web前端获取此ID，不为空则使用当前ID
        "UseTestData":1,	//是否使用测试数据，0-不使用 1-使用
        "LocalReadFileName":"",	//本地读档的文件名称xxx.json，根据本地json文件来生成房间布局以及物体，为空则不从本地读档，从服务端获取相关数据，不为空则为本地读档，需填写文件名xxx.json(../StreamingAssets/xxx.json)，测试：SaveScene_test_20231110.json
        "SendEntityInfoHZ": 10.0	//发送视觉感知实体信息频率, n秒/次
    },
    "HttpConfig": {
        "IP": "10.11.81.241", //服务器10.101.80.21   本机10.11.81.241
        "Port": "4006"
    },
    "MqttConfig": {
        "ClientIP": "10.5.24.28" //开发环境10.5.24.28   正式环境10.5.24.27
    },
    "VideoStreaming": {
        "Frame": 20, //帧数
        "Quality": 50 //画质【1，100】
    }
}
```



## 读档

两种读档方式用于生成房间布局以及物品



### 本地读档：

配置方式：

1.需要提供指定格式的json文件xxx.json，并放置在../StreamingAssets/xxx.json目录下

2.打开../StreamingAssets/Config.json文件，修改CoreConfig的LocalReadFileName的值为xxx.json

### 服务器读档

根据web前端跳转UnityWebgl时传来的CanReadFile参数来判定是否需要读档，CanReadFile=1时可从服务器读档，反之则为非读档，即随机生成场景

### 优先级

本地读档 >服务器读档>不读档(随机生成)

## 核心脚本

程序执行入口：GameLaunch.cs

资源异步加载：LoadAssetsByAddressable.cs

核心数据缓存：MainData.cs

程序主逻辑：GameLogic.cs

指令任务调度：TaskCenter.cs

Http、Mqtt接口管理：InterfaceDataCenter.cs

随机生成房间：GenerateRoomData.cs、GenerateRoomItemModel.cs、GenerateRoomBorderModel.cs

机器人AI寻路：AIRobotMove.cs



## AB包

资源路径：Assets/GameMain/AB/...



## 程序运行流程

### 初始化场景

1. 读取Config.txt配置文件获取场景ID，Http、Mqtt接口IP，端口等重要信息用于初始化场景数据

2. 通过用Addressable异步加载ab资源

3. 通过配置文件信息，获取场景数据来源，优先级：测试版 > 本地读档 > 服务器读档 > 不读当(随机生成)

   测试版：即MainData.UseTestData=true，获取数据全程跳过接口，改为固定写死的数据

   本地读档：获取数据改为本地json文件中的事先写好的数据，具体用法，事先在StreamingAssets文件夹下存储好固定格式的json文件，再在../StreamingAssets/Config.txt中LocalReadFileName字段中填写前者json文件的文件名称(需要带后缀.json)

   服务器读档：从http接口中获取之前存档的json数据， 先进行预读档，通过http接口获取当前场景ID的所有数据信息并缓存，查询当前场景实例是否存在于web端列表中，若不存在则窗体弹出无法获取窗体信息，无法生成场景

   不读当(随机生成)：从http接口中获取基础数据，再由引擎中生成随机数，获取具体数据并缓存

4. 创建机器人根节点

5. 注册消息事件，场景生成完毕后回调，具体内容包括对未存档过的场景，自动存档一次，场景原点偏移，机器人实体的生成，初始化相机，初始化视频流，缓存所有实体物品数据信息，提交场景图物体与房间的邻接关系，提交场景图布局房间与房间位置关系，启动协程持续提交场景全局实体信息以及摄像机前的实体信息，初始化任务调度模块

6. 获取并缓存房间与房间邻接关系数据以及所有房间实体数据，若为服务器读档 调用http接口获取，缓存数据并返回

7. 接入Mqtt通信，其中PC端(win、Linux)通过WebGLSupport发起，Web端通过h5中js实现

8. 协程等待ab资源全部加载完毕，以及http接口数据缓存完毕后解析缓存的数据，生成对应的实体(墙壁、门、地板、天花板、其他物品)，生成场景主UI窗体

9. 等待所有实体生成完毕，发送场景生成完毕消息标识



### 运行时功能

1.“启动、停止、暂停、继续”，调用Http接口改变仿真引擎状态，simulator/changeSimulatorState

2.“重新生成场景”，整体房间布局不变，重新生成不同规格的房间，房间中的物品相对调整，调用api：GameLogic.GetInstance.GenerateScene()

3.“机器人重定位”，调用api：GameLogic.GetInstance.GenerateRobot()

4.“场景布局存档”，存储当前场景中必要的数据，调用api：DataSave.GetInstance.Save()，

存储接口地址：http://10.101.80.74:8080/simulation/history/add

存储数据结构：List<RoomInfo> 、List<RoomBaseInfo>、List<BorderEntityData>、PostThingGraph、GetEnvGraph_data、List<RoomMatData>

5.“实时视频流”，开启后通过Mqtt通信持续传输第一、三人称的相机视频流信息，渲染的质量和发送的频率可在配置文件../StreamingAssets/Config.txt中调整， 调用api：LiveStreaming.GetInstance.IsBeginLiveStreaming = ison



