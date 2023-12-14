@echo off
echo wscript.sleep 1000 > sleep.vbs
start /wait sleep.vbs
start "" D:/UnityProject/JHGit/ZJLab_DigtalTwin/Assets/../_Build\_Win\Win_Emulation20231212\Simulation_2023_12_12_15_39_49.exe
del /f /s /q sleep.vbs
exit
