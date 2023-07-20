using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试网络通信
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.12.10
    /// 版本：1.0
    /// </summary>
    public class TestNetworkHttp : MonoBehaviour
    {
        public string phoneNum = "13026112540";
        //飞村正式环境用户登录获取验证码
        public string url = "https://apizgc.tokenbty.com/v1/customer/code/get_code";

        private void Start()
        {
            NetworkHttp.GetInstance.SendRequest(RequestType.Get, url, new Dictionary<string, string>()
            {
                { "platform","3d"},//接口调用来源（pc,ios,android,3d）
                { "mobile",phoneNum},//手机号码
                { "type","1"},//验证码类型（1短信2语音）
            }, (string json) =>
            {
                Debug.Log("服务器返回 json：" + json);
            }, null, null, (p, k) =>
            {
                Debug.Log($"请求失败 错误信息：{p}，请求失败啊的接口地址：{k}");
            });
        }
    }
}