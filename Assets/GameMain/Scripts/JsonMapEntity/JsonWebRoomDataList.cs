/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/12/19 14:25:39
/// </summary>
public class JsonWebRoomDataList
{  
	public int recode;
	public string msg;
	public JsonWebRoomDataList_data[] data;
	public bool succ;
	public class JsonWebRoomDataList_data
	{
		public int flag;
		public string createTime;
		public string thirdChannel;
		public string envSimulator;
		public string id;
	}
}