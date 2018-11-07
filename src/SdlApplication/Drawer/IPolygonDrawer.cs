using System;
using System.Collections.Generic;
using SdlApplication.Figure;

namespace SdlApplication.Drawer
{
    public interface IPolygonDrawer
    {
        void Draw(IntPtr renderer, List<Edge2D> edges);
    }
}
