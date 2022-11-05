@echo off
setlocal

set command="Plexdata.Graylog.Simulator.exe --all --version --trace --debug"

cmd /K %command% && exit
