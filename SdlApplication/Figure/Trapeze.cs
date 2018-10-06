using System;
using System.Drawing;
using SDL2;

namespace SdlApplication.Figure
{
    public class Trapeze : GenericFigure
    {
        private readonly int _width;
        private readonly int _height;

        public Trapeze(int centerX, int centerY, double angle, int width, int height) : base(centerX, centerY, angle)
        {
            _width = width;
            _height = height;
            InitializeVertexes();
            CalculateCurrentPosition();
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
