using UnityEngine;
/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/8/21 10:44:53
/// </summary>
public class GetEnvGraph
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
	public GetEnvGraph_data data;
	public class GetEnvGraph_data
	{
		/// <summary>
		/// 
		/// </summary>
		public GetEnvGraph_data_items[] items;
	}
	public class GetEnvGraph_data_items
	{
		/// <summary>
		/// 
		/// </summary>
		public string id;
		/// <summary>
		/// 
		/// </summary>
		public string name;
		/// <summary>
		/// 当前房间所有邻接关系
		/// </summary>
		public GetEnvGraph_data_items_relatedThing[] relatedThing;
	}
	public class GetEnvGraph_data_items_relatedThing
	{
        /// <summary>
        /// 另⼀个具有空间关系的物体
        /// </summary>
        public GetEnvGraph_data_items_relatedThing_target target;
        /// <summary>
        /// 物体空间四周关系，例如top left right bottom
		/// </summary>
		public string relationship;
	}
	public class GetEnvGraph_data_items_relatedThing_target
	{
		/// <summary>
		/// 
		/// </summary>
		public string id;
		/// <summary>
		/// 
		/// </summary>
		public string name;
	}
}