using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InsectLairIncident
{
    public class CompSealable_Conditional : CompSealable
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Créer notre propre bouton au lieu de compter sur base (qui peut ne rien retourner)
            MapPortal portal = parent as MapPortal;
            if (portal == null)
                yield break;

            bool bossDefeated = false;
            if (portal.PocketMap != null)
            {
                MapComponent_HiveQueenTracker tracker = portal.PocketMap.GetComponent<MapComponent_HiveQueenTracker>();
                bossDefeated = (tracker != null && tracker.IsQueenDead());
            }

            CompProperties_Sealable props = base.props as CompProperties_Sealable;

            Command_Action sealCmd = new Command_Action
            {
                defaultLabel = props?.sealCommandLabel ?? "Collapse cave entrance",
                defaultDesc = props?.sealCommandDesc ?? "Permanently seal this entrance.",
                icon = ContentFinder<Texture2D>.Get(props?.sealTexPath ?? "UI/Commands/FillInCaveEntrance", true),
                action = delegate
                {
                    string confirmText = props?.confirmSealText ?? "Are you sure you want to seal this entrance?";
                    if (props != null && !string.IsNullOrEmpty(props.confirmSealText))
                    {
                        // Compter les colonists dans la cave
                        int colonistsInside = portal.PocketMap?.mapPawns.FreeColonistsSpawnedCount ?? 0;
                        if (colonistsInside > 0)
                        {
                            confirmText = string.Format(props.confirmSealText, $"\n\n{colonistsInside} colonist(s) are still inside!");
                        }
                        else
                        {
                            confirmText = string.Format(props.confirmSealText, "");
                        }
                    }

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        confirmText,
                        delegate
                        {
                            base.Seal();
                        }
                    ));
                }
            };

            // Griser si boss pas mort
            if (!bossDefeated)
            {
                if (portal.PocketMap != null)
                {
                    sealCmd.Disable("Boss must be defeated first. The lair will auto-collapse 5 seconds after boss death.");
                }
                else
                {
                    sealCmd.Disable("Enter the lair and defeat the boss first.");
                }
            }

            yield return sealCmd;
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
