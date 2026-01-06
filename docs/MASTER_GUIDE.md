# 📚 Guide Maître - Modding RimWorld

**Auteur:** Gilith
**Version:** 3.1
**Dernière mise à jour:** 2026-01-06
**Environnement:** WSL2 Ubuntu + Windows, RimWorld 1.6

---

## 📋 Table des Matières

1. [Structure d'un Mod](#structure-dun-mod)
2. [Outils de Développement](#outils-de-développement)
3. [Compilation C#](#compilation-c)
4. [Lancement et Tests](#lancement-et-tests)
5. [Workflow Complet](#workflow-complet)
6. [Production et Distribution](#production-et-distribution)
7. [Erreurs Courantes](#erreurs-courantes)
8. [Exemples Pratiques](#exemples-pratiques)
9. [Ressources](#ressources)

---

## 🏗️ Structure d'un Mod

### Arborescence Standard

```
RimWorld/Mods/VotreMod/
├── About/
│   ├── About.xml              # Métadonnées obligatoires
│   └── Preview.png            # Image 640x360px
├── Source/                    # Code C# source
│   ├── *.cs
│   └── build.sh               # Script de compilation
├── 1.4/                       # Version RimWorld 1.4
│   ├── Assemblies/
│   │   └── VotreMod.dll
│   └── Defs/
│       └── *.xml
├── 1.5/                       # Version RimWorld 1.5/1.6
│   ├── Assemblies/
│   │   └── VotreMod.dll
│   └── Defs/
│       └── *.xml
├── LoadFolders.xml            # Gestion des versions
├── README.md
└── CHANGELOG.md
```

### About.xml (Obligatoire)

```xml
<?xml version="1.0" encoding="utf-8"?>
<ModMetaData>
    <name>Votre Mod</name>
    <author>Votre Nom</author>
    <packageId>votrenom.votremod</packageId>
    <supportedVersions>
        <li>1.4</li>
        <li>1.5</li>
        <li>1.6</li>
    </supportedVersions>
    <description>Description de votre mod</description>
    <modDependencies>
        <li>
            <packageId>ludeon.rimworld.biotech</packageId>
            <displayName>Biotech</displayName>
        </li>
    </modDependencies>
    <loadAfter>
        <li>ludeon.rimworld.biotech</li>
        <li>brrainz.harmony</li>
    </loadAfter>
</ModMetaData>
```

### LoadFolders.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
    <v1.4>
        <li>/</li>
        <li>1.4</li>
    </v1.4>
    <v1.5>
        <li>/</li>
        <li>1.5</li>
    </v1.5>
    <v1.6>
        <li>/</li>
        <li>1.5</li>  <!-- RW 1.6 utilise les defs 1.5 -->
    </v1.6>
</loadFolders>
```

---

## 🛠️ Outils de Développement

### TDBug - Debug Enhancement Mod ✅ INSTALLÉ

**Location:** `/home/gilith/Rimworld mod/RimWorld/Mods/TDBug`

**Utilité:**
- Outils de debug/dev pour modders RimWorld
- Compatible RimWorld 1.6
- Toujours garder activé pendant le développement

**Installation:**
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods"
git clone https://github.com/alextd/RimWorld-TDBug.git TDBug
```

### Doorstop - Hot Reload & Debugging ✅ INSTALLÉ

**Location:** `/home/gilith/Rimworld mod/RimWorld/`

**Fonctionnalités:**
- 🔥 **Hot Reload** - Recompiler sans redémarrer RimWorld
- 🐛 Debugging avec breakpoints
- 🔌 Support IDE (Rider, Visual Studio, dnSpyEx)

**Fichiers installés:**
- `winhttp.dll` - Unity Doorstop v4.4.0
- `Doorstop.dll` - pardeike's RimWorld-Doorstop (2.9MB)
- `doorstop_config.ini` - Configuration (debug port: 50000)

**Utilisation du Hot Reload:**

1. Marquer les méthodes comme rechargeables:
```csharp
using HarmonyLib;

[Reloadable]
public void MaMethode()
{
    // Cette méthode peut être rechargée à chaud
}
```

2. Recompiler pendant que RimWorld tourne:
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/Source"
./build.sh
# Les changements sont appliqués IMMÉDIATEMENT sans redémarrage!
```

**Debugging avec IDE:**
- **Rider:** "Attach to Unity Process" → Host: 127.0.0.1, Port: 50000
- **Visual Studio:** "Debug → Attach to Process" → Mono port
- **dnSpyEx:** Attach to process

**Avantages:**
- ⚡ Gain de temps MASSIF (pas besoin de redémarrer)
- 🔄 Itération rapide pendant debug
- 🎯 Tester fixes immédiatement

### Better Log (Optionnel)

**Status:** Non installé (nécessite Steam Workshop)

**Workshop ID:** 3531364227 (version 1.6 Temp)
**URL:** https://steamcommunity.com/sharedfiles/filedetails/?id=3531364227

**Utilité:**
- Logs colorés
- Filtrage avancé des messages
- Aide à identifier erreurs rapidement

**Installation:** S'abonner via Steam Workshop

**Note:** L'original Better Log a des problèmes avec 1.6. Utiliser la version [1.6 Temp].

📖 **Documentation complète:** `/home/gilith/Rimworld mod/docs/DEVELOPMENT_TOOLS.md`

---

## 🔨 Compilation C#

### Méthode 1: Mono C# Compiler (mcs) ⭐ RECOMMANDÉ

**Pourquoi mcs?**
- ✅ Supporte syntaxes C# modernes (`?.`, `=>`, `$""`)
- ✅ Disponible dans WSL: `sudo apt-get install mono-complete`
- ✅ Rapide et fiable

**Script de Compilation** (`Source/build.sh`):

```bash
#!/bin/bash
echo "🔨 Compilation de VotreMod..."

# Chemins
RIMWORLD_LIBS="/home/gilith/Rimworld mod/RimWorld/RimWorldWin64_Data/Managed"
HARMONY_DLL="/home/gilith/Rimworld mod/mod rimworld/Harmony/Current/Assemblies/0Harmony.dll"
OUTPUT_15="../1.5/Assemblies"
OUTPUT_14="../1.4/Assemblies"

# Créer dossiers de sortie
mkdir -p "$OUTPUT_15" "$OUTPUT_14"

# Compilation avec mcs
mcs -target:library \
    -out:"$OUTPUT_15/VotreMod.dll" \
    -reference:"$RIMWORLD_LIBS/Assembly-CSharp.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.CoreModule.dll" \
    -reference:"$RIMWORLD_LIBS/UnityEngine.dll" \
    -reference:"$HARMONY_DLL" \
    -reference:"$RIMWORLD_LIBS/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    *.cs

if [ $? -eq 0 ]; then
    echo "✅ Compilation réussie!"
    cp "$OUTPUT_15/VotreMod.dll" "$OUTPUT_14/"
    echo "✅ DLL copiée vers 1.4 et 1.5"
    ls -lh "$OUTPUT_15/VotreMod.dll"
else
    echo "❌ Erreur de compilation"
    exit 1
fi
```

**Utilisation:**
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/Source"
chmod +x build.sh
./build.sh
```

### Méthode 2: Ligne de commande directe

```bash
cd Source/
mcs -target:library \
    -out:"../1.5/Assemblies/VotreMod.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.dll" \
    -reference:"/path/to/Harmony/0Harmony.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/netstandard.dll" \
    -nowarn:0219,0162,0414 \
    *.cs && cp "../1.5/Assemblies/VotreMod.dll" "../1.4/Assemblies/"
```

### ⚠️ NE PAS utiliser csc.exe

Le compilateur Windows `csc.exe` (`/mnt/c/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe`) ne supporte que C# 5:
- ❌ Pas de `?.` (null-conditional)
- ❌ Pas de `=>` (expression-bodied members)
- ❌ Pas de `$""` (string interpolation)

**Utiliser mcs à la place!**

---

## 🧪 Lancement et Tests

### Option 1: Lancement Direct (Interface Graphique)

```bash
cd "/home/gilith/Rimworld mod"
./RimWorld/RimWorldWin64.exe -savedatafolder="$(wslpath -w "$(pwd)/TestData")" &
```

**Détails:**
- `-savedatafolder` : Environnement de test isolé
- `&` : Exécution en arrière-plan
- Timeout après 2 minutes (normal)

**Arrêter RimWorld:**
```bash
taskkill.exe /F /IM RimWorldWin64.exe
```

### Option 2: Tests Headless (Validation XML)

**Script Python** (`C:\Users\gilit\test_rimworld.py`):

```python
#!/usr/bin/env python3
import subprocess
from pathlib import Path

RIMWORLD = Path(r"\\wsl.localhost\Ubuntu\home\gilith\Rimworld mod\RimWorld\RimWorldWin64.exe")
TESTDATA = Path(r"\\wsl.localhost\Ubuntu\home\gilith\Rimworld mod\TestData")

process = subprocess.Popen(
    [
        str(RIMWORLD),
        "-batchmode",
        "-nographics",
        "-nosound",
        "-quicktest",
        f"-savedatafolder={TESTDATA}",
        "-logFile", "-"
    ],
    stdout=subprocess.PIPE,
    stderr=subprocess.STDOUT,
    text=True,
    bufsize=1
)

for line in process.stdout:
    line = line.strip()
    if 'ERROR' in line or 'Exception' in line:
        if 'shader' not in line.lower():
            print(f"[ERROR] {line[:120]}")
```

**Lancement depuis WSL:**
```bash
cd "/home/gilith/Rimworld mod"
powershell.exe -Command "python C:\Users\gilit\test_rimworld.py"
```

**Durée:** ~30 secondes
**Vérifie:** Erreurs XML uniquement

### Option 3: Dev Mode QuickTest

1. Lancer RimWorld normalement
2. Options → Dev mode (activer)
3. Console debug: `` ` `` ou `~`
4. Commandes utiles:
   - `incident VotreIncident` - Déclencher incident
   - Ctrl+Shift+O - Console de logs

### Logs

**Emplacement Windows:**
```
C:\Users\<username>\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log
```

**Depuis WSL:**
```bash
tail -100 "/mnt/c/Users/gilit/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log"

# Filtrer pour votre mod
tail -500 "/mnt/c/Users/gilit/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log" | \
    grep -i "VotreModName\|ERROR\|Exception" -A2

# Suivi en temps réel
tail -f "/mnt/c/Users/gilit/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log"
```

---

## 🔄 Workflow Complet

### 1. Créer la Structure

```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods"
mkdir -p NouveauMod/{About,Source,1.4/{Assemblies,Defs},1.5/{Assemblies,Defs}}
```

### 2. Créer About.xml et LoadFolders.xml

Copier les templates du début de ce guide.

### 3. Développer le Code C#

**Template de base:**
```csharp
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VotreMod
{
    public class VotreClasse : ThingComp
    {
        public override void CompTick()
        {
            base.CompTick();
            // Votre logique ici
        }
    }
}
```

### 4. Compiler

```bash
cd NouveauMod/Source
./build.sh
```

### 5. Créer les Définitions XML

**Exemple de ThingDef:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
    <ThingDef ParentName="BuildingBase">
        <defName>VotreThing</defName>
        <label>votre chose</label>
        <description>Description</description>
        <thingClass>VotreMod.VotreClasse</thingClass>
        <graphicData>
            <texPath>Things/Building/VotreThing</texPath>
            <graphicClass>Graphic_Single</graphicClass>
        </graphicData>
        <statBases>
            <MaxHitPoints>100</MaxHitPoints>
        </statBases>
        <size>(1,1)</size>
    </ThingDef>
</Defs>
```

### 6. Configurer ModsConfig.xml pour Tests

```bash
nano "/home/gilith/Rimworld mod/TestData/Config/ModsConfig.xml"
```

**Ajouter:**
```xml
<activeMods>
    <li>ludeon.rimworld</li>
    <li>ludeon.rimworld.royalty</li>
    <li>ludeon.rimworld.ideology</li>
    <li>ludeon.rimworld.biotech</li>
    <li>ludeon.rimworld.anomaly</li>
    <li>ludeon.rimworld.odyssey</li>
    <li>brrainz.harmony</li>
    <li>votrenom.votremod</li>  <!-- Votre mod -->
</activeMods>
```

### 7. Tester

```bash
cd "/home/gilith/Rimworld mod"
./RimWorld/RimWorldWin64.exe -savedatafolder="$(wslpath -w "$(pwd)/TestData")" &
```

### 8. Itérer

**Cycle de développement (Standard):**
1. Modifier le code C#
2. `./build.sh`
3. `taskkill.exe /F /IM RimWorldWin64.exe`
4. Relancer RimWorld
5. Tester
6. Retour à 1

**Cycle de développement (Avec Hot Reload - Doorstop):**
1. Modifier le code C#
2. Marquer méthodes avec `[Reloadable]`
3. `./build.sh`
4. **RimWorld reste ouvert** - changements appliqués automatiquement!
5. Tester immédiatement
6. Retour à 1

⚡ **Le hot reload élimine 90% du temps de redémarrage!**

---

## 📦 Production et Distribution

### Structure Dev vs Prod

**Versions Dev** (dans `RimWorld/Mods/`):
- ✅ Code source C# (`Source/`)
- ✅ Scripts de build (`.sh`)
- ✅ Fichiers Git (`.git`, `.gitignore`)
- ✅ Configuration IDE (`.vscode`)
- ✅ Documentation développement (`DOCUMENTATION.md`, `CHANGELOG_v*.md`)

**Versions Prod** (dans `prod/mods/`):
- ✅ DLLs compilées uniquement
- ✅ XML Defs
- ✅ Documentation utilisateur (`README.md`, `CHANGELOG.md`)
- ❌ Pas de source
- ❌ Pas de Git
- ❌ Pas de build scripts

### Workflow de Release

#### 1. Développement
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/Source"
./build.sh
# Tester avec hot reload
```

#### 2. Copier vers Production
```bash
cd "/home/gilith/Rimworld mod"
rm -rf "prod/mods/VotreMod"
cp -r "RimWorld/Mods/VotreMod" "prod/mods/"
```

#### 3. Nettoyer la Version Prod
```bash
cd "prod/mods/VotreMod"
rm -rf Source .git .vscode .gitignore *.sh
rm -f *GUIDE*.md *FIXES*.md *INSTALLATION*.md *DOCUMENTATION*.md *CHANGELOG_v*.md
```

#### 4. Créer le ZIP de Distribution
```bash
cd "/home/gilith/Rimworld mod/prod/mods"
zip -r "../../VotreMod_v1.0.0.zip" VotreMod/
```

Le ZIP sera créé à la racine du workspace (`/home/gilith/Rimworld mod/VotreMod_v1.0.0.zip`).

#### 5. Vérifier le Contenu
```bash
unzip -l "/home/gilith/Rimworld mod/VotreMod_v1.0.0.zip"
```

Doit contenir uniquement:
- `VotreMod/About/`
- `VotreMod/1.*/` (avec Assemblies/ et Defs/)
- `VotreMod/LoadFolders.xml`
- `VotreMod/README.md`
- `VotreMod/CHANGELOG.md`

### Mise à Jour prod/README.md

Après chaque release, mettre à jour `/home/gilith/Rimworld mod/prod/README.md` avec:
- Nouvelle version
- Description des changements
- Exemples de commandes avec bonne version

---

## 🐛 Erreurs Courantes

### Compilation

#### "Could not find type 'VotreMod.VotreClasse'"

**Cause:** DLL pas compilée ou pas au bon endroit

**Solution:**
```bash
ls -la "/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/1.5/Assemblies/"
# Doit contenir VotreMod.dll
```

#### "The predefined type 'System.ValueType' is defined in an assembly that is not referenced"

**Cause:** Référence à `netstandard.dll` manquante

**Solution:** Ajouter `-reference:"$RIMWORLD_LIBS/netstandard.dll"` à mcs

#### "Syntax error, '?' expected" ou "Invalid expression term '.'"

**Cause:** Syntaxe C# moderne avec csc.exe (C# 5)

**Solution:** Utiliser `mcs` au lieu de `csc.exe`

### XML

#### "Could not resolve cross-reference: No X found"

**Cause:** Référence à un DefName inexistant

**Solution:** Vérifier le nom exact dans les defs vanilla

#### "Config error: same research view coords"

**Cause:** Deux recherches au même emplacement

**Solution:** Changer `<researchViewX>` ou `<researchViewY>`

#### "Duplicate def name X"

**Cause:** Deux defs avec le même `<defName>`

**Solution:** Renommer l'un des deux

### Runtime

#### "NullReferenceException"

**Solution:** Ajouter vérifications:
```csharp
// ❌ Dangereux
var value = pawn.health.hediffSet.GetFirstHediff();

// ✅ Sûr
if (pawn?.health?.hediffSet != null)
{
    var value = pawn.health.hediffSet.GetFirstHediff();
}
```

### WSL

#### "run-detectors: unable to find an interpreter for powershell.exe"

**Solution:**
```powershell
# Depuis Windows PowerShell:
wsl --shutdown
# Puis redémarrer WSL
```

---

## 📖 Exemples Pratiques

### Exemple 1: Beerophage

**Concept:** Xenotype qui utilise de la bière au lieu du sang (comme Sanguophage)

**Approche intelligente:**
- Réutilise `Gene_Hemogen` vanilla, renommé en "beergen"
- `Building_Bed` pour Beer Casket
- Composant custom `CompAssignableToPawn_SingleOwner` pour limitation à 1 propriétaire

**Localisation:** `/home/gilith/Rimworld mod/RimWorld/Mods/Beerophage`

**Leçons:**
1. Réutiliser systèmes vanilla plutôt que recréer
2. Composants pour modifier comportements
3. LoadFolders.xml pour multi-versions

### Exemple 2: Insect Lair Incident

**Concept:** Cave d'insectes qui spawn sur la map comme incident aléatoire

**Techniques:**
- `BuildingGroundSpawner` pour émergence progressive
- `MapComponent` pour tracking de vagues
- Harmony patches pour remplacement de pawns
- GenStep pour boss post-génération

**Localisation:** `/home/gilith/Rimworld mod/RimWorld/Mods/InsectLairIncident`

**Problème majeur résolu:** Boss ne spawnait pas dans boss room
- **Cause:** Harmony remplaçait HiveQueen pendant `GenerateBossRoom`
- **Solution:** Ne PAS remplacer pendant génération, remplacer après avec GenStep

**Leçons:**
1. Vanilla `GenerateBossRoom` est fragile, attendre post-génération
2. PawnGenerationRequest est un struct, créer nouvelle instance
3. LoadFolders.xml crucial pour éviter chargement de mauvaise DLL

**Release:**
- Version dev: `RimWorld/Mods/InsectLairIncident/`
- Version prod: `prod/mods/InsectLairIncident/`
- Distribution: `InsectLairIncident_v2.3.0.zip` (28KB)

---

## 🧰 Commandes Rapides

### Compilation
```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/Source"
./build.sh
```

### Lancement
```bash
cd "/home/gilith/Rimworld mod" && ./RimWorld/RimWorldWin64.exe -savedatafolder="$(wslpath -w "$(pwd)/TestData")" &
```

### Arrêt
```bash
taskkill.exe /F /IM RimWorldWin64.exe
```

### Test Headless
```bash
powershell.exe -Command "python C:\Users\gilit\test_rimworld.py"
```

### Distribution
```bash
# 1. Copier vers prod
cd "/home/gilith/Rimworld mod"
rm -rf "prod/mods/VotreMod"
cp -r "RimWorld/Mods/VotreMod" "prod/mods/"

# 2. Nettoyer
cd "prod/mods/VotreMod"
rm -rf Source .git .vscode .gitignore *.sh
rm -f *DOCUMENTATION*.md *CHANGELOG_v*.md

# 3. Créer ZIP
cd "/home/gilith/Rimworld mod/prod/mods"
zip -r "../../VotreMod_v1.0.0.zip" VotreMod/
```

### Archivage Backup
```bash
tar -czf "VotreMod_backup_$(date +%Y%m%d).tar.gz" \
    "RimWorld/Mods/VotreMod" \
    "TestData/Config/ModsConfig.xml"
```

---

## 📚 Ressources

### Documentation Officielle
- [RimWorld Wiki - Modding](https://rimworldwiki.com/wiki/Modding)
- [XML File Structure](https://rimworldwiki.com/wiki/Modding_Tutorials/XML_File_Structure)
- [Harmony Docs](https://harmony.pardeike.net/)

### Outils

**Installés:**
- **TDBug** - Debug tools pour modders (installé dans `/RimWorld/Mods/TDBug`)
- **Doorstop** - Hot reload + debugging (installé dans `/RimWorld/`)

**Recommandés:**
- **ILSpy** - Désassembleur .NET pour examiner Assembly-CSharp.dll
- **dnSpyEx** - Debugger .NET avec support Doorstop
- **monodis** - Désassembler DLLs sous Linux
- **JetBrains Rider** - IDE premium pour C# (meilleur support RimWorld)
- **Visual Studio Code** - IDE gratuit léger

### Exemples Vanilla
```bash
# Trouver exemples dans defs vanilla
grep -r "XenotypeDef" "/path/to/RimWorld/Data/" -A20
grep -r "GeneDef" "/path/to/RimWorld/Data/" -A20
```

### Community
- [r/RimWorldMods](https://reddit.com/r/RimWorldMods)
- [RimWorld Discord - #modding](https://discord.gg/rimworld)
- [Ludeon Forums - Mods](https://ludeon.com/forums/index.php?board=12.0)

---

## 📋 Checklist Finale

Avant de publier un mod:

- [ ] About.xml contient `<supportedVersions>`
- [ ] About.xml contient `<packageId>` unique
- [ ] Test automatisé sans erreur
- [ ] Test manuel en jeu réussi
- [ ] Aucune erreur dans Player.log liée au mod
- [ ] README.md créé avec instructions
- [ ] Preview.png ajouté (640x360px)
- [ ] Licence spécifiée
- [ ] Compatibilité vérifiée avec mods populaires
- [ ] Changelog documenté
- [ ] Version prod créée dans `prod/mods/`
- [ ] ZIP de distribution créé à la racine du workspace
- [ ] `prod/README.md` mis à jour avec nouvelle version

---

## 💡 Bonnes Pratiques

1. **Compilation:** Toujours utiliser `mcs`
2. **Tests:** Environnement isolé avec TestData
3. **Versions:** LoadFolders.xml pour 1.4/1.5/1.6
4. **Documentation:** README + CHANGELOG au minimum
5. **Archivage:** Backups réguliers des versions stables
6. **Git:** Branch-per-mod si repository multi-mods
7. **Logs:** Lire Player.log après chaque test
8. **Dev Mode:** Meilleur ami du moddeur
9. **Hot Reload:** Marquer méthodes avec `[Reloadable]` pour itération rapide
10. **TDBug:** Toujours activé pendant développement
11. **Distribution:** Toujours créer ZIP depuis `prod/mods/` (pas depuis dev!)

---

**Guide créé par Gilith**
**Dernière révision:** 2026-01-06
**Version:** 3.1
