/// <summary>
/// Json映射的实体类 以下代码自动生成
/// 接口地址：
/// 创建时间：2023/9/18 16:37:08
/// </summary>
public class ConfigData
{
    public ConfigData_CoreConfig CoreConfig;
    public ConfigData_HttpConfig HttpConfig;
	public ConfigData_MqttConfig MqttConfig;
    public ConfigData_VideoStreaming VideoStreaming;

    public class ConfigData_CoreConfig
    {
        public string SceneID;
        public int UseTestData;
        /// <summary>
        /// //读档根据本地json文件来生成房间布局以及物体，为空则从服务端获取相关数据，不为空则填写文件名xxx.json(../StreamingAssets/xxx.json)
        /// </summary>
        public string LocalReadFileName;
        /// <summary>
        /// 发送视觉感知实体信息频率, n秒/次
        /// </summary>
        public float SendEntityInfoHZ;
    }
    public class ConfigData_HttpConfig
	{
		public string IP;
		public string Port;
	}
	public class ConfigData_MqttConfig
	{
		public string ClientIP;
	}
    public class ConfigData_VideoStreaming
    {
        public int Frame; 
		public int Quality;
    }
}