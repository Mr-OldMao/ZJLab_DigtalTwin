using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static JsonEntityThingGraph;

/// <summary>
/// 标题：测试Json数据收发基于HTTP协议
/// 功能：接口数据收发测试=》收到数据后反序列化到对象=》缓存重要信息
/// 目的：室内房间模型根据缓存的节点信息来生成
/// 作者：毛俊峰
/// 时间：2023.07.21
/// </summary>
public class TestJsonDataSendRecv : MonoBehaviour
{
    void Start()
    {
        SendHttpMsg();
    }


    private void SendHttpMsg()
    {
        string url = "http://10.11.81.241:4006/brain/show/thingGraph";
        MFramework.NetworkHttp.GetInstance.SendRequest(RequestType.Post, url, new Dictionary<string, string> { }, (string jsonStr) =>
        {
            JsonEntityThingGraph jsonEntityThingGraph = JsonTool.GetInstance.JsonToObjectByLitJson<JsonEntityThingGraph>(jsonStr);
            ChacheNodeInfo(jsonEntityThingGraph);
            Debug.Log("recv callback jsonStr:" + jsonStr + ",jsonEntityThingGraph:" + jsonEntityThingGraph + ",m_DicNodeInfo:" + m_DicNodeInfo);
        });
    }

    private void ChacheNodeInfo(JsonEntityThingGraph jsonEntityThingGraph)
    {
        if (jsonEntityThingGraph == null)
        {
            return;
        }
        //缓存节点id、名称信息
        for (int i = 0; i < jsonEntityThingGraph?.data?.nodes?.Length; i++)
        {
            JsonEntity_data_nodes node = jsonEntityThingGraph?.data?.nodes[i];
            if (!m_DicNodeInfo.ContainsKey(node.id))
            {
                m_DicNodeInfo.Add(node.id, new NodeInfo
                {
                    itemName = node.label,
                    roomType = RoomType.Null,//TODO  jsonStr需要增加 节点所属房间的字段
                });
            }
        }
        //缓存节点指向关系
        for (int i = 0; i < jsonEntityThingGraph?.data?.edges?.Length; i++)
        {
            JsonEntity_data_edges relation = jsonEntityThingGraph?.data?.edges[i];
            if (m_DicNodeInfo.ContainsKey(relation.from) && m_DicNodeInfo.ContainsKey(relation.to))
            {
                NodeInfo curNode = m_DicNodeInfo[relation.from];
                NodeInfo beforeNode = m_DicNodeInfo[relation.to];

                if (curNode.beforeNodeItemInfoArr == null)
                {
                    curNode.beforeNodeItemInfoArr = new List<NodeInfo> { beforeNode };
                }
                else if (!curNode.beforeNodeItemInfoArr.Contains(beforeNode))
                {
                    curNode.beforeNodeItemInfoArr.Add(beforeNode);
                }
                else
                {
                    Debug.LogError("curNode=>beforeNode info exist,curNode:" + curNode + ",beforeNode:" + beforeNode);
                }

                if (beforeNode.nextNodeItemInfoArr == null)
                {
                    beforeNode.nextNodeItemInfoArr = new List<NodeInfo> { curNode };
                }
                else if (!beforeNode.nextNodeItemInfoArr.Contains(curNode))
                {
                    beforeNode.nextNodeItemInfoArr.Add(curNode);
                }
                else
                {
                    Debug.LogError("curNode=>nextNode info exist,curNode:" + beforeNode + ",nextNode:" + curNode);
                }
            }
            else
            {
                Debug.LogError("node is null,formNode:" + relation.from + ",toNode:" + relation.to);
            }
        }
    }

    /// <summary>
    /// 缓存行为树图中所有节点信息  k-id v-节点信息
    /// </summary>
    private Dictionary<int, NodeInfo> m_DicNodeInfo = new Dictionary<int, NodeInfo>();

    /// <summary>
    /// 节点信息(当前节点信息，描述各个节点间的关系)
    /// </summary>
    class NodeInfo
    {
        /// <summary>
        /// 当前节点名称
        /// </summary>
        public string itemName;
        /// <summary>
        /// 当前节点属于哪个房间
        /// </summary>
        public RoomType roomType;
        /// <summary>
        /// 当前节点的下一级后继节点
        /// </summary>
        public List<NodeInfo> nextNodeItemInfoArr;
        /// <summary>
        /// 当前节点的上一级前驱节点，此节点为空则表示为根节点
        /// </summary>
        public List<NodeInfo> beforeNodeItemInfoArr;
    }
}
