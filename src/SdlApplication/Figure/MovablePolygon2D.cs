using System;
using System.Collections.Generic;
using System.Drawing;
using Clipping2D.Polygon;
using SdlApplication.Extension;

namespace SdlApplication.Figure
{
    public class MovablePolygon2D : Polygon2D
    {
        private readonly int _moveStep = 5;
        private readonly double _rotationStep = 2 * Math.PI / 30;

        private Point _center;
        private Point _minPoint;
        private Point _maxPoint;
        private double _rotationAngle;

        public MovablePolygon2D(List<Point> vertexes, int centerX, int centerY, double angle, 
            int minX, int maxX, int minY, int maxY) : base(vertexes)
        {
            _center = new Point(centerX, centerY);
            _rotationAngle = angle;
            SetMovementBorders(minX, maxX, minY, maxY);
            CalculateCurrentEdges();
        }

        public void CalculateCurrentEdges()
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
                case MoveDirection.Right:
                    return MoveTo(_center.X + _moveStep, _center.Y);
                case MoveDirection.Down:
                    return MoveTo(_center.X, _center.Y + _moveStep);
                case MoveDirection.Left:
                    return MoveTo(_center.X - _moveStep, _center.Y);
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
    }
}
