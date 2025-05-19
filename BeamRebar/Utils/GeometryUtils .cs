using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamRebar.Utils
{
    public static class GeometryUtils
    {
        public static List<Solid> GetSolids(this Element element)
        {
            List<Solid> solids = new List<Solid>();
            Options options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = false };

            GeometryElement geoElement = element.get_Geometry(options);
            if (geoElement == null) return solids;

            foreach (GeometryObject geoObj in geoElement)
            {
                if (geoObj is Solid solid && solid.Volume > 0)
                {
                    solids.Add(solid);
                }
                else if (geoObj is GeometryInstance geoInstance)
                {
                    GeometryElement instanceGeometry = geoInstance.GetSymbolGeometry();
                    foreach (GeometryObject instObj in instanceGeometry)
                    {
                        if (instObj is Solid instSolid && instSolid.Volume > 0)
                            solids.Add(instSolid);
                    }
                }
            }

            return solids;
        }
    }
}
