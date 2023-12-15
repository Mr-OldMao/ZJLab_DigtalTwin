using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFramework;
using static GetThingGraph;
using static JsonAddEntity;
using static GameLogic2;
/// <summary>
/// 标题：更新数据缓存
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.10.11
/// </summary>
public class MainDataTool : SingletonByMono<MainDataTool>
{
    /// <summary>
    /// h5外部调用，用于初始化场景id
    /// </summary>
    /// <param name="param">多个参数，参数间用”|“ 隔开，【0】SceneID 【1】CanReadFile</param>
    public void InitMainDataParam(string param)
    {
        string[] paramsStr = param.Split('|');
        if (paramsStr?.Length >= 0)
        {
            MainData.SceneID = paramsStr[0];
        }
        if (paramsStr?.Length >= 1)
        {
            MainData.CanReadFile = paramsStr[1] == "1";
            MainData.CacheData_CanReadFile = MainData.CanReadFile;
        }
        else
        {
            MainData.CanReadFile = false;
            MainData.CacheData_CanReadFile = MainData.CanReadFile;
        }

        Debug.Log("[Unity] InitMainDataParam , SceneID:" + MainData.SceneID);
        new ReadConfigFile(() =>
        {
            MainData.UseTestData = MainData.ConfigData.CoreConfig?.UseTestData == 1;
            if (!string.IsNullOrEmpty(MainData.ConfigData.CoreConfig?.SceneID))
            {
                MainData.SceneID = MainData.ConfigData.CoreConfig?.SceneID;
                Debugger.Log("change IDScene");
            }
            if (MainData.ConfigData.CoreConfig.SendEntityInfoHZ <= 0)
            {
                MainData.ConfigData.CoreConfig.SendEntityInfoHZ = 3f;
            }
            Debugger.Log("MainDataDisplay   SceneID：" + MainData.SceneID
                + ",UseTestData：" + MainData.UseTestData
                + ",SendEntityInfoHZ：" + MainData.ConfigData.CoreConfig.SendEntityInfoHZ
                + ",Http_IP：" + MainData.ConfigData.HttpConfig.IP
                + ",Http_Port：" + MainData.ConfigData.HttpConfig.Port
                + ",Mqtt_IP：" + MainData.ConfigData.MqttConfig.ClientIP
                + ",Vs_Frame：" + MainData.ConfigData.VideoStreaming.Frame
                + ",Vs_Quality：" + MainData.ConfigData.VideoStreaming.Quality,
                LogTag.Forever);
            MsgEvent.SendMsg(MsgEventName.InitComplete);
        });
    }
    #region 场景中新增指定实体
    /// <summary>
    /// 生成实体放置到指定位置
    /// </summary>
    public void AddEntityToTargetPlace(JsonAddEntity jsonAddEntity)
    {
        for (int i = 0; i < jsonAddEntity?.entityInfo?.Length; i++)
        {
            JsonAddEntity_entityInfo entityInfo = jsonAddEntity?.entityInfo?[i];
            JsonAddEntity_entityInfo_parentEntityInfo parentEntityInfo = entityInfo?.parentEntityInfo;
            //驼峰原则 首字母大写
            string entityType = entityInfo.type[..1].ToUpper() + entityInfo.type[1..].ToLower();
            LoadAssetsByAddressable.GetInstance.GetEntityRes(entityType, entityInfo.modelId, (obj) =>
            {
                bool addResult = false;
                if (obj != null)
                {
                    addResult = true;
                    GameObject clone = Instantiate(obj);
                    /*设置根节点*/
                    string roomObjStr = entityInfo.roomInfo.roomType.ToString() + "_" + entityInfo.roomInfo.roomID;
                    GameObject roomEntity = GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform.Find(roomObjStr)?.gameObject;
                    if (roomEntity == null)
                    {
                        roomEntity = new GameObject(roomObjStr);
                        roomEntity.transform.SetParent(GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform);
                    }
                    GameObject parentObj = null;
                    string parentID = "";
                    //当前实体有父对象则放置在父对象下，没有则放在指定房间容器内
                    if (!string.IsNullOrEmpty(parentEntityInfo?.id) && !string.IsNullOrEmpty(parentEntityInfo?.type))
                    {
                        parentID = parentEntityInfo.id;
                        Transform parentEntity = GenerateRoomItemModel.GetInstance.ItemEntityGroupNode.transform.Find<Transform>(parentEntityInfo.type + "_" + parentEntityInfo.id);
                        Transform parentEntityPutArea = parentEntity?.Find("PutArea/" + entityInfo.putArea);
                        if (parentEntityPutArea != null)
                        {
                            //放置在其他物品下
                            parentObj = parentEntityPutArea.gameObject;
                        }
                        else
                        {
                            //放置在房间下
                            parentObj = roomEntity;
                        }
                        Debug.Log($"add item suc, item:{clone}" +
                            $"roomContainer:{roomEntity}," +
                               $"parent:{parentEntityInfo.type} " + "_" +
                               $"{parentEntityInfo.id}," +
                               $"putArea:{entityInfo.putArea}");
                        clone.transform.SetParent(parentObj.transform);
                        //实体位置pos，高度y使用PutArea/xxx 节点的高度，x/y使用web前端发来的数值
                        clone.transform.position = new Vector3(entityInfo.pos.x, parentObj.transform.position.y, entityInfo.pos.y);
                        clone.transform.Find("Model").transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        parentID = entityInfo.roomInfo.roomID;
                        parentObj = roomEntity;
                        clone.transform.SetParent(parentObj.transform, false);
                        clone.transform.position = new Vector3(entityInfo.pos.x, 0, entityInfo.pos.y);
                        clone.transform.Find("Model").transform.localPosition = Vector3.zero;
                    }
                    string key = entityInfo.type + "_" + entityInfo.id;
                    clone.name = key;

                    //缓存数据1
                    if (!MainData.CacheItemsEntity.ContainsKey(key))
                    {
                        MainData.CacheItemsEntity.Add(key, clone);
                        //更新数据2
                        UpdateSceneItemsInfoData(parentID, entityInfo.id, entityInfo.type, clone.transform.position, clone.transform.rotation.eulerAngles, entityInfo.dynamic == 1, entityInfo.putArea);

                        //更新全局实体数据
                        UpdateEnityInfoTool.GetInstance.UpdateSceneEntityInfo();
                        DataSave.GetInstance.SaveGetThingGraph_data_items(MainData.CacheSceneItemsInfo);
                    }
                    else
                    {
                        Debug.LogError("物品已存在，新增物品更新数据失败 ");
                    }
                }

                //web端新增物体后回调
                JsonWebGlobalEntityData jsonWebGlobalEntityData = new JsonWebGlobalEntityData()
                {
                    result = addResult ? 1 : 0,
                    entityType = entityInfo.type,
                    entityID = entityInfo.id,
                    postThingGraph = MainData.CacheSceneItemsInfo
                };
                InterfaceDataCenter.GetInstance.SendMQTTUpdateEntity(jsonWebGlobalEntityData);

                string resStr = addResult ? "成功" : "失败";
                UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams
                {
                    txtHintContent = $"新增物体{resStr},{entityInfo.type}_{entityInfo.id}",
                    delayCloseUIFormTime = 2
                });
            });

        }
    }

    /// <summary>
    /// 更新新增的实体数据信息
    /// </summary>
    /// <param name="parentEntityName"></param>
    /// <param name="curID"></param>
    /// <param name="curPos"></param>
    /// <param name="curRot"></param>
    /// <param name="isDynamic"></param>
    /// <param name="relationship"></param>
    private void UpdateSceneItemsInfoData(string parentEntityName, string curID, string curType, Vector3 curPos, Vector3 curRot, bool isDynamic, string relationship)
    {
        //find parentID
        //父对象容器
        List<GetThingGraph_data_items_relatedThing> parentContainer = null;
        for (int i = 0; i < MainData.CacheSceneItemsInfo.items.Count; i++)
        {
            string id = MainData.CacheSceneItemsInfo.items[i].id;
            if (id == parentEntityName)
            {
                if (MainData.CacheSceneItemsInfo.items[i].relatedThing == null)
                {
                    MainData.CacheSceneItemsInfo.items[i].relatedThing = new List<GetThingGraph_data_items_relatedThing>();
                }
                parentContainer = MainData.CacheSceneItemsInfo.items[i].relatedThing;
                break;
            }
            else
            {
                parentContainer = FindTargetIDData(ref MainData.CacheSceneItemsInfo.items[i].relatedThing, parentEntityName);
            }
        }
        if (parentContainer != null)
        {

            parentContainer.Add(new GetThingGraph_data_items_relatedThing
            {
                relationship = relationship,
                target = new GetThingGraph_data_items_relatedThing_target
                {
                    id = curID,
                    name = curType,
                    position = new float[] { curPos.x, curPos.y, curPos.z },
                    rotation = new float[] { curRot.x, curRot.y, curRot.z },
                    dynamic = isDynamic,
                    relatedThing = null
                }
            });
        }
    }

    /// <summary>
    /// 递归寻找目标实体json数据信息
    /// </summary>
    /// <param name="node"></param>
    /// <param name="targetID"></param>
    /// <returns></returns>
    private List<GetThingGraph_data_items_relatedThing> FindTargetIDData(ref List<GetThingGraph_data_items_relatedThing> node, string targetID)
    {
        for (int i = 0; i < node.Count; i++)
        {
            int tempI = i;
            if (node[tempI].target.id == targetID)
            {
                if (node[tempI].target.relatedThing == null)
                {
                    node[tempI].target.relatedThing = new List<GetThingGraph_data_items_relatedThing>();
                }
                return node[tempI].target.relatedThing;
            }
            else if (i == node.Count - 1)
            {
                if (node[tempI].target.relatedThing != null)
                {
                    return FindTargetIDData(ref node[tempI].target.relatedThing, targetID);
                }
                else
                {
                    return null;
                }
            }
        }
        return null;
    }

    #endregion

    #region 场景中删除指定实体

    /// <summary>
    /// 删除指定实体
    /// </summary>
    public void DelEntityToTargetPlace(JsonDelEntity jsonDelEntity)
    {
        for (int i = 0; i < jsonDelEntity.entityInfo?.Length; i++)
        {
            bool result = false;
            string targetID = jsonDelEntity.entityInfo[i].id;
            string targetType = jsonDelEntity.entityInfo[i].type;
            string key = targetType + "_" + targetID;
            bool delChind = jsonDelEntity.entityInfo[i].delChind == 1;
            if (MainData.CacheItemsEntity.ContainsKey(key))
            {
                //删除实体对应的缓存数据节点
                if (DelTargetNode(targetID, delChind))
                {
                    GameObject entity = MainData.CacheItemsEntity[key];
                    //更新数据1
                    if (delChind)
                    {
                        MainData.CacheItemsEntity.Remove(key);
                    }
                    else
                    {
                        GameObject PutArea = entity.transform.Find("PutArea")?.gameObject;
                        for (int j = 0; j < PutArea?.transform.childCount; j++)
                        {
                            Transform tempNode = PutArea?.transform.GetChild(j);
                            while (tempNode.childCount > 0)
                            {
                                Transform chindEntity = tempNode.GetChild(0);
                                chindEntity.parent = entity.transform.parent;
                            }
                        }
                        MainData.CacheItemsEntity.Remove(key);
                    }
                    //销毁实体
                    if (entity != null)
                    {
                        Destroy(entity);
                        result = true;

                        //更新全局实体数据
                        UpdateEnityInfoTool.GetInstance.UpdateSceneEntityInfo();
                        DataSave.GetInstance.SaveGetThingGraph_data_items(MainData.CacheSceneItemsInfo);
                    }
                }
                else
                {
                    Debug.LogError("数据更新 失败");
                }
            }
            else
            {
                Debug.LogError("删除指定实体失败 key：" + key);
            }

            //web端删除物体后回调
            JsonWebGlobalEntityData jsonWebGlobalEntityData = new JsonWebGlobalEntityData()
            {
                result = result ? 1 : 0,
                entityType = targetType,
                entityID = targetID,
                postThingGraph = MainData.CacheSceneItemsInfo
            };
            InterfaceDataCenter.GetInstance.SendMQTTUpdateEntity(jsonWebGlobalEntityData);


            string resStr = result ? "成功" : "失败";
            UIManager.GetInstance.GetUIFormLogicScript<UIFormHintNotBtn>().Show(new UIFormHintNotBtn.ShowParams
            {
                txtHintContent = $"删除物体{resStr},{targetType}_{targetID}",
                delayCloseUIFormTime = 2
            });
        }
    }

    /// <summary>
    /// 删除缓存数据中的目标节点
    /// </summary>
    /// <param name="targetNodeID"></param>
    /// <param name="delChind"></param>
    /// <returns></returns>
    private bool DelTargetNode(string targetNodeID, bool delChind)
    {
        List<GetThingGraph_data_items_relatedThing> parentNode = null;
        for (int i = 0; i < MainData.CacheSceneItemsInfo?.items?.Count; i++)
        {
            List<GetThingGraph_data_items_relatedThing> curRelatedThing = MainData.CacheSceneItemsInfo.items[i].relatedThing;

            parentNode = RecurveFind(ref curRelatedThing, targetNodeID);
            if (parentNode != null)
            {
                Debug.Log("isFind!");
                //foreach (var item in parentNode)
                //{
                //    Debug.Log("foreach ,name:" + item.target.name + ",id:" + item.target.id);
                //}
                break;
            }
        }
        if (parentNode != null)
        {
            GetThingGraph_data_items_relatedThing targetNode = parentNode.Find((p) => { return p.target.id == targetNodeID; });
            //直接删除目标节点，以及目标节点的子节点
            if (delChind)
            {
                parentNode.Remove(targetNode);
            }
            else
            {
                //只删除目标节点，不删除目标节点的子节点
                //若不想删除目标阶段的子节点，则需要把目标节点子节点移植到目标节点父节点,再删除目标节点
                for (int i = 0; i < targetNode.target.relatedThing?.Count; i++)
                {
                    parentNode.Add(targetNode.target.relatedThing[i]);
                }
                parentNode.Remove(targetNode);
            }
        }
        return parentNode != null;
    }

    /// <summary>
    /// 递归查找目标节点  返回目标节点父节点 List<GetThingGraph_data_items_relatedThing，目标节点则在前者对象集合中
    /// </summary>
    /// <param name="curRelatedThing"></param>
    /// <param name="targetNodeID"></param>
    /// <returns></returns>
    private List<GetThingGraph_data_items_relatedThing> RecurveFind(ref List<GetThingGraph_data_items_relatedThing> curRelatedThing, string targetNodeID)
    {
        for (int i = 0; i < curRelatedThing.Count; i++)
        {
            int tempI = i;
            if (curRelatedThing[tempI].target.id == targetNodeID)
            {
                return curRelatedThing;
            }
            else
            {
                if (curRelatedThing[tempI].target.relatedThing?.Count > 0)
                {
                    return RecurveFind(ref curRelatedThing[tempI].target.relatedThing, targetNodeID);
                }
            }
        }
        return null;
    }
    #endregion

}
