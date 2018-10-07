using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Extension;
using SdlApplication.Figure;

namespace SdlApplication.Clipping
{
    public class ClippingService
    {
        private static readonly double _precision = 0.00001;

        public static ClippingResult ClipLineByPolygon((Point Start, Point End) line, GenericFigure polygon)
        {
            var result = new ClippingResult();
            List<double> lineVector = line.Start.VectorTo(line.End);
            bool crossingPointsExist = false;

            foreach (FigurePlane plane in polygon.Planes())
            {
                if (result.Position != LinePosition.OutsideFully)
                {
                    List<double> insideNormalVector = polygon.GetNormalInsideVectorForPlane(plane.PlaneNumber);
                    List<double> wVector = plane.Start.VectorTo(line.Start);
                    double q = insideNormalVector.ScalarMultiplicationWith(wVector);
                    double p = insideNormalVector.ScalarMultiplicationWith(lineVector);

                    if (Math.Abs(p) <= _precision)
                    {
                        if (q < 0)
                        {
                            result.Position = LinePosition.OutsideFully;
                        }
                    }
                    else
                    {
                        double t = -q / p;
                        var crossPoint = t.GetLinePoint(line);

                        if (crossPoint.IsPointBelongToLine((plane.Start, plane.End)))
                        {
                            if ((p < 0) && (t > result.t0) && (t < result.t1))
                            {
                                result.t1 = t;
                                crossingPointsExist = true;
                            }
                            if ((p > 0) && (t > result.t0) && (t < result.t1))
                            {
                                result.t0 = t;
                                crossingPointsExist = true;
                            }
                        }
                    }
                }
            }

            if (!crossingPointsExist)
            {
                result.Position = GetLinePosition(line, polygon);
            }

            return result;
        }

        private static LinePosition GetLinePosition((Point Start, Point End) line, GenericFigure polygon)
        {
            PointPosition startPointPosition = polygon.GetPointPosition(line.Start);
            PointPosition endPointPosition = polygon.GetPointPosition(line.End);

            if ((startPointPosition == PointPosition.Inside && endPointPosition == PointPosition.Inside)
                || (startPointPosition == PointPosition.OnPlane && endPointPosition == PointPosition.OnPlane)
                || (startPointPosition == PointPosition.OnPlane && endPointPosition == PointPosition.Inside)
                || (startPointPosition == PointPosition.Inside && endPointPosition == PointPosition.OnPlane))
            {
                return LinePosition.InsideFully;
            }

            if ((startPointPosition == PointPosition.Outside && endPointPosition == PointPosition.Outside)
                || (startPointPosition == PointPosition.OnPlane && endPointPosition == PointPosition.Outside)
                || (startPointPosition == PointPosition.Outside && endPointPosition == PointPosition.OnPlane))
            {
                return LinePosition.OutsideFully;
            }

            return LinePosition.InsidePartial;
        }
    }
}
