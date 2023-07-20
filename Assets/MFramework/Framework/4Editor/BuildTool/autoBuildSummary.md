# 自动化打包总结

**Jenkins自动化Unity打包，实质就是Jenkins调用了Windows批处理(cmd、bat)命令执行代码的静态方法。**



## **保姆教程** 

https://blog.csdn.net/linxinfa/article/details/118816132



## .bat打包核心命令

```
@echo off
:: 此处执行Unity打包
echo 正在打包中请稍后...
"D:\UnityEngine\2021.3.20f1c1\Editor\Unity.exe" ^
-quit ^
-batchmode ^
-projectPath "D:\LearnPath\AutoBuildByJenkins\LearnJenkins" ^
-executeMethod TestEditor.BuildApk1 ^
-logFile "D:\AutoBuildlog.txt" ^
echo 打包完成
exit

@echo off
:: 此处执行Unity打包
echo 正在打包中请稍后...
"[Unity编辑器全路径]" ^
-quit ^
-batchmode ^
-projectPath "[项目路径Assets前一级目录]" ^
-executeMethod 类名.静态方法名^
-logFile "本地路径用来存放打包日志" ^
echo 打包完成
exit
```



## .bat命令1

```
::功能：Unity自动打包基础版 实质 通过命令行调用Unity的静态函数TestEditor.BuildApk
::运行方式1 双击当前.bat文件
::运行方式2 使用cmd，输入当前bat全路径，如 :C:\Users\Mao\Desktop\autoUnityBuild1.bat

@echo off
:: 此处执行Unity打包
echo 正在打包中请稍后...
"D:\UnityEngine\2021.3.20f1c1\Editor\Unity.exe" ^
-quit ^
-batchmode ^
-projectPath "D:\LearnPath\AutoBuildByJenkins\LearnJenkins" ^
-executeMethod BuildTool.BuildByCMD ^
-logFile "D:\AutoBuildlog.txt" ^
echo 打包完成
exit
```



## .bat命令2

```
::功能：Unity自动打包进阶版 自动检测并关闭Unity程序，打包时可传入参数产品名称productName和版本号version
::运行方式1 双击当前.bat文件
::运行方式2 使用cmd，输入当前bat全路径 productName version，如 :C:\Users\Mao\Desktop\autoUnityBuild2.bat 自动打包程序测试 1.0.0 

@echo off
set /p ProductName="请输入产品名(允许中文名) ProductName:"
set /p Version="请输入版本号(格式:x.x.x) Version:"
set /p OutPutPath="请输包体输出位置文件夹路径(格式:C:\Users\Mao\Desktop) OutPutPath:"

::判断Unity是否运行中
echo 正在检测Unity.exe进程是否关闭...
TASKLIST /V /S localhost /U %username%>tmp_process_list.txt
TYPE tmp_process_list.txt |FIND "Unity.exe"

IF ERRORLEVEL 0 (GOTO UNITY_IS_RUNNING)
ELSE (GOTO START_UNITY)

:UNITY_IS_RUNNING
::杀掉Unity
echo 正在关闭Unity.exe进程...
TASKKILL /F /IM Unity.exe
::停1秒
PING 127.0.0.1 -n 1 >NUL
GOTO START_UNITY

:START_UNITY
:: 此处执行Unity打包
echo Unity正在打包中请稍后...
"D:\UnityEngine\2021.3.20f1c1\Editor\Unity.exe" ^
-quit ^
-batchmode ^
-projectPath "D:\LearnPath\AutoBuildByJenkins\LearnJenkins" ^
-executeMethod BuildTool.BuildByCMD ^
-logFile "D:\AutoBuildlog.txt" ^
--productName:%ProductName% ^
--version:%Version% ^
--outPutPath:%OutPutPath%
echo 打包完成
pause
```



## .bat命令2

```
::功能：Unity自动打包进阶版 自动检测并关闭Unity程序，打包时可传入参数产品名称productName和版本号version以及导出的位置
::运行方式 当前bat全路径 productName version，如 C:\Users\Mao\Desktop\autoUnityBuild3.bat 自动打包程序测试 1.0.0  C:\Users\Mao\Desktop

@echo off
::判断Unity是否运行中
echo 正在检测Unity.exe进程是否关闭...
TASKLIST /V /S localhost /U %username%>tmp_process_list.txt
TYPE tmp_process_list.txt |FIND "Unity.exe"

IF ERRORLEVEL 0 (GOTO UNITY_IS_RUNNING)
ELSE (GOTO START_UNITY)

:UNITY_IS_RUNNING
::杀掉Unity
echo 正在关闭Unity.exe进程...
TASKKILL /F /IM Unity.exe
::停1秒
PING 127.0.0.1 -n 1 >NUL
GOTO START_UNITY

:START_UNITY
:: 此处执行Unity打包
echo Unity正在打包中请稍后...
"D:\UnityEngine\2021.3.20f1c1\Editor\Unity.exe" ^
-quit ^
-batchmode ^
-projectPath "D:\LearnPath\AutoBuildByJenkins\LearnJenkins" ^
-executeMethod BuildTool.BuildByCMD ^
-logFile "D:\AutoBuildlog.txt" ^
--productName:%1 ^
--version:%2 ^
--outPutPath:%3
echo 打包完成
pause
```

