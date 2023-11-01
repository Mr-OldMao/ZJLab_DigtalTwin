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