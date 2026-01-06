# 🍺 Decadents Mod - Plan d'Implémentation

**Date:** 2026-01-06
**Basé sur:** Beerophage v1.0 + VRE Sanguophage
**Concept:** MDebaque (Discord 02/01/2026 & 04/01/2026)

---

## 📋 Vue d'Ensemble

### Trois Xenotypes "Decadents"

1. **Le Festoyeur** (Beer-based) - Extension de Beerophage
   - Ressource: **Beergen**
   - Drogues: Bière, Ambrosia
   - Style: Nain barbare corps à corps
   - Fût: **Beer Casket** (déjà existant dans Beerophage)

2. **Le Baron** (Stimulants) - Nouveau
   - Ressource: **Stimgen** ou **Rushgen**
   - Drogues: Wake-up, Go-juice, Flake, Yayo
   - Style: Vampire waster baron
   - Fût: **Stim Casket**

3. **Le Rêveur** (Psychédéliques) - Nouveau
   - Ressource: **Dreamgen** ou **Psychegen**
   - Drogues: Smokeleaf, Psychite tea
   - Style: Mystique méditatif
   - Fût: **Dream Casket**

### Système de Connexion

Inspiré de **VRE Sanguophage** et vanilla **CompProperties_DeathrestBindable**:
- Chaque fût peut se connecter à des **bâtiments secondaires**
- Les bâtiments appliquent des **hediffs permanents** (bonus actifs APRÈS le repos)
- Consommation de drogue pour fonctionner
- Limite de connexions stackables

---

## 🎯 Architecture Technique

### Système de Base (Hérité de Beerophage)

#### Gènes de Ressource
```csharp
// Déjà fait pour Beergen
Gene_Hemogen → Beergenic

// À créer
Gene_Hemogen → Stimgenic (Baron)
Gene_Hemogen → Dreamgenic (Rêveur)
```

#### Fûts de Repos
```csharp
// Déjà fait
Building_BeerCasket : Building_Bed
    - CompRefuelable (Beer)
    - CompPowerTrader
    - Restauration Beergen pendant sommeil

// À créer
Building_StimCasket : Building_Bed
    - CompRefuelable (Wake-up/Go-juice/Flake/Yayo)
    - CompPowerTrader
    - Restauration Stimgen

Building_DreamCasket : Building_Bed
    - CompRefuelable (Smokeleaf/Psychite tea)
    - CompPowerTrader
    - Restauration Dreamgen
```

### Nouveau Système : Bâtiments Secondaires Connectables

#### Utilisation de CompProperties_DeathrestBindable (Vanilla Biotech)

**Avantages :**
- ✅ Système vanilla éprouvé
- ✅ UI de connexion existante
- ✅ Gestion automatique des liens
- ✅ Support hediff automatique

**Implémentation :**

```xml
<!-- Exemple: Amplificateur de Bière -->
<ThingDef ParentName="DeathrestBuildingBase">
  <defName>BeerAmplifier</defName>
  <label>beer amplifier</label>
  <description>Augmente les bonus obtenus en buvant de la bière. Doit être connecté à un Beer Casket.</description>
  <thingClass>Building</thingClass>
  <size>(1,1)</size>
  <comps>
    <li Class="CompProperties_DeathrestBindable">
      <hediffToApply>BeerAmplified</hediffToApply>
      <soundStart>PsychofluidPump_Start</soundStart>
      <soundEnd>PsychofluidPump_Stop</soundEnd>
      <soundWorking>PsychofluidPump_Ambience</soundWorking>
    </li>
    <li Class="CompProperties_Power">
      <basePowerConsumption>50</basePowerConsumption>
    </li>
    <li Class="CompProperties_Refuelable">
      <fuelConsumptionRate>0.5</fuelConsumptionRate>
      <fuelCapacity>10</fuelCapacity>
      <fuelLabel>Beer</fuelLabel>
      <fuelFilter>
        <thingDefs><li>Beer</li></thingDefs>
      </fuelFilter>
    </li>
  </comps>
</ThingDef>
```

**Important:** Le système DeathrestBindable vanilla fonctionne uniquement avec les **Deathrest Caskets** (Sanguophages). On doit soit :

**Option A:** Étendre nos caskets pour supporter DeathrestBindable
```csharp
// Modifier Building_BeerCasket
public class Building_BeerCasket : Building_Bed
{
    // Ajouter support pour CompDeathrestBindable
    // Problème: Nécessite que le pawn ait Gene_Deathrest
}
```

**Option B:** Créer notre propre système de connexion (Recommandé)
```csharp
// Nouveau composant inspiré de DeathrestBindable
public class CompProperties_DecadentBindable : CompProperties
{
    public HediffDef hediffToApply;
    public float deathrestEffectivenessFactor = 1f; // Bonus vitesse repos
    public int stackLimit = 1; // Nombre max de ce type

    public CompProperties_DecadentBindable()
    {
        compClass = typeof(CompDecadentBindable);
    }
}

public class CompDecadentBindable : ThingComp
{
    private CompProperties_DecadentBindable Props => (CompProperties_DecadentBindable)props;

    // Logique de connexion au casket
    // Application du hediff au réveil
}
```

---

## 📦 Structure des Fichiers

### Extension de Beerophage

```
Decadents/  (ou Beerophage v2.0?)
├── About/
│   └── About.xml
├── Source/
│   ├── Buildings/
│   │   ├── Building_BeerCasket.cs          # Déjà existant
│   │   ├── Building_StimCasket.cs          # Nouveau
│   │   ├── Building_DreamCasket.cs         # Nouveau
│   │   └── Building_DecadentExtender.cs    # Base pour bâtiments secondaires
│   ├── Comps/
│   │   ├── CompDecadentBindable.cs         # Système de connexion
│   │   └── CompProperties_DecadentBindable.cs
│   ├── Genes/
│   │   ├── Gene_BeergenEmpowerment.cs      # Déjà existant
│   │   ├── Gene_StimgenEmpowerment.cs      # Nouveau
│   │   └── Gene_DreamgenEmpowerment.cs     # Nouveau
│   └── ...
├── 1.6/
│   ├── Assemblies/
│   │   └── Decadents.dll
│   ├── Defs/
│   │   ├── GeneDefs/
│   │   │   ├── Festoyeur_Genes.xml
│   │   │   ├── Baron_Genes.xml
│   │   │   └── Reveur_Genes.xml
│   │   ├── ThingDefs_Buildings/
│   │   │   ├── Caskets.xml                 # 3 types de fûts
│   │   │   ├── BeerExtenders.xml           # Beer Amplifier, Ambrosia Infuser, etc.
│   │   │   ├── StimExtenders.xml           # Wake-up Injector, Go-juice Pump, etc.
│   │   │   └── DreamExtenders.xml          # Smokeleaf Vaporizer, Psychite Diffuser, etc.
│   │   ├── HediffDefs/
│   │   │   └── Decadent_Hediffs.xml        # Tous les bonus permanents
│   │   └── XenotypeDefs/
│   │       ├── Festoyeur.xml
│   │       ├── Baron.xml
│   │       └── Reveur.xml
│   └── ...
└── LoadFolders.xml
```

---

## 🧬 Détail des Xenotypes

### 1. Le Festoyeur (Extension Beerophage)

**Gènes :**
- `Beergenic` (ressource beergen)
- `BeergenDrain` (consommation augmentée)
- `AlcoholTolerance_Enhanced`
- `BeergenEmpowerment` (bonus quand >70%)
- `MeleeSpecialist` (nouveau - bonus corps à corps)
- `Tough` (peau épaisse)
- `BadMining` (malus minier)
- `GoodSocial` (bonus social)
- `GoodGrowing` (bonus agriculture)

**Bâtiments Secondaires :**
1. **Beer Amplifier** (déjà Beer Meditation Chamber?)
   - Hediff: `BeerAmplified` (+15% gain beergen de la bière)
   - Fuel: Beer

2. **Ambrosia Infuser**
   - Hediff: `AmbrosiaBlessed` (+0.05 immunity gain speed, +10% psychic sensitivity)
   - Fuel: Ambrosia

### 2. Le Baron (Nouveau)

**Gènes :**
- `Stimgenic` (ressource stimgen)
- `StimgenDrain`
- `DrugTolerance_Stimulants` (immunité addiction stimulants)
- `StimgenEmpowerment` (bonus vitesse/conscience quand >70%)
- `FastMovement` (vitesse augmentée)
- `BadSocial` (malus social - agressif)
- `Insomniac` (besoin moins de sommeil)

**Bâtiments Secondaires :**
1. **Wake-up Injector**
   - Hediff: `Wakeful` (+20% consciousness, -20% sleep fall rate)
   - Fuel: Wake-up

2. **Go-juice Pump**
   - Hediff: `Accelerated` (+10% move speed, +15% melee dodge)
   - Fuel: Go-juice

3. **Flake Dispenser**
   - Hediff: `EuphoriaDose` (+5 mood, +10% pain shock threshold)
   - Fuel: Flake

4. **Yayo Refiner**
   - Hediff: `YayoBoosted` (+15% work speed, +10% global learning factor)
   - Fuel: Yayo

### 3. Le Rêveur (Nouveau)

**Gènes :**
- `Dreamgenic` (ressource dreamgen)
- `DreamgenDrain`
- `DrugTolerance_Psychedelics` (immunité addiction psychédéliques)
- `DreamgenEmpowerment` (bonus psychic quand >70%)
- `PsychicSensitivity_Enhanced`
- `SlowMovement` (malus vitesse - contemplatif)
- `GoodSocial` (bonus social - calme)
- `GoodIntellectual` (bonus recherche)

**Bâtiments Secondaires :**
1. **Smokeleaf Vaporizer**
   - Hediff: `Mellow` (+10 mood, +30% pain, -15% consciousness)
   - Fuel: Smokeleaf joint

2. **Psychite Diffuser**
   - Hediff: `Enlightened` (+20% psychic sensitivity, +15% negotiation ability)
   - Fuel: Psychite tea

3. **Royal Jelly Chamber** (Bonus)
   - Hediff: `RoyalBoon` (+0.1 immunity gain speed, +5% global learning factor)
   - Fuel: Royal jelly

---

## 🔧 Étapes d'Implémentation

### Phase 1 : Préparer la Base (Refactoring Beerophage)

1. **Renommer le mod**
   - `Beerophage` → `Decadents`
   - Garder tout le code Beerophage existant

2. **Créer la structure multi-ressources**
   ```csharp
   // Base abstraite pour les 3 systèmes
   public abstract class Gene_DecadentResource : Gene_Hemogen
   {
       protected abstract string ResourceName { get; }
       protected abstract float DailyLoss { get; }
   }

   public class Gene_Beergenic : Gene_DecadentResource { /* ... */ }
   public class Gene_Stimgenic : Gene_DecadentResource { /* ... */ }
   public class Gene_Dreamgenic : Gene_DecadentResource { /* ... */ }
   ```

3. **Créer système de connexion**
   ```csharp
   CompDecadentBindable.cs
   CompProperties_DecadentBindable.cs
   ```

### Phase 2 : Baron (Stimulants)

1. **Créer le casket**
   - Copier `Building_BeerCasket.cs` → `Building_StimCasket.cs`
   - Adapter pour Stimgen
   - Multi-fuel : Wake-up, Go-juice, Flake, Yayo

2. **Créer les gènes**
   - `Stimgenic`
   - `StimgenDrain`
   - `StimgenEmpowerment`

3. **Créer les bâtiments secondaires**
   - 4 extenders (un par drogue)

4. **Créer le xenotype**
   - Définition complète Baron

### Phase 3 : Rêveur (Psychédéliques)

1. **Créer le casket**
   - `Building_DreamCasket.cs`
   - Multi-fuel : Smokeleaf, Psychite tea

2. **Créer les gènes**
   - `Dreamgenic`
   - `DreamgenDrain`
   - `DreamgenEmpowerment`

3. **Créer les bâtiments secondaires**
   - 2-3 extenders

4. **Créer le xenotype**
   - Définition complète Rêveur

### Phase 4 : Polish & Balance

1. **Tester chaque xenotype**
2. **Balance des coûts et bonus**
3. **Textures et sons**
4. **Documentation**

---

## ⚖️ Considérations de Balance

### Coûts de Construction

**Caskets :**
- Beer Casket: 100 Steel, 3 Components, 10 Beer
- Stim Casket: 120 Steel, 4 Components, 5 Wake-up, 5 Go-juice
- Dream Casket: 80 Steel, 3 Components, 10 Smokeleaf, 5 Psychite tea

**Extenders :**
- Petits (1x1): 50-75 Steel, 2 Components, drogue respective
- Moyens (1x2): 100-150 Steel, 3 Components
- Grands (2x2): 200-250 Steel, 4-6 Components

### Consommation de Drogue

**Pendant le repos (fûts) :**
- Bière: 0.2/jour
- Stimulants: 0.1/jour (plus puissants)
- Psychédéliques: 0.15/jour

**Pendant actif (extenders) :**
- 0.5/jour si connecté et actif

### Bonus des Hediffs

**Légers (stackable x2-4) :**
- +5-10% stat principale
- Coût drogue modéré

**Moyens (stackable x2) :**
- +10-20% stat principale
- +bonus secondaire
- Coût drogue élevé

**Puissants (unique) :**
- +20-30% stat principale
- +multiples bonus
- Coût drogue très élevé

---

## 🎨 Artwork Nécessaire

### Textures

1. **Caskets (3x):**
   - StimCasket (style high-tech, tubes néon)
   - DreamCasket (style mystique, fumée)
   - Beer Casket (déjà existant)

2. **Extenders (~10-12 bâtiments):**
   - Style cohérent par thème
   - Tailles variées (1x1, 1x2, 2x2)

3. **Icons:**
   - Ressources (Stimgen, Dreamgen icônes)
   - Hediffs (une dizaine)

### Sons

Possibilité de réutiliser sons vanilla :
- PsychofluidPump (VRE)
- Hemopump (VRE)
- DeathrestAccelerator (Biotech)

---

## 📝 Questions Ouvertes

1. **Nom du mod :**
   - "Decadents" (concept MDebaque)
   - "Beerophage Extended"
   - "Vanilla Expanded - Decadents"

2. **Dépendance Beerophage :**
   - Intégrer directement tout le code Beerophage ?
   - Ou faire une dépendance séparée ?
   - → **Recommandation : Intégrer tout, c'est un mod unique**

3. **Compatibilité :**
   - Besoin de Biotech ? (pour Gene_Hemogen)
   - → **Oui, absolument requis**

4. **Artwork :**
   - Créer nouvelles textures ?
   - Réutiliser vanilla + mods ?
   - → **Commencer avec vanilla, améliorer plus tard**

5. **Système de connexion :**
   - Utiliser DeathrestBindable vanilla ? (requiert Gene_Deathrest)
   - Créer CompDecadentBindable custom ? (plus flexible)
   - → **Recommandation : Custom, plus de contrôle**

---

## 🚀 Prochaines Étapes

1. **Décider du nom final du mod**
2. **Créer la branche git `mod/decadents`**
3. **Copier Beerophage comme base**
4. **Implémenter CompDecadentBindable**
5. **Créer Baron (Phase 2)**
6. **Tests et itérations**

---

**Auteur:** Gilith + Claude Code
**Inspiré par:** MDebaque (concept), VRE Sanguophage (système), Beerophage (base code)
**Date:** 2026-01-06
