@echo off

if [%1]==[] goto usage
msbuild /t:clean,build /p:configuration=%1 /p:targetProfile=%1 /p:VisualStudioVersion=11.0 CloudLink.Commerce.sln
goto :eof
:usage
@echo Usage: %0 ^<Build Configuration^>
exit /B 1

