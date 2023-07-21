/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/7/21 14:13:30
/// </summary>
public class JsonEntityThingGraph
{  
	/// <summary>
	/// 
	/// </summary>
	public int code;
	/// <summary>
	/// 
	/// </summary>
	public string message;
	/// <summary>
	/// 
	/// </summary>
	public bool success;
	/// <summary>
	/// 
	/// </summary>
	public JsonEntity_data data;
	/// <summary>
	/// 
	/// </summary>
	public object results;
	public class JsonEntity_data
	{
		/// <summary>
		/// 
		/// </summary>
		public JsonEntity_data_edges[] edges;
		/// <summary>
		/// 
		/// </summary>
		public JsonEntity_data_nodes[] nodes;
		/// <summary>
		/// 
		/// </summary>
		public object time;
		/// <summary>
		/// 
		/// </summary>
		public object knowNodeList;
		/// <summary>
		/// 
		/// </summary>
		public object name;
		/// <summary>
		/// 
		/// </summary>
		public object visionState;
		/// <summary>
		/// 
		/// </summary>
		public object logUrl;
	}
	public class JsonEntity_data_edges
	{
		/// <summary>
		/// 
		/// </summary>
		public int from;
		/// <summary>
		/// 
		/// </summary>
		public int to;
		/// <summary>
		/// 
		/// </summary>
		public bool dashes;
		/// <summary>
		/// 
		/// </summary>
		public string color;
		/// <summary>
		/// 
		/// </summary>
		public int highLight;
		/// <summary>
		/// 
		/// </summary>
		public string title;
		/// <summary>
		/// 
		/// </summary>
		public string label;
		/// <summary>
		/// 
		/// </summary>
		public JsonEntity_data_edges_arrows arrows;
	}
	public class JsonEntity_data_edges_arrows
	{
		/// <summary>
		/// 
		/// </summary>
		public JsonEntity_data_edges_arrows_from from;
		/// <summary>
		/// 
		/// </summary>
		public JsonEntity_data_edges_arrows_to to;
	}
	public class JsonEntity_data_edges_arrows_from
	{
		/// <summary>
		/// 
		/// </summary>
		public bool enabled;
	}
	public class JsonEntity_data_edges_arrows_to
	{
		/// <summary>
		/// 
		/// </summary>
		public bool enabled;
	}
	public class JsonEntity_data_nodes
	{
		/// <summary>
		/// 
		/// </summary>
		public int id;
		/// <summary>
		/// 
		/// </summary>
		public string label;
		/// <summary>
		/// 
		/// </summary>
		public string title;
		/// <summary>
		/// 
		/// </summary>
		public int level;
		/// <summary>
		/// 
		/// </summary>
		public int highLight;
		/// <summary>
		/// 
		/// </summary>
		public string shape;
		/// <summary>
		/// 
		/// </summary>
		public string color;
	}
}