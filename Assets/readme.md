# 仿真程序手册



## 配置文件

路径：StreamingAssets/Config.txt

```json
{
    "CoreConfig": {
        "SceneID": "", 	//唯一标识当前仿真程序，为空则从Web前端获取此ID，不为空则使用当前ID
        "UseTestData":1,	//是否使用测试数据，0-不使用 1-使用
        "LocalReadFileName":"",	//本地读档的文件名称xxx.json，根据本地json文件来生成房间布局以及物体，为空则不为本地读档，从服务端获取相关数据，不为空则为本地读档，需填写文件名xxx.json(../StreamingAssets/xxx.json)，测试：SaveScene_test_20231110.json
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

两种读档方式：

### 本地读档：

配置方式：

需要提供指定格式的json文件xxx.json，并放置在../StreamingAssets/xxx.json目录下

打开../StreamingAssets/Config.json文件，修改CoreConfig的LocalReadFileName的值为xxx.json

### 服务器读档

根据web前端跳转UnityWebgl时传来的CanReadFile参数来判定是否需要读档，CanReadFile=1时可从服务器读档，反之则为非读档，即随机生成场景

### 优先级

若本地读档和服务器读档同时开启时，本地读档优先级更高



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

