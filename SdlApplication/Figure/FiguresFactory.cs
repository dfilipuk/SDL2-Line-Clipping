//#define CLIPPING_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Drawer;

namespace SdlApplication.Figure
{
    public static class FiguresFactory
    {
        private static readonly int _ellipseVertexesCount = 25;

#if CLIPPING_DEBUG
        private static readonly IPolygonDrawer _rectangeDrawer = new UniversalPolygonDrawer();
        private static readonly IPolygonDrawer _trapezeDrawer = new UniversalPolygonDrawer();
        private static readonly IPolygonDrawer _ellipseDrawer = new UniversalPolygonDrawer();
#else
        private static readonly IPolygonDrawer _rectangeDrawer = new SquarePolygonDrawer();
        private static readonly IPolygonDrawer _trapezeDrawer = new SquarePolygonDrawer();
        private static readonly IPolygonDrawer _ellipseDrawer = new RoundPolygonDrawer();
#endif

        public static MovablePolygon2D CreateRectangle(int centerX, int centerY, double angle, int minX, int maxX,
            int minY, int maxY, int width, int height)
        {
            var vertexes = new List<Point>();
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            vertexes.AddRange(new[]
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

            return new MovablePolygon2D(vertexes, _rectangeDrawer, centerX, centerY, angle, minX, maxX, minY,
                maxY);
        }

        public static MovablePolygon2D CreateTrapeze(int centerX, int centerY, double angle, int minX, int maxX,
            int minY, int maxY, int width, int height)
        {
            var vertexes = new List<Point>();
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            vertexes.AddRange(new[]
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

            return new MovablePolygon2D(vertexes, _trapezeDrawer, centerX, centerY, angle, minX, maxX, minY,
                maxY);
        }

        public static MovablePolygon2D CreateEllipse(int centerX, int centerY, double angle, int minX, int maxX, int minY, int maxY, int a, int b)
        {
            var vertexes = new List<Point>();
            double step = 2 * Math.PI / _ellipseVertexesCount;
            double currentAngle = 2 * Math.PI;

            for (int i = 0; i < _ellipseVertexesCount; i++, currentAngle -= step)
            {
                vertexes.Add(new Point
                {
                    X = (int)Math.Round(a * Math.Cos(currentAngle)),
                    Y = (int)Math.Round(b * Math.Sin(currentAngle)),
                });
            }

            return new MovablePolygon2D(vertexes, _ellipseDrawer, centerX, centerY, angle, minX, maxX, minY,
                maxY);
        }
    }
}
