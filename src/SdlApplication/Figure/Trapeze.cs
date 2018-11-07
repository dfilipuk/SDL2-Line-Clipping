using System;
using System.Drawing;
using SdlApplication.Utils;
using SDL2;

namespace SdlApplication.Figure
{
    public class Trapeze : Polygon2D
    {
        private readonly int _dotLength = 10;
        private readonly int _width;
        private readonly int _height;
        private readonly Drawer _drawer;

        public Trapeze(int centerX, int centerY, double angle, int minX, int maxX, int minY, int maxY, int width, int height)
            : base(centerX, centerY, angle, minX, maxX, minY, maxY)
        {
            _width = width;
            _height = height;
            _drawer = new Drawer();
            InitializeVertexes();
            CalculateCurrentPosition();
        }

        public override void Draw(IntPtr renderer)
        {
            foreach (Edge2D edge in _edges)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                foreach (var line in edge.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }

                foreach (var line in edge.NotVisibleParts)
                {
                    _drawer.DrawDottledLine(renderer, line.Start, line.End, _dotLength);
                }
            }
        }

        protected override void InitializeVertexes()
        {
            int halfWidth = _width / 2;
            int halfHeight = _height / 2;

            _initialVertexes.AddRange(new[]
            {
                new Point()
                {
                    X = halfWidth,
                    Y = -halfHeight
                },
                new Point
                {
                    X = 0,
                    Y = -halfHeight
                },
                new Point
                {
                    X = -halfWidth,
                    Y = halfHeight
                },
                new Point
                {
                    X = halfWidth,
                    Y = halfHeight
                },
            });
        }
    }
}
