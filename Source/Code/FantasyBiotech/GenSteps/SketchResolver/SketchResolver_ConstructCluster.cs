using RimWorld.SketchGen;

namespace FantasyBiotech
{
    public class SketchResolver_ConstructCluster : SketchResolver
    {
        public override void ResolveInt(SketchResolveParams parms)
        {
            ConstructClusterGenerator.ResolveSketch(parms);
        }

        public override bool CanResolveInt(SketchResolveParams parms)
        {
            return true;
        }
    }
}
