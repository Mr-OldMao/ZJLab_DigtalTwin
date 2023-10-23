/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/10/23 10:50:27
/// </summary>
public class Feature_Robot_Pos
{  
	public string robotId;
	public long timestamp;
	public Feature_Robot_Pos_data data;
	public object clientId;
	public class Feature_Robot_Pos_data
	{
		public Feature_Robot_Pos_data_feature feature;
		public string featureId;
	}
	public class Feature_Robot_Pos_data_feature
	{
		public float[] orientation;
		public float[] position;
	}
}