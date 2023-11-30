/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：Web端改变布局数据结构
/// 创建时间：2023/11/27 10:32:45
/// </summary>
public class JsonChangeRoomLayout
{
    public string sceneID;
    public string roomType;
    public string roomID;
    //public JsonChangeRoomLayout_oldRoomPos oldRoomPos;
    //public JsonChangeRoomLayout_newRoomPos newRoomPos;
    public JsonChangeRoomLayout_offsetPos offsetPos;
    public string ChangeTime;



    public class JsonChangeRoomLayout_oldRoomPos
    {
        public JsonChangeRoomLayout_oldRoomPos_roomPosMin roomPosMin;
        public JsonChangeRoomLayout_oldRoomPos_roomPosMax roomPosMax;
    }
    public class JsonChangeRoomLayout_oldRoomPos_roomPosMin
    {
        public float x;
        public float y;
    }
    public class JsonChangeRoomLayout_oldRoomPos_roomPosMax
    {
        public float x;
        public float y;
    }
    public class JsonChangeRoomLayout_offsetPos
    {
        public float x;
        public float y;
    }
    public class JsonChangeRoomLayout_newRoomPos
    {
        public JsonChangeRoomLayout_newRoomPos_roomPosMin roomPosMin;
        public JsonChangeRoomLayout_newRoomPos_roomPosMax roomPosMax;
    }
    public class JsonChangeRoomLayout_newRoomPos_roomPosMin
    {
        public float x;
        public float y;
    }
    public class JsonChangeRoomLayout_newRoomPos_roomPosMax
    {
        public float x;
        public float y;
    }
}