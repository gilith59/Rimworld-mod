# Organisation du Workspace RimWorld Modding

**Date**: 2026-01-02
**Dernière mise à jour**: 2026-01-06 (Ajout structure prod/)

---

## 🎯 Structure Recommandée

```
/home/gilith/Rimworld mod/
│
├── 📁 RimWorld/                              # Installation RimWorld + Mods DEV
│   ├── RimWorldWin64.exe
│   ├── RimWorldWin64_Data/Managed/          # DLLs de référence pour compilation
│   └── Mods/
│       ├── Beerophage/                       # ✅ VERSION DEV (avec Source/)
│       │   ├── About/About.xml
│       │   ├── Source/*.cs                   # Sources C#
│       │   ├── 1.4/                          # Version RimWorld 1.4
│       │   │   ├── Assemblies/BeerophageMod.dll
│       │   │   └── Defs/*.xml
│       │   ├── 1.5/                          # Version RimWorld 1.5/1.6
│       │   │   ├── Assemblies/BeerophageMod.dll
│       │   │   └── Defs/*.xml
│       │   ├── LoadFolders.xml
│       │   └── CHANGELOG.md
│       │
│       ├── InsectLairIncident/               # ✅ VERSION DEV (avec Source/)
│       │   ├── Source/*.cs
│       │   ├── 1.6/Assemblies/
│       │   └── ...
│       │
│       └── [Autres mods futurs ici]
│
├── 📦 prod/                                  # Mods PRODUCTION (prêts à distribuer)
│   ├── mods/
│   │   ├── Beerophage/                       # ✅ VERSION PROD (sans Source/)
│   │   └── InsectLairIncident/               # ✅ VERSION PROD (sans Source/)
│   └── README.md                              # Guide de distribution
│
├── 📁 TestData/                              # Environnement de test isolé
│   └── Config/ModsConfig.xml                 # Configuration pour tests
│
├── 📚 docs/                                  # Documentation centralisée
│   ├── MASTER_GUIDE.md                        # ⭐ GUIDE PRINCIPAL
│   └── ...
│
├── 🔧 references/                            # DLLs de référence (Harmony, etc.)
│
├── 📄 README.md                              # ✅ Guide principal workspace
├── 📄 ORGANISATION_WORKSPACE.md              # ✅ Ce fichier
├── 📄 Makefile                               # Automatisation compilation
│
├── 📦 *.zip                                  # Packages de distribution
└── 🧹 cleanup_now.sh                         # Script de nettoyage

❌ SUPPRIMÉS (duplicatas):
├── beerophage_source/
├── beerophage_extracted/
├── BeerophageMod_Source/
├── RimWorld/Mods/BeerophageMod/
├── mod rimworld/Beerophage/
├── rimworld mod venant de f/BeerophageMod/
├── Rimworld ordi portable/Beerophage/
├── Rimworld ordi portable/Beerophage_new/
└── *.zip (sauf archives .tar.gz)
```

---

## 🎯 Règles d'Organisation

### 1. Un Seul Mod Actif

**Location unique**: `/home/gilith/Rimworld mod/RimWorld/Mods/VotreMod/`

**Pourquoi?**
- RimWorld charge depuis ce dossier
- Pas de confusion sur quelle version est active
- Facilite le debugging

### 2. Sources dans le Mod

**Structure**:
```
Mods/VotreMod/
├── Source/              # Code C# source
│   ├── *.cs
│   └── build.sh         # Script de compilation
├── 1.4/Assemblies/      # DLL compilée pour 1.4
├── 1.5/Assemblies/      # DLL compilée pour 1.5/1.6
└── CHANGELOG.md         # Historique des modifications
```

**Avantages**:
- Sources et DLL ensemble
- Facile à archiver
- Facile à partager

### 3. Archives Datées

**Format**: `NomMod_TYPE_YYYYMMDD.tar.gz`

**Exemples**:
- `Beerophage_FINAL_20260102.tar.gz` - Version finale stable
- `Beerophage_BACKUP_20260102.tar.gz` - Backup avant modifications
- `Beerophage_WIP_20260102.tar.gz` - Work in progress

**Contenu d'une archive**:
```bash
tar -czf VotreMod_FINAL_$(date +%Y%m%d).tar.gz \
    RimWorld/Mods/VotreMod \
    TestData/Config/ModsConfig.xml \
    GUIDE_*.md
```

### 4. Documentation au Root

**À la racine du workspace**:
- `GUIDE_COMPLET_MODDING_RIMWORLD.md` - Guide de référence
- `SYNTHESE_SESSION_YYYYMMDD.md` - Notes de session
- `ORGANISATION_WORKSPACE.md` - Ce fichier

**Pourquoi?**
- Accessible rapidement
- Valable pour tous les mods
- Facile à retrouver

---

## 🧹 Nettoyage Régulier

### Script de Nettoyage

```bash
./cleanup_beerophage_duplicates.sh
```

**Ce qu'il fait**:
1. Liste les duplicatas
2. Demande confirmation
3. Supprime les anciens dossiers
4. Garde uniquement:
   - Version active dans `RimWorld/Mods/`
   - Archives `.tar.gz`
   - Guides de documentation

### Quand Nettoyer?

- ✅ Après chaque session majeure
- ✅ Avant d'archiver une version finale
- ✅ Quand le workspace devient confus
- ✅ Avant de commencer un nouveau mod

### Que Garder?

**À TOUJOURS garder**:
- `RimWorld/Mods/VotreMod/` - Version active
- `*.tar.gz` - Archives datées
- `GUIDE_*.md` - Documentation
- `TestData/` - Configuration de test

**À SUPPRIMER**:
- Dossiers dupliqués (`beerophage_source`, etc.)
- Anciens `.zip` non archivés
- Dossiers de test temporaires
- Fichiers `:Zone.Identifier`

---

## 📋 Workflow Nouveau Mod

### 1. Créer la Structure

```bash
cd "/home/gilith/Rimworld mod/RimWorld/Mods"
mkdir -p NouveauMod/{About,Source,1.4/{Assemblies,Defs},1.5/{Assemblies,Defs}}
```

### 2. Copier Templates

```bash
# About.xml
cp Beerophage/About/About.xml NouveauMod/About/
nano NouveauMod/About/About.xml  # Modifier nom, packageId, etc.

# LoadFolders.xml
cp Beerophage/LoadFolders.xml NouveauMod/

# Script de build
cp Beerophage/Source/build.sh NouveauMod/Source/
nano NouveauMod/Source/build.sh  # Changer noms DLL
```

### 3. Développer

```bash
cd NouveauMod/Source
nano VotreClasse.cs
./build.sh
```

### 4. Tester

```bash
# Ajouter au ModsConfig.xml
nano "/home/gilith/Rimworld mod/TestData/Config/ModsConfig.xml"

# Lancer RimWorld
cd "/home/gilith/Rimworld mod"
./RimWorld/RimWorldWin64.exe -savedatafolder="$(wslpath -w "$(pwd)/TestData")" &
```

### 5. Documenter

```bash
cd NouveauMod
nano CHANGELOG.md
# Noter tous les changements au fur et à mesure
```

### 6. Archiver (quand stable)

```bash
cd "/home/gilith/Rimworld mod"
tar -czf "NouveauMod_FINAL_$(date +%Y%m%d).tar.gz" \
    RimWorld/Mods/NouveauMod \
    TestData/Config/ModsConfig.xml
```

---

## 🚨 Pièges à Éviter

### ❌ NE PAS:

1. **Créer plusieurs versions du même mod** dans différents dossiers
   - Garde uniquement celle dans `RimWorld/Mods/`
   - Archive les anciennes versions

2. **Laisser des dossiers de test** trainer
   - Nettoie `beerophage_source`, `BeerophageMod_Source`, etc.
   - Utilise le script de nettoyage

3. **Oublier de documenter** au fur et à mesure
   - Crée CHANGELOG.md dès le début
   - Note chaque modification importante

4. **Mélanger sources et builds**
   - Sources dans `Source/`
   - DLLs dans `*/Assemblies/`
   - XMLs dans `*/Defs/`

5. **Garder trop de backups**
   - Archive finale suffit
   - Supprime les zips intermédiaires
   - Git est meilleur pour versioning

### ✅ FAIRE:

1. **Un seul emplacement actif** par mod
2. **Sources dans le mod** (`VotreMod/Source/`)
3. **Archives datées** pour versions importantes
4. **Documentation claire** à la racine
5. **Nettoyage régulier** des duplicatas

---

## 📊 État Actuel (Post-Nettoyage)

### ✅ Fichiers Importants

```bash
# Version active
ls -lh RimWorld/Mods/Beerophage/
# → Mod complet et fonctionnel

# Archives
ls -lh *Beerophage*.tar.gz
# → Beerophage_FINAL_20260102.tar.gz (915KB)

# Documentation
ls -lh GUIDE_*.md SYNTHESE_*.md
# → 3 guides complets

# Scripts
ls -lh *.sh
# → cleanup_beerophage_duplicates.sh
```

### 🗑️ À Supprimer

Exécutez le script de nettoyage:
```bash
./cleanup_beerophage_duplicates.sh
```

---

## 🎯 Checklist Mensuelle

- [ ] Nettoyer les duplicatas: `./cleanup_beerophage_duplicates.sh`
- [ ] Archiver les mods stables: `tar -czf ...`
- [ ] Mettre à jour GUIDE si nouvelles découvertes
- [ ] Vérifier TestData/Config/ModsConfig.xml
- [ ] Supprimer anciens logs RimWorld
- [ ] Backup des archives sur autre disque

---

**Dernière mise à jour**: 2026-01-06
**Script de nettoyage**: `cleanup_now.sh`
**Statut**: ✅ Workspace organisé avec structure dev/prod
