using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MouseControlObj
{
    /// <summary>
    /// 坐标轴,具体哪个轴
    /// </summary>
    public class Axis : MonoBehaviour
    {

        [SerializeField]
        /// <summary>
        /// 初始材质
        /// </summary>
        private Material[] originalMats;
        [SerializeField]
        /// <summary>
        /// 鼠标进入的时候的材质
        /// </summary>
        private Material[] mouseInMats;
        [SerializeField]
        /// <summary>
        /// 鼠标拖拽的时候的材质
        /// </summary>
        private Material[] mouseDragMats;
        [SerializeField]
        /// <summary>
        /// 未激活的时候的材质，即在拖拽其他轴的时候调用
        /// </summary>
        private Material[] unActiveMats;
        /// <summary>
        /// 当前是哪个轴
        /// </summary>
        [SerializeField]
        private ControlAxis axis;

        private MeshRenderer _meshRenderer;

        private MeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }
                return _meshRenderer;
            }
        }

        private MouseControlObjAxis controlObjAxis;
        /// <summary>
        /// 坐标系
        /// </summary>
        private MouseControlObjAxis ControlObjAxis
        {
            get
            {
                if (controlObjAxis == null)
                {
                    controlObjAxis = GetComponentInParent<MouseControlObjAxis>();
                }
                return controlObjAxis;
            }
        }

        private MouseControlObjScaleAxis controlObjScaleAxis;
        /// <summary>
        /// 缩放坐标系
        /// </summary>
        private MouseControlObjScaleAxis ControlObjScaleAxis
        {
            get
            {
                if (controlObjScaleAxis == null)
                {
                    controlObjScaleAxis = GetComponentInParent<MouseControlObjScaleAxis>();
                }
                return controlObjScaleAxis;
            }
        }
        private MouseControlObjRotAxis controlObjRotAxis;
        /// <summary>
        /// 旋转坐标系
        /// </summary>
        private MouseControlObjRotAxis ControlObjRotAxis
        {
            get
            {
                if (controlObjRotAxis == null)
                {
                    controlObjRotAxis = GetComponentInParent<MouseControlObjRotAxis>();
                }
                return controlObjRotAxis;
            }
        }


        
        public ControlAxis Get_axis()
        {
            return axis;
        }

        /// <summary>
        /// 设置为初始材质
        /// </summary>
        public void SetTo_originalMats()
        {
            if (meshRenderer.materials.Length == 1)
            {
                meshRenderer.material = originalMats[0];
            }
            else
            {
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    int index = i;
                    meshRenderer.materials[index] = originalMats[index];
                }
            }

        }
        /// <summary>
        /// 设置为鼠标进入材质
        /// </summary>
        public void SetTo_mouseInMats()
        {
            if (meshRenderer.materials.Length == 1)
            {
                meshRenderer.material = mouseInMats[0];
            }
            else
            {
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    int index = i;
                    meshRenderer.materials[index] = mouseInMats[index];
                }
            }
        }
        /// <summary>
        /// 设置为鼠标拖拽材质
        /// </summary>
        public void SetTo_mouseDragMats()
        {
            if (meshRenderer.materials.Length == 1)
            {
                meshRenderer.material = mouseDragMats[0];
            }
            else
            {
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    int index = i;
                    meshRenderer.materials[index] = mouseDragMats[index];
                }
            }
        }
        /// <summary>
        /// 设置为未激活材质
        /// </summary>
        public void SetTo_unActiveMats()
        {
            if (meshRenderer.materials.Length == 1)
            {
                meshRenderer.material = unActiveMats[0];
            }
            else
            {
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    int index = i;
                    meshRenderer.materials[index] = unActiveMats[index];
                }
            }
        }

    }
}
