using RimWorld;
using Verse;
using System.Linq;

namespace InsectLairIncident
{
    // Composant sur la map PARENT (surface) qui surveille la pocket map
    public class MapComponent_InsectLairMonitor : MapComponent
    {
        private MapPortal cachedPortal;

        public MapComponent_InsectLairMonitor(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // Ne vérifier que toutes les 60 ticks (1 seconde) pour économiser CPU
            if (Find.TickManager.TicksGame % 60 != 0)
                return;

            // Trouver le portal InsectLairEntrance sur cette map
            if (cachedPortal == null || cachedPortal.Destroyed)
            {
                cachedPortal = map.listerThings.AllThings
                    .OfType<MapPortal>()
                    .FirstOrDefault(p => p.def.defName == "InsectLairEntrance");
            }

            if (cachedPortal != null && cachedPortal.PocketMap != null)
            {
                // Lier la pocket map au portal ID pour le tracking de geneline
                GameComponent_InsectLairGenelines globalComp = Current.Game.GetComponent<GameComponent_InsectLairGenelines>();
                if (globalComp != null)
                {
                    globalComp.LinkPocketMapToPortal(cachedPortal.PocketMap.uniqueID, cachedPortal.thingIDNumber);
                }

                // Vérifier le tracker de la pocket map
                MapComponent_HiveQueenTracker tracker = cachedPortal.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
                if (tracker != null)
                {
                    // Appeler IsQueenDead() pour trigger le message si le boss vient de mourir
                    tracker.IsQueenDead();
                }
            }
        }
    }
}
