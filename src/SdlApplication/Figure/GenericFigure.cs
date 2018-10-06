using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Extension;

namespace SdlApplication.Figure
{
    public abstract class GenericFigure
    {
        private readonly int _moveStep = 5;
        private readonly double _rotationStep = 2 * Math.PI / 50;

        private Point _center;
        private double _rotationAngle;

        protected List<FigurePlane> _planes;
        protected List<Point> _initialVertexes;
        protected Dictionary<int, List<double>> _normalInsideVectors;

        public GenericFigure(int centerX, int centerY, double angle)
        {
            _center = new Point(centerX, centerY);
            _rotationAngle = angle;
            _planes = new List<FigurePlane>();
            _initialVertexes = new List<Point>();
            _normalInsideVectors = new Dictionary<int, List<double>>();
        }

        protected abstract void InitializeVertexes();

        public abstract void Draw(IntPtr renderer);

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

            for (int i = 0; i < vertexesCount; i++)
            {
                int nextVertexInd = (i + 1) % vertexesCount;
                _planes[i].Start = _initialVertexes[i].RotateAndMove(_rotationAngle, _center);
                _planes[i].End = _initialVertexes[nextVertexInd].RotateAndMove(_rotationAngle, _center);
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

        public void Move(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    _center.Y -= _moveStep;
                    break;
                case MoveDirection.Right:
                    _center.X += _moveStep;
                    break;
                case MoveDirection.Down:
                    _center.Y += _moveStep;
                    break;
                case MoveDirection.Left:
                    _center.X -= _moveStep;
                    break;
            }
        }

        private void CalculateNormalInsideVectorForPlane(int planeInd)
        {
            int nextPlaneId = (planeInd + 1) % _initialVertexes.Count;
            List<double> planeVector = _planes[planeInd].Start.VectorTo(_planes[planeInd].End);
            List<double> testVector = _planes[planeInd].Start.VectorTo(_planes[nextPlaneId].End);
            List<double> planeInsideNormalVector = new List<double>();
            planeInsideNormalVector.AddRange(new[] { -planeVector[1] / planeVector[0], 1 });

            if (planeInsideNormalVector.ScalarMultiplicationWith(testVector) < 0)
            {
                planeInsideNormalVector.MultiplyByScalar(-1);
            }

            _normalInsideVectors.Add(planeInd, planeInsideNormalVector);
        }
    }
}
