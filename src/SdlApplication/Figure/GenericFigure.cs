using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Clipping;
using SdlApplication.Extension;
using SDL2;

namespace SdlApplication.Figure
{
    public abstract class GenericFigure
    {
        private readonly double _precision = 0.00001;
        private readonly int _moveStep = 25;
        private readonly double _rotationStep = 2 * Math.PI / 10;

        private Point _center;
        private Point _minPoint;
        private Point _maxPoint;
        private double _rotationAngle;

        protected List<FigurePlane> _planes;
        protected List<Point> _initialVertexes;
        protected Dictionary<int, List<double>> _normalInsideVectors;

        public GenericFigure(int centerX, int centerY, double angle, int minX, int maxX, int minY, int maxY)
        {
            _center = new Point(centerX, centerY);
            _rotationAngle = angle;
            _planes = new List<FigurePlane>();
            _initialVertexes = new List<Point>();
            _normalInsideVectors = new Dictionary<int, List<double>>();
            SetMovementBorders(minX, maxX, minY, maxY);
        }

        protected abstract void InitializeVertexes();

        public virtual void Draw(IntPtr renderer)
        {
            foreach (FigurePlane plane in _planes)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 255);

                foreach (var line in plane.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }

                SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

                foreach (var line in plane.NotVisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }
            }
        }

        public PointPosition GetPointPosition(Point point)
        {
            PointPosition result = PointPosition.Inside;

            for (int i = 0; i < _planes.Count && result == PointPosition.Inside; i++)
            {
                var testVector = point.VectorTo(_planes[i].Start);
                var normalInsideVector = GetNormalInsideVectorForPlane(i);
                double scalarMultiplication = testVector.ScalarMultiplicationWith(normalInsideVector);

                if (scalarMultiplication > 0)
                {
                    result = PointPosition.Outside;
                }
                else if (Math.Abs(scalarMultiplication) <= _precision)
                {
                    if (point.IsPointBelongToLine((_planes[i].Start, _planes[i].End)))
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
            foreach (var plane in _planes)
            {
                plane.ResetClipping();
            }
        }

        public void ClipByPolygon(GenericFigure polygon, ClippingType type)
        {
            foreach (FigurePlane plane in _planes)
            {
                var visibleParts = plane.VisibleParts.ToArray();
                plane.VisibleParts.Clear();

                foreach (var visiblePart in visibleParts)
                {
                    ClippingResult result = ClippingService.ClipLineByPolygon(visiblePart, polygon);

                    if (result.Position == LinePosition.OutsideFully)
                    {
                        if (type == ClippingType.Inside)
                        {
                            plane.NotVisibleParts.Add(visiblePart);
                        }
                        else if (type == ClippingType.External)
                        {
                            plane.VisibleParts.Add(visiblePart);
                        }
                    }
                    else if (result.Position == LinePosition.InsideFully)
                    {
                        if (type == ClippingType.Inside)
                        {
                            plane.VisibleParts.Add(visiblePart);
                        }
                        else if (type == ClippingType.External)
                        {
                            plane.NotVisibleParts.Add(visiblePart);
                        }
                    }
                    else if (result.Position == LinePosition.InsidePartial)
                    {
                        if ((result.t0 == 0) && (result.t1 != 1))
                        {
                            var crossPoint = result.t1.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                plane.VisibleParts.Add((visiblePart.Start, crossPoint));
                                plane.NotVisibleParts.Add((crossPoint, visiblePart.End));
                            }
                            else if (type == ClippingType.External)
                            {
                                plane.NotVisibleParts.Add((visiblePart.Start, crossPoint));
                                plane.VisibleParts.Add((crossPoint, visiblePart.End));
                            }
                        }
                        else if ((result.t0 != 0) && (result.t1 == 1))
                        {
                            var crossPoint = result.t0.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                plane.VisibleParts.Add((crossPoint, visiblePart.End));
                                plane.NotVisibleParts.Add((visiblePart.Start, crossPoint));
                            }
                            else if (type == ClippingType.External)
                            {
                                plane.NotVisibleParts.Add((crossPoint, visiblePart.End));
                                plane.VisibleParts.Add((visiblePart.Start, crossPoint));
                            }
                        }
                        else
                        {
                            var crossPoint1 = result.t0.GetLinePoint(visiblePart);
                            var crossPoint2 = result.t1.GetLinePoint(visiblePart);

                            if (type == ClippingType.Inside)
                            {
                                plane.VisibleParts.Add((crossPoint1, crossPoint2));
                                plane.NotVisibleParts.Add((visiblePart.Start, crossPoint1));
                                plane.NotVisibleParts.Add((crossPoint2, visiblePart.End));
                            }
                            else if (type == ClippingType.External)
                            {
                                plane.NotVisibleParts.Add((crossPoint1, crossPoint2));
                                plane.VisibleParts.Add((visiblePart.Start, crossPoint1));
                                plane.VisibleParts.Add((crossPoint2, visiblePart.End));
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<FigurePlane> Planes()
        {
            foreach (FigurePlane plane in _planes)
            {
                yield return plane;
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
            _planes.Clear();

            for (int i = 0; i < vertexesCount; i++)
            {
                int nextVertexInd = (i + 1) % vertexesCount;
                Point start = _initialVertexes[i].RotateAndMove(_rotationAngle, _center);
                Point end = _initialVertexes[nextVertexInd].RotateAndMove(_rotationAngle, _center);
                _planes.Add(new FigurePlane(start, end, i));
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
        }

        private void CalculateNormalInsideVectorForPlane(int planeInd)
        {
            int nextPlaneId = (planeInd + 1) % _initialVertexes.Count;
            List<double> planeVector = _planes[planeInd].Start.VectorTo(_planes[planeInd].End);
            List<double> testVector = _planes[planeInd].Start.VectorTo(_planes[nextPlaneId].End);
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
