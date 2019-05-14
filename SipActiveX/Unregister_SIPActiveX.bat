@echo off
echo --------------------------------------------------
echo   Unregistering conaito VoIP SIP ActiveX control
echo --------------------------------------------------
echo Make sure you're member of the administrator group
echo otherwise the registration will fail.
regsvr32.exe /u SipSdk.2.0.dll
