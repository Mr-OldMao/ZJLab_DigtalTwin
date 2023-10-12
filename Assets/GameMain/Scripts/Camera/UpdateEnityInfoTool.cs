using System.Collections.Generic;
using UnityEngine;
using static GetThingGraph;
using MFramework;
/// <summary>
/// 标题：更新室内实体信息具体实现工具类
/// 功能：
/// 作者：毛俊峰
/// 时间：2023.09.21
/// </summary>
public class UpdateEnityInfoTool : SingletonByMono<UpdateEnityInfoTool>
{
    private Camera cameraListener;

    #region 更新全场景实体数据信息
    /// <summary>
    /// 对外接口 更新整个场景的实体信息数据（除天花板，地砖，墙壁以外）
    /// </summary>
    public void UpdateSceneEntityInfo()
    {
        for (int i = 0; i < MainData.CacheSceneItemsInfo.items.Count; i++)
        {
            UpdateRelatedThing(ref MainData.CacheSceneItemsInfo.items[i].relatedThing);
        }
    }

    /// <summary>
    /// 具体实现 更新整个场景的实体信息数据 
    /// </summary>
    /// <param name="relatedThingArr"></param>
    private void UpdateRelatedThing(ref List<GetThingGraph_data_items_relatedThing> relatedThingArr)
    {
        for (int j = 0; j < relatedThingArr.Count; j++)
        {
            int tempJ = j;
            //key 除了“门”模型外都是 name_id 格式
            string key = string.Empty;
            if (relatedThingArr[tempJ].target.name.Contains("Door"))
            {
                key = relatedThingArr[tempJ].target.id;
            }
            else
            {
                key = relatedThingArr[tempJ].target.name + "_" + relatedThingArr[tempJ].target.id;
            }
            GameObject targetObj = GetItemEntity(key);
            if (targetObj != null)
            {
                //更新实体位置信息
                Transform modelTrans = targetObj.transform.Find("Model");
                if (modelTrans != null)
                {
                    relatedThingArr[tempJ].target.position = new float[] { modelTrans.position.x, modelTrans.position.y, modelTrans.position.z };
                    relatedThingArr[tempJ].target.rotation = new float[] { modelTrans.rotation.eulerAngles.x, modelTrans.rotation.eulerAngles.y, modelTrans.rotation.eulerAngles.z };
                }
                else
                {
                    Debug.LogError("not Model node , key:" + key);
                }
            }

            //寻找是否存在子对象
            if (relatedThingArr[tempJ].target.relatedThing?.Count > 0)
            {
                UpdateRelatedThing(ref relatedThingArr[tempJ].target.relatedThing);
            }
        }
    }
    #endregion


    #region 更新相机视野范围内的实体信息

    /// <summary>
    /// 更新相机视野范围内的实体信息
    /// </summary>
    public void UpdateCameraFOVEntityInfo()
    {
        if (MainData.CacheCameraItemsInfo == null)
        {
            MainData.CacheCameraItemsInfo = new PostThingGraph { id = MainData.ID };
        }
        if (MainData.CacheCameraItemsInfo.items == null)
        {
            MainData.CacheCameraItemsInfo.items = new List<GetThingGraph_data_items>();
        }
        MainData.CacheCameraItemsInfo.items.Clear();
        for (int i = 0; i < MainData.CacheSceneItemsInfo.items.Count; i++)
        {
            int tempI = i;
            GetThingGraph_data_items temp = new GetThingGraph_data_items
            {
                id = MainData.CacheSceneItemsInfo.items[tempI].id,
                name = MainData.CacheSceneItemsInfo.items[tempI].name,
                relatedThing = new List<GetThingGraph_data_items_relatedThing>(),
                dynamic = MainData.CacheSceneItemsInfo.items[tempI].dynamic,
                position = MainData.CacheSceneItemsInfo.items[tempI].position,
                rotation = MainData.CacheSceneItemsInfo.items[tempI].rotation
            };
            MainData.CacheCameraItemsInfo.items.Add(temp);
            for (int j = 0; j < MainData.CacheSceneItemsInfo.items[tempI].relatedThing.Count; j++)
            {
                int tempJ = j;
                GetThingGraph_data_items_relatedThing relatedThing = MainData.CacheSceneItemsInfo.items[tempI].relatedThing[tempJ];
                if (JudgeEntityExistCameraFOV(relatedThing))
                {
                    MainData.CacheCameraItemsInfo.items[tempI].relatedThing.Add(relatedThing);
                    Debug.Log($"更新相机视野范围内的实体信息 itemName:{relatedThing.target.name}" +
                    $",id:{relatedThing.target.id}" +
                    $",pos:{relatedThing.target.position}");
                }
                else
                {
                    //Debug.Log($"更新相机视野范围内的实体信息 ,不在视野范围内 itemName:{relatedThing.target.name}");
                }
            }
        }
    }



    /// <summary>
    /// 判定实体是否存在与相机视野范围内
    /// </summary>
    /// <param name="getThingGraph_Data_Items"></param>
    /// <param name="callback"></param>
    private bool JudgeEntityExistCameraFOV(GetThingGraph_data_items_relatedThing getThingGraph_Data_Items)
    {
        bool isExist = false;
        //key 除了“门”模型外都是 name_id 格式
        string key = string.Empty;
        if (getThingGraph_Data_Items.target.name.Contains("Door"))
        {
            key = getThingGraph_Data_Items.target.id;
        }
        else
        {
            key = getThingGraph_Data_Items.target.name + "_" + getThingGraph_Data_Items.target.id;
        }
        if (!MainData.CacheItemsEntity.ContainsKey(key))
        {
            Debug.LogError("not exist key : " + key);
            return isExist;
        }
        GameObject targetEntity = MainData.CacheItemsEntity[key];
        isExist = IsCameraVisible(targetEntity);
        return isExist;
    }

    /// <summary>
    /// 判断物体是否在相机视野范围内
    /// </summary>
    /// <param name="obj">物体</param>
    /// <returns>是否在相机范围内</returns>
    private bool IsCameraVisible(GameObject obj, float radius = 5)
    {
        return JudgeItemFrontCamera(obj) && JudgeIncludeDis(obj, radius) && JudgeRayObstacle(obj);
    }

    /// <summary>
    /// 判断物体是否在相机前方   T-在相机前方
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool JudgeItemFrontCamera(GameObject obj)
    {
        if (cameraListener == null)
        {
            Camera[] cameraArr = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameraArr)
            {
                if (camera.gameObject.name == "CameraFirstListener")
                {
                    cameraListener = camera;
                }
            }
        }
        bool res = false;
        Vector3 objPos = obj.transform.position;
        Vector2 viewPos = cameraListener.WorldToViewportPoint(objPos);
        Vector3 dir = (objPos - cameraListener.transform.position).normalized;
        float dot = Vector3.Dot(cameraListener.transform.forward, dir);
        //判断物体是否在相机前面
        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
        {
            res = true;
        }
        else
        {
            res = false;
        }

        ////TEST
        //if (res == false)
        //{
        //    Debug.LogError("物体未相机前方 " + obj);
        //}
        return res;
    }

    /// <summary>
    /// 判定物体到摄像机的距离指定范围之内，T-在范围之内
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="dis"></param>
    private bool JudgeIncludeDis(GameObject obj, float dis)
    {
        Vector3 pos1 = cameraListener.transform.position;
        Vector3 pos2 = obj.transform.position;

        bool res = Vector3.Distance(pos1, pos2) <= dis;
        ////TEST
        //if (res == false)
        //{
        //    Debug.LogError("物体到摄像机的距离未在指定范围之内 " + obj);
        //}
        return res;
    }

    /// <summary>
    /// 判定摄像机到目标物体之间是否有障碍阻挡   无障碍-T 有障碍-F 
    /// </summary>
    /// <returns></returns>
    public bool JudgeRayObstacle(GameObject obj)
    {
        bool res = false;
        if (cameraListener == null)
        {
            Camera[] cameraArr = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameraArr)
            {
                if (camera.gameObject.name == "CameraFirstListener")
                {
                    cameraListener = camera;
                }
            }
        }
        Ray ray = new Ray(cameraListener.transform.position, obj.transform.position - cameraListener.transform.position);
        RaycastHit raycastHit;
        LayerMask layerMask = ~((1 << 6) | (1 << 7));//打开除了"Player","IndoorItem"之外的层;
        if (Physics.Raycast(ray, out raycastHit, 20, layerMask))
        {
            //Debug.LogError("obj " + obj + " ray ，coll" + raycastHit.collider.name + ",parent:" + raycastHit.collider.transform.parent);
            if (raycastHit.collider.tag == "Wall" || raycastHit.collider.tag == "Door")
            {
                //Debug.LogError("obj " + obj + "被墙体遮挡，coll" + raycastHit.collider.name);
                //Debug.DrawLine(cameraListener.transform.position, raycastHit.collider.gameObject.transform.position);
                res = false;
            }
            else
            {
                res = true;
            }
        }
        ////TEST
        //if (res == false)
        //{
        //    Debug.LogError("摄像机到目标物体之间有障碍阻挡 " + obj);
        //}
        return res;
    }
    #endregion

    private GameObject GetItemEntity(string key)
    {
        GameObject res = null;
        if (!MainData.CacheItemsEntity.ContainsKey(key))
        {
            GameObject entity = GameObject.Find(key);
            if (entity != null)
            {
                MainData.CacheItemsEntity.Add(key, entity);
            }
            else
            {
                Debug.LogError("not exist key : " + key);
            }
        }
        else
        {
            res = MainData.CacheItemsEntity[key];
        }
        return res;
    }

}
