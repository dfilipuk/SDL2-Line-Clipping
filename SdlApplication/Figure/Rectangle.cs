using System;
using System.Drawing;
using SdlApplication.Utils;
using SDL2;

namespace SdlApplication.Figure
{
    public class Rectangle : GenericFigure
    {
        private readonly int _width;
        private readonly int _height;

        public Rectangle(int centerX, int centerY, double angle, int width, int height) : base(centerX, centerY, angle)
        {
            _width = width;
            _height = height;
            InitializeVertexes();
            CalculateCurrentPosition();
        }

        public override void Draw(IntPtr renderer)
        {
            foreach (FigurePlane plane in _planes)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                foreach (var line in plane.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }
            }
        }

        protected override void InitializeVertexes()
        {
            int halfWidth = _width / 2;
            int halfHeight = _height / 2;

            _initialVertexes.AddRange(new []
            {
                new Point
                {
                    X = halfWidth,
                    Y = -halfHeight
                },
                new Point
                {
                    X = -halfWidth,
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
