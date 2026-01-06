# Research: PitGate (Fleshbeast) & InsectLairEntrance (Insectoid)

## 📋 Overview

Recherche pour le nouveau mod qui combinera les mécaniques de PitGate (Anomaly DLC) et InsectLairEntrance (Odyssey DLC).

---

## 🔴 PIT GATE (FLESHBEAST)

### Localisation
- **Fichier principal**: `/RimWorld/Data/Anomaly/Defs/ThingDefs_Buildings/Buildings_Misc.xml`
- **DLC**: Anomaly

### Définition XML

```xml
<ThingDef ParentName="BuildingNaturalBase">
  <defName>PitGate</defName>
  <label>pit gate</label>
  <description>A massive, foreboding hole that connects the surface with a dark network of underground caves. This portal can spawn various fleshbeast incidents.</description>
  <thingClass>PitGate</thingClass>
  <size>(8,8)</size>
  <tickerType>Normal</tickerType>
```

### Propriétés Clés

- **Taille**: (8,8) - Grande structure
- **Classe C#**: `PitGate`
- **Type**: Portal naturel non destructible
- **Rotation**: Non rotatable
- **Passability**: Impassable
- **Portal Properties**:
  - `pocketMapGenerator`: Undercave
  - `exitDef`: PitGateExit
  - `traverseSound`: Portal_Traverse_Vent

### Incidents Associés

**FleshbeastEmergence** (`PitGateIncidentDefs.xml`):
```xml
<PitGateIncidentDef>
  <defName>FleshbeastEmergence</defName>
  <workerClass>PitGateIncidentWorker_Fleshbeast</workerClass>
  <baseChance>1</baseChance>
  <durationRangeTicks>600</durationRangeTicks> <!-- 10 seconds -->
  <usesThreatPoints>true</usesThreatPoints>
  <letterText>A horde of fleshbeasts is emerging from the pit gate. Get ready!</letterText>
</PitGateIncidentDef>
```

### Map Generator: Undercave

**Fichier**: `/Anomaly/Defs/MapGeneration/UndercaveMapGenerator.xml`

```xml
<MapGeneratorDef Name="Undercave">
  <defName>Undercave</defName>
  <isUnderground>true</isUnderground>
  <forceCaves>true</forceCaves>
  <pocketMapProperties>
    <biome>Undercave</biome>
    <temperature>15</temperature>
  </pocketMapProperties>
  <genSteps>
    <li>Fleshmass</li>
    <li>FleshSacks</li>
    <li>Fleshbulbs</li>
    <li>Dreadmeld</li>
    <li>PlaceCaveExit</li>
    <li>UndercaveInterest</li>
  </genSteps>
</MapGeneratorDef>
```

### Structures Connexes

1. **PitGateSpawner** - Spawns the pit gate on map
2. **PitGateExit** - Exit portal from undercave
3. **PitBurrow** - Smaller 3x3 pit variant
4. **EndlessPit variants** - 2x2c, 3x2c, 3x3c

### Components

```xml
<comps>
  <li Class="CompProperties_AnomalyCollaboratable"/>
  <li Class="CompProperties_Studyable">
    <frequency>Rare</frequency>
    <knowledge>0~0.3</knowledge>
  </li>
  <li Class="CompProperties_TemperatureSource">
    <temperatureCelsius>15</temperatureCelsius>
    <radius>8</radius>
  </li>
</comps>
```

---

## 🟢 INSECT LAIR ENTRANCE (INSECTOID)

### Localisation
- **Fichier principal**: `/RimWorld/Data/Odyssey/Defs/ThingDefs_Buildings/Buildings_Natural.xml`
- **DLC**: Odyssey

### Définition XML

```xml
<ThingDef ParentName="BuildingBase">
  <defName>InsectLairEntrance</defName>
  <label>cave entrance</label>
  <description>The entrance to a cave network. It is possible to climb down into the caves below.</description>
  <thingClass>InsectLairEntrance</thingClass>
  <size>(6,6)</size>
```

### Propriétés Clés

- **Taille**: (6,6) - Structure moyenne
- **Classe C#**: `InsectLairEntrance`
- **Type**: Portal vers lair d'insectes
- **Graphique**: `CaveEntranceA`
- **Portal Properties**:
  - `pocketMapGenerator`: InsectLair
  - `exitDef`: CaveExit

### Map Generator: InsectLair

**Fichier**: `/Odyssey/Defs/MapGeneration/InsectLairMapGenerator.xml`

```xml
<MapGeneratorDef>
  <defName>InsectLair</defName>
  <isUnderground>true</isUnderground>
  <forceCaves>true</forceCaves>
  <pocketMapProperties>
    <biome>Underground</biome>
    <temperature>15</temperature>
  </pocketMapProperties>
  <genSteps>
    <li>PlaceCaveExit</li>
    <li>InsectLair_Cave</li>
    <li>InsectLair_GlowPods</li>
  </genSteps>
</MapGeneratorDef>

<GenStepDef>
  <defName>InsectLair_Cave</defName>
  <genStep Class="GenStep_InsectLairCave" />
</GenStepDef>

<GenStepDef>
  <defName>InsectLair_GlowPods</defName>
  <genStep Class="GenStep_ScatterGroup">
    <things>
      <GlowPod>1</GlowPod>
    </things>
    <validators>
      <li Class="ScattererValidator_TerrainDef">
        <terrainDef>InsectSludge</terrainDef>
      </li>
    </validators>
  </genStep>
</GenStepDef>
```

### Components Spéciaux

```xml
<comps>
  <li Class="CompProperties_Sealable">
    <sealCommandLabel>Collapse cave entrance</sealCommandLabel>
    <sealCommandDesc>Choose a colonist to permanently collapse the entrance. Anything or anyone left below will be lost forever.</sealCommandDesc>
    <confirmSealText>Collapsing the cave entrance is permanent...</confirmSealText>
    <destroyPortal>true</destroyPortal>
  </li>
  <li Class="CompProperties_LeaveFilthOnDestroyed">
    <filthDef>Filth_LooseGround</filthDef>
    <thickness>2</thickness>
  </li>
</comps>
```

### Structures Connexes

1. **BurrowWall** - Walls made of insect secretion (linked texture atlas)
2. **CaveExit** - 3x3 rope line with light shaft effect
3. **GlowPod** - Lighting on InsectSludge terrain

---

## 🔄 CAVE EXIT (Commun aux deux)

### Localisation
- **Fichier**: `/Core/Defs/ThingDefs_Buildings/Buildings_Misc.xml`

### Définition

```xml
<ThingDef Name="CaveExit" ParentName="PocketMapExit">
  <defName>CaveExit</defName>
  <label>cave exit</label>
  <description>A rope line which links to the surface above.</description>
  <size>(3,3)</size>
  <thingClass>CaveExit</thingClass>
  <comps>
    <li Class="CompProperties_Glower">
      <glowRadius>10</glowRadius>
      <glowColor>(140,160,184,0)</glowColor>
    </li>
    <li Class="CompProperties_Effecter">
      <effecterDef>UndercaveMapExitLightshafts</effecterDef>
    </li>
  </comps>
</ThingDef>
```

---

## 📊 Comparaison Technique

| Propriété | PitGate | InsectLairEntrance |
|-----------|---------|-------------------|
| **Taille** | (8,8) | (6,6) |
| **Classe C#** | PitGate | InsectLairEntrance |
| **Pocket Map** | Undercave | InsectLair |
| **Biome** | Undercave (Anomaly) | Underground |
| **Température** | 15°C | 15°C |
| **Incident System** | ✅ PitGateIncidentDef | ❌ None |
| **Sealable** | ❌ No | ✅ Yes (CompProperties_Sealable) |
| **Studyable** | ✅ Yes | ❌ No |
| **Collaboratable** | ✅ Yes | ❌ No |
| **Temperature Source** | ✅ Yes (8 radius) | ❌ No |

---

## 🎯 Classes C# à Analyser

### PitGate System
- `PitGate` - Main building class
- `PitGateIncidentWorker_Fleshbeast` - Spawn logic
- `IncidentWorker_PitGate` - Incident trigger
- `GenStep_Fleshmass` - Fleshmass generation
- `GenStep_FleshSacks` - Threat point-based spawning
- `GenStep_UndercaveInterest` - POI generation

### InsectLair System
- `InsectLairEntrance` - Main entrance class
- `CaveExit` - Exit rope
- `GenStep_InsectLairCave` - Cave structure generation
- `SitePartWorker_InsectLair` - World map site
- `CompSealable` - Collapse mechanic

---

## 💡 Idées pour le Nouveau Mod

### Option 1: Portal Hybride
- Créer un portal unique qui peut générer **soit** Undercave **soit** InsectLair
- Taille intermédiaire (7x7)
- Incident system pour les deux types

### Option 2: Dual Portal System
- Deux portals distincts qui peuvent se spawner ensemble
- PitGate pour fleshbeasts + Cave entrance pour insects
- Interactions entre les deux systèmes

### Option 3: Unified Underground Network
- Un seul type de portal
- Map generator qui mélange Fleshmass + InsectLair
- Nouveau biome "Corrupted Underground"

---

## 📝 Prochaines Étapes

1. ✅ Recherche structures de base (COMPLÉTÉ)
2. ⏳ Décider de l'approche du mod (Option 1/2/3)
3. ⏳ Analyser les classes C# dans Assembly-CSharp.dll
4. ⏳ Créer structure de dossiers pour nouveau mod
5. ⏳ Définir les XML de base
6. ⏳ Implémenter les classes C# custom

---

*Recherche effectuée le 2026-01-02*
