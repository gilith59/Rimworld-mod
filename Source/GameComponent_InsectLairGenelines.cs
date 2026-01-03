using System.Collections.Generic;
using Verse;

namespace InsectLairIncident
{
    /// <summary>
    /// GameComponent global pour tracker les genelines des caves InsectLair actives
    /// Persiste entre les maps (colonie et pocket maps)
    /// </summary>
    public class GameComponent_InsectLairGenelines : GameComponent
    {
        // Map de parentMapId -> geneline choisie
        private Dictionary<int, GenelineData> activeGenelines = new Dictionary<int, GenelineData>();

        public GameComponent_InsectLairGenelines(Game game)
        {
        }

        /// <summary>
        /// Enregistre la geneline pour une cave
        /// </summary>
        public void RegisterGeneline(int parentMapId, GenelineData geneline)
        {
            activeGenelines[parentMapId] = geneline;
            Log.Warning($"[InsectLairIncident] Geneline registered for map {parentMapId}: {geneline.defName} (Boss: {geneline.boss.defName})");
        }

        /// <summary>
        /// Récupère la geneline pour une cave
        /// Pour les pocket maps, retourne la première geneline active trouvée
        /// </summary>
        public GenelineData GetGeneline(Map map)
        {
            // Chercher d'abord par mapId exact
            if (activeGenelines.TryGetValue(map.uniqueID, out GenelineData geneline))
            {
                return geneline;
            }

            // Si pas trouvé (pocket map), retourner n'importe quelle geneline active
            // Car il ne devrait y avoir qu'une seule cave active à la fois
            foreach (var kvp in activeGenelines)
            {
                return kvp.Value;
            }

            return null;
        }

        /// <summary>
        /// Supprime la geneline quand la cave est résolue
        /// </summary>
        public void RemoveGeneline(int parentMapId)
        {
            activeGenelines.Remove(parentMapId);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeGenelines, "activeGenelines", LookMode.Value, LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && activeGenelines == null)
            {
                activeGenelines = new Dictionary<int, GenelineData>();
            }
        }
    }
}
