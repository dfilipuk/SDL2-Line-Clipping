using System;
using System.Collections.Generic;
using System.Drawing;
using SdlApplication.Extension;
using SdlApplication.Figure;
using SDL2;

namespace SdlApplication.Drawer
{
    public class SquarePolygonDrawer : IPolygonDrawer
    {
        private readonly int _dotLength = 10;

        public void Draw(IntPtr renderer, List<Edge2D> edges)
        {
            foreach (Edge2D edge in edges)
            {
                SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

                foreach (var line in edge.VisibleParts)
                {
                    SDL.SDL_RenderDrawLine(renderer, line.Start.X, line.Start.Y, line.End.X, line.End.Y);
                }

                foreach (var line in edge.NotVisibleParts)
                {
                    DrawDottledLine(renderer, line.Start, line.End, _dotLength);
                }
            }
        }

        private void DrawDottledLine(IntPtr renderer, Point start, Point end, int dottleLength)
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
