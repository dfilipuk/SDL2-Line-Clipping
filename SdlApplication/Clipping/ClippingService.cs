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

            foreach (FigurePlane plane in polygon.Planes())
            {
                if (result.Status == LineStatus.InsideFullyOrPartial)
                {
                    List<double> insideNormalVector = polygon.GetNormalInsideVectorForPlane(plane.PlaneNumber);
                    List<double> wVector = plane.Start.VectorTo(line.Start);
                    double q = insideNormalVector.ScalarMultiplicationWith(wVector);
                    double p = insideNormalVector.ScalarMultiplicationWith(lineVector);

                    if (Math.Abs(p) <= _precision)
                    {
                        if (q < 0)
                        {
                            result.Status = LineStatus.OutsideFully;
                        }
                    }
                    else
                    {
                        double t = -q / p;

                        if ((p < 0) && (t > result.t0) && (t < result.t1))
                        {
                            result.t1 = t;
                        }
                        if ((p > 0) && (t > result.t0) && (t < result.t1))
                        {
                            result.t0 = t;
                        }
                    }
                }
            }

            return result;
        }
    }
}
