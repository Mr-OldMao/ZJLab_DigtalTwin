/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/10/10 9:59:46
/// </summary>
public class JsonAddEntity
{
    public string idScene = MainData.IDScene;
    public JsonAddEntity_entityInfo[] entityInfo;
	public class JsonAddEntity_entityInfo
	{
		public string id;
		public string type;
		public string modelId;
        /// <summary>
        /// 是否为动态模型  0-否 1-是
        /// </summary>
        public int dynamic;
        public string putArea;
        public JsonAddEntity_entityInfo_pos pos;
		public JsonAddEntity_entityInfo_roomInfo roomInfo;
		public JsonAddEntity_entityInfo_parentEntityInfo parentEntityInfo;
	}
	public class JsonAddEntity_entityInfo_pos
	{
		public float x;
		public float y;
	}
	public class JsonAddEntity_entityInfo_roomInfo
	{
		public string roomType;
		public string roomID;
	}
	public class JsonAddEntity_entityInfo_parentEntityInfo
	{
		public string id;
		public string type;
	}
}