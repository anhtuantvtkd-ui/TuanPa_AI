@echo off
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "ACAD=C:\Program Files\Autodesk\AutoCAD 2021"
set "DLL=D:\TuanPa_AI\products\meps\release\TVXD.BG_V4\MEPS060\TVXD.Meps.dll"
"%CSC%" /target:library /out:"D:\TuanPa_AI\products\meps\test\TestMTextWriter.dll" /r:"%ACAD%\acdbmgd.dll" /r:"%ACAD%\acmgd.dll" /r:"%ACAD%\accoremgd.dll" /r:"%DLL%" "D:\TuanPa_AI\products\meps\test\TestMTextWriter.cs"
