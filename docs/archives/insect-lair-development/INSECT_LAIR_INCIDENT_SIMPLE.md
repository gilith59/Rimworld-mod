# 🐛 Insect Lair Incident - Plan Simplifié (Option 2)

## 🎯 Objectif

Transformer le vanilla `InsectLairEntrance` en incident aléatoire qui spawn sur la map, avec HiveQueen boss obligatoire pour collapse.

---

## ✅ Ce qui reste 100% Vanilla

1. **InsectLairEntrance** - Building portal (on le spawne juste différemment)
2. **MapGeneratorDef InsectLair** - Cave generation complète
3. **GenStep_InsectLairCave** - Tunnels et hives
4. **GlowPods, InsectSludge** - Décor
5. **CaveExit** - Sortie
6. **Rewards** - Gravcore, GravlitePanel, ancient crates
7. **HiveQueen** - Boss vanilla

---

## 🔧 Ce qu'on ajoute (3 fichiers C#, 3 fichiers XML)

### Structure

```
RimWorld/Mods/InsectLairIncident/
├── About/
│   └── About.xml
├── 1.5/
│   ├── Assemblies/
│   │   └── InsectLairIncident.dll
│   └── Defs/
│       ├── IncidentDefs/
│       │   └── Incident_InsectLair.xml
│       ├── GenStepDefs/
│       │   └── GenStep_HiveQueen.xml
│       └── ThingDefs_Buildings/
│           └── Buildings_InsectLair.xml (patch sealable)
└── Source/
    ├── IncidentWorker_InsectLairSpawn.cs
    ├── GenStep_SpawnHiveQueen.cs
    └── MapComponent_HiveQueenTracker.cs
```

---

## 📄 Fichier 1: IncidentWorker_InsectLairSpawn.cs

```csharp
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    // Spawne le vanilla InsectLairEntrance comme incident
    public class IncidentWorker_InsectLairSpawn : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            // Chercher emplacement 6x6
            if (!TryFindSpawnCell(map, out IntVec3 cell))
                return false;

            // Spawner le vanilla InsectLairEntrance directement
            ThingDef entranceDef = ThingDef.Named("InsectLairEntrance");
            Thing entrance = ThingMaker.MakeThing(entranceDef);
            GenSpawn.Spawn(entrance, cell, map);

            // Lettre
            Find.LetterStack.ReceiveLetter(
                "Insect Lair Emergence",
                "An insect lair has opened! Insects infest the caves below. Defeat the Hive Queen to seal it.",
                LetterDefOf.ThreatBig,
                new TargetInfo(cell, map)
            );

            return true;
        }

        private bool TryFindSpawnCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomCellNear(
                map.Center,
                map,
                Mathf.RoundToInt(map.Size.x * 0.3f),
                c => CanPlaceAt(c, map),
                out cell
            );
        }

        private bool CanPlaceAt(IntVec3 center, Map map)
        {
            CellRect rect = CellRect.CenteredOn(center, 6, 6).ExpandedBy(2);

            foreach (IntVec3 c in rect)
            {
                if (!c.InBounds(map) || c.Roofed(map))
                    return false;

                TerrainDef terrain = c.GetTerrain(map);
                if (!terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    return false;
            }

            return true;
        }
    }
}
```

---

## 📄 Fichier 2: GenStep_SpawnHiveQueen.cs

```csharp
using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    // Ajoute la HiveQueen dans la cave vanilla
    public class GenStep_SpawnHiveQueen : GenStep
    {
        public override int SeedPart => 123456789;

        public override void Generate(Map map, GenStepParams parms)
        {
            // Trouver position éloignée du CaveExit
            if (!TryFindQueenSpot(map, out IntVec3 spot))
                return;

            // Spawner HiveQueen vanilla
            PawnKindDef queenKind = PawnKindDef.Named("HiveQueen");
            Pawn queen = PawnGenerator.GeneratePawn(queenKind, Faction.OfInsects);
            GenSpawn.Spawn(queen, spot, map);

            // Enregistrer dans MapComponent
            MapComponent_HiveQueenTracker tracker = map.GetComponent<MapComponent_HiveQueenTracker>();
            if (tracker == null)
            {
                tracker = new MapComponent_HiveQueenTracker(map);
                map.components.Add(tracker);
            }
            tracker.RegisterQueen(queen);

            // Spawner gardes
            for (int i = 0; i < Rand.Range(3, 6); i++)
            {
                if (CellFinder.TryRandomClosewalkCellNear(spot, map, 5, out IntVec3 guardSpot))
                {
                    Pawn guard = PawnGenerator.GeneratePawn(PawnKindDefOf.Megaspider, Faction.OfInsects);
                    GenSpawn.Spawn(guard, guardSpot, map);
                }
            }
        }

        private bool TryFindQueenSpot(Map map, out IntVec3 spot)
        {
            // Chercher loin du CaveExit
            Building exit = map.listerBuildings.allBuildingsColonist
                .FirstOrDefault(b => b.def.defName == "CaveExit");

            IntVec3 center = exit?.Position ?? map.Center;

            return CellFinder.TryFindRandomCellNear(
                center,
                map,
                40,
                c => c.Standable(map) &&
                     c.Roofed(map) &&
                     (exit == null || c.DistanceTo(exit.Position) > 20),
                out spot
            );
        }
    }
}
```

---

## 📄 Fichier 3: MapComponent_HiveQueenTracker.cs

```csharp
using RimWorld;
using Verse;

namespace InsectLairIncident
{
    // Track si HiveQueen est morte pour permettre collapse
    public class MapComponent_HiveQueenTracker : MapComponent
    {
        private Pawn queen;
        private bool queenDead = false;

        public MapComponent_HiveQueenTracker(Map map) : base(map)
        {
        }

        public void RegisterQueen(Pawn pawn)
        {
            queen = pawn;
        }

        public bool IsQueenDead()
        {
            if (queen == null)
                return true; // Pas de queen = peut collapse

            if (queenDead)
                return true;

            if (queen.Dead || queen.Destroyed)
            {
                queenDead = true;
                Messages.Message(
                    "The Hive Queen has been defeated! The insect lair can now be collapsed.",
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
            Scribe_Values.Look(ref queenDead, "queenDead");
        }
    }
}
```

---

## 📄 XML 1: Incident_InsectLair.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <IncidentDef>
    <defName>InsectLairEmergence</defName>
    <label>insect lair emergence</label>
    <category>ThreatBig</category>
    <targetTags>
      <li>Map_PlayerHome</li>
    </targetTags>
    <workerClass>InsectLairIncident.IncidentWorker_InsectLairSpawn</workerClass>
    <baseChance>1.5</baseChance>
    <minPopulation>4</minPopulation>
    <earliestDay>20</earliestDay>
    <minThreatPoints>500</minThreatPoints>
  </IncidentDef>

</Defs>
```

---

## 📄 XML 2: GenStep_HiveQueen.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <!-- Ajoute notre GenStep au MapGeneratorDef vanilla -->
  <MapGeneratorDef>
    <defName>InsectLair</defName>
    <genSteps>
      <li>InsectLair_SpawnHiveQueen</li>
    </genSteps>
  </MapGeneratorDef>

  <GenStepDef>
    <defName>InsectLair_SpawnHiveQueen</defName>
    <order>553</order> <!-- Après InsectLair_GlowPods (552) -->
    <genStep Class="InsectLairIncident.GenStep_SpawnHiveQueen" />
  </GenStepDef>

</Defs>
```

---

## 📄 XML 3: Buildings_InsectLair.xml (Patch CompSealable)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>

  <!-- Patch le vanilla InsectLairEntrance -->
  <ThingDef>
    <defName>InsectLairEntrance</defName>
    <comps>
      <!-- Remplacer CompSealable vanilla par notre version conditionnelle -->
      <li Class="InsectLairIncident.CompProperties_Sealable_Conditional">
        <sealTexPath>UI/Commands/FillInCaveEntrance</sealTexPath>
        <sealCommandLabel>Collapse cave entrance</sealCommandLabel>
        <sealCommandDesc>Choose a colonist to permanently collapse the entrance. The Hive Queen must be defeated first.</sealCommandDesc>
        <cannotSealLabel>Cannot collapse (Hive Queen alive)</cannotSealLabel>
        <confirmSealText>Collapsing the cave entrance is permanent. Anything or anyone left below will be lost forever.\n\nAre you sure?</confirmSealText>
        <destroyPortal>true</destroyPortal>
      </li>
    </comps>
  </ThingDef>

</Defs>
```

**CompSealable_Conditional.cs** (mini fichier):

```csharp
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace InsectLairIncident
{
    public class CompSealable_Conditional : CompSealable
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                Command cmd = gizmo as Command;
                if (cmd != null)
                {
                    // Vérifier si HiveQueen morte
                    MapComponent_HiveQueenTracker tracker = parent.Map?.GetComponent<MapComponent_HiveQueenTracker>();
                    if (tracker != null && !tracker.IsQueenDead())
                    {
                        cmd.Disable("Hive Queen must be defeated first");
                    }
                }
                yield return gizmo;
            }
        }
    }

    public class CompProperties_Sealable_Conditional : CompProperties_Sealable
    {
        public CompProperties_Sealable_Conditional()
        {
            compClass = typeof(CompSealable_Conditional);
        }
    }
}
```

---

## 📊 Total

**4 fichiers C#** (~200 lignes):
1. IncidentWorker_InsectLairSpawn.cs
2. GenStep_SpawnHiveQueen.cs
3. MapComponent_HiveQueenTracker.cs
4. CompSealable_Conditional.cs

**3 fichiers XML** (~50 lignes):
1. Incident_InsectLair.xml
2. GenStep_HiveQueen.xml
3. Buildings_InsectLair.xml

---

## ✅ Résultat

- ✅ InsectLairEntrance vanilla spawn comme incident
- ✅ Cave vanilla générée avec tous ses rewards
- ✅ HiveQueen vanilla spawn au fond
- ✅ Collapse bloqué tant que HiveQueen vit
- ✅ **RIEN d'autre ne change!**

---

*Plan ultra-simplifié - Option 2*
