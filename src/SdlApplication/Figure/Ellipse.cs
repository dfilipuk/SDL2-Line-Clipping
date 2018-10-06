using System;
using System.Drawing;
using SDL2;

namespace SdlApplication.Figure
{
    public class Ellipse : GenericFigure
    {
        private readonly int _vertexesCount = 25;
        private readonly int _a;
        private readonly int _b;

        public Ellipse(int centerX, int centerY, double angle, int a, int b) : base(centerX, centerY, angle)
        {
            _a = a;
            _b = b;
            InitializeVertexes();
            CalculateCurrentPosition();
        }

        public override void Draw(IntPtr renderer)
        {
            foreach (FigurePlane plane in _planes)
            {
                SDL.SDL_RenderDrawLine(renderer, plane.Start.X, plane.Start.Y, plane.End.X, plane.End.Y);
            }
        }

        protected override void InitializeVertexes()
        {
            double step = 2 * Math.PI / _vertexesCount;
            double angle = 2 * Math.PI;

            for (int i = 0; i < _vertexesCount; i++, angle -= step)
            {
                _initialVertexes.Add(new Point
                {
                    X = (int) Math.Round(_a * Math.Cos(angle)),
                    Y = (int) Math.Round(_b * Math.Sin(angle)),
                });
                _planes.Add(new FigurePlane() { PlaneNumber = i });
            }
        }
    }
}
