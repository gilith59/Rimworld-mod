# 📝 Changelog - Insect Lair Incident v2.1

**Date:** 2026-01-05
**Type:** Feature Update + Test Version

---

## 🎯 Modifications Principales

### 1. Distribution des Genelines Rééquilibrée
**Avant (v2.0):**
- VFEI_Sorne (Empress): 80%
- Autres genelines: 5% chacun

**Après (v2.1 PROD):**
- VFEI_Sorne (Empress): 60%
- VFEI_Nuchadus (Titantick): 10%
- VFEI_Chelis (Teramantis): 10%
- VFEI_Kemia (Gigamite): 10%
- VFEI_Xanides (Silverfish): 10%

**Raison:** Retour utilisateur MDebaque - sur 20 lairs, un seul boss différent observé.

---

### 2. Messages Découverte Boss (6 messages uniques)

Quand le joueur découvre le boss dans la cave, un message personnalisé s'affiche avec son :

**Messages par boss :**

1. **VFEI2_Empress (Sorne)**
   > "You've discovered the Empress! This massive insectoid queen commands her swarm with terrifying intelligence. Defeat her to collapse the lair."

2. **VFEI2_Titantick (Nuchadus)**
   > "You've discovered the Titantick! This explosive behemoth is heavily armored and extremely dangerous. Defeat it to collapse the lair."

3. **VFEI2_Teramantis (Chelis)**
   > "You've discovered the Teramantis! This colossal mantis-like creature is a apex predator. Defeat it to collapse the lair."

4. **VFEI2_Gigamite (Kemia)**
   > "You've discovered the Gigamite! This enormous mite can spit devastating acid. Defeat it to collapse the lair."

5. **VFEI2_Silverfish (Xanides)**
   > "You've discovered the Silverfish! This armored insectoid is nearly impenetrable. Defeat it to collapse the lair."

6. **HiveQueen (Vanilla)**
   > "You've discovered the Hive Queen! This ancient insectoid monarch rules the depths. Defeat her to collapse the lair."

**Son:** `Quest_Accepted` (notification sonore positive)

---

### 3. Bouton Collapse Toujours Visible

**Avant:**
- Bouton collapse invisible jusqu'à mort du boss
- Message apparaissait seulement au survol après mort

**Après:**
- Bouton collapse **toujours visible** dès le début
- **Grisé avec tooltip** tant que boss vivant:
  - Avant d'entrer: `"Enter the lair and defeat the boss first."`
  - Boss vivant: `"Boss must be defeated first. The lair will auto-collapse 72h after boss death."`
  - Boss mort: Bouton actif (mais pas nécessaire car auto-collapse)

---

### 4. Auto-Collapse 72h Après Mort Boss

**Nouvelle mécanique:**
- Timer de **72 heures** (180,000 ticks) démarre à la mort du boss
- Cave entrance disparaît **automatiquement** (comme PitGate dans Anomaly)
- Message final: `"The insect lair has collapsed! The entrance has sealed itself."`
- Son de destruction: `Building_Deconstructed`

**Inspiré de:** Anomaly DLC - PitGate incident

---

### 5. Message + Son à la Mort du Boss

**Nouveau message immédiat:**
> "The [Boss Name] has been defeated! The insect lair will automatically collapse in 72 hours."

**Détails:**
- Type: `PositiveEvent` (message vert)
- Son: `Quest_Concluded` (son de quête terminée)
- Apparaît **immédiatement** à la mort, pas besoin de sortir de la cave

---

### 6. Empêcher Incidents Multiples Simultanés

**Problème identifié:**
- Spawner 10x incidents avec dev mode remplaçait tous par le dernier
- Seulement le dernier spawné générait des insectes

**Solution:**
- Vérification dans `IncidentWorker_InsectLairSpawn`
- Refuse de spawn si un `InsectLairEntrance` ou `InsectLairSpawner` existe déjà
- Garantit **un seul lair actif à la fois**

---

## 🧪 Version TEST Créée

**PackageId:** `gilith.insectlairincident.test`
**Nom:** "Insect Lair Incident [TEST VERSION]"

### Différences TEST vs PROD

| Feature | PROD (v2.1) | TEST |
|---------|-------------|------|
| **Émergence** | 8-16 heures (20000~40000 ticks) | **2-5 minutes** (3000~7500 ticks) |
| **Auto-collapse** | 72 heures (180000 ticks) | **5 minutes** (7500 ticks) |
| **Empress chance** | 60% | **20%** |
| **Autres genelines** | 10% chacun | **20% chacun** |

### Utilisation Version TEST

**Emplacement:**
- Dev: `/RimWorld/Mods/InsectLairIncident_TEST/`
- Prod: `/prod/mods/InsectLairIncident_TEST/`

**Pour tester:**
1. Désactiver "Insect Lair Incident" (prod)
2. Activer "Insect Lair Incident [TEST VERSION]"
3. Dev mode: `incident insectlairemergence`
4. Attendre 2-5 minutes au lieu de 8-16h
5. Tuer boss, attendre 5 minutes au lieu de 72h

**Avantages:**
- Tests complets en 10-15 minutes (vs 80+ heures)
- Variété des boss garantie (20% chaque)
- Feedback rapide sur les modifications

---

## 📂 Fichiers Modifiés

### Code C#

**GenelineHelper.cs**
```csharp
// PROD: Empress 60%, autres 10%
if (genelineDef.defName == "VFEI_Sorne")
    copies = 60;
else
    copies = 10;

// TEST: Toutes 20%
copies = 20;
```

**MapComponent_HiveQueenTracker.cs**
- Ajout timer auto-collapse (72h prod / 5min test)
- Messages découverte boss (6 messages uniques)
- Message mort boss avec son
- Méthode `TriggerAutoCollapse()` pour détruire entrance
- Référence à `parentMap` pour auto-collapse cross-map

**CompSealable_Conditional.cs**
- Bouton toujours visible
- Grisé avec tooltips selon état (pas entré / boss vivant / boss mort)

**IncidentWorker_InsectLairSpawn.cs**
- Check `InsectLairEntrance` ou `InsectLairSpawner` existant
- Refuse spawn si lair déjà actif

**GenStep_SpawnHiveQueen.cs**
- Passe `parentMap` au tracker pour auto-collapse

### XML

**Buildings_InsectLairSpawner.xml**
```xml
<!-- PROD -->
<groundSpawnerSpawnDelay>20000~40000</groundSpawnerSpawnDelay>

<!-- TEST -->
<groundSpawnerSpawnDelay>3000~7500</groundSpawnerSpawnDelay>
```

**compile.sh**
- Chemin Harmony: `/references/Harmony/` (au lieu de `/mod rimworld/`)

---

## 🐛 Bugs Corrigés

1. **Distribution genelines déséquilibrée** - 80% Empress → 60% Empress
2. **Pas de feedback à la découverte du boss** - Messages uniques ajoutés
3. **Bouton collapse caché** - Maintenant toujours visible mais grisé
4. **Collapse manuel seulement** - Auto-collapse 72h ajouté
5. **Message mort boss invisible** - Message immédiat avec son
6. **Incidents multiples cassent le mod** - Vérification ajoutée

---

## 📊 Statistiques

**DLL Size:**
- v2.0: 25 KB
- v2.1: 29 KB (+4 KB)

**Lignes de code ajoutées:** ~150 lignes

**Tests requis:**
1. ✅ Compilation réussie (PROD + TEST)
2. ⏳ Test découverte des 6 boss
3. ⏳ Test auto-collapse 72h (ou 5min en TEST)
4. ⏳ Test incidents multiples bloqués
5. ⏳ Test variété genelines (20 spawns)

---

## 🔄 Retour Utilisateur Initial (MDebaque)

**Problèmes rapportés:**
1. ✅ Sur 20 lairs, un seul insecte alternatif → **CORRIGÉ** (60%/10% au lieu de 80%/5%)
2. ✅ Collapse manuel seulement → **CORRIGÉ** (auto-collapse 72h comme PitGate)
3. ✅ Message collapse pas visible → **CORRIGÉ** (message immédiat + son)
4. ✅ Pas de message découverte boss → **CORRIGÉ** (6 messages uniques)
5. ✅ Spawner x10 incidents casse tout → **CORRIGÉ** (un seul lair actif)

---

**Version suivante prévue:** v2.2 (après feedback tests v2.1)

**Auteur:** Gilith
**Date:** 2026-01-05
