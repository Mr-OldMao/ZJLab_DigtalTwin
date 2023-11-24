
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisScaleForDistance : MonoBehaviour
{
    [SerializeField]
    /// <summary>
    /// 原始缩放，最大缩放
    /// </summary>
    private Vector3 originalScale;
    [SerializeField]
    /// <summary>
    /// 原始缩放的时候对距离为多少
    /// </summary>
    private float originalScaleDis;

    [SerializeField]
    /// <summary>
    /// 目标物体
    /// </summary>
    private Transform targetObj;
    [SerializeField]
    /// <summary>
    /// 最大缩放值
    /// </summary>
    private float maxScale = -1;
    // Start is called before the first frame update
    void Start()
    {
        //targetObj = GetComponent<LookAtCamera>().Get_targetCam();
        if (targetObj == null)
        {
            targetObj = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetObj == null)
        {
            transform.localScale=originalScale;
            //targetObj = GetComponent<BaseWorldUIIcon>().targetCam.transform;
            return;
        }
        float disFromTargetObj = Vector3.Distance(transform.position, targetObj.position);
        float coefficient = disFromTargetObj / originalScaleDis;
        transform.localScale = new Vector3(originalScale.x * coefficient, originalScale.y * coefficient, originalScale.z * coefficient);
        if (maxScale>0&&transform.localScale.x> maxScale)
        {
            transform.localScale = new Vector3(originalScale.x * maxScale, originalScale.y * maxScale, originalScale.z * maxScale);
        }
       
    }

    //private IEnumerator IHideTime
}
