using Clipping2D.Drawer;

namespace SdlApplication.Drawer
{
    public static class DrawerFactory
    {
        public static IPolygonDrawer UniversalDrawer { get; } = new UniversalPolygonDrawer();
        public static IPolygonDrawer SquareDrawer { get; } = new SquarePolygonDrawer();
        public static IPolygonDrawer RoundDrawer { get; } = new RoundPolygonDrawer();
    }
}
