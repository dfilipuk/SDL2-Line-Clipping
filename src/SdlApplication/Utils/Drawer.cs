using System;
using System.Drawing;
using SdlApplication.Extension;
using SDL2;

namespace SdlApplication.Utils
{
    public class Drawer
    {
        public void DrawDottledLine(IntPtr renderer, Point start, Point end, int dottleLength)
        {
            var lineVector = start.VectorTo(end);
            double lineLength = Math.Sqrt(lineVector[0] * lineVector[0] + lineVector[1] * lineVector[1]);
            double mu = dottleLength / lineLength;

            if (mu < 1)
            {
                double currentMu = mu;
                bool drawCurrentDot = true;
                Point prevPoint = start;

                while (currentMu <= 1)
                {
                    Point currentPoint = currentMu.GetLinePoint((start, end));

                    if (drawCurrentDot)
                    {
                        SDL.SDL_RenderDrawLine(renderer, prevPoint.X, prevPoint.Y, currentPoint.X, currentPoint.Y);
                    }

                    prevPoint = currentPoint;
                    drawCurrentDot = !drawCurrentDot;
                    currentMu += mu;
                }
            }
        }
    }
}
