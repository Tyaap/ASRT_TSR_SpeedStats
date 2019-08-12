namespace SpeedStatsTSR
{
    struct Graph
    {
        public float[] dataPoints;
        public int width;
        public int height;
        public int margin;
        public float minValue;
        public float maxValue;
        public int flags;
        public int colour;
    }

    public enum CurrentModeEnum
    {
        Car,
        Boat,
        Plane,
    }

    public enum CtrlType
    {
        CtrlCEvent = 0,
        CtrlBreakEvent = 1,
        CtrlCloseEvent = 2,
        CtrlLogoffEvent = 5,
        CtrlShutdownEvent = 6
    }
}
