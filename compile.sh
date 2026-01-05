#!/bin/bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/InsectLairIncident/Source"

# Compiler avec mcs (Mono)
mcs -target:library \
-out:"../1.6/Assemblies/InsectLairIncident.dll" \
-reference:"../../../RimWorldWin64_Data/Managed/Assembly-CSharp.dll" \
-reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" \
-reference:"../../../RimWorldWin64_Data/Managed/UnityEngine.dll" \
-reference:"/home/gilith/Rimworld mod/references/Harmony/Current/Assemblies/0Harmony.dll" \
-reference:"../../../RimWorldWin64_Data/Managed/netstandard.dll" \
-nowarn:0219,0162,0414 \
*.cs
