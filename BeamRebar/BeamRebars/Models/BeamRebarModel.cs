using System;
using System.Collections.Generic;
using System.Linq;
using BeamRebar.Utils;
namespace BeamRebar.BeamRebars.Models
{
    public class BeamRebarModel
    {
        public XYZ Direction { get; set; }
        public XYZ StartPointTop { get; set; }
        public XYZ EndPointTop { get; set; }
        public XYZ StartPointBot { get; set; }
        public XYZ EndPointBot { get;  set; }
        public double B { get;  set; }
        public double H { get;  set; }
        public XYZ XVector { get;  set; }
        public XYZ ZVector { get;  set; }
        public Plane LeftPlane { get; set; }
        public Plane RightPlane { get; set; }
        public FamilyInstance Beam { get; internal set; }

        public BeamRebarModel(FamilyInstance beam, List<FamilyInstance> columns, Plane leftPlane, Plane rightPlane)
        {
            if (beam == null) throw new ArgumentNullException(nameof(beam));
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            var symbol = beam.Symbol ?? throw new InvalidOperationException("Beam symbol is null.");
            var solid = symbol.GetSolids().OrderByDescending(x => x.Volume).FirstOrDefault()
                        ?? throw new InvalidOperationException("No valid solid found in beam symbol.");
            var bb = solid.GetBoundingBox();
            H = bb.Max.Z - bb.Min.Z;
            B = bb.Max.Y - bb.Min.Y;
            XVector = beam.GetTransform().BasisX;
            ZVector = beam.GetTransform().BasisZ;
            var curve = (beam.Location as LocationCurve)?.Curve
                        ?? throw new InvalidOperationException("Beam location is not a curve.");
            // Adjust the curve by intersecting with columns
            foreach (var column in columns)
            {
                var columnSolid = column.Symbol?.GetSolids().OrderByDescending(x => x.Volume).FirstOrDefault();
                if (columnSolid == null) continue;
                var intersectionResult = columnSolid.IntersectWithCurve(curve, new SolidCurveIntersectionOptions
                {
                    ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside
                });
                if (intersectionResult == null || intersectionResult.SegmentCount == 0)
                    throw new InvalidOperationException("No valid curve segment found after intersection.");
                curve = intersectionResult.GetCurveSegment(0);
            }
            var transform = beam.GetTransform();
            var maxPoint = transform.OfPoint(bb.Max);
            var minPoint = transform.OfPoint(bb.Min);
            var normal = transform.OfVector(XYZ.BasisY);
            LeftPlane = Plane.CreateByNormalAndOrigin(normal, maxPoint);
            RightPlane = Plane.CreateByNormalAndOrigin(normal, minPoint);
            var startPoint = curve.GetEndPoint(0);
            var endPoint = curve.GetEndPoint(1);
            StartPointTop = ProjectPointOntoPlane(startPoint, LeftPlane);
            EndPointTop = ProjectPointOntoPlane(endPoint, LeftPlane);
            StartPointBot = new XYZ(StartPointTop.X, StartPointTop.Y, minPoint.Z);
            EndPointBot = new XYZ(EndPointTop.X, EndPointTop.Y, minPoint.Z);
            // Create a model curve in a transaction
            using (var transaction = new Transaction(DocumentUtils.Document, "Create line"))
            {
                try
                {
                    transaction.Start();
                    var sketchPlane = SketchPlane.Create(DocumentUtils.Document, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, StartPointTop));
                    DocumentUtils.Document.Create.NewModelCurve(Line.CreateBound(StartPointTop, EndPointTop), sketchPlane);
                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                    throw;
                }
            }
            Direction = (EndPointTop - StartPointTop).Normalize();
        }
        private XYZ ProjectPointOntoPlane(XYZ point, Plane plane)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (plane == null) throw new ArgumentNullException(nameof(plane));
            var normal = plane.Normal;
            var distance = normal.DotProduct(point - plane.Origin);
            return point - distance * normal;
        }
    }
}
