using System.Collections.Generic;
using System.Drawing;

namespace SdlApplication.Figure
{
    public class FigurePlane
    {
        public int PlaneNumber { get; }
        public Point Start { get; }
        public Point End { get; }

        public List<(Point Start, Point End)> VisibleParts { get; }
        public List<(Point Start, Point End)> NotVisibleParts { get; }

        public FigurePlane(Point start, Point end, int number)
        {
            Start = start;
            End = end;
            PlaneNumber = number;
            VisibleParts = new List<(Point Start, Point End)>();
            NotVisibleParts = new List<(Point Start, Point End)>();
            ResetClipping();
        }

        public void ResetClipping()
        {
            VisibleParts.Clear();
            NotVisibleParts.Clear();
            VisibleParts.Add((Start, End));
        }
    }
}
