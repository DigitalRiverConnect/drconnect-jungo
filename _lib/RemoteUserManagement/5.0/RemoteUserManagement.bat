DEL /F /Q *.dll
DEL /F /Q *.pdb
DEL /F /Q Common_RemoteUserManagement.cs
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\xsd.exe" Common.xsd RemoteUserManagement.xsd /c /l:CS /f /n:GlobalCommerce.IntegrationServices.RemoteUserManagement
C:\Windows\Microsoft.NET\Framework\v4.0.30319\Csc.exe /noconfig /nowarn:1701,1702 /errorreport:prompt /warn:4 /define:DEBUG;TRACE /reference:C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll /reference:C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Xml.dll /debug+ /debug:full /filealign:512 /optimize- /out:RemoteUserManagement.dll /target:library AssemblyInfo.cs Common_RemoteUserManagement.cs
REM "C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\sgen.exe" /reference:C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Xml.dll /assembly:RemoteUserManagement.dll /force
XCOPY RemoteUserManagement*.dll ..\.. /Y /C /R
