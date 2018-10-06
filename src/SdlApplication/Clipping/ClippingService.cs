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

        public static ClippingResult ClipLineByPolygon(Point v0, Point v1, GenericFigure polygon)
        {
            var result = new ClippingResult();
            List<double> lineVector = v0.VectorTo(v1);

            foreach (FigurePlane plane in polygon.Planes())
            {
                if (result.Status == LineStatus.Visible)
                {
                    List<double> insideNormalVector = polygon.GetNormalInsideVectorForPlane(plane.PlaneNumber);
                    List<double> wVector = plane.Start.VectorTo(v0);
                    double q = insideNormalVector.ScalarMultiplicationWith(wVector);
                    double p = insideNormalVector.ScalarMultiplicationWith(lineVector);

                    if (Math.Abs(p) <= _precision)
                    {
                        if (Math.Abs(q) <= _precision)
                        {
                            result.Status = LineStatus.NotVisible;
                        }
                    }
                    else
                    {
                        double t = -q / p;

                        if ((t >= 0) && (t <= 1))
                        {
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
            }

            return result;
        }
    }
}
