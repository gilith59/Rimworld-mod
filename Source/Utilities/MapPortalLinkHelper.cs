namespace InsectLairIncident
{
    /// <summary>
    /// Helper pour passer le portal ID entre patches Harmony
    /// lors de la génération de pocket maps
    /// </summary>
    public static class MapPortalLinkHelper
    {
        public static int currentGeneratingPortalID = -1;
    }
}
