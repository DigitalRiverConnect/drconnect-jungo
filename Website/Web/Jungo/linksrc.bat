if not exist N2\NUL goto LINKIT
if exist N2\Login.aspx goto LINKED
echo removing empty N2 folder first
rmdir N2
:LINKIT
mklink /J N2 ..\..\..\N2cms\src\Mvc\MvcTemplates\N2
echo linkit
goto DONE
:LINKED
echo already linked
:DONE
