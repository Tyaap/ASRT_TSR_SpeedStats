using System;
using System.Diagnostics;
using System.Drawing;
using RTSSSharedMemoryNET;

namespace SpeedStatsTSR
{
    class Program
    {
        static Graph speedGraph = new Graph
        {
            dataPoints = new float[400],
            width = 800,
            height = 300,
            margin = 0,
            minValue = 0,
            maxValue = 350,
            flags = 0,
            colour = Color.Magenta.ToArgb()
        };

        static Graph accelGraph = new Graph
        {
            dataPoints = new float[400],
            width = 800,
            height = 300,
            margin = 0,
            minValue = -20,
            maxValue = 20,
            flags = 0,
            colour = Color.Green.ToArgb()
        };

        static UIntPtr processHandle;
        static OSD osd;
        static float speed = 0;
        static float oldSpeed = 0;
        static int timeout = 0;
        static bool validHandle = false;

        static void Main(string[] args)
        {
            //enforces a nice cleanup
            //just hitting X or Ctrl+C normally won't actually dispose the using() below
            /*
            ExitHandler.Init(ctrlType =>
            {
                Console.WriteLine("\nCleaning up and exiting...");
                return true; //cancel event
            });
            */

            ///////////////////////////////////////////////////////////////////

            /*
            Console.WriteLine("Current OSD entries:");
            var osdEntries = OSD.GetOSDEntries();
            foreach (var osd in osdEntries)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(osd.Owner);
                Console.ResetColor();
                Console.WriteLine("{0}\n", osd.Text);
            }
            */

            ///////////////////////////////////////////////////////////////////

            /*
            Console.WriteLine("Current app entries with GPU contexts:");
            var appEntries = OSD.GetAppEntries().Where(x => (x.Flags & AppFlags.MASK) != AppFlags.None).ToArray();
            foreach (var app in appEntries)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("{0}:{1}", app.ProcessId, app.Name);
                Console.ResetColor();
                Console.WriteLine("{0}, {1}FPS", app.Flags, app.InstantaneousFrames);
            }
            Console.WriteLine();
            */

            ///////////////////////////////////////////////////////////////////

            while (true)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName("GameApp_PcDx11_x64Final");
                    processHandle = NativeMethods.OpenProcess(16, false, processes[0].Id);
                    validHandle = true;
                    Console.WriteLine("Connected to TSR!");
                    osd = new OSD("TSR_Speedo");

                    Console.WriteLine("Connected to Rivatuner!");
                    Console.WriteLine("The speed stats will now be displayed.");
                    UpdateOverlay(osd);
                    while (true)
                    {
                        System.Threading.Thread.Sleep(10);
                        GetSpeed();
                        if (!validHandle)
                        {
                            speedGraph.dataPoints = new float[speedGraph.dataPoints.Length];
                            accelGraph.dataPoints = new float[accelGraph.dataPoints.Length];
                            throw new System.IO.IOException("Invalid process handle.");
                        }
                        else if (speed != oldSpeed)
                        {
                            UpdateOverlay(osd);
                            timeout = 0;
                        }
                        else if (timeout < 500)
                        {
                            timeout++;
                        }
                        else
                        {
                            osd.Update("");
                        }
                    }

                }
                catch (Exception e)
                {
                    if (e is IndexOutOfRangeException)
                    {
                        Console.WriteLine("Failed to connect to TSR. Waiting 5 seconds before trying again...");
                    }
                    else if (e is System.IO.FileNotFoundException)
                    {
                        Console.WriteLine("Failed to connect to Rivatuner. Waiting 5 seconds before trying again...");
                    }
                    else if (e is System.IO.InvalidDataException)
                    {
                        Console.WriteLine("Lost connection to Rivatuner. Waiting 5 seconds before trying again...");
                        osd = null;
                    }
                    else if (e is System.IO.IOException)
                    {
                        Console.WriteLine("Lost connection to TSR. Waiting 5 seconds before trying again...");
                        osd.Dispose();
                    }
                    else
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }

        }

        public static void UpdateOverlay(OSD osd)
        {
            for (int i = 0; i < speedGraph.dataPoints.Length - 1; i++)
            {
                speedGraph.dataPoints[i] = speedGraph.dataPoints[i + 1];
            }
            for (int i = 0; i < accelGraph.dataPoints.Length - 1; i++)
            {
                accelGraph.dataPoints[i] = accelGraph.dataPoints[i + 1];
            }

            speedGraph.dataPoints[speedGraph.dataPoints.Length - 1] = speed;
            accelGraph.dataPoints[accelGraph.dataPoints.Length - 1] = speed - oldSpeed;

            string osdText = "";
            osdText += string.Format("<C0={0:X8}>", speedGraph.colour);
            osdText += string.Format("<C1={0:X8}>", accelGraph.colour);

            unsafe
            {
                uint offset = 0;
                fixed (float* ptr_f = speedGraph.dataPoints)
                {
                    osdText += string.Format("<C0>Speed: {1}\n<OBJ={0:X8}><A0><S1><C>\n", offset, speed.ToString("0.0000"));
                    offset += osd.EmbedGraph(offset, ptr_f, 0, (uint)speedGraph.dataPoints.Length, speedGraph.width,
                        speedGraph.height, speedGraph.margin, speedGraph.minValue, speedGraph.maxValue, (uint)speedGraph.flags);
                }
                fixed (float* ptr_f = accelGraph.dataPoints)
                {
                    osdText += string.Format("<C1>Accel: {1}\n<OBJ={0:X8}><C>\n", offset, (speed - oldSpeed).ToString("0.0000"));
                    offset += osd.EmbedGraph(offset, ptr_f, 0, (uint)speedGraph.dataPoints.Length, speedGraph.width,
                        accelGraph.height, accelGraph.margin, accelGraph.minValue, accelGraph.maxValue, (uint)accelGraph.flags);
                }
            }

            osd.Update(string.Format(osdText));
        }

        public static bool GetSpeed()
        {
            byte[] lpBuffer = new byte[4];
            if (!NativeMethods.ReadProcessMemory(processHandle, GetCarPointer(), lpBuffer, (UIntPtr)4, UIntPtr.Zero))
            {
                oldSpeed = 0;
                speed = 0;
                return false;
            }
            oldSpeed = speed;
            speed = Math.Abs(BitConverter.ToSingle(lpBuffer, 0) * 4);
            return true;
        }

        public static UIntPtr GetCarPointer()
        {
            byte[] lpBuffer = new byte[8];
            validHandle = NativeMethods.ReadProcessMemory(processHandle, (UIntPtr)0x1410B2548, lpBuffer, (UIntPtr)8, UIntPtr.Zero);
            NativeMethods.ReadProcessMemory(processHandle, (UIntPtr)BitConverter.ToUInt64(lpBuffer, 0) + 0x8, lpBuffer, (UIntPtr)8, UIntPtr.Zero);
            return (UIntPtr)BitConverter.ToUInt64(lpBuffer, 0) + 0xA8;
        }
    }
}