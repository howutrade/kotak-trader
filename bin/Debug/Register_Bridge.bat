@echo off

set DLLPATH=%~dp0
set OSBIT=32
SET APPNAME=KotakNet.Core
SET APPNAME1=KotakNet.Bridge
SET APPNAME2=KotakNet.Rtd

if exist "C:\Windows\SysWOW64" (
  set OSBIT=64
)

echo Copying dependency DLLs...

if %OSBIT% == 64 (
  xcopy /h /y /r "%DLLPATH%WebSocket4Net.dll" "C:\Windows\SysWOW64"
  xcopy /h /y /r "%DLLPATH%SuperSocket.ClientEngine.dll" "C:\Windows\SysWOW64"
  xcopy /h /y /r "%DLLPATH%Newtonsoft.Json.dll" "C:\Windows\SysWOW64"
)

xcopy /h /y /r "%DLLPATH%WebSocket4Net.dll" "C:\Windows\System32"
xcopy /h /y /r "%DLLPATH%SuperSocket.ClientEngine.dll" "C:\Windows\System32"
xcopy /h /y /r "%DLLPATH%Newtonsoft.Json.dll" "C:\Windows\System32"

echo Unregistering existing DLLs if any...

if %OSBIT% == 64 (
  cd /d %WINDIR%\Microsoft.NET\Framework64\v4*
  regasm "C:\Windows\SysWOW64\%APPNAME%.dll" /tlb /codebase /u
  regasm "C:\Windows\SysWOW64\%APPNAME1%.dll" /tlb /codebase /u
  regasm "C:\Windows\SysWOW64\%APPNAME2%.dll" /tlb /codebase /u
)

cd /d %WINDIR%\Microsoft.NET\Framework\v4*
regasm "C:\Windows\System32\%APPNAME%.dll" /tlb /codebase /u
regasm "C:\Windows\System32\%APPNAME1%.dll" /tlb /codebase /u
regasm "C:\Windows\System32\%APPNAME2%.dll" /tlb /codebase /u

echo Deleting existing DLLs if any...

if %OSBIT% == 64 (
  del /q /f /a C:\Windows\SysWOW64\%APPNAME%.dll 
  del /q /f /a C:\Windows\SysWOW64\%APPNAME%.tlb
  del /q /f /a C:\Windows\SysWOW64\%APPNAME1%.dll 
  del /q /f /a C:\Windows\SysWOW64\%APPNAME1%.tlb
  del /q /f /a C:\Windows\SysWOW64\%APPNAME2%.dll 
  del /q /f /a C:\Windows\SysWOW64\%APPNAME2%.tlb
)

del /q /f /a C:\Windows\System32\%APPNAME%.dll 
del /q /f /a C:\Windows\System32\%APPNAME%.tlb
del /q /f /a C:\Windows\System32\%APPNAME1%.dll 
del /q /f /a C:\Windows\System32\%APPNAME1%.tlb
del /q /f /a C:\Windows\System32\%APPNAME2%.dll 
del /q /f /a C:\Windows\System32\%APPNAME2%.tlb

echo Copying latest DLLs...

if %OSBIT% == 64 (
  xcopy /h /y /r "%DLLPATH%%APPNAME%.dll" "C:\Windows\SysWOW64"
  xcopy /h /y /r "%DLLPATH%%APPNAME1%.dll" "C:\Windows\SysWOW64"
  xcopy /h /y /r "%DLLPATH%%APPNAME2%.dll" "C:\Windows\SysWOW64"
)

xcopy /h /y /r "%DLLPATH%%APPNAME%.dll" "C:\Windows\System32"
xcopy /h /y /r "%DLLPATH%%APPNAME1%.dll" "C:\Windows\System32"
xcopy /h /y /r "%DLLPATH%%APPNAME2%.dll" "C:\Windows\System32"

echo Registering latest DLLs...

if %OSBIT% == 64 (
  cd /d %WINDIR%\Microsoft.NET\Framework64\v4*
  regasm "C:\Windows\SysWOW64\%APPNAME%.dll" /tlb /codebase
  regasm "C:\Windows\SysWOW64\%APPNAME1%.dll" /tlb /codebase
  regasm "C:\Windows\SysWOW64\%APPNAME2%.dll" /tlb /codebase
)

cd /d %WINDIR%\Microsoft.NET\Framework\v4*
regasm "C:\Windows\System32\%APPNAME%.dll" /tlb /codebase 
regasm "C:\Windows\System32\%APPNAME1%.dll" /tlb /codebase 
regasm "C:\Windows\System32\%APPNAME2%.dll" /tlb /codebase 

pause