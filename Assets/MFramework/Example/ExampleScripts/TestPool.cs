using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFramework
{
    /// <summary>
    /// 标题：测试对象池
    /// 功能：
    /// 作者：毛俊峰
    /// 时间：2022.
    /// 版本：1.0
    /// </summary>
    public class TestPool : MonoBehaviour
    {
        class TestClass { }
        public GameObject cube;
        public Transform cubeParent;


        private Pool<TestClass> m_PoolClass;
        private Pool<GameObject> m_PoolObj;
        private void Start()
        {
            InitPool();
            Test1();
            Test2();
        }

        private void InitPool()
        {
            m_PoolClass = new Pool<TestClass>(() => { return new TestClass(); }, null, null, 100);

            m_PoolObj = new Pool<GameObject>(() =>
            {
                //回调 获取对象-创建新对象后回调
                GameObject newCube = Instantiate<GameObject>(cube);
                newCube.transform.SetParent(cubeParent);
                newCube.gameObject.SetActive(false);
                return newCube;
            }, (GameObject cube) =>
            {
                //获取对象后回调
                //cube.gameObject.SetActive(true);
            }, (GameObject cube) =>
            {
                //回收对象时回调
                //cube.gameObject.SetActive(false);
                Debug.Log("回收对象：" + cube.name);
            }, 10);
        }


        private void Test1()
        {
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolClass.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolClass.GetCurUningObjCount);

            TestClass obj = m_PoolClass.Allocate();
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolClass.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolClass.GetCurUningObjCount);

            m_PoolClass.Recycle(obj);
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolClass.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolClass.GetCurUningObjCount);

            for (int i = 0; i < 10; i++)
            {
                m_PoolClass.Allocate();
            }
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolClass.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolClass.GetCurUningObjCount);
        }

        private void Test2()
        {
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolObj.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolObj.GetCurUningObjCount);

            //分配一个对象
            GameObject obj = m_PoolObj.Allocate();
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolObj.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolObj.GetCurUningObjCount);

            //回收一个指定对象
            m_PoolObj.Recycle(obj);
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolObj.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolObj.GetCurUningObjCount);

            //分配七个对象
            for (int i = 0; i < 7; i++)
            {
                GameObject tempObj = m_PoolObj.Allocate();
                tempObj.name = i.ToString();
            }
            Debug.Log("当前对象池 未使用的对象个数：" + m_PoolObj.GetCurUnuserObjCount + " 已使用对象个数：" + m_PoolObj.GetCurUningObjCount);

            //获取所有正在使用的对象
            m_PoolObj.GetUsingObjs[2].name = "222";

            //根据对象名 获取正在使用的对象
            GameObject findObj = m_PoolObj.GetUsingObjsByObjName("3");
            Debug.Log(findObj);


            //回收所有正在使用的对象
            m_PoolObj.RecycleAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                m_PoolObj.Allocate();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (m_PoolObj.GetUsingObjs.Count > 0)
                {
                    m_PoolObj.Recycle(m_PoolObj.GetUsingObjs[0]);
                }
            }
        }
    }
}