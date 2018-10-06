using System;
using System.Drawing;

namespace SdlApplication.Extension
{
    public static class AffineTransformation
    {
        public static Point RotateAndMove(this Point initialPoint, double angle, Point center)
        {
            Point result = new Point
            {
                X = (int)Math.Round(initialPoint.X * Math.Cos(angle) - initialPoint.Y * Math.Sin(angle)) + center.X,
                Y = (int)Math.Round(initialPoint.X * Math.Sin(angle) + initialPoint.Y * Math.Cos(angle)) + center.Y
            };
            return result;
        }
    }
}
