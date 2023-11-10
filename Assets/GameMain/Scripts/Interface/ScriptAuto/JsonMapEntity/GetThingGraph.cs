using System;
using System.Collections.Generic;
using static GetThingGraph;
/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/8/21 10:43:35
/// </summary>
public class GetThingGraph
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
	public GetThingGraph_data data;
	public class GetThingGraph_data
	{
		/// <summary>
		/// 
		/// </summary>
		public List<GetThingGraph_data_items> items;
	}
	[Serializable]
	public class GetThingGraph_data_items
	{
        /// <summary>
        /// 
        /// </summary>
        public float[] position;
		/// <summary>
		/// 
		/// </summary>
		public float[] rotation;
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
        public List<GetThingGraph_data_items_relatedThing> relatedThing;
        /// <summary>
        /// 当前物体是否是动态的
        /// </summary>
        public bool dynamic;
	}
	[Serializable]
	public class GetThingGraph_data_items_relatedThing
	{
        /// <summary>
        /// 另⼀个具有空间关系的物体
        /// </summary>
        public GetThingGraph_data_items_relatedThing_target target;
        /// <summary>
        /// 物体空间关系，例如in on below above
        /// </summary>
        public string relationship;
	}
	[Serializable]
	public class GetThingGraph_data_items_relatedThing_target
	{
		/// <summary>
		/// 
		/// </summary>
		public float[] position;
		/// <summary>
		/// 
		/// </summary>
		public float[] rotation;
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
        public List<GetThingGraph_data_items_relatedThing> relatedThing;
        /// <summary>
        /// 
        /// </summary>
        public bool dynamic;
	}
}
[Serializable]
public class PostThingGraph
{
    public List<GetThingGraph_data_items> items;
	public string id;
    public string idScene = MainData.SceneID;
}