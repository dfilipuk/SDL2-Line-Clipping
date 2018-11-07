using System;
using System.Collections.Generic;
using SdlApplication.Figure;
using SDL2;

namespace SdlApplication.Drawer
{
    public class RoundPolygonDrawer : IPolygonDrawer
    {
        public void Draw(IntPtr renderer, List<Edge2D> edges)
        {
            bool drawCurrentNotVisiblePlane = true;

            foreach (Edge2D edge in edges)
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
    }
}
