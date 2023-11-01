# 仿真程序手册



## 配置文件

路径：StreamingAssets/Config.txt

```json
{
    "CoreConfig": {
        "SceneID": ""	//唯一标识当前仿真程序，为空则从Web前端获取此ID，不为空则使用当前ID
    },
    "HttpConfig": {
        "IP": "10.101.80.21", //服务器10.101.80.21   本机10.11.81.241
        "Port": "4006"
    },
    "MqttConfig": {
        "ClientIP": "10.5.24.27"  //开发环境10.5.24.28   正式环境10.5.24.27
    },
    "VideoStreaming": {
        "Frame": 20, //帧数
        "Quality": 50 //画质【1，100】
    }
}
```



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

