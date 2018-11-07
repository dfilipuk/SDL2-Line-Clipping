using System;
using System.Collections.Generic;
using System.Drawing;
using Clipping2D.Clipping;
using Clipping2D.Drawer;
using Clipping2D.Extension;

namespace Clipping2D.Polygon
{
    public class Polygon2D
    {
        private readonly double _precision = 0.00001;

        private IPolygonDrawer _polygonDrawer;

        protected List<Edge2D> _edges;
        protected List<Point> _initialVertexes;
        protected Dictionary<int, List<double>> _normalInsideVectors;

        internal IEnumerable<Edge2D> Edges
        {
            get
            {
                foreach (Edge2D edge in _edges)
                {
                    yield return edge;
                }
            }
        }

        public Polygon2D(List<Point> vertexes, IPolygonDrawer polygonDrawer)
        {
            _initialVertexes = vertexes;
            _normalInsideVectors = new Dictionary<int, List<double>>();
            _edges = new List<Edge2D>();
            _polygonDrawer = polygonDrawer;

            CalculateInitialEdges();
        }

        public void Draw(IntPtr renderer)
        {
            _polygonDrawer.Draw(renderer, _edges);
        }

        public void ResetClipping()
        {
            foreach (var plane in _edges)
            {
                plane.ResetClipping();
            }
        }

        public void ClipByPolygon(Polygon2D polygon, ClippingType type)
        {
            foreach (Edge2D edge in _edges)
            {
                var visibleParts = edge.VisibleParts.ToArray();
                edge.VisibleParts.Clear();

                foreach (var visiblePart in visibleParts)
                {
                    ClippingResult result = ClippingService.ClipLineByPolygon(visiblePart, polygon);

                    if (result.Position == LinePosition.OutsideFully)
                    {
                        if (type == ClippingType.Inside)
                        {
                            edge.NotVisibleParts.Add(visiblePart);
                        }
                        else if (type == ClippingType.External)
                        {
                            edge.VisibleParts.Add(visiblePart);
                        }
                    }
                    else if (result.Position == LinePosition.InsideFully)
                    {
                        if (type == ClippingType.Inside)
                        {
                            edge.VisibleParts.Add(visiblePart);
                        }
                        else if (type == ClippingType.External)
                        {
                            edge.NotVisibleParts.Add(visiblePart);
                        }
                    }
                    else if (result.Position == LinePosition.InsidePartial)
                    {
                        if ((result.t0 == 0) && (result.t1 != 1))
                        {
                            var crossPoint = result.t1.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                edge.VisibleParts.Add((visiblePart.Start, crossPoint));
                                edge.NotVisibleParts.Add((crossPoint, visiblePart.End));
                            }
                            else if (type == ClippingType.External)
                            {
                                edge.NotVisibleParts.Add((visiblePart.Start, crossPoint));
                                edge.VisibleParts.Add((crossPoint, visiblePart.End));
                            }
                        }
                        else if ((result.t0 != 0) && (result.t1 == 1))
                        {
                            var crossPoint = result.t0.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                edge.VisibleParts.Add((crossPoint, visiblePart.End));
                                edge.NotVisibleParts.Add((visiblePart.Start, crossPoint));
                            }
                            else if (type == ClippingType.External)
                            {
                                edge.NotVisibleParts.Add((crossPoint, visiblePart.End));
                                edge.VisibleParts.Add((visiblePart.Start, crossPoint));
                            }
                        }
                        else
                        {
                            var crossPoint1 = result.t0.GetLinePoint(visiblePart);
                            var crossPoint2 = result.t1.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                edge.VisibleParts.Add((crossPoint1, crossPoint2));
                                edge.NotVisibleParts.Add((visiblePart.Start, crossPoint1));
                                edge.NotVisibleParts.Add((crossPoint2, visiblePart.End));
                            }
                            else if (type == ClippingType.External)
                            {
                                edge.NotVisibleParts.Add((crossPoint1, crossPoint2));
                                edge.VisibleParts.Add((visiblePart.Start, crossPoint1));
                                edge.VisibleParts.Add((crossPoint2, visiblePart.End));
                            }
                        }
                    }
                }
            }
        }

        internal List<double> GetNormalInsideVectorForPlane(int planeIndex)
        {
            if (!_normalInsideVectors.ContainsKey(planeIndex))
            {
                CalculateNormalInsideVectorForPlane(planeIndex);
            }

            return _normalInsideVectors[planeIndex];
        }

        internal PointPosition GetPointPosition(Point point)
        {
            PointPosition result = PointPosition.Inside;

            for (int i = 0; i < _edges.Count && result == PointPosition.Inside; i++)
            {
                var testVector = point.VectorTo(_edges[i].Start);
                var normalInsideVector = GetNormalInsideVectorForPlane(i);
                double scalarMultiplication = testVector.ScalarMultiplicationWith(normalInsideVector);

                if (scalarMultiplication > 0)
                {
                    result = PointPosition.Outside;
                }
                else if (Math.Abs(scalarMultiplication) <= _precision)
                {
                    if (point.IsPointBelongToLine((_edges[i].Start, _edges[i].End)))
                    {
                        result = PointPosition.OnPlane;
                    }
                    else
                    {
                        result = PointPosition.Outside;
                    }
                }
            }

            return result;
        }

        private void CalculateInitialEdges()
        {
            int vertexesCount = _initialVertexes.Count;
            _normalInsideVectors.Clear();
            _edges.Clear();

            for (int i = 0; i < vertexesCount; i++)
            {
                int nextVertexInd = (i + 1) % vertexesCount;
                _edges.Add(new Edge2D(_initialVertexes[i], _initialVertexes[nextVertexInd], i));
            }
        }

        private void CalculateNormalInsideVectorForPlane(int planeInd)
        {
            int nextPlaneId = (planeInd + 1) % _initialVertexes.Count;
            List<double> planeVector = _edges[planeInd].Start.VectorTo(_edges[planeInd].End);
            List<double> testVector = _edges[planeInd].Start.VectorTo(_edges[nextPlaneId].End);
            List<double> planeInsideNormalVector = new List<double>();

            if (planeVector[0] != 0)
            {
                planeInsideNormalVector.AddRange(new[] { -planeVector[1] / planeVector[0], 1 });
            }
            else
            {
                planeInsideNormalVector.AddRange(new[] { 1D, 0 });
            }

            if (planeInsideNormalVector.ScalarMultiplicationWith(testVector) < 0)
            {
                planeInsideNormalVector.MultiplyByScalar(-1);
            }

            _normalInsideVectors.Add(planeInd, planeInsideNormalVector);
        }
    }
}
