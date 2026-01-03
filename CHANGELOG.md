# Beerophage Mod - Changelog

## Version Finale - 2026-01-02

### ✅ Fonctionnalités Complètes

#### Système de Beergen (Gene_Hemogen)
- Ressource "beergen" comme l'hemogen des Sanguophages
- Affichage automatique du gizmo à côté du bouton Draft
- Déclin de 2% par jour
- Restauration en buvant de la bière
- Fonctionne en caravane et sur la carte monde

#### Xenotype Beerophage
- Tous les gènes configurés et testés
- Besoin de beergen pour survivre
- Capacités spéciales liées à l'alcool

#### Beer Casket
- Lit spécial pour Beerophages
- Régénère le beergen pendant le sommeil
- Consomme de la bière comme carburant (CompRefuelable)
- Nécessite électricité (CompPower)
- **FIXÉ**: Limitation à un seul propriétaire (comme lit simple)

#### Beer Meditation Chamber
- Alternative au Beer Casket
- Méditation active pour régénération rapide
- Jobs d'entrée/sortie dédiés
- Hediff spécial pendant la méditation
- Pas de système de propriétaire (fonctionne comme prévu)

#### Ability: Poing Ivre
- Pouvoir activable manuellement
- Bonus de dégâts mêlée
- Hediff temporaire après utilisation
- Cooldown et conditions d'activation

#### Système d'Empowerment
- Bonus automatiques quand beergen > 70%
- Hediff ajouté/retiré dynamiquement
- Encourage à maintenir le beergen élevé

#### Filtrage d'Alcool Personnalisé
- Beerophages réagissent différemment à l'alcool
- Moins de malus, plus de beergen restauré
- Système de tolérance amélioré

### 🔧 Corrections Appliquées

#### Session 2026-01-02

1. **Compilation C# avec syntaxes modernes**
   - Problème: csc.exe (C# 5) ne supportait pas `?.`, `=>`, `$""`
   - Solution: Utilisation de `mcs` (Mono C# compiler)
   - Commande: `mcs -target:library -reference:... *.cs`

2. **Limitation à un seul propriétaire pour BeerCasket**
   - Problème: `maxAssignedPawnsCount=1` dans XML ignoré
   - Tentatives échouées:
     - Override `SleepingSlotsCount` → propriété non virtuelle
     - Override `TryAssignPawn()` → méthode non virtuelle
   - Solution finale: Composant personnalisé `CompAssignableToPawn_SingleOwner`
   - Fichiers créés:
     - `Source/CompAssignableToPawn_SingleOwner.cs`
     - `Source/CompProperties_AssignableToPawn_SingleOwner.cs`
   - XML modifié: `<li Class="BeerophageMod.CompProperties_AssignableToPawn_SingleOwner">`

3. **Conflit de position de recherche**
   - Problème: BeerCasketTechnology au même emplacement que Machining
   - Solution: `<researchViewY>` changé de 3.5 à 4.5

4. **Support RimWorld 1.6**
   - Ajout de `<li>1.6</li>` dans `<supportedVersions>`

5. **WSL PowerShell Interop**
   - Problème: `powershell.exe` inaccessible après un moment
   - Solution: `wsl --shutdown` puis redémarrer WSL

### 📁 Structure Finale

```
Beerophage/
├── About/
│   └── About.xml                          # Métadonnées (versions 1.4, 1.5, 1.6)
├── Source/                                # Code C# source
│   ├── Building_BeerCasket.cs             # Lit spécial avec restauration beergen
│   ├── Building_BeerMeditationChamber.cs  # Chambre de méditation
│   ├── CompAbilityEffect_PoingIvre.cs     # Effet de l'ability Poing Ivre
│   ├── CompAssignableToPawn_SingleOwner.cs # Composant pour 1 seul propriétaire
│   ├── CompProperties_AssignableToPawn_SingleOwner.cs
│   ├── CompBeergenRestoration.cs          # Restauration passive beergen
│   ├── Gene_BeergenEmpowerment.cs         # Bonus à haut beergen
│   ├── Gene_BeerophageAlcoholCraving.cs   # Système de craving
│   ├── HediffComp_GrantAbility.cs         # Octroie abilities via hediff
│   ├── IngestionOutcomeDoer_BeerophageAlcoholFilter.cs
│   ├── IngestionOutcomeDoer_OffsetBeergen.cs # Restaure beergen en buvant
│   ├── JobDriver_EnterBeerMeditation.cs   # Job d'entrée méditation
│   └── JobDriver_ExitBeerMeditation.cs    # Job de sortie méditation
├── 1.4/
│   ├── Assemblies/
│   │   └── BeerophageMod.dll              # DLL compilée pour RW 1.4
│   └── Defs/                              # Définitions XML spécifiques 1.4
│       ├── GeneDefs/
│       ├── HediffDefs/
│       ├── ThingDefs_Buildings/
│       ├── ResearchProjectDefs/
│       └── ...
├── 1.5/
│   ├── Assemblies/
│   │   └── BeerophageMod.dll              # DLL compilée pour RW 1.5/1.6
│   └── Defs/                              # Définitions XML spécifiques 1.5
│       ├── GeneDefs/
│       │   └── BeerophageGenes.xml        # Gène Beergenic (Gene_Hemogen)
│       ├── HediffDefs/                    # 6 hediffs
│       ├── ThingDefs_Buildings/
│       │   ├── BeerCasket.xml             # Lit avec restauration
│       │   └── BeerMeditationChamber.xml  # Chambre de méditation
│       ├── ResearchProjectDefs/
│       │   └── BeerophageResearch.xml     # 2 recherches
│       └── ...
├── LoadFolders.xml                        # Charge le bon dossier selon version
├── CHANGELOG.md                           # Ce fichier
├── README.md                              # Documentation utilisateur
├── C#_COMPILATION_GUIDE.md                # Guide de compilation
└── VSCode_COMPILATION_GUIDE.md            # Guide VSCode
```

### 🔨 Compilation

**Commande unique (depuis Source/)**:
```bash
mcs -target:library -out:"../1.5/Assemblies/BeerophageMod.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/Assembly-CSharp.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.CoreModule.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/UnityEngine.dll" \
    -reference:"/path/to/RimWorld/RimWorldWin64_Data/Managed/netstandard.dll" \
    *.cs && cp "../1.5/Assemblies/BeerophageMod.dll" "../1.4/Assemblies/"
```

### 🎮 Tests

**Environnement**:
- RimWorld 1.6.4633 rev1261
- WSL2 Ubuntu + Windows
- Tests avec Dev Mode QuickTest

**Résultats**:
- ✅ 0 erreurs XML
- ✅ 0 erreurs de compilation
- ✅ Xenotype Beerophage disponible
- ✅ Gizmo beergen s'affiche
- ✅ Beer Casket fonctionne et limite à 1 propriétaire
- ✅ Beer Meditation Chamber fonctionne
- ✅ Ability Poing Ivre activable
- ✅ Restauration beergen en buvant de la bière

### 📝 Notes de Développement

**Leçons apprises**:
1. Toujours utiliser `mcs` pour du code C# moderne
2. Les propriétés XML comme `maxAssignedPawnsCount` ne sont pas toujours respectées
3. Créer des composants personnalisés quand override ne fonctionne pas
4. Vérifier qu'une méthode est virtuelle avant de l'override
5. Tester régulièrement avec Dev QuickTest pendant le développement

**Pièges évités**:
- ❌ Ne PAS utiliser csc.exe pour du code avec `?.`, `=>`, ou `$""`
- ❌ Ne PAS supposer que les paramètres XML fonctionnent sans tester
- ❌ Ne PAS oublier `netstandard.dll` dans les références de compilation
- ✅ TOUJOURS compiler avec mcs
- ✅ TOUJOURS tester en jeu après chaque changement majeur

### 🔗 Dépendances

**Requises**:
- RimWorld 1.4, 1.5, ou 1.6
- DLC Biotech (pour Gene_Hemogen)

**Optionnelles**:
- Harmony (chargé automatiquement par RimWorld)

### 👤 Crédits

- Développement: gilith + Claude
- Inspiration: Système Sanguophage de Biotech
- Tests: gilith

---

**Date de finalisation**: 2026-01-02 13:15
**Statut**: ✅ Production-ready
**Version RimWorld**: 1.6.4633 rev1261
