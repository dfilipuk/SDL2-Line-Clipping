namespace SdlApplication.Clipping
{
    public class ClippingResult
    {
        public LineStatus Status { get; set; }
        public double t0 { get; set; }
        public double t1 { get; set; }

        public ClippingResult()
        {
            Status = LineStatus.InsideFullyOrPartial;
            t0 = 0;
            t1 = 1;
        }
    }
}
