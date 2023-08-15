using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : Task
{
        //指令解析
    public string command;
    public string Role;
    public string Action;
    public string TargetObject;

    public float disfromtarget ;
    public Vector3 destination;
    // The agent component 
    
    public UnityEngine.AI.NavMeshAgent m_Agent;
    public Animator m_Animator;
    
    public bool m_IsWalking = false;//行走状态
    public float threshold = 0.1f;//不同物体的距离阈值

    // Start is called before the first frame update
    void Start()
    {
        //获取动画机和导航代理组件
        m_IsWalking = false;
        m_Animator = gameObject.GetComponent<Animator>();
        m_Agent= gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();

        //指令解析

        string[] parts = command.Split(' ');
        Role = parts[0];
        Action = parts[1];
        TargetObject = parts[2];
        GameObject goal = GameObject.Find("bin011(1)");
        if (goal == null)
        {
            Debug.Log("The script didn't find the goal object");
        }

        Debug.Log("导航目标"+goal);

        //不是导航到目标点位，要计算目标有一定距离的位置
        Vector3 goal_position=goal.transform.position;
        float distance = Vector3.Distance(this.transform.position, goal_position);
        Vector3 distance_vector = goal_position-this.transform.position;
        distance_vector = (distance-1.5f)/distance * distance_vector;
        Vector3 final_position=this.transform.position + distance_vector;



        m_Agent.destination = final_position;
        Debug.Log("导航目标位置"+final_position);
        m_Agent.isStopped = false;
        m_Agent.destination = goal_position;//goal.transform.position; 
        Debug.Log("终点"+m_Agent.destination);
        //m_Animator.SetBool("isWalking",true);
    }

    // 持续判断是否到达目标点位，如果到达
    void Update()
    {
         if(Action == "Walk")
        {
            if(m_IsWalking == true)
            {
                //Debug.Log("remain_distance"+m_Agent.remainingDistance);
               
                disfromtarget = m_Agent.remainingDistance;
                if(m_Agent.remainingDistance <= threshold)
                {
                    // 停止代理和动作
                    m_Agent.isStopped = true;
                    // 动作处理（例如：停止走路、触发动画）
                    m_IsWalking = false;
                    m_Animator.SetBool("isWalking",m_IsWalking);//恢复待机动画
                    Debug.Log("Walk task has succeed");
                    Status = TaskStatus.Success;
                }
     

            }
        
            m_IsWalking = true;//代表开始导航，进入移动状态
            m_Animator.SetBool("isWalking",m_IsWalking);//若处于移动状态，则播放行走动画
            Debug.Log("Walk task has been executed!");
            Status = TaskStatus.Running;


        }
        else
        {
            Debug.LogWarning("The command is not walk");
            Status = TaskStatus.Failure;
        }
  
    }
}




