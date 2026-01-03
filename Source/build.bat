@echo off
echo Building Beerophage Mod Assembly...

REM Set paths - adjust these to your RimWorld installation
set RIMWORLD_PATH=..\..\..\rimworld base\RimWorldWin64_Data\Managed
set OUTPUT_PATH=..\1.5\Assemblies

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_PATH%" mkdir "%OUTPUT_PATH%"

REM Compile using C# compiler
csc /target:library ^
    /out:"%OUTPUT_PATH%\BeerophageMod.dll" ^
    /reference:"%RIMWORLD_PATH%\Assembly-CSharp.dll" ^
    /reference:"%RIMWORLD_PATH%\UnityEngine.CoreModule.dll" ^
    /reference:"%RIMWORLD_PATH%\mscorlib.dll" ^
    /reference:"%RIMWORLD_PATH%\System.dll" ^
    /reference:"%RIMWORLD_PATH%\System.Core.dll" ^
    *.cs

if %errorlevel% equ 0 (
    echo Build successful! DLL created at %OUTPUT_PATH%\BeerophageMod.dll
    REM Copy to 1.4 folder as well
    copy "%OUTPUT_PATH%\BeerophageMod.dll" "..\1.4\Assemblies\"
) else (
    echo Build failed with error code %errorlevel%
    pause
)

pause