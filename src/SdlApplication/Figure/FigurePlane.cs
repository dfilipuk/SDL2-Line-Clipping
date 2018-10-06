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
        public List<(Point Start, Point End)> NotVisiblePartsParts { get; }

        public FigurePlane(Point start, Point end, int number)
        {
            Start = start;
            End = end;
            PlaneNumber = number;
            VisibleParts = new List<(Point Start, Point End)>();
            NotVisiblePartsParts = new List<(Point Start, Point End)>();
            VisibleParts.Add((Start, End));
        }
    }
}
