using LitJson;
namespace MFramework
{
    /// <summary>
    /// 描述：用于解析Json，Json与objet相互转换
    /// 作者：毛俊峰
    /// 时间：2022.04.06
    /// 版本：1.0
    /// </summary>
    public class JsonTool : Singleton<JsonTool>
    {
        #region Json字符串转object 基于LitJson
        /// <summary>
        /// Json转object基于LitJson，返回JsonData
        /// </summary>
        /// <param name="jsonStr"></param>
        public JsonData JsonToObjectByLitJson(string jsonStr)
        {
            JsonData jd = JsonMapper.ToObject(jsonStr);
            return jd;
        }
        /// <summary>
        /// Json字符串转object基于LitJson，返回T对象
        /// 注意：T可以为类或结构体，其中json的key要与其 字段或属性名相同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public T JsonToObjectByLitJson<T>(string jsonStr)
        {
            T jd = JsonMapper.ToObject<T>(jsonStr);
            return jd;
        }
        #endregion

        #region object转Json 基于LitJson todo

        /// <summary>
        /// 对象实体 转 Json字符串 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string ObjectToJsonStringByLitJson<T>(T obj)
        {
            string jsonString = JsonMapper.ToJson(obj);
            return jsonString;
        }

        #endregion
    }
}
