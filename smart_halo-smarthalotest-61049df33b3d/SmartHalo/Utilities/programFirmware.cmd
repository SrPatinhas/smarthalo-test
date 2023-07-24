
@Echo off

nrfjprog --family nRF52 -e
nrfjprog --family nRF52 --program pre_release_v03.hex
nrfjprog --family nRF52 -r

IF %ERRORLEVEL% GTR 0 Echo An error has occured, SmartHalo wasn't programmed successfully
IF %ERRORLEVEL% EQU 0 Echo SmartHalo was programmed successfully

pause