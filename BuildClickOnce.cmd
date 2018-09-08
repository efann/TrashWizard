@echo on
set lcMSBuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

rmdir bin /S /Q
%lcMSBuild% /target:build
%lcMSBuild% TrashWizard.csproj @TrashWizard.Response.txt

