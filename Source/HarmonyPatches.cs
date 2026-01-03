using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace InsectLairIncident
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("com.insectlairincident.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Log.Message("[InsectLairIncident] Harmony patches applied");
        }
    }

    /// <summary>
    /// Patch pour remplacer les insectes vanilla par ceux de la geneline choisie
    /// dans la génération de cave InsectLair
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        private static bool isGeneratingInsectLair = false;
        private static GenelineData currentGeneline = null;

        public static void SetInsectLairGeneration(bool active, GenelineData geneline = null)
        {
            isGeneratingInsectLair = active;
            currentGeneline = geneline;
        }

        public static bool Prefix(ref PawnGenerationRequest request)
        {
            // Seulement actif pendant la génération d'InsectLair
            if (!isGeneratingInsectLair || currentGeneline == null)
                return true;

            // Seulement pour les insectoïdes de faction insecte
            if (request.Faction != Faction.OfInsects)
                return true;

            // Si c'est un insectoïde vanilla, le remplacer par un de la geneline
            if (IsVanillaInsect(request.KindDef))
            {
                // IMPORTANT: Ne PAS remplacer HiveQueen ici car ça crash GenerateBossRoom vanilla
                // La HiveQueen sera remplacée APRÈS génération dans GenStep_TrackVanillaHiveQueen
                if (request.KindDef.defName == "HiveQueen" || request.KindDef.defName == "Insect_MegaspiderQueen")
                {
                    // Laisser vanilla générer la HiveQueen normalement
                    return true;
                }

                // Pour les autres insectes, choisir un insecte aléatoire de la geneline
                PawnKindDef replacementKind = currentGeneline.GetRandomInsect();
                Log.Warning($"[InsectLairIncident] Replacing {request.KindDef.defName} with {replacementKind.defName} from {currentGeneline.defName} geneline");

                // Créer une nouvelle request en copiant tous les champs publics
                var newRequest = new PawnGenerationRequest(
                    replacementKind,
                    request.Faction,
                    request.Context,
                    request.Tile
                );

                // Copier les autres propriétés importantes
                newRequest.ForceGenerateNewPawn = request.ForceGenerateNewPawn;
                newRequest.AllowDead = request.AllowDead;
                newRequest.AllowDowned = request.AllowDowned;
                newRequest.CanGeneratePawnRelations = request.CanGeneratePawnRelations;
                newRequest.MustBeCapableOfViolence = request.MustBeCapableOfViolence;
                newRequest.ColonistRelationChanceFactor = request.ColonistRelationChanceFactor;
                newRequest.ForceAddFreeWarmLayerIfNeeded = request.ForceAddFreeWarmLayerIfNeeded;
                newRequest.AllowGay = request.AllowGay;
                newRequest.AllowPregnant = request.AllowPregnant;
                newRequest.AllowFood = request.AllowFood;
                newRequest.AllowAddictions = request.AllowAddictions;
                newRequest.Inhabitant = request.Inhabitant;
                newRequest.CertainlyBeenInCryptosleep = request.CertainlyBeenInCryptosleep;
                newRequest.ForceRedressWorldPawnIfFormerColonist = request.ForceRedressWorldPawnIfFormerColonist;
                newRequest.WorldPawnFactionDoesntMatter = request.WorldPawnFactionDoesntMatter;
                newRequest.BiocodeWeaponChance = request.BiocodeWeaponChance;
                newRequest.BiocodeApparelChance = request.BiocodeApparelChance;
                newRequest.ExtraPawnForExtraRelationChance = request.ExtraPawnForExtraRelationChance;
                newRequest.RelationWithExtraPawnChanceFactor = request.RelationWithExtraPawnChanceFactor;
                newRequest.ValidatorPreGear = request.ValidatorPreGear;
                newRequest.ValidatorPostGear = request.ValidatorPostGear;
                newRequest.MinChanceToRedressWorldPawn = request.MinChanceToRedressWorldPawn;
                newRequest.FixedBiologicalAge = request.FixedBiologicalAge;
                newRequest.FixedChronologicalAge = request.FixedChronologicalAge;
                newRequest.FixedGender = request.FixedGender;
                newRequest.FixedLastName = request.FixedLastName;
                newRequest.FixedBirthName = request.FixedBirthName;
                newRequest.FixedTitle = request.FixedTitle;
                newRequest.FixedIdeo = request.FixedIdeo;
                newRequest.ForceNoIdeo = request.ForceNoIdeo;
                newRequest.ForceNoBackstory = request.ForceNoBackstory;
                newRequest.ForbidAnyTitle = request.ForbidAnyTitle;
                newRequest.ForceDead = request.ForceDead;
                newRequest.ForcedXenotype = request.ForcedXenotype;
                newRequest.ForcedCustomXenotype = request.ForcedCustomXenotype;
                newRequest.AllowedXenotypes = request.AllowedXenotypes;
                newRequest.ForceBaselinerChance = request.ForceBaselinerChance;
                newRequest.PawnKindDefGetter = request.PawnKindDefGetter;
                newRequest.ExcludeBiologicalAgeRange = request.ExcludeBiologicalAgeRange;
                newRequest.BiologicalAgeRange = request.BiologicalAgeRange;
                newRequest.ForceRecruitable = request.ForceRecruitable;

                request = newRequest;
            }

            return true;
        }

        private static bool IsVanillaInsect(PawnKindDef kind)
        {
            if (kind == null)
                return false;

            string defName = kind.defName;
            return defName == "Megascarab" ||
                   defName == "Spelopede" ||
                   defName == "Megaspider" ||
                   defName == "HiveQueen" ||
                   defName == "Insect_MegaspiderQueen";
        }
    }

    /// <summary>
    /// Patch pour activer le remplacement des insectes pendant la génération de cave
    /// </summary>
    [HarmonyPatch(typeof(GenStep_InsectLairCave), "Generate")]
    public static class GenStep_InsectLairCave_Generate_Patch
    {
        public static void Prefix(Map map)
        {
            // Récupérer la geneline depuis le GameComponent global
            GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
            GenelineData geneline = globalComp?.GetGeneline(map);

            if (geneline != null)
            {
                Log.Warning($"[InsectLairIncident] Activating geneline spawning for cave generation: {geneline.defName}");
                PawnGenerator_GeneratePawn_Patch.SetInsectLairGeneration(true, geneline);
            }
            else
            {
                Log.Warning("[InsectLairIncident] No geneline found for cave generation!");
            }
        }

        public static void Postfix()
        {
            // Désactiver après génération
            PawnGenerator_GeneratePawn_Patch.SetInsectLairGeneration(false, null);
            // Log.Message("[InsectLairIncident] Deactivated geneline spawning");
        }
    }
}
