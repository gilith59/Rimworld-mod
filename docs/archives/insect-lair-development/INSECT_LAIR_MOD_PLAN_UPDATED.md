# 🐛 Insect Lair Incident Mod - Plan Détaillé (MISE À JOUR)

## 📋 Concept

Créer un incident qui fait spawner un **InsectLairEntrance** aléatoirement sur la map de la colonie (comme PitGate), avec:
- ✅ Spawning d'insectoïdes à la surface (comme FleshbeastEmergence)
- ✅ Toutes les rewards habituelles d'un InsectLair (Gravcore, GravlitePanel, ancient crates)
- ✅ Boss: **HiveQueen VANILLA (Odyssey)** dans la lair souterraine
- ✅ Ne peut être effondré QUE si la HiveQueen est tuée (bouton grisé sinon)

---

## 🎯 Objectifs du Mod

### 1. **Incident de Spawn sur Map**
Créer un incident similaire à `IncidentWorker_PitGate` qui spawne un `InsectLairEntrance` sur la map active.

### 2. **Système d'Émergence d'Insectoïdes**
Créer des incidents d'insectoïdes qui émergent de la lair (similaire à `FleshbeastEmergence`).

### 3. **HiveQueen Boss (VANILLA)**
✅ **Utiliser la HiveQueen vanilla du DLC Odyssey** - pas besoin de custom race!

### 4. **Système de Collapse Conditionnel**
Modifier `CompProperties_Sealable` pour griser le bouton tant que la HiveQueen est vivante.

---

## 📁 Structure du Mod

```
RimWorld/Mods/InsectLairIncident/
├── About/
│   ├── About.xml
│   └── Preview.png
├── 1.5/
│   ├── Assemblies/
│   │   └── InsectLairIncident.dll
│   └── Defs/
│       ├── IncidentDefs/
│       │   └── Incidents_InsectLair.xml
│       ├── ThingDefs_Buildings/
│       │   └── Building_InsectLairEntrance.xml
│       └── InsectLairIncidentDefs/
│           └── InsectLairIncidentDefs.xml
└── Source/
    ├── IncidentWorker_InsectLairSpawn.cs
    ├── InsectLairIncidentDef.cs
    ├── InsectLairIncidentWorker_InsectEmergence.cs
    ├── Building_InsectLairEntrance_Custom.cs
    ├── CompProperties_Sealable_ConditionalQueen.cs
    ├── CompSealable_ConditionalQueen.cs
    ├── GenStep_InsectLairWithQueen.cs
    └── MapComponent_InsectLairQueen.cs
```

**✅ SIMPLIFIÉ**: Pas besoin de `PawnKindDefs/` ni `ThingDefs_Races/` - on utilise la HiveQueen vanilla!

---

## 🔧 Composants Techniques

### 1. **IncidentWorker_InsectLairSpawn.cs**

```csharp
using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    public class IncidentWorker_InsectLairSpawn : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            // Ne pas spawner si un InsectLairEntrance existe déjà
            if (map.listerBuildings.AllBuildingsColonistOfClass<Building_InsectLairEntrance_Custom>().Any())
                return false;

            // Ne pas spawner si pas assez d'espace
            if (!TryFindSpawnCell(map, out IntVec3 _))
                return false;

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            if (!TryFindSpawnCell(map, out IntVec3 spawnCell))
                return false;

            // Spawner le InsectLairEntranceSpawner (comme PitGateSpawner)
            ThingDef spawnerDef = DefDatabase<ThingDef>.GetNamed("InsectLairEntranceSpawner");
            Thing spawner = ThingMaker.MakeThing(spawnerDef);
            GenSpawn.Spawn(spawner, spawnCell, map, WipeMode.VanishOrMoveAside);

            // Lettre d'avertissement
            string letterText = "InsectLairSpawnedText".Translate();
            string letterLabel = "InsectLairSpawnedLabel".Translate();

            Find.LetterStack.ReceiveLetter(
                letterLabel,
                letterText,
                LetterDefOf.ThreatBig,
                new TargetInfo(spawnCell, map)
            );

            return true;
        }

        private bool TryFindSpawnCell(Map map, out IntVec3 cell)
        {
            // Chercher une cellule 6x6 valide, loin des structures
            return CellFinder.TryFindRandomCellNear(
                map.Center,
                map,
                Mathf.RoundToInt(Mathf.Min(map.Size.x, map.Size.z) * 0.3f),
                (IntVec3 c) => CanPlaceLairAt(c, map),
                out cell
            );
        }

        private bool CanPlaceLairAt(IntVec3 center, Map map)
        {
            // Vérifier zone 6x6 + 3 cellules de buffer
            CellRect rect = CellRect.CenteredOn(center, 6, 6).ExpandedBy(3);

            if (!rect.InBounds(map))
                return false;

            foreach (IntVec3 c in rect)
            {
                if (!c.InBounds(map))
                    return false;

                if (c.Roofed(map))
                    return false;

                TerrainDef terrain = c.GetTerrain(map);
                if (!terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    return false;

                // Pas trop proche de structures importantes
                if (c.GetEdifice(map) != null)
                    return false;
            }

            return true;
        }
    }
}
```

---

### 2. **Building_InsectLairEntrance_Custom.cs**

```csharp
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace InsectLairIncident
{
    public class Building_InsectLairEntrance_Custom : Building_Portal
    {
        private int lastIncidentTick = -9999;
        private const int IncidentCooldownTicks = 15000; // 6.25 heures

        public override void Tick()
        {
            base.Tick();

            // Pas en dev mode
            if (DebugSettings.godMode)
                return;

            // Cooldown entre incidents
            if (Find.TickManager.TicksGame < lastIncidentTick + IncidentCooldownTicks)
                return;

            // Chance de déclencher incident d'émergence
            if (Rand.MTBEventOccurs(2.5f, GenDate.TicksPerDay, 1000))
            {
                TryTriggerInsectEmergence();
            }
        }

        private void TryTriggerInsectEmergence()
        {
            // Choisir un incident aléatoire
            List<InsectLairIncidentDef> availableIncidents = DefDatabase<InsectLairIncidentDef>
                .AllDefsListForReading
                .Where(def => def.baseChance > 0)
                .ToList();

            if (!availableIncidents.Any())
                return;

            InsectLairIncidentDef chosenIncident = availableIncidents.RandomElementByWeight(def => def.baseChance);

            // Créer l'incident
            IncidentParms parms = new IncidentParms
            {
                target = Map,
                points = StorytellerUtility.DefaultThreatPointsNow(Map),
                forced = true
            };

            // Exécuter
            chosenIncident.Worker.TryExecute(parms);

            lastIncidentTick = Find.TickManager.TicksGame;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastIncidentTick, "lastIncidentTick", -9999);
        }
    }
}
```

---

### 3. **CompSealable_ConditionalQueen.cs**

```csharp
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace InsectLairIncident
{
    public class CompSealable_ConditionalQueen : CompSealable
    {
        public override string CompInspectStringExtra()
        {
            if (!CanSealNow())
            {
                return "CannotCollapseQueenAlive".Translate();
            }

            return base.CompInspectStringExtra();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                // Désactiver le gizmo si la HiveQueen est vivante
                Command command = gizmo as Command;
                if (command != null && !CanSealNow())
                {
                    command.Disable("CannotCollapseQueenAlive".Translate());
                }

                yield return gizmo;
            }
        }

        private bool CanSealNow()
        {
            // Vérifier si la HiveQueen est morte via MapComponent
            MapComponent_InsectLairQueen component = parent.Map.GetComponent<MapComponent_InsectLairQueen>();

            if (component == null)
                return true; // Pas de système de HiveQueen, permettre

            return component.IsQueenDead();
        }
    }

    public class CompProperties_Sealable_ConditionalQueen : CompProperties_Sealable
    {
        public CompProperties_Sealable_ConditionalQueen()
        {
            compClass = typeof(CompSealable_ConditionalQueen);
        }
    }
}
```

---

### 4. **MapComponent_InsectLairQueen.cs**

```csharp
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    public class MapComponent_InsectLairQueen : MapComponent
    {
        private Pawn queen;
        private bool queenSpawned = false;
        private bool queenDead = false;

        public MapComponent_InsectLairQueen(Map map) : base(map)
        {
        }

        public void RegisterQueen(Pawn queenPawn)
        {
            queen = queenPawn;
            queenSpawned = true;
            queenDead = false;
        }

        public bool IsQueenDead()
        {
            if (!queenSpawned)
                return false; // Pas encore spawnée

            if (queenDead)
                return true; // Déjà marquée comme morte

            // Vérifier si la HiveQueen est morte
            if (queen == null || queen.Dead || queen.Destroyed)
            {
                queenDead = true;

                // Message au joueur
                Messages.Message(
                    "HiveQueenDefeated".Translate(),
                    MessageTypeDefOf.PositiveEvent
                );

                return true;
            }

            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref queen, "queen");
            Scribe_Values.Look(ref queenSpawned, "queenSpawned", false);
            Scribe_Values.Look(ref queenDead, "queenDead", false);
        }
    }
}
```

---

### 5. **GenStep_InsectLairWithQueen.cs**

```csharp
using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    public class GenStep_InsectLairWithQueen : GenStep
    {
        public override int SeedPart => 987654321;

        public override void Generate(Map map, GenStepParams parms)
        {
            // Laisser le GenStep_InsectLairCave vanilla faire son travail d'abord
            // Ensuite spawner la HiveQueen

            // Trouver une bonne position pour la HiveQueen (au fond de la cave)
            IntVec3 queenSpot;
            if (!TryFindQueenSpawnSpot(map, out queenSpot))
            {
                Log.Warning("[InsectLairIncident] Could not find spawn spot for HiveQueen");
                return;
            }

            // Spawner la HiveQueen (VANILLA)
            PawnKindDef queenKind = DefDatabase<PawnKindDef>.GetNamed("HiveQueen");
            Pawn queen = PawnGenerator.GeneratePawn(queenKind, Faction.OfInsects);
            GenSpawn.Spawn(queen, queenSpot, map);

            // Enregistrer la HiveQueen dans le MapComponent
            MapComponent_InsectLairQueen component = map.GetComponent<MapComponent_InsectLairQueen>();
            if (component == null)
            {
                component = new MapComponent_InsectLairQueen(map);
                map.components.Add(component);
            }
            component.RegisterQueen(queen);

            // Spawner des gardes autour de la HiveQueen
            SpawnQueenGuards(map, queenSpot);
        }

        private bool TryFindQueenSpawnSpot(Map map, out IntVec3 spot)
        {
            // Chercher loin de la sortie (CaveExit)
            Building caveExit = map.listerBuildings.allBuildingsColonist
                .FirstOrDefault(b => b.def.defName == "CaveExit");

            IntVec3 searchCenter = caveExit != null
                ? caveExit.Position
                : map.Center;

            return CellFinder.TryFindRandomCellNear(
                searchCenter,
                map,
                50, // Distance max de recherche
                (IntVec3 c) =>
                    c.Standable(map) &&
                    !c.Roofed(map) == false && // Doit être sous toit
                    c.GetFirstBuilding(map) == null &&
                    (caveExit == null || c.DistanceTo(caveExit.Position) > 25), // Loin de la sortie
                out spot
            );
        }

        private void SpawnQueenGuards(Map map, IntVec3 queenPos)
        {
            // Spawner 3-5 megaspiders autour de la HiveQueen
            int guardCount = Rand.Range(3, 5);

            for (int i = 0; i < guardCount; i++)
            {
                IntVec3 guardSpot;
                if (CellFinder.TryFindRandomCellNear(queenPos, map, 5,
                    c => c.Standable(map) && c.GetFirstPawn(map) == null,
                    out guardSpot))
                {
                    PawnKindDef guardKind = PawnKindDefOf.Megaspider;
                    Pawn guard = PawnGenerator.GeneratePawn(guardKind, Faction.OfInsects);
                    GenSpawn.Spawn(guard, guardSpot, map);
                }
            }
        }
    }
}
```

---

### 6. **InsectLairIncidentWorker_InsectEmergence.cs**

```csharp
using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    public class InsectLairIncidentWorker_InsectEmergence : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            // Trouver le InsectLairEntrance
            Building_InsectLairEntrance_Custom lair = map.listerBuildings
                .AllBuildingsColonistOfClass<Building_InsectLairEntrance_Custom>()
                .FirstOrDefault();

            if (lair == null)
                return false;

            // Spawner des insectes autour de la lair
            InsectLairIncidentDef def = (InsectLairIncidentDef)this.def;

            int insectCount = Mathf.RoundToInt(parms.points / 100f); // 1 insecte par 100 threat points
            insectCount = Mathf.Clamp(insectCount, 2, 8);

            for (int i = 0; i < insectCount; i++)
            {
                IntVec3 spawnSpot;
                if (CellFinder.TryFindRandomCellNear(
                    lair.Position,
                    map,
                    10,
                    c => c.Standable(map) && c.GetFirstPawn(map) == null,
                    out spawnSpot))
                {
                    PawnKindDef insectKind = ChooseInsectKind();
                    Pawn insect = PawnGenerator.GeneratePawn(insectKind, Faction.OfInsects);
                    GenSpawn.Spawn(insect, spawnSpot, map);

                    // Tunnel effect (comme fleshbeast)
                    insect.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                }
            }

            // Lettre d'avertissement
            Find.LetterStack.ReceiveLetter(
                def.letterLabel,
                def.letterText,
                LetterDefOf.ThreatBig,
                new TargetInfo(lair.Position, map)
            );

            return true;
        }

        private PawnKindDef ChooseInsectKind()
        {
            // 60% Megascarab, 30% Megaspider, 10% Spelopede
            float rand = Rand.Value;

            if (rand < 0.6f)
                return PawnKindDefOf.Megascarab;
            else if (rand < 0.9f)
                return PawnKindDefOf.Megaspider;
            else
                return PawnKindDefOf.Spelopede;
        }
    }
}
```

---

### 7. **InsectLairIncidentDef.cs**

```csharp
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    public class InsectLairIncidentDef : Def
    {
        public float baseChance = 1f;
        public int durationRangeTicks = 600;
        public bool usesThreatPoints = true;
        public int disableEnteringTicks = 600;
        public string disableEnteringReason = "";
        public string letterLabel = "";
        public string letterText = "";

        private IncidentWorker workerInt;

        public IncidentWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (IncidentWorker)System.Activator.CreateInstance(workerClass);
                    workerInt.def = this as IncidentDef; // Cast nécessaire
                }
                return workerInt;
            }
        }

        public System.Type workerClass;
    }
}
```

---

## 📄 XML Definitions

### **Incidents_InsectLair.xml**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <!-- Incident principal: Spawn de la Lair -->
  <IncidentDef>
    <defName>InsectLairSpawn</defName>
    <label>insect lair emergence</label>
    <category>ThreatBig</category>
    <targetTags>
      <li>Map_PlayerHome</li>
    </targetTags>
    <workerClass>InsectLairIncident.IncidentWorker_InsectLairSpawn</workerClass>
    <baseChance>1.5</baseChance>
    <minPopulation>4</minPopulation>
    <earliestDay>15</earliestDay>
    <pointsScaleable>true</pointsScaleable>
    <minThreatPoints>400</minThreatPoints>
  </IncidentDef>

</Defs>
```

### **InsectLairIncidentDefs.xml**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <InsectLairIncidentDef>
    <defName>InsectEmergence</defName>
    <label>insect emergence</label>
    <workerClass>InsectLairIncident.InsectLairIncidentWorker_InsectEmergence</workerClass>
    <baseChance>1</baseChance>
    <durationRangeTicks>600</durationRangeTicks>
    <usesThreatPoints>true</usesThreatPoints>
    <disableEnteringTicks>600</disableEnteringTicks>
    <disableEnteringReason>insects are emerging</disableEnteringReason>
    <letterLabel>Insect emergence</letterLabel>
    <letterText>A swarm of insects is emerging from the underground lair. Prepare for battle!</letterText>
  </InsectLairIncidentDef>

</Defs>
```

### **Building_InsectLairEntrance.xml**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <ThingDef ParentName="BuildingBase">
    <defName>InsectLairEntranceSpawner</defName>
    <label>underground emergence</label>
    <description>Something is emerging from the ground here.</description>
    <thingClass>BuildingGroundSpawner</thingClass>
    <destroyable>false</destroyable>
    <holdsRoof>true</holdsRoof>
    <selectable>true</selectable>
    <tickerType>Normal</tickerType>
    <useHitPoints>false</useHitPoints>
    <drawerType>RealtimeOnly</drawerType>
    <size>(6,6)</size>
    <uiIconPath>UI/Icons/UndergroundEmergence</uiIconPath>
    <building>
      <groundSpawnerSustainedEffecter>EmergencePointSustained6X6</groundSpawnerSustainedEffecter>
      <groundSpawnerCompleteEffecter>EmergencePointComplete6X6</groundSpawnerCompleteEffecter>
      <groundSpawnerThingToSpawn>InsectLairEntrance_Custom</groundSpawnerThingToSpawn>
      <groundSpawnerSpawnDelay>5000~15000</groundSpawnerSpawnDelay>
      <groundSpawnerDestroyAdjacent>true</groundSpawnerDestroyAdjacent>
      <groundSpawnerSustainerSound>PitGateOpening</groundSpawnerSustainerSound>
      <groundSpawnerLetterLabel>Insect lair opened</groundSpawnerLetterLabel>
      <groundSpawnerLetterText>The ground has collapsed, revealing an entrance to an underground insect hive. Insects will periodically emerge from the depths. You must venture inside and defeat the Hive Queen to collapse the entrance.</groundSpawnerLetterText>
    </building>
  </ThingDef>

  <ThingDef ParentName="BuildingBase">
    <defName>InsectLairEntrance_Custom</defName>
    <label>insect lair entrance</label>
    <description>The entrance to a massive underground insect hive. Insects periodically emerge to attack the colony. The entrance cannot be collapsed until the Hive Queen deep within is defeated.</description>
    <size>(6,6)</size>
    <useHitPoints>false</useHitPoints>
    <thingClass>InsectLairIncident.Building_InsectLairEntrance_Custom</thingClass>
    <tickerType>Normal</tickerType>
    <rotatable>false</rotatable>
    <terrainAffordanceNeeded>Heavy</terrainAffordanceNeeded>
    <canOverlapZones>false</canOverlapZones>
    <graphicData>
      <graphicClass>Graphic_Single</graphicClass>
      <texPath>Things/Building/CaveEntranceA</texPath>
      <drawSize>(6,6)</drawSize>
    </graphicData>
    <altitudeLayer>FloorEmplacement</altitudeLayer>
    <fillPercent>0</fillPercent>
    <passability>Impassable</passability>
    <holdsRoof>true</holdsRoof>
    <destroyable>false</destroyable>
    <building>
      <isEdifice>true</isEdifice>
      <deconstructible>false</deconstructible>
      <isTargetable>false</isTargetable>
      <isInert>true</isInert>
      <claimable>false</claimable>
      <expandHomeArea>false</expandHomeArea>
    </building>
    <portal>
      <pocketMapGenerator>InsectLairWithQueen</pocketMapGenerator>
      <exitDef>CaveExit</exitDef>
      <enteredLetterLabel>Insect Hive</enteredLetterLabel>
      <enteredLetterText>You have entered a massive underground insect hive. The tunnels are crawling with megaspiders and other horrors. Deep within, the Hive Queen awaits. Defeat her to collapse the entrance and end the infestation.</enteredLetterText>
      <enteredLetterDef>ThreatBig</enteredLetterDef>
    </portal>
    <statBases>
      <Flammability>0</Flammability>
    </statBases>
    <inspectorTabs>
      <li>ITab_ContentsMapPortal</li>
    </inspectorTabs>
    <comps>
      <li Class="InsectLairIncident.CompProperties_Sealable_ConditionalQueen">
        <sealTexPath>UI/Commands/FillInCaveEntrance</sealTexPath>
        <sealCommandLabel>Collapse insect lair</sealCommandLabel>
        <sealCommandDesc>Choose a colonist to permanently collapse the entrance. Anything or anyone left below will be lost forever.</sealCommandDesc>
        <cannotSealLabel>Cannot collapse (Hive Queen alive)</cannotSealLabel>
        <confirmSealText>Collapsing the insect lair is permanent. Anything or anyone left below will be lost forever.\n\nAre you sure you want to continue?</confirmSealText>
        <destroyPortal>true</destroyPortal>
      </li>
      <li Class="CompProperties_LeaveFilthOnDestroyed">
        <filthDef>Filth_LooseGround</filthDef>
        <thickness>2</thickness>
      </li>
    </comps>
  </ThingDef>

</Defs>
```

---

## 🐛 HiveQueen Vanilla (Odyssey DLC)

### ✅ **Utilise la race vanilla - PAS de custom race nécessaire!**

**PawnKindDef**: `HiveQueen`
**ThingDef**: `HiveQueen`

### Stats:
- **baseHealthScale**: 9.8 (presque 10x un megaspider!)
- **baseBodySize**: 4.5
- **combatPower**: 500
- **Armor**: Blunt 0.27, Sharp 0.22
- **isBoss**: true
- **needsRest**: false (pas besoin de dormir!)
- **manhunterOnDamageChance**: 1.0 (toujours manhunter)
- **Ability**: EggSpew (spawn des œufs)
- **Butcher**: 150 InsectJelly

### Fonctionnalités Automatiques:
- **CompProperties_LetterOnRevealed**: Lettre automatique "You've discovered the center of the nest. It's guarded by a massive insect queen!"
- **Sounds**: Custom sounds (Pawn_HiveQueen_Wounded, Death, Call, Angry)
- **Graphics**: Custom texture 4x size avec dessicated variant

---

## 🎮 Fonctionnalités Complètes

### ✅ Ce qui fonctionne comme PitGate

1. **Spawning aléatoire**: InsectLairEntranceSpawner apparaît sur la map comme PitGateSpawner
2. **Émergence périodique**: Insectes émergent régulièrement (MTB: 2.5 jours)
3. **Système de portail**: Descendre dans la lair souterraine
4. **Rewards**: Toutes les rewards vanilla d'InsectLair (Gravcore, GravlitePanel, ancient crates)

### ✅ Boss System

1. **HiveQueen (Vanilla)**: Boss vanilla avec 9.8x HP, armor, EggSpew ability
2. **CompProperties_LetterOnRevealed**: Lettre automatique quand découverte
3. **Condition de collapse**: Bouton grisé tant que la HiveQueen vit
4. **MapComponent**: Track l'état de la HiveQueen
5. **Message**: "Hive Queen defeated!" quand tuée
6. **Butcher reward**: 150 InsectJelly en plus!

### ✅ Équilibrage

- **Taille de la lair**: 6x6 (même que vanilla InsectLairEntrance)
- **Cooldown émergence**: 6.25 heures entre chaque vague
- **Insectes par vague**: 2-8 selon threat points (60% Megascarab, 30% Megaspider, 10% Spelopede)
- **Combat Power HiveQueen**: 500 (vanilla, équivalent à ~5 Megaspiders)
- **HiveQueen Guards**: 3-5 Megaspiders autour d'elle

---

## 🚀 Prochaines Étapes

1. ✅ Plan mis à jour avec HiveQueen vanilla
2. ⏳ Créer structure de dossiers
3. ⏳ Implémenter classes C#
4. ⏳ Créer XML definitions
5. ⏳ Compiler avec mcs
6. ⏳ Tester en dev quicktest
7. ⏳ Ajuster balance

---

## 📝 Notes Importantes

### Défis Techniques

1. **InsectLairIncidentDef**: Custom def type (comme PitGateIncidentDef)
2. **MapComponent persistence**: Doit survivre aux saves/loads
3. **Portal pocket map**: Réutiliser le generator vanilla mais ajouter la HiveQueen
4. **CompSealable override**: Modifier comportement vanilla

### Compatibilité

- **DLC requis**: Odyssey (pour InsectLair pocket map generator ET HiveQueen vanilla)
- **Version**: 1.5
- **Dépendances**: Aucune (standalone)
- ✅ **HiveQueen**: Utilise la race vanilla du DLC Odyssey (définie dans `/Odyssey/Defs/ThingDefs_Races/Races_Animal_Insect.xml`)

### Avantages d'utiliser HiveQueen Vanilla

✅ **Pas besoin de créer**:
- Custom ThingDef race
- Custom PawnKindDef
- Custom graphics
- Custom sounds
- Custom body parts

✅ **On obtient gratuitement**:
- Lettre automatique quand révélée
- Sounds professionnels
- Graphics 4x size
- EggSpew ability
- 150 InsectJelly reward
- isBoss flag
- Stats parfaitement équilibrés

---

*Plan mis à jour le 2026-01-02 - Utilise HiveQueen Vanilla!*
