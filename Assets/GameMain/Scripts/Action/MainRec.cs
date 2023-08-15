using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;



/// <summary>
/// 标题：mqtt收到的指令中转接收处理
/// 功能：
/// 作者：弓利剑
/// 时间：20230731
/// /// /// </summary>

public class MainRec : MonoBehaviour
{
    public string current_command;
    public string last_command;
    public Task component_task;

    public Walk walkscript;


    // Start is called before the first frame update
    void Start()
    {
        current_command = "";
        last_command = current_command;
    }

    // Update is called once per frame
    void Update()
    {
        if (current_command != last_command)
        {
            last_command = current_command;
            action_execute(current_command);
        }
        //条件判断任务是否执行成功，成功则销毁组件脚本
        // else if (current_command == last_command)
        // {
        //     Component[] components = GetComponents<Component>();
        //     // if(components[3].Status == TaskStatus.Success)
        //     // {
        //     //     Destroy(component[3]);
        //     // }
        // }
    }
    //命令执行时，先将对应脚本添加到物体上，然后执行，执行完毕后销毁
    void action_execute(string command)
    {
        string[] commandParts = command.Split(' ');
        if (commandParts.Length >= 3)
        {
            string scriptName = commandParts[1]; // 获取脚本名称
            AddScript(scriptName);
        }
        else
        {
            Debug.LogWarning("Invalid command format: " + command);
        }
    }

    void AddScript(string scriptName)
    {
        walkscript =gameObject.AddComponent<Walk>();
        walkscript.command= current_command;
        // // 动态加载并添加脚本组件
        // System.Type scriptType = System.Type.GetType(scriptName);
        // if (scriptType != null)
        // {
        //     if (!gameObject.TryGetComponent(scriptType, out Component existingScript))
        //     {
        //         Component newScript = gameObject.AddComponent(scriptType);
        //         Component[] components = GetComponents<Component>();
        //         //components[3].command= current_command;报错
        //         Debug.Log(scriptName + " script added to GameObject.");
        //     }
        //     else
        //     {
        //         Debug.LogWarning(scriptName + " script already exists on GameObject.");
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Script type not found: " + scriptName);
        // }
    }
    
}
