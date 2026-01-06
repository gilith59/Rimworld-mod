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
        // Map de portalThingID -> geneline choisie (permet multiple lairs simultanés)
        private Dictionary<int, GenelineData> activeGenelines = new Dictionary<int, GenelineData>();

        // Map de pocketMapId -> portalThingID (pour retrouver la geneline depuis la pocket map)
        private Dictionary<int, int> pocketMapToPortal = new Dictionary<int, int>();

        public GameComponent_InsectLairGenelines(Game game)
        {
        }

        /// <summary>
        /// Enregistre la geneline pour une cave via le portal ID
        /// </summary>
        public void RegisterGeneline(int portalThingID, GenelineData geneline)
        {
            activeGenelines[portalThingID] = geneline;
            Log.Warning($"[InsectLairIncident] Geneline registered for portal {portalThingID}: {geneline.defName} (Boss: {geneline.boss.defName})");
        }

        /// <summary>
        /// Lie une pocket map à son portal
        /// </summary>
        public void LinkPocketMapToPortal(int pocketMapId, int portalThingID)
        {
            pocketMapToPortal[pocketMapId] = portalThingID;
        }

        /// <summary>
        /// Récupère la geneline pour une cave (via portal ID ou pocket map ID)
        /// </summary>
        public GenelineData GetGeneline(int portalThingID)
        {
            if (activeGenelines.TryGetValue(portalThingID, out GenelineData geneline))
            {
                return geneline;
            }
            return null;
        }

        /// <summary>
        /// Récupère la geneline depuis une pocket map
        /// </summary>
        public GenelineData GetGenelineFromPocketMap(Map pocketMap)
        {
            if (pocketMapToPortal.TryGetValue(pocketMap.uniqueID, out int portalID))
            {
                return GetGeneline(portalID);
            }
            return null;
        }

        /// <summary>
        /// Supprime la geneline quand la cave est résolue
        /// </summary>
        public void RemoveGeneline(int portalThingID)
        {
            activeGenelines.Remove(portalThingID);

            // Nettoyer aussi le mapping pocket map
            List<int> keysToRemove = new List<int>();
            foreach (var kvp in pocketMapToPortal)
            {
                if (kvp.Value == portalThingID)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (int key in keysToRemove)
            {
                pocketMapToPortal.Remove(key);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref activeGenelines, "activeGenelines", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref pocketMapToPortal, "pocketMapToPortal", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeGenelines == null)
                    activeGenelines = new Dictionary<int, GenelineData>();
                if (pocketMapToPortal == null)
                    pocketMapToPortal = new Dictionary<int, int>();
            }
        }
    }
}
