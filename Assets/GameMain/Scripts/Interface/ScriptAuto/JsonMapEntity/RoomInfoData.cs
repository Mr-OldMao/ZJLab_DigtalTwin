using System.Collections.Generic;
/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/9/8 15:12:42
/// </summary>
public class RoomInfoData
{  
	public List<RoomInfoData_roomInfos> roomInfos;
	public class RoomInfoData_roomInfos
	{
        public string id;
        public string roomType;
		public int[] minPos;
		public int[] maxPos;
	}
}