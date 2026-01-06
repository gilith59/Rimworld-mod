using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace InsectLairIncident
{
    /// <summary>
    /// Helper class pour gérer l'intégration avec VFE Insectoids genelines
    /// </summary>
    public static class GenelineHelper
    {
        private static bool? vfeInsectoidsLoaded = null;
        private static Type insectGenelineDefType = null;
        private static int lastCheckedGameID = -1;

        /// <summary>
        /// Vérifie si VFE Insectoids est chargé
        /// </summary>
        public static bool IsVFEInsectoidsLoaded()
        {
            // Reset cache si nouvelle partie
            int currentGameID = Current.Game?.uniqueIDsManager?.GetNextThingID() ?? 0;
            if (lastCheckedGameID != currentGameID)
            {
                vfeInsectoidsLoaded = null;
                insectGenelineDefType = null;
                lastCheckedGameID = currentGameID;
            }

            if (vfeInsectoidsLoaded.HasValue)
            {
                return vfeInsectoidsLoaded.Value;
            }

            // Check si le mod VFE Insectoids est chargé en cherchant directement le type
            insectGenelineDefType = GenTypes.GetTypeInAnyAssembly("VFEInsectoids.InsectGenelineDef", "VFEInsectoids");
            vfeInsectoidsLoaded = (insectGenelineDefType != null);

            Log.Warning($"[InsectLairIncident] VFE Insectoids active check: {vfeInsectoidsLoaded.Value}");
            Log.Warning($"[InsectLairIncident] InsectGenelineDef type found: {insectGenelineDefType != null}");

            return vfeInsectoidsLoaded.Value;
        }

        /// <summary>
        /// Choisit une geneline aléatoire basée sur les poids VFE
        /// </summary>
        public static GenelineData ChooseRandomGeneline()
        {
            if (!IsVFEInsectoidsLoaded())
            {
                return GetVanillaGeneline();
            }

            try
            {
                // Récupérer toutes les genelines via DefDatabase
                var genelinesMethod = typeof(DefDatabase<>).MakeGenericType(insectGenelineDefType).GetMethod("get_AllDefsListForReading");
                var allGenelines = genelinesMethod.Invoke(null, null) as System.Collections.IList;

                Log.Warning($"[InsectLairIncident] Found {allGenelines?.Count ?? 0} genelines in VFE");

                if (allGenelines == null || allGenelines.Count == 0)
                {
                    Log.Warning("[InsectLairIncident] Aucune geneline trouvée dans VFE Insectoids. Fallback vanilla.");
                    return GetVanillaGeneline();
                }

                // Construire liste pondérée avec distribution égale (TEST VERSION)
                // Toutes les genelines: 20% chacune
                List<Def> weightedGenelines = new List<Def>();
                foreach (var geneline in allGenelines)
                {
                    Def genelineDef = geneline as Def;

                    // TEST VERSION: Distribution égale pour faciliter les tests
                    int copies = 20; // 20% chacun

                    for (int i = 0; i < copies; i++)
                    {
                        weightedGenelines.Add(genelineDef);
                    }
                }

                // Choisir aléatoirement
                Def chosenGeneline = weightedGenelines.RandomElement();

                Log.Warning($"[InsectLairIncident] Geneline choisie: {chosenGeneline.defName}");

                return ExtractGenelineData(chosenGeneline);
            }
            catch (Exception ex)
            {
                Log.Error($"[InsectLairIncident] Erreur lors du choix de geneline: {ex}. Fallback vanilla.");
                return GetVanillaGeneline();
            }
        }

        /// <summary>
        /// Extrait les données d'une geneline VFE
        /// </summary>
        private static GenelineData ExtractGenelineData(Def genelineDef)
        {
            var bossField = insectGenelineDefType.GetField("boss");
            var hiveField = insectGenelineDefType.GetField("hive");
            var insectsField = insectGenelineDefType.GetField("insects");

            PawnKindDef boss = bossField.GetValue(genelineDef) as PawnKindDef;
            ThingDef hive = hiveField.GetValue(genelineDef) as ThingDef;
            var insectsList = insectsField.GetValue(genelineDef) as System.Collections.IList;

            // Convertir la liste d'insectes (List<PawnGenOption>)
            List<PawnKindDef> insectKinds = new List<PawnKindDef>();
            if (insectsList != null)
            {
                foreach (var pawnGenOption in insectsList)
                {
                    var kindField = pawnGenOption.GetType().GetField("kind");
                    PawnKindDef kind = kindField.GetValue(pawnGenOption) as PawnKindDef;
                    if (kind != null)
                    {
                        insectKinds.Add(kind);
                    }
                }
            }

            return new GenelineData
            {
                defName = genelineDef.defName,
                boss = boss,
                hive = hive,
                insects = insectKinds,
                isVanilla = false
            };
        }

        /// <summary>
        /// Retourne une geneline vanilla par défaut
        /// </summary>
        private static GenelineData GetVanillaGeneline()
        {
            return new GenelineData
            {
                defName = "Vanilla",
                boss = PawnKindDef.Named("HiveQueen"), // HiveQueen vanilla
                hive = ThingDefOf.Hive,
                insects = new List<PawnKindDef>
                {
                    PawnKindDefOf.Megascarab,
                    PawnKindDefOf.Spelopede,
                    PawnKindDefOf.Megaspider
                },
                isVanilla = true
            };
        }
    }

    /// <summary>
    /// Données d'une geneline (VFE ou vanilla)
    /// </summary>
    public class GenelineData : IExposable
    {
        public string defName;
        public PawnKindDef boss;
        public ThingDef hive;
        public List<PawnKindDef> insects;
        public bool isVanilla;

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Defs.Look(ref boss, "boss");
            Scribe_Defs.Look(ref hive, "hive");
            Scribe_Collections.Look(ref insects, "insects", LookMode.Def);
            Scribe_Values.Look(ref isVanilla, "isVanilla");
        }

        /// <summary>
        /// Choisit un insecte aléatoire de cette geneline
        /// </summary>
        public PawnKindDef GetRandomInsect()
        {
            if (insects == null || insects.Count == 0)
            {
                return PawnKindDefOf.Megascarab; // Fallback
            }
            return insects.RandomElement();
        }
    }
}
