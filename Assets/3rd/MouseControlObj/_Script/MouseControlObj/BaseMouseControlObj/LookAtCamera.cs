using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 /// <summary>
 /// 始终朝向相机的物体
 /// </summary>
public class LookAtCamera : MonoBehaviour
{
    /// <summary>
    /// 旋转轴
    /// </summary>
    public enum RotAxis
    {
        Normal,
        X,
        Y,
        Z
    }
    [SerializeField]
    private RotAxis rotAxis;

    // Start is called before the first frame update
    void Start()
    {
        rotAxis = RotAxis.Y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);//始终看向相机 
        switch (rotAxis)
        {
            case RotAxis.Normal:
                break;
            case RotAxis.X:
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, 0);
                break;
            case RotAxis.Y:
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                break;
            case RotAxis.Z:
                transform.localEulerAngles = new Vector3(0,0, transform.localEulerAngles.z);
                break;
            default:
                break;
        }
        
    }
}
