namespace SpeedStatsASRT
{
    public class ExitHandler
    {
        private const uint SC_CLOSE = 0xF060;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_BYCOMMAND = 0x00000000;

        private static NativeMethods.HandlerRountine _handlerRountine;

        public static void Init(NativeMethods.HandlerRountine handlerRountine)
        {
            _handlerRountine = handlerRountine;
            NativeMethods.SetConsoleCtrlHandler(_handlerRountine, true);
            DisableConsoleClose();
        }

        private static void DisableConsoleClose()
        {
            var hMenu = NativeMethods.GetSystemMenu(NativeMethods.GetConsoleWindow(), false);
            NativeMethods.EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED); //disables the upper-right Close (X) button in the titlebar
            NativeMethods.RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND); //removes the Close option in the Alt-Space menu
        }
    }
}