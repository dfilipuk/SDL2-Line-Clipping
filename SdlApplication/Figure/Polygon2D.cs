using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Clipping;
using SdlApplication.Extension;
using SDL2;

namespace SdlApplication.Figure
{
    public abstract class Polygon2D
    {
        private readonly double _precision = 0.00001;
        private readonly int _moveStep = 5;
        private readonly double _rotationStep = 2 * Math.PI / 30;

        private Point _center;
        private Point _minPoint;
        private Point _maxPoint;
        private double _rotationAngle;

        protected List<Edge2D> _edges;
        protected List<Point> _initialVertexes;
        protected Dictionary<int, List<double>> _normalInsideVectors;

        public Polygon2D(int centerX, int centerY, double angle, int minX, int maxX, int minY, int maxY)
        {
            _center = new Point(centerX, centerY);
            _rotationAngle = angle;
            _edges = new List<Edge2D>();
            _initialVertexes = new List<Point>();
            _normalInsideVectors = new Dictionary<int, List<double>>();
            SetMovementBorders(minX, maxX, minY, maxY);
        }

        protected abstract void InitializeVertexes();

        public virtual void Draw(IntPtr renderer)
        {
            foreach (Edge2D edge in _edges)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255);

                foreach (var line in edge.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }

                SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

                foreach (var line in edge.NotVisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }
            }
        }

        public PointPosition GetPointPosition(Point point)
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

        public IEnumerable<Edge2D> Planes()
        {
            foreach (Edge2D edge in _edges)
            {
                yield return edge;
            }
        }

        public List<double> GetNormalInsideVectorForPlane(int planeIndex)
        {
            if (!_normalInsideVectors.ContainsKey(planeIndex))
            {
                CalculateNormalInsideVectorForPlane(planeIndex);
            }

            return _normalInsideVectors[planeIndex];
        }

        public void CalculateCurrentPosition()
        {
            int vertexesCount = _initialVertexes.Count;
            _normalInsideVectors.Clear();
            _edges.Clear();

            for (int i = 0; i < vertexesCount; i++)
            {
                int nextVertexInd = (i + 1) % vertexesCount;
                Point start = _initialVertexes[i].RotateAndMove(_rotationAngle, _center);
                Point end = _initialVertexes[nextVertexInd].RotateAndMove(_rotationAngle, _center);
                _edges.Add(new Edge2D(start, end, i));
            }
        }

        public void Rotate(RotateDirection direction)
        {
            switch (direction)
            {
                case RotateDirection.Right:
                    _rotationAngle += _rotationStep / (2 * Math.PI);
                    break;
                case RotateDirection.Left:
                    _rotationAngle -= _rotationStep / (2 * Math.PI);
                    break;
            }
        }

        public bool Move(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return MoveTo(_center.X, _center.Y - _moveStep);
                    break;
                case MoveDirection.Right:
                    return MoveTo(_center.X + _moveStep, _center.Y);
                    break;
                case MoveDirection.Down:
                    return MoveTo(_center.X, _center.Y + _moveStep);
                    break;
                case MoveDirection.Left:
                    return MoveTo(_center.X - _moveStep, _center.Y);
                    break;
                default:
                    return false;
            }
        }

        public bool MoveTo(int x, int y)
        {
            if (x >= _minPoint.X && x <= _maxPoint.X && y >= _minPoint.Y && y <= _maxPoint.Y)
            {
                _center.X = x;
                _center.Y = y;
                return true;
            }

            return false;
        }

        public void SetMovementBorders(int minX, int maxX, int minY, int maxY)
        {
            _minPoint = new Point
            {
                X = minX,
                Y = minY
            };
            _maxPoint = new Point
            {
                X = maxX,
                Y = maxY
            };

            _center.X = _center.X < minX ? minX : _center.X;
            _center.X = _center.X > maxX ? maxX : _center.X;
            _center.Y = _center.Y < minX ? minY : _center.Y;
            _center.Y = _center.Y > maxX ? maxY : _center.Y;
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
