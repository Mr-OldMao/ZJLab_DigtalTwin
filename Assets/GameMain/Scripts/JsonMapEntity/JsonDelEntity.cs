/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/10/11 10:17:23
/// </summary>
public class JsonDelEntity
{
    public string idScene = MainData.SceneID;
    public JsonDelEntity_entityInfo[] entityInfo;
	public class JsonDelEntity_entityInfo
	{
		public string id;
        public string type;
		/// <summary>
		/// 是否也删除所有子物体  0-不删除 1-删除
		/// </summary>
		public int delChind;
	}
}