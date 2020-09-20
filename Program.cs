using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using ScannerAll;
//using KBWait;

namespace Proh
{
    static class Program
    {
        //public static MessageHooker oKeyW;				// ������-���������� �������
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        //------ Hide/Show Taskbar and Taskmanager
        private const int SW_HIDE = 0x00;
        private const int SW_SHOW = 0x0001;

        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern bool EnableWindow(IntPtr hwnd, bool enabled);

        [DllImport("coredll.dll", SetLastError = true)]
        public extern static bool SetRect(ref Rectangle r, int xLeft, int yTop, int xRight, int yBottom);

        [DllImport("coredll.dll", SetLastError = true)]
        public extern static bool SystemParametersInfo(int Act, int Pars, ref Rectangle r, int WinIni);


        [DllImport("touch.dll", SetLastError = true)]
        public extern static void TouchPanelDisable();
        [DllImport("touch.dll", SetLastError = true)]
        public extern static bool TouchPanelEnable(IntPtr CallBackFunc);



        private static void ShowTaskbar()
        {
            IntPtr h = FindWindow("HHTaskBar", "");
            ShowWindow(h, SW_SHOW);
            EnableWindow(h, true);
        }
        private static void HideTaskbar()
        {
            IntPtr h = FindWindow("HHTaskBar", "");
            ShowWindow(h, SW_HIDE);
            EnableWindow(h, false);
        }


        [MTAThread]
        static void Main()
        {
            if (false == Is1st(true))
                return;

            // ������
            BarcodeScanner xBCScanner = BarcodeScannerFacade.GetBarcodeScanner(null);

            // ����� ������� ������� - ���� �����
            Rectangle rtDesktop, rtNew;

            rtDesktop = Screen.PrimaryScreen.Bounds;
            rtNew = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            // ��������� �����
            //TouchPanelDisable();


            //oKeyW = new MessageHooker();			// �������� �������������
            //oKeyW.SetHook();						// ��������� �����������

            //Rectangle rtNew = new Rectangle();
            SetRect(ref rtNew, 0, 0, 240, 320);

            SystemParametersInfo(47, 0, ref rtNew, 1);

            HideTaskbar();

            MainF frmMain = new MainF(xBCScanner, rtNew);
            Application.Run(frmMain);

            ShowTaskbar();

            SystemParametersInfo(47, 0, ref rtDesktop, 1);
            //mutex.Close();
            Is1st(false);

        }

        static System.IO.FileStream fsFlag = null;
        static bool Is1st(bool bOnEnter)
        {
            bool bRet = false;

            string sTmp = @"\tmponly";

            if (bOnEnter == true)
            {
                try
                {
                    fsFlag = System.IO.File.Create(sTmp);
                    bRet = true;
                }
                catch{}
            }
            else
            {
                if (fsFlag != null)
                    fsFlag.Close();
            }

            return (bRet);

            //System.Diagnostics.Process[] prc = 
            //    System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);

            //return( (prc.Length = 1)?true:false ); 

        }
    }
}
