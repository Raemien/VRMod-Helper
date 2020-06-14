using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Valve.VR;

namespace VRModHelper
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetForegroundWindow(IntPtr p);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        static readonly string[] GMOD_PROCESSES = { "hl2", "gmod" }; // For 32 + 64-bit gmod support
        static void StartVRMod()
        {
            Process[] gmodprocs = null;
            Process realproc = null;
            //Process gmod = Process.GetProcessesByName("notepad++.exe").First();
            for (int i = 0; i < GMOD_PROCESSES.Length; i++)
            {
                gmodprocs = Process.GetProcessesByName(GMOD_PROCESSES[i]);
            }

            for (int i = 0; i < gmodprocs.Length; i++)
            {
                if (gmodprocs[i].MainWindowTitle.Contains("Garry's Mod"))
                {
                    realproc = gmodprocs[i];
                    Thread.Sleep(50);
                    Console.WriteLine("Launching VRMod...");
                    SetForegroundWindow(realproc.MainWindowHandle);
                }

            }
            System.Diagnostics.Process.Start(Application.ExecutablePath);
            Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_NUMPAD5, false, Keyboard.InputType.Keyboard);
            Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_NUMPAD5, true, Keyboard.InputType.Keyboard);


            Thread.Sleep(500);
            SetForegroundWindow(realproc.MainWindowHandle);
            Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_LEFTMOUSEBUTTON, false, Keyboard.InputType.Mouse);
            Keyboard.SendKey(Keyboard.DirectXKeyStrokes.DIK_LEFTMOUSEBUTTON, true, Keyboard.InputType.Mouse);
        }

        static void Main(string[] args)
        {
            Thread.Sleep(100);
            bool vrmodStarted = false;
            Console.TreatControlCAsInput = false;
            string ResourcePath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName + "/Resources/";

            // init
            var error = EVRInitError.None;

            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
            if (error != EVRInitError.None) throw new Exception();

            OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
            if (error != EVRInitError.None) throw new Exception();

            OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
            if (error != EVRInitError.None) throw new Exception();


            // create overlay, ...
            var overlay = OpenVR.Overlay;

            ulong overlayHandle = 0, thumbnailHandle = 0;

            overlay.CreateDashboardOverlay("VRModOverlay" + new Random().Next(1, 4096), "Start VRMod", ref overlayHandle, ref thumbnailHandle);
            overlay.SetOverlayFromFile(thumbnailHandle, $"{ResourcePath}/vrmod_icon.png");

            overlay.SetOverlayWidthInMeters(overlayHandle, 2.5f);
            overlay.SetOverlayInputMethod(overlayHandle, VROverlayInputMethod.Mouse);
            Console.CancelKeyPress += (s, e) => overlay.DestroyOverlay(overlayHandle);

            bool gmodActive = (Process.GetProcessesByName("hl2") != null || Process.GetProcessesByName("gmod") != null);
            Console.WriteLine(gmodActive ? "Found Garry's Mod." : "Could not find Garry's Mod.");

            Console.WriteLine("Ready to launch VRMod.\nOnce your game has loaded, select the 'Start VRMod' button inside your SteamVR dashboard.");

            while (true)
            {
                if (overlay.IsOverlayVisible(overlayHandle))
                {
                    overlay.SetOverlayFromFile(overlayHandle, $"{ResourcePath}/conceptbackground.jpg");
                    Thread.Sleep(200);
                    if (!vrmodStarted)
                    {
                        Thread launchVRMod = new Thread(StartVRMod);
                        StartVRMod();
                        System.Environment.Exit(0);
                        vrmodStarted = true;
                    }
                }
                vrmodStarted = !overlay.IsOverlayVisible(overlayHandle);
            }
        }
    }
}