@echo off
setlocal

set configuration=%1
set pub-win-x86="..\..\pub\Plexdata.Graylog.Simulator-win-x86"
set pub-win-x64="..\..\pub\Plexdata.Graylog.Simulator-win-x64"
set cmd-sources="..\..\src\Plexdata.Graylog.Simulator\Scripts\*.cmd"

if "%configuration%" == "Release" (

    if not exist %pub-win-x86% ( 
        echo Create publish folder ^%pub-win-x86%
        mkdir %pub-win-x86% 
    )

    echo Copy batch files into ^%pub-win-x86%
    copy /v /y %cmd-sources% %pub-win-x86% > nul

    if not exist %pub-win-x64% (
        echo Create publish folder ^%pub-win-x64%
        mkdir %pub-win-x64% 
    )

    echo Copy batch files into ^%pub-win-x64%
    copy /v /y %cmd-sources% %pub-win-x64% > nul

)