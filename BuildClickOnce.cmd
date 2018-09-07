@echo on
set lcYear=%date:~10,4%
set lcMSBuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

rmdir bin /S /Q
%lcMSBuild% /target:build
%lcMSBuild% /target:publish /property:ApplicationVersion=%lcYear%.0.0.34

