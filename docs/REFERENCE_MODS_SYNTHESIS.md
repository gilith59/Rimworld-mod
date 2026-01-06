# 📚 Synthèse des Mods de Référence pour Decadents

**Date:** 2026-01-06
**But:** Analyser les mods de référence pour identifier des systèmes réutilisables/inspirants

---

## 🎯 Mods Analysés

1. **AlphaGenes** - Système de ressources custom
2. **VRE Sanguophage** - Système DeathrestBindable + Draincasket
3. **VanillaExpandedFramework** - Framework général (Comps, Hediffs)
4. **VanillaRacesExpanded-Android** - Système de power/reactor
5. **VFE-Insectoids2** - Système de genelines

---

## 1️⃣ AlphaGenes - Custom Gene Resources

### 📍 Location
`/home/gilith/Rimworld mod/references/AlphaGenes/`

### 🔑 Fonctionnalités Intéressantes

#### A) Système de Ressource Metal
**Fichier:** `1.5/Source/AlphaGenes/AlphaGenes/GeneResource/Gene_Resource_Metal.cs`

**Concept:**
- Les pawns avec le gène "Metal Eater" ont une ressource "Metal" (comme Hemogen)
- Doivent consommer du métal (steel, plasteel, etc.) pour survivre
- Calcul basé sur la **masse** du métal consommé
- Support métabolisme (biostatMet affecte la consommation)

**Code Clé:**
```csharp
public class Gene_Resource_Metal : Gene_Resource, IGeneResourceDrain
{
    // Interface pour les ressources qui se drainent
    public Gene_Resource Resource => this;
    public bool CanOffset => pawn.Spawned && Active;
    public float ResourceLossPerDay => def.resourceLossPerDay;

    // Calcul masse désirée basé sur resource manquante
    public float MassDesired
    {
        get
        {
            float diff = targetValue - Value;
            if(cachedEffiency != null)
            {
                diff *= cachedEffiency.EffiencyFactor;
            }
            return diff * 10f; // *10 pour mass
        }
    }

    // Restauration basée sur masse du métal
    public float GetResourceRestore(Thing thing)
    {
        float mass = 0f;
        // Peut manger objets forgés en métal !
        if (!thing.def.IsStuff)
        {
            if (thing.Stuff?.IsMetal ?? false)
            {
                mass += thing.Stuff.statBases.First(x => x.stat == StatDefOf.Mass).value
                       * thing.def.CostStuffCount;
            }
            else if(thing.def.CostList != null)
            {
                foreach (var resource in thing.def.CostList)
                {
                    if (resource.thingDef.IsMetal)
                    {
                        mass += resource.thingDef.statBases.First(x => x.stat == StatDefOf.Mass).value
                               * resource.count;
                    }
                }
            }
        }
        else if (thing.def.IsMetal)
        {
            mass = thing.def.statBases.First(x => x.stat == StatDefOf.Mass).value;
        }

        float resourceRestore = mass / 10f; // Diviser par 10 pour "nutrition"
        if (cachedEffiency != null)
        {
            resourceRestore /= cachedEffiency.EffiencyFactor;
        }
        return resourceRestore;
    }

    // Tick pour drain
    public override void Tick()
    {
        base.Tick();
        if (CanOffset)
        {
            if (pawn.IsHashIntervalTick(300)) // Hash ticks
            {
                float loss = (-ResourceLossPerDay / 200f); // 200 hash ticks/jour

                // Métabolisme affecte perte
                int biostatMet = 0;
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (!gene.Overridden)
                        biostatMet += gene.def.biostatMet;
                }
                loss *= GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate((float)biostatMet);

                Value += loss;

                // Ajouter hediff craving si 0
                if (before > 0f && Value <= 0f)
                {
                    if (!pawn.health.hediffSet.HasHediff(InternalDefOf.AG_MineralCraving))
                    {
                        pawn.health.AddHediff(InternalDefOf.AG_MineralCraving);
                    }
                }
            }
        }
    }
}
```

**Leçons pour Decadents:**
- ✅ `IGeneResourceDrain` interface pour standardiser les ressources
- ✅ Support du métabolisme (biostatMet) pour drain
- ✅ `IsHashIntervalTick(300)` pour optimisation
- ✅ Hediff de craving quand ressource à 0
- ✅ Système d'efficacité (`Gene_Resource_MineralEffiency`)

#### B) Interface IGeneResourceDrain
```csharp
public interface IGeneResourceDrain
{
    Gene_Resource Resource { get; }
    bool CanOffset { get; }
    float ResourceLossPerDay { get; }
    Pawn Pawn { get; }
    string DisplayLabel { get; }
    bool ShouldConsumeNow();
}
```

**Utilité:** Standardiser toutes nos ressources (Beergen, Stimgen, Dreamgen)

#### C) Gizmo Dev Utilities
**Fichier:** `GeneResourceDrainUtility.cs` (probablement)

Utilitaires pour ajouter des gizmos de dev (fill resource, drain resource, etc.)

### 💡 Application à Decadents

**Pour Beergen/Stimgen/Dreamgen:**
1. Créer interface `IDecadentResource` similaire à `IGeneResourceDrain`
2. Chaque ressource hérite de `Gene_Hemogen` + implémente interface
3. Support métabolisme pour drain variable
4. Hediff craving automatique à 0
5. Système d'efficacité optionnel (gène qui augmente gain)

**Exemple:**
```csharp
public interface IDecadentResource
{
    Gene_Resource Resource { get; }
    bool CanOffset { get; }
    float ResourceLossPerDay { get; }
    Pawn Pawn { get; }
    string DisplayLabel { get; }
    string ResourceDefName { get; } // "Beergen", "Stimgen", "Dreamgen"
}

public class Gene_Beergenic : Gene_Hemogen, IDecadentResource
{
    public Gene_Resource Resource => this;
    public bool CanOffset => pawn.Spawned && Active;
    // ... etc
}
```

---

## 2️⃣ VRE Sanguophage - DeathrestBindable System

### 📍 Location
`/home/gilith/Rimworld mod/RimWorld/Mods/VanillaRacesExpanded-Sanguophage/`

### 🔑 Fonctionnalités Intéressantes

#### A) Bâtiments Connectables
**Fichier:** `1.6/Defs/ThingDefs_Buildings/Buildings_Deathrest.xml`

**Exemple: Invocation Matrix**
```xml
<ThingDef ParentName="DeathrestBuildingBase">
    <defName>VRE_InvocationMatrix</defName>
    <thingClass>VanillaRacesExpandedSanguophage.Building_Deathrest_Extender</thingClass>
    <comps>
        <li Class="CompProperties_DeathrestBindable">
            <soundStart>PsychofluidPump_Start</soundStart>
            <soundEnd>PsychofluidPump_Stop</soundEnd>
            <soundWorking>PsychofluidPump_Ambience</soundWorking>
        </li>
        <li Class="CompProperties_Power">
            <basePowerConsumption>200</basePowerConsumption>
        </li>
        <li Class="CompProperties_Refuelable">
            <fuelConsumptionRate>0.5</fuelConsumptionRate>
            <fuelCapacity>5</fuelCapacity>
            <fuelLabel>Hemogen</fuelLabel>
            <fuelFilter>
                <thingDefs><li>HemogenPack</li></thingDefs>
            </fuelFilter>
        </li>
    </comps>
</ThingDef>
```

**Exemple: Hemodynamic Accelerator**
```xml
<li Class="CompProperties_DeathrestBindable">
    <stackLimit>2</stackLimit>
    <deathrestEffectivenessFactor>1.3</deathrestEffectivenessFactor>
    <displayTimeActive>false</displayTimeActive>
    <soundWorking>DeathrestAccelerator_Ambience</soundWorking>
</li>
```

**Exemple: Hemogen Solidifier**
```xml
<li Class="CompProperties_DeathrestBindable">
    <stackLimit>4</stackLimit>
    <hediffToApply>VRE_SolidifiedHemogen</hediffToApply>
    <soundStart>PsychofluidPump_Start</soundStart>
    <soundEnd>PsychofluidPump_Stop</soundEnd>
</li>
```

**Exemple: Small Hemogen Amplifier**
```xml
<li Class="CompProperties_DeathrestBindable">
    <hediffToApply>VRE_HemogenAmplifiedWeak</hediffToApply>
</li>
```

**Exemple: Small Hemopump**
```xml
<li Class="CompProperties_DeathrestBindable">
    <hemogenLimitOffset>0.15</hemogenLimitOffset>
    <soundStart>Hemopump_Start</soundStart>
</li>
```

#### B) Building_Deathrest_Extender
**Fichier:** `1.6/Source/.../Buildings/Building_Deathrest_Extender.cs`

```csharp
public class Building_Deathrest_Extender: Building
{
    public AbilityDef abilitySelected = InternalDefOf.VRE_Coagulate_SingleUse;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref abilitySelected, "abilitySelected");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo g in base.GetGizmos())
        {
            yield return g;
        }

        // Gizmo pour sélectionner ability
        yield return new Command_SingleUseAbilities(this)
        {
            icon = ContentFinder<Texture2D>.Get(abilitySelected.iconPath, false),
            defaultDesc = "VRE_DefaultAbility".Translate(),
            hotKey = KeyBindingDefOf.Misc1,
            building = this
        };
    }
}
```

### 💡 Application à Decadents

**Problème:** `CompProperties_DeathrestBindable` est vanilla et nécessite `Gene_Deathrest`

**Solutions:**

**Option A: Créer CompDecadentBindable custom** (Recommandé)
```csharp
public class CompProperties_DecadentBindable : CompProperties
{
    // Application de hediff
    public HediffDef hediffToApply;

    // Bonus vitesse de repos
    public float restEffectivenessFactor = 1f;

    // Augmentation max resource
    public float resourceLimitOffset = 0f;

    // Stack limit
    public int stackLimit = 1;

    // Sons
    public SoundDef soundStart;
    public SoundDef soundEnd;
    public SoundDef soundWorking;

    public CompProperties_DecadentBindable()
    {
        compClass = typeof(CompDecadentBindable);
    }
}

public class CompDecadentBindable : ThingComp
{
    private CompProperties_DecadentBindable Props => (CompProperties_DecadentBindable)props;

    // Logique de connexion
    // Application hediff au réveil
    // Gestion stack limit
}
```

**Option B: Étendre nos Caskets pour supporter vanilla DeathrestBindable**
- Moins flexible
- Nécessite pawn avec Gene_Deathrest
- Pas recommandé

#### C) CompDraincasket (VRE Sanguophage)
**Fichier:** `Source/.../Comps/CompDraincasket.cs`

**Concept:** Casket avancé qui contient des pawns et consomme nutrition

```csharp
public class CompDraincasket : CompRefuelable, IThingHolderWithDrawnPawn, ISuspendableThingHolder
{
    public ThingOwner innerContainer;
    public StorageSettings allowedNutritionSettings;
    public float nutritionConsumptionRate = 1f;
    public bool pawnStarving = false;
    public float starvingCounter = 0;

    public Pawn Occupant => innerContainer.OfType<Pawn>().FirstOrDefault();

    // Consomme fuel par tick
    public override void CompTickInterval(int delta)
    {
        var occupant = Occupant;
        if ((flickComp == null || flickComp.SwitchIsOn) && occupant != null)
        {
            ConsumeFuel(ConsumptionRatePerTick * nutritionConsumptionRate * delta);
        }

        // Génère hemogen toutes les heures si fuel > 0
        if (parent.IsHashIntervalTick(60000, delta))
        {
            if (occupant != null && Fuel > 0)
            {
                compResourceHemogen?.PipeNet.DistributeAmongStorage(
                    VanillaRacesExpandedSanguophage_Settings.drainCasketAmount * nutritionConsumptionRate
                );
            }
        }

        // Check starving
        if (parent.IsHashIntervalTick(6000, delta))
        {
            if (Fuel > 0)
            {
                pawnStarving = false;
                starvingCounter = 0;
            }
            else
            {
                pawnStarving = true;
            }
        }

        // Kill si starving trop longtemps
        if (pawnStarving)
        {
            starvingCounter += delta * nutritionConsumptionRate;
            if (Fuel == 0 && starvingCounter > starvingMaxTicks)
            {
                EjectAndKillContents(parent.Map);
            }
        }
    }

    public bool TryAcceptPawn(Pawn pawn)
    {
        pawnStarving = false;
        starvingCounter = 0;
        RecachePawnData(pawn);
        innerContainer.ClearAndDestroyContents();

        var num = pawn.DeSpawnOrDeselect();
        if (pawn.holdingOwner != null)
        {
            pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
        }
        else
        {
            innerContainer.TryAdd(pawn);
        }

        return true;
    }

    // Dessine pawn dans casket
    public override void PostDraw()
    {
        if (Occupant is null) return;
        var drawLoc = parent.DrawPos;
        drawLoc.y += 10;
        Occupant.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc, null, neverAimWeapon: true);
    }
}
```

**Leçons pour Decadents:**
- ✅ `ThingOwner innerContainer` pour stocker pawn
- ✅ `IThingHolderWithDrawnPawn` interface pour dessiner pawn
- ✅ `ISuspendableThingHolder` pour suspendre besoins
- ✅ Nutrition consumption rate variable par pawn (métabolisme)
- ✅ System de starving avec timer
- ✅ `PostDraw()` pour afficher pawn dans lit

**Différence avec nos Caskets:**
- VRE: Pawn DANS le casket (innerContainer)
- Nous: Pawn COUCHÉ SUR le lit (Building_Bed normal)
- Les deux approches sont valides!

---

## 3️⃣ VanillaExpandedFramework - General Utilities

### 📍 Location
`/home/gilith/Rimworld mod/references/vanilla-expanded-source/VanillaExpandedFramework/`

### 🔑 Fonctionnalités Intéressantes

#### A) CompApparelHediffs
**Fichier:** `Source/VEF/Apparels/Comps/CompApparelHediffs.cs`

**Concept:** Applique automatiquement des hediffs quand apparel est porté

```csharp
public class CompApparelHediffs : ThingComp
{
    private CompProperties_ApparelHediffs Props => (CompProperties_ApparelHediffs)props;

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        if (Props.hediffDef != null)
        {
            pawn.health.AddHediff(Props.hediffDef);
        }
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
        if (hediff != null)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }
}
```

**Application à Decadents:** Similaire pour nos bâtiments connectés

#### B) CompHediffGiver
**Fichier:** `Source/VEF/Buildings/Comps/CompHediffGiver.cs`

**Concept:** Comp qui donne hediff dans un rayon (AoE hediff giver)

```csharp
public class CompProperties_HediffGiver : CompProperties
{
    public HediffDef hediffDef;
    public float severityIncrease = 0f;
    public float radius;
    public List<StatDef> stats; // Stats qui modifient severity
    public int tickRate = 500;
}

public class CompHediffGiver : ThingComp
{
    public override void CompTickInterval(int delta)
    {
        if (!parent.IsHashIntervalTick(Props.tickRate, delta) || !parent.Spawned)
            return;

        IReadOnlyList<Pawn> pawnList = parent.Map.mapPawns.AllPawnsSpawned;

        for (int i = pawnList.Count - 1; i >= 0; i--)
        {
            Pawn pawn = pawnList[i];
            if (pawn.Position.DistanceToSquared(parent.Position) > Props.radius * Props.radius)
                continue;

            float adjustedSeverity = Props.severityIncrease;

            // Modifier par stats du pawn
            if (!Props.stats.NullOrEmpty())
            {
                for (int j = 0; j < Props.stats.Count; j++)
                {
                    adjustedSeverity *= pawn.GetStatValue(Props.stats[j]);
                }
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                hediff.Severity += adjustedSeverity;
            }
            else
            {
                hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                hediff.Severity = adjustedSeverity;
                pawn.health.AddHediff(hediff);
            }
        }
    }
}
```

**Application à Decadents:** Potentiellement pour effets passifs des fûts

#### C) CompRefuelable_DualFuel
**Fichier:** `Source/VEF/Buildings/Comps/CompRefuelable_DualFuel.cs`

**Concept:** Building avec DEUX types de fuel (primaire + secondaire)

```csharp
public class CompProperties_Refuelable_DualFuel : CompProperties_Refuelable
{
    public float secondaryFuelCapacity = 10f;
    public float initialSecondaryFuelPercent = 0f;
    public float autoRefuelSecondaryPercent = 0.3f;
    public ThingFilter secondaryFuelFilter;
    public string secondaryFuelLabel;

    // Facteur difficulty
    public bool factorSecondaryByDifficulty = false;
}

public class CompRefuelable_DualFuel : CompRefuelable
{
    private float secondaryFuel;
    private float configuredSecondaryTargetFuelLevel = -1f;
    public bool allowAutoRefuelSecondary = true;

    public float SecondaryFuel => secondaryFuel;
    public float SecondaryFuelPercentOfMax => secondaryFuel / Props.secondaryFuelCapacity;
    public bool HasSecondaryFuel => secondaryFuel > 0f;

    public void ConsumeSecondaryFuel(float amount)
    {
        if (secondaryFuel <= 0f) return;
        secondaryFuel -= amount;
        if (secondaryFuel <= 0f) secondaryFuel = 0f;
    }

    public void RefuelSecondary(float amount)
    {
        secondaryFuel += amount * Props.SecondaryFuelMultiplierCurrentDifficulty;
        if (secondaryFuel > Props.secondaryFuelCapacity)
            secondaryFuel = Props.secondaryFuelCapacity;
    }
}
```

**Application à Decadents:** Si on veut des fûts avec 2 types de drugs (ex: Baron avec wake-up + go-juice)

#### D) Pattern: Comp + CompProperties
**Standard RimWorld pour composants custom:**

```csharp
// 1. Properties (définition XML)
public class CompProperties_MyComp : CompProperties
{
    public float someValue;
    public HediffDef hediff;

    public CompProperties_MyComp()
    {
        compClass = typeof(CompMyComp);
    }
}

// 2. Comp (logique C#)
public class CompMyComp : ThingComp
{
    private CompProperties_MyComp Props => (CompProperties_MyComp)props;

    public override void CompTick()
    {
        base.CompTick();
        // Logique...
    }
}

// 3. XML Usage
/*
<comps>
    <li Class="MyNamespace.CompProperties_MyComp">
        <someValue>1.5</someValue>
        <hediff>MyHediff</hediff>
    </li>
</comps>
*/
```

---

## 4️⃣ VanillaRacesExpanded-Android - Power System

### 📍 Location
`/home/gilith/Rimworld mod/references/vanilla-expanded-source/VanillaRacesExpanded-Android/`

### 🔑 Fonctionnalités Intéressantes

#### A) Gene_SyntheticBody
**Fichier:** `1.6/Source/Genes/Gene_SyntheticBody.cs`

**Concept:** Gène principal des androids, gère awakening system

```csharp
public class Gene_SyntheticBody : Gene
{
    public bool autoRepair = true;

    public override void PostAdd()
    {
        base.PostAdd();
        // Ajoute hediffs à toutes les body parts
        foreach (var bodyPart in this.pawn.def.race.body.AllParts.OrderByDescending(x => x.Index))
        {
            var hediffDef = bodyPart.def.GetAndroidCounterPart();
            if (hediffDef != null && this.pawn.health.hediffSet.GetNotMissingParts().Contains(bodyPart))
            {
                var hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart);
                pawn.health.hediffSet.AddDirect(hediff);
            }
        }
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        // Check awakening basé sur mood
        if (pawn.IsHashIntervalTick(GenDate.TicksPerHour, delta) && !pawn.IsAwakened() && Rand.Chance(0.5f))
        {
            if (pawn.needs.mood.CurLevel <= 0.05f)
            {
                Awaken("Title", "Low mood description");
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            }
            if (pawn.needs.mood.CurLevel >= 0.8f)
            {
                Awaken("Title", "High mood description");
                // Donne inspiration
            }
        }
    }

    public void Awaken(TaggedString title, TaggedString description)
    {
        // Génère traits
        PawnGenerator.GenerateTraits(pawn, new PawnGenerationRequest(...));

        // Remove gènes "awakening only"
        foreach (var gene in pawn.genes.GenesListForReading.ToList())
        {
            if (gene.def is AndroidGeneDef geneDef && geneDef.removeWhenAwakened)
            {
                pawn.genes.RemoveGene(gene);
            }
        }

        // Effets visuels
        MoteMaker.MakeColonistActionOverlay(pawn, VREA_DefOf.VREA_AndroidAwakenedMote);
        pawn.needs.AddOrRemoveNeedsAsAppropriate();
    }
}
```

**Leçons pour Decadents:**
- ✅ `PostAdd()` pour setup initial du gène
- ✅ Système de transformation/awakening basé sur conditions
- ✅ Removal automatique de gènes après trigger
- ✅ `pawn.genes.RemoveGene()` pour retirer dynamiquement
- ✅ Hash interval check pour events rares

#### B) Building_AndroidSleepMode
**Fichier:** `1.6/Source/Buildings/Building_AndroidSleepMode.cs`

**Concept:** Building "virtuel" qui représente un android en sleep mode

```csharp
public class Building_AndroidSleepMode : Building
{
    public Pawn android;

    public override string Label => android.Label + " (" + "VREA.SleepMode".Translate().ToLower() + ")";

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        android.DynamicDrawPhaseAt(DrawPhase.Draw, DrawPos);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            defaultLabel = "VREA.SleepModeOff".Translate(),
            defaultDesc = "VREA.SleepModeOffDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/SleepMode_WakeUp"),
            action = delegate
            {
                GenSpawn.Spawn(android, Position, Map);
                Find.Selector.Select(android);
                this.Destroy();
            }
        };
    }
}
```

**Leçons pour Decadents:**
- ✅ Building qui "contient" un pawn sans ThingOwner
- ✅ Override Label pour custom name
- ✅ Gizmo pour "réveiller" = spawn pawn + destroy building
- ✅ Pattern pour "stasis" states

**Application à Decadents:** Pourrait être utile pour un état de "torpeur" si overdose de drogue?

---

## 5️⃣ Beerophage (Notre Base Actuelle)

### 🔑 Systèmes Déjà Implémentés

#### A) Building_BeerCasket
- Hérite de `Building_Bed`
- `CompRefuelable` pour consommation drogue
- `CompPowerTrader` pour électricité
- Restaure ressource pendant sommeil
- `CompAssignableToPawn_SingleOwner` pour limitation propriétaire unique

#### B) Gene_Hemogen Reuse
- `Gene_Hemogen` vanilla réutilisé pour "Beergenic"
- Gizmo automatique
- Support caravane
- Resource drain quotidien

#### C) IngestionOutcomeDoer
- `IngestionOutcomeDoer_OffsetBeergen` : Restaure beergen en buvant
- `IngestionOutcomeDoer_BeerophageAlcoholFilter` : Modifie effets alcool

---

## 🎯 Synthèse Pour Decadents

### 📋 Systèmes à Créer

#### 1. Interface IDecadentResource
```csharp
public interface IDecadentResource
{
    Gene_Resource Resource { get; }
    bool CanOffset { get; }
    float ResourceLossPerDay { get; }
    Pawn Pawn { get; }
    string DisplayLabel { get; }
    string ResourceDefName { get; }
}
```

**Implémenté par:**
- `Gene_Beergenic` (déjà fait)
- `Gene_Stimgenic` (à créer)
- `Gene_Dreamgenic` (à créer)

#### 2. CompDecadentBindable System
```csharp
// Properties
public class CompProperties_DecadentBindable : CompProperties
{
    public HediffDef hediffToApply;
    public float restEffectivenessFactor = 1f;
    public float resourceLimitOffset = 0f;
    public int stackLimit = 1;
    public SoundDef soundStart;
    public SoundDef soundEnd;
    public SoundDef soundWorking;
}

// Comp
public class CompDecadentBindable : ThingComp
{
    private CompProperties_DecadentBindable Props => ...;

    // Trouve le casket connecté
    public Building_DecadentCasket FindConnectedCasket() { ... }

    // Vérifie si actif (powered, fueled, pawn sleeping)
    public bool IsActive { get; }

    // Applique hediff au réveil
    public void ApplyHediffToPawn(Pawn pawn) { ... }

    // Gestion stack limit
    public bool CanConnect() { ... }
}
```

#### 3. Building_DecadentCasket (Base abstraite)
```csharp
public abstract class Building_DecadentCasket : Building_Bed
{
    protected CompRefuelable compRefuelable;
    protected CompPowerTrader compPowerTrader;

    // Type de ressource à restaurer
    protected abstract string ResourceGeneName { get; }

    // Trouve tous les extenders connectés
    public List<CompDecadentBindable> GetConnectedExtenders() { ... }

    // Applique tous les hediffs au réveil
    protected void ApplyExtenderHediffs(Pawn pawn) { ... }

    // Restaure ressource
    protected virtual void RestoreResource(Pawn pawn) { ... }
}

// Implémentations
public class Building_BeerCasket : Building_DecadentCasket
{
    protected override string ResourceGeneName => "Beergenic";
}

public class Building_StimCasket : Building_DecadentCasket
{
    protected override string ResourceGeneName => "Stimgenic";
}

public class Building_DreamCasket : Building_DecadentCasket
{
    protected override string ResourceGeneName => "Dreamgenic";
}
```

---

## 📚 Patterns et Best Practices Identifiés

### 1. Hash Interval Ticks
**AlphaGenes:**
```csharp
if (pawn.IsHashIntervalTick(300)) // ~5 secondes
{
    // Logique moins fréquente
}
```

**Performance:** N'exécute pas chaque tick, économise CPU

### 2. Interface pour Types Similaires
**AlphaGenes:** `IGeneResourceDrain`
**Nous:** `IDecadentResource`

**Avantages:**
- Code générique réutilisable
- Utilitaires communs
- Standardisation

### 3. Comp Pattern
**Standard RimWorld:**
- `CompProperties` (XML config)
- `ThingComp` (logique C#)
- Composable, réutilisable

### 4. Gene + Hediff Combo
**Pattern commun:**
- Gène donne capacité
- Hediff donne bonus temporaire
- Hediff ajouté/retiré dynamiquement

### 5. ExposeData pour Save
```csharp
public override void ExposeData()
{
    base.ExposeData();
    Scribe_Values.Look(ref myField, "myField");
    Scribe_Defs.Look(ref myDef, "myDef");
}
```

**Critique:** Toute donnée non sauvegardée = perdue au load

### 6. Gizmos pour UI
```csharp
public override IEnumerable<Gizmo> GetGizmos()
{
    foreach (Gizmo g in base.GetGizmos())
        yield return g;

    // Mes gizmos custom
    yield return new Command_Action { ... };
}
```

---

## 🚀 Prochaines Étapes Pour Decadents

### Phase 1: Infrastructure
1. ✅ `IDecadentResource` interface
2. ✅ `CompDecadentBindable` + `CompProperties_DecadentBindable`
3. ✅ `Building_DecadentCasket` base class
4. ✅ Refactor `Gene_Beergenic` pour implémenter interface

### Phase 2: Baron (Stimulants)
1. `Gene_Stimgenic`
2. `Building_StimCasket`
3. 4x Extenders (Wake-up, Go-juice, Flake, Yayo)
4. Hediffs de bonus
5. Xenotype Baron complet

### Phase 3: Rêveur (Psychédéliques)
1. `Gene_Dreamgenic`
2. `Building_DreamCasket`
3. 2-3x Extenders (Smokeleaf, Psychite tea, Royal jelly)
4. Hediffs de bonus
5. Xenotype Rêveur complet

### Phase 4: Polish
1. Balance
2. Textures custom
3. Sons
4. Documentation

---

## 📖 Ressources Utiles Identifiées

### Code à Réutiliser
- AlphaGenes `Gene_Resource_Metal.cs` - Template ressource custom
- AlphaGenes `IGeneResourceDrain` - Interface pattern
- VRE Sanguophage `Building_Deathrest_Extender.cs` - Template extender
- VEF `CompApparelHediffs.cs` - Pattern hediff auto-apply

### XML à S'inspirer
- VRE Sanguophage `Buildings_Deathrest.xml` - Extenders defs
- AlphaGenes GeneDefs - Ressources custom

### Patterns Généraux
- Comp + CompProperties pattern
- Hash interval ticks optimisation
- Gene + Hediff combo
- ExposeData save pattern

---

## 🔍 Autres Mods Scannés

Les mods suivants ont été scannés mais ne contenaient pas de patterns directement applicables à Decadents:

### Mods Animaux (Non pertinent)
- VanillaAnimalsExpanded-* (9 mods) - Ajoutent des animaux, aucun système de ressource/gène custom

### Mods Furniture/Buildings (Patterns déjà couverts)
- VanillaFurnitureExpanded-* (6 mods) - Utilisent patterns standards (CompPower, CompRefuelable, etc.)
- VanillaApparelExpanded
- VanillaArmourExpanded

### Mods Factions (Pas de système de ressource)
- VanillaFactionsExpanded-Deserters
- VanillaFactionsExpanded-Medieval2

### Mods Plants/Food (Non pertinent)
- VanillaFoodVarietyExpanded
- VanillaPlantsExpanded
- VanillaPlantsExpanded-Mushrooms

### Mods Autres (Scannés mais non prioritaires)
- VanillaHelixienGasExpanded - Gaz custom, mais système trop spécifique
- VanillaGravshipExpanded - Vaisseaux, non pertinent
- VanillaVehiclesExpanded - Véhicules, patterns trop complexes
- VanillaOutpostsExpanded - Map components, pas de gènes
- VanillaIdeologyExpanded-* - Idéologie, non pertinent
- VanillaQuestsExpanded-* - Quêtes, non pertinent
- VanillaRacesExpanded-Archon - Scanné, moins de patterns que Sanguophage
- VanillaRacesExpanded-Fungoid - Un seul gène custom simple (Gene_RandomGene)

### VanillaPsycastsExpanded
**Partiellement analysé**
- Système d'abilities complexe
- AbilityExtensions pour customiser abilities
- Pattern Hediff + Ability combo
- **Pourrait être utile** si on ajoute des capacités psychiques aux Decadents (plus tard)

### VanillaGeneticsExpanded (1.3 seulement)
**Note:** Version 1.3 uniquement dans references, pas analysé en détail
- Système d'hybrides animaux/humains
- DefExtensions pour qualité
- CompGiveHediff pour abilities
- **Moins pertinent** car focus sur hybrides, pas ressources

---

## 📊 Mods les Plus Utiles (Classement)

### 🥇 Top 1: AlphaGenes
- ✅ `IGeneResourceDrain` interface parfaite pour nos ressources
- ✅ `Gene_Resource_Metal` = template exact pour nos ressources custom
- ✅ Système d'efficacité
- ✅ Support métabolisme
- ✅ Hash interval optimization

### 🥈 Top 2: VRE Sanguophage
- ✅ CompDeathrestBindable = template pour nos extenders
- ✅ CompDraincasket = alternative design pour caskets
- ✅ Pattern hediff application
- ✅ XML defs examples parfaits

### 🥉 Top 3: VanillaExpandedFramework
- ✅ CompHediffGiver pour AoE hediffs
- ✅ CompRefuelable_DualFuel si besoin 2 drugs
- ✅ Pattern Comp + CompProperties
- ✅ Nombreux utilities généraux

### 🏅 Top 4: VRE Android
- ✅ Gene awakening system (transformation)
- ✅ PostAdd() setup pattern
- ✅ Building_AndroidSleepMode (stasis pattern)
- ⚠️ Moins directement applicable mais bon à connaître

---

**Auteur:** gilith59
**Date:** 2026-01-06
**Status:** ✅ Synthèse complète - Tous les mods vanilla-expanded-source scannés
