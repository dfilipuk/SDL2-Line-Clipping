using System;
using System.Drawing;
using SDL2;

namespace SdlApplication.Figure
{
    public class Ellipse : Polygon2D
    {
        private readonly int _vertexesCount = 25;
        private readonly int _a;
        private readonly int _b;

        public Ellipse(int centerX, int centerY, double angle, int minX, int maxX, int minY, int maxY, int a, int b) 
            : base(centerX, centerY, angle, minX, maxX, minY, maxY)
        {
            _a = a;
            _b = b;
            InitializeVertexes();
            CalculateCurrentPosition();
        }

        public override void Draw(IntPtr renderer)
        {
            bool drawCurrentNotVisiblePlane = true;

            foreach (Edge2D edge in _edges)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                foreach (var line in edge.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                    drawCurrentNotVisiblePlane = true;
                }

                if (edge.VisibleParts.Count == 0)
                {
                    if (drawCurrentNotVisiblePlane)
                    {
                        foreach (var line in edge.NotVisibleParts)
                        {
                            SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                        }
                    }
                    drawCurrentNotVisiblePlane = !drawCurrentNotVisiblePlane;
                }
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
            }
        }
    }
}
