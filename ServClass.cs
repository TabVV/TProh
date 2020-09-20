using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

using ScannerAll;
using PDA.OS;
using PDA.Service;
using SkladAll;
using ExprDll;

using FRACT = System.Decimal;

namespace Proh
{
    public partial class MainF : Form
    {


        private void SetMainFuncDict(ScannerAll.TERM_TYPE ttX, string sExeDir)
        {
            string[] sH = new string[0];

            xFuncs = new FuncDic(sExeDir + "\\KeyMap.xml");
            if (xFuncs.Loaded == false)
            {
                xFuncs.SetNewFunc(W32.VK_F1,        Keys.None,      AppC.F_HELP,        "",     "справка по командам");
                xFuncs.SetNewFunc(W32.VK_F2,        Keys.None,      AppC.F_UPLD_DOC,    "F2",   " - записать");
                xFuncs.SetNewFunc(W32.VK_F3,        Keys.None,      AppC.F_OPENSH,      "F3",   " - открыть шлагбаум");
                xFuncs.SetNewFunc(W32.VK_F4,        Keys.None,      AppC.F_CLOSESH,     "F4",   " - закрыть шлагбаум");
                xFuncs.SetNewFunc(W32.VK_SPACE,     Keys.None,      AppC.F_CHGZMK,      "SP",   " - смена шлагбаума");
                xFuncs.SetNewFunc(W32.VK_F6,        Keys.None,      AppC.F_MENU,        "F6",   " - меню");
                xFuncs.SetNewFunc(W32.VK_BACK,      Keys.None,      AppC.F_DEL_REC,     "BKSP", " - удалить");
                xFuncs.AddNewFunc(0,                Keys.None,      -1,                 "-><-", " - смена значения");
                xFuncs.SetNewFunc(W32.VK_F7,        Keys.None,      AppC.F_QUIT,        "F7",   " - выход");

                xFuncs.AddNewFunc(W32.VK_F1_PC,     Keys.None,      AppC.F_HELP,        "",     " - помощь");
                xFuncs.AddNewFunc(W32.VK_F2_PC,     Keys.None,      AppC.F_UPLD_DOC,    "",     " - записать");
                xFuncs.AddNewFunc(W32.VK_F7_PC,     Keys.None,      AppC.F_QUIT,        "",     " - выход");
                xFuncs.AddNewFunc(W32.VK_F6_PC,     Keys.None,      AppC.F_MENU,        "",     " - меню");
                switch (ttX)
                {
                    case TERM_TYPE.DL_SCORP:
                        break;
                    case TERM_TYPE.HWELL6100:
                        break;

/*
                        sH = new string[]{
@"F2   - записать данные
F3     - открыть
F4     - закрыть
F5     - смена шлагбаума
F6     - меню
ESC    - отмена
BKSP   - удалить
-> <-  - смена значений
SP     - переход на вкладки
.      - данные по авто
F7     - выход"};
 */ 
                    case TERM_TYPE.DOLPH7850:
                        xFuncs.SetDefaultFunc();
                        xFuncs.SetNewFunc(W32.VK_F6,    Keys.None,  AppC.F_OPENSH,      "F6",       " - открыть");
                        xFuncs.SetNewFunc(W32.VK_F7,    Keys.None,  AppC.F_CLOSESH,     "F7",       " - закрыть");
                        xFuncs.AddNewFunc(W32.VK_F9,    Keys.None,  AppC.F_MENU,        "F9",       " - меню");
                        xFuncs.SetNewFunc(W32.VK_F10,   Keys.None,  AppC.F_CHGZMK,      "F10",      " - смена ШБ");
                        xFuncs.SetNewFunc(W32.VK_ESC,   Keys.Shift, AppC.F_QUIT,        "SFT-ESC",  " - выход");

                        xFuncs.AddNewFunc(W32.VK_F2_PC, Keys.None, AppC.F_UPLD_DOC,     "",         " - записать");
                        xFuncs.AddNewFunc(W32.VK_F11_PC,Keys.None, AppC.F_QUIT,         "",         " - выход");

/*
                        sH = new string[]{
@"F2          - записать данные
F6          - открыть
F7          - закрыть
F9          - меню
F10         - смена шлагбаума
ESC         - отмена
DEL         - удалить
-> <-       - смена значений
TAB[TAB-SFT]- переход на вкладки
SFT-OK      - данные по авто
SFT-ESC     - выход"};
 */ 
                        break;
                }
                xFuncs.SetDefaultHelp();
            }

        }


        // установка флага режима редактирования
        private void SetEditMode(bool bEdit)
        {
            bEditMode = bEdit;
        }


        // выделение всего поля при входе (текстовы поля)
        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        // вывод панели обмена данными
        public class FuncPanel
        {
            private MainF xF;
            private Point pInvisible,
                pVisible;
            private bool bActive;

            // поправка на WinMobile экран
            private int nWMDelta;

            public FuncPanel(MainF f)
            {
                xF = f;
                pInvisible = xF.pnLoadDoc.Location;
#if DOLPH7850
                nWMDelta = -10;
#else
                nWMDelta = 0;
#endif

                pVisible = new Point(6, 60);
                bActive = false;
            }

            private void ShowPNow(int x, int y, string sH, string sR)
            {
                if (bActive == false)
                {
                    bActive = true;
                    xF.pnLoadDoc.SuspendLayout();
                    xF.pnLoadDoc.Left = x;
                    xF.pnLoadDoc.Top = y + nWMDelta;
                    xF.lFuncNamePan.Text = sH;
                    xF.tbPanP1.Text = sR;
                    xF.lpnLoadInf.Text = "<Enter> - начать";

                    xF.pnLoadDoc.Visible = true;
                    xF.pnLoadDoc.Enabled = true;
                    xF.pnLoadDoc.ResumeLayout();
                    xF.pnLoadDoc.Refresh();
                }
            }

            public void ShowP(string s, string sR)
            {
                ShowPNow(pVisible.X, pVisible.Y, s, sR);
            }

            public void ShowP(int x, int y, string s, string sR)
            {
                ShowPNow(x, y, s, sR);
            }

            public void UpdateHead(string s)
            {
                xF.lFuncNamePan.Text = s;
                xF.lFuncNamePan.Refresh();
            }

            public void UpdateReg(string s)
            {
                xF.tbPanP1.Text = s;
                xF.tbPanP1.Refresh();
            }

            public void UpdateHelp(string s)
            {
                xF.lpnLoadInf.Text = s;
                xF.lpnLoadInf.Refresh();
            }


            public void HideP()
            {
                if (bActive == true)
                {
                    bActive = false;
                    xF.pnLoadDoc.SuspendLayout();
                    xF.pnLoadDoc.Location = pInvisible;
                    xF.pnLoadDoc.Visible = false;
                    xF.pnLoadDoc.Enabled = false;
                    xF.pnLoadDoc.ResumeLayout();
                }
            }

            public bool IsShown
            {
                get { return bActive; }
            }

            public string RegInf
            {
                get { return xF.tbPanP1.Text; }
                set
                {
                    xF.tbPanP1.Text = value;
                    xF.tbPanP1.Refresh();
                }

            }

        }




    }

    // возвращаемое значение функцией контроля введенных параметров на панели
    //public struct VerRet
    //{
    //    public int nRet;
    //    public Control cWhereFocus;
    //}

    //public delegate VerRet VerifyEditFields();

    public class ServClass
    {
        // делегат проверки корректных данных
        //public static VerifyEditFields dgVerEd;

        //public static string TimeDiff(int t1, int t2)
        //{
        //    TimeSpan tsDiff = new TimeSpan(0, 0, 0, 0, t2 - t1);
        //    return (tsDiff.TotalSeconds.ToString());

        //    //return( tsDiff.TotalSeconds.ToString() + "." + tsDiff.Milliseconds.ToString() + "(" +
        //        //tsDiff.Ticks.ToString() + ")" );
        //}

        public static void ErrorMsg(string sE)
        {
            MessageBox.Show(sE, "Ошибка!");
        }

        public static void ErrorMsg(string sE, string sH, bool bSound)
        {
            ServClass.PlayMelody(W32.MB_3GONG_EXCLAM);
            MessageBox.Show(sE, sH);
        }


        public static void CheckDateTime(string sD, string sT)
        {
            try
            {
                DateTime xD = DateTime.ParseExact(sD + sT, "yyyyMMddHH:mm:ss", null);
            }
            catch
            {
            }
        }

        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        extern static void MessageBeep(uint BeepType);

        public static void PlayMelody(uint nSoundType)
        {
            MessageBeep(nSoundType);
        }

        [DllImport("Coredll.Dll")]
        public static extern short GetKeyState(int nVirtKey);


        public static int ReadXMLWrite2File(System.IO.Stream stm, ref string sFileTmp)
        {
            string sBuf = "";
            int nFileSize = 0;

            sFileTmp = "tmpnsi";
            try
            {
                StreamWriter sw = File.CreateText(sFileTmp);
                StreamReader sr = new StreamReader(stm);
                sBuf = sr.ReadLine();
                while ((sBuf != "") && (sr.EndOfStream == false))
                {
                    sw.WriteLine(sBuf);
                    sBuf = sr.ReadLine();
                }
                sBuf = sr.ReadLine();

                sw.Flush();
                nFileSize = (int)sw.BaseStream.Length;
                sw.Close();
            }
            catch(Exception ex)
            {
                throw new Exception("Ошибка чтения ответа", ex);
            }

            return (nFileSize);
        }






        // обновление связей
        public static void RefreshAvtBind(AppC.EditListC aEd)
        {
            foreach (Control e in aEd)
                foreach (Binding b in e.DataBindings)
                    b.ReadValue();
        }

        // поиск текущего
        //public static Control FindCurInEdit(AppC.EditListC aEd)
        //{
        //    Control xCur = null;
        //    foreach (Control e in aEd)
        //        if (e.Focused == true)
        //        {
        //            xCur = e;
        //            break;
        //        }
        //    return (xCur);
        //}
        
        // попытка перехода на следующее поле при редактировании
        //public static bool TryEditNextFiled(Control xCur, int nCommand, AppC.EditListC aEd)
        //{
        //    bool bRet = AppC.RC_OKB;
        //    bool bLookForNext = false;          // запоминем следующий 
        //    Control xPrev = null, xNext = null, // предыдущий-последующий
        //        xT = null,                      // рабочая
        //        xFirst = null;                  // первый

        //    // если текущий неизвестен
        //    if (xCur == null)
        //        xCur = FindCurInEdit(aEd);

        //    // поиск предыдущий/последующий, первый
        //    for (int i = 0; i < aEd.Count; i++)
        //    {
        //        if (aEd[i] == xCur)
        //        {// нашли текущий
        //            xPrev = xT;
        //            bLookForNext = true;
        //        }
        //        else
        //        {
        //            if (aEd[i].Enabled)
        //            {// или предыдущий или следующий
        //                if (xFirst == null)
        //                    xFirst = aEd[i];
        //                xT = aEd[i];
        //                if (bLookForNext == true)
        //                {
        //                    bLookForNext = false;
        //                    xNext = aEd[i];
        //                }
        //            }
        //        }
        //    }

        //    if (nCommand == AppC.CC_NEXTOVER)               // окончание редактирования ?
        //    {// переход по Enter
        //        if (xNext == null)
        //        {// следующего нет, это последнее поле
        //            VerRet vRet = dgVerEd();
        //            if (vRet.nRet == AppC.RC_OK)
        //                return (AppC.RC_OKB);
        //            // еще не все поля корректны, редактирование продолжается на следующем поле
        //            bRet = AppC.RC_OKB;
        //            xNext = vRet.cWhereFocus;
        //            if (xNext != null)
        //            {
        //                if (xNext.Enabled == false)
        //                    xNext = null;
        //            }
        //        }
        //        nCommand = AppC.CC_NEXT;
        //    }

        //    if (nCommand == AppC.CC_NEXT)
        //    {// переход на следующий
        //        if (xNext != null)
        //            xNext.Focus();
        //        else if ((xFirst != null) && (xFirst != xCur))
        //            xFirst.Focus();
        //        else
        //            bRet = AppC.RC_CANCELB;
        //    }
        //    else if (nCommand == AppC.CC_PREV)
        //    {// переход на предыдующий
        //        if (xPrev != null)
        //            xPrev.Focus();
        //        else if ((xT != null) && (xT != xCur))
        //            xT.Focus();
        //        else
        //            bRet = AppC.RC_CANCELB;
        //    }
        //    return (bRet);
        //}

        // изменение массива управления редактированием полей
        // aEnn - массив доступных
        // aDis - массив недоступных
        //public static void ChangeEdArrDet(Control[] aEnn, Control[] aDis, AppC.EditListC aEd)
        //{
        //    int i;
        //    if (aEnn != null)
        //    {
        //        foreach (Control c in aEnn)
        //        {
        //            i = SearchControl(c, aEd);
        //            if (i >= 0)
        //            {
        //                aEd[i].xControl.Enabled = true;
        //                aEd[i].bIsEnable = true;
        //            }
        //        }
        //    }
        //    if (aDis != null)
        //    {
        //        foreach (Control c in aDis)
        //        {
        //            i = SearchControl(c, aEd);
        //            if (i >= 0)
        //            {
        //                aEd[i].xControl.Enabled = false;
        //                aEd[i].bIsEnable = false;
        //            }
        //        }
        //    }
        //}

        // поиск конкретного контрола в списке
        //public static int SearchControl(Control c, AppC.EditListC aEd)
        //{
        //    int ret = -1;
        //    for (int i = 0; i < aEd.Count; i++)
        //    {
        //        if (aEd[i] == c)
        //        {
        //            ret = i;
        //            break;
        //        }
        //    }
        //    return (ret);
        //}


        public class DGTBoxColorColumn : DGCustomColumn
        {
            // Let's add this so user can access 
            public virtual TextBox TextBox
            {
                get { return this.HostedControl as TextBox; }
            }

            protected override string GetBoundPropertyName()
            {
                return "Text";                                                          // Need to bount to "Text" property on TextBox
            }

            protected override Control CreateHostedControl()
            {
                TextBox box = new TextBox();                                            // Our hosted control is a TextBox

                box.BorderStyle = BorderStyle.None;                                     // It has no border
                box.Multiline = true;                                                   // And it's multiline
                box.TextAlign = this.Alignment;                                         // Set up aligment.

                return box;
            }

            protected override bool DrawBackground(Graphics g, Rectangle bounds, int rowNum,
                Brush backBrush, System.Data.DataRow dr)
            {
                Brush background = backBrush;
                bool bSelAll = false,
                    bSel = (((SolidBrush)backBrush).Color != Owner.SelectionBackColor) ? false : true;

                if (bSel == false)
                {// вообще-то не выделена, но иногда надо выделить
                    switch (this.TableInd)
                    {
                        case NSI.TBD_DOC:
                            if (System.DBNull.Value != dr["SOURCE"])
                                if ((int)dr["SOURCE"] > 2)
                                {
                                    background = this.AltSolidBrush;
                                }
                            break;
                        case NSI.TBD_TTN:
                            if (System.DBNull.Value != dr["CTRL"])
                                if ((int)dr["CTRL"] > 0)
                                    background = this.AltSolidBrush;
                            break;
                    }
                }

                g.FillRectangle(background, bounds);
                return (bSelAll);
            }



        }



        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUS
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public uint dwTotalPhys;
            public uint dwAvailPhys;
            public uint dwTotalPageFile;
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
        }

        [DllImport("coredll")]
        static extern void GlobalMemoryStatus(ref MEMORYSTATUS buf);

        public static void MemInfo()
        {
            string s = "";
            MEMORYSTATUS memSt = new MEMORYSTATUS();
            GlobalMemoryStatus(ref memSt);

            uint i = (memSt.dwAvailPageFile / 1024);
            s += "Available Page File (kb):" + i.ToString() + "\r\n";

            i = (memSt.dwAvailPhys / 1024);
            s += "Available Virtual Memory (kb):" + i.ToString() + "\r\n";

            i = memSt.dwMemoryLoad;
            s += "Memory In Use :" + i.ToString() + "\r\n";

            i = (memSt.dwTotalPageFile / 1024);
            s += "Total Page Size (kb):" + i.ToString() + "\r\n";

            i = (memSt.dwTotalPhys / 1024);
            s += "Total Physical Memory (kb):" + i.ToString() + "\r\n";


            i = (memSt.dwTotalVirtual / 1024);
            s += "Total Virtual Memory (kb):" + i.ToString();

            MessageBox.Show(s, "Memory_Stat");

        }














        // Это что ? Подсветка ошибочного ввода ?
        public static void TBColor(TextBox tb, bool bBadVal)
        {
            if (bBadVal == true)
            {
                Color c = tb.BackColor;
            }
            else
            {
                Color c = tb.BackColor;
            }

        }



        // Поиск активного Control на вкладке
        public static Control GetPageControl(Control page, int nWhatFind, Control xFind)
        {
            foreach (Control ctl in page.Controls)
            {
                switch (nWhatFind)
                {
                    case 0:
                        if (ctl.TabIndex == 0)
                            return ctl;
                        break;
                    case 1:
                        if (ctl.Focused == true)
                            return ctl;
                        break;
                    case 2:
                        if (ctl == xFind)
                            return ctl;
                        break;
                }
            }
            return (null);
        }


    }


    #region Not_Used_But_Tested
/*
 * 
 * 
 
 * 
 *  
 * 
 * 
/* Замена Enter на Tab

 tried SelectNextControl() and couldn't get it to work like the ordinary hardware tab-button. I ended up using P/Invoke and keybd_event() to simulate tab key presses. It works perfectly and you only need to set one form property to true and add one form event handler (not one event handler for every control). If you put the code below in a base form and derive all your forms from that form you will get tab-handling automatically - set it and forget it.

1. Add the following P/Invoke code. NOTE: You should wrap this code in a conditional define (for instance DESIGN) which is true when you need VS design support. The VS designer will not let you design your forms if you have P/Invoke code anywhere in your project...

#if !DESIGN
     [DllImport("coredll.dll")]
    internal extern static void keybd_event(byte bVk, byte bScan, Int32 dwFlags, Int32   dwExtraInfo);
    internal const int KEYEVENTF_KEYUP = 0x02;
    internal const int VK_TAB = 0x09;
    internal const int VK_SHIFT = 0x10;
#endif


2. Set property KeyPreview to true for the form. This gives the form a chance to see the key presses before the controls get them.


3. Add an event handler for the key down event of the form:

private void Form_KeyDown(object sender, KeyEventArgs e)
{
#if !DESIGN
  if (e.KeyCode == System.Windows.Forms.Keys.Up)
  {
    keybd_event(VK_SHIFT, VK_SHIFT, 0, 0);
    keybd_event(VK_TAB, VK_TAB, 0, 0);
    keybd_event(VK_TAB, VK_TAB, KEYEVENTF_KEYUP, 0);
    keybd_event(VK_SHIFT, VK_SHIFT, KEYEVENTF_KEYUP, 0);
    e.Handled = true;
  }
  else if(e.KeyCode == System.Windows.Forms.Keys.Down)
  {
    keybd_event(VK_TAB, VK_TAB, 0, 0);
    keybd_event(VK_TAB, VK_TAB, KEYEVENTF_KEYUP, 0);
    e.Handled = true;
  }
#endif
}

    --- сабклассинг ---
 * 
    using System.Drawing;
    using System.Runtime.InteropServices;

    public sealed class Win32
    {
        /// <summary>
        /// A callback to a Win32 window procedure (wndproc)
        /// </summary>
        /// <param name="hwnd">The handle of the window receiving a message</param>
        /// <param name="msg">The message</param>
        /// <param name="wParam">The message's parameters (part 1)</param>
        /// <param name="lParam">The message's parameters (part 2)</param>
        /// <returns>A integer as described for the given message in MSDN</returns>
        public delegate int WndProc(IntPtr hwnd, uint msg, uint wParam, int lParam);

#if DESKTOP
    [DllImport("user32.dll")]
#else
        [DllImport("coredll.dll")]
#endif
        public extern static int DefWindowProc(
            IntPtr hwnd, uint msg, uint wParam, int lParam);

#if DESKTOP
    [DllImport("user32.dll")]
#else
        [DllImport("coredll.dll")]
#endif
        public extern static IntPtr SetWindowLong(
            IntPtr hwnd, int nIndex, IntPtr dwNewLong);

        public const int GWL_WNDPROC = -4;

#if DESKTOP
    [DllImport("user32.dll")]
#else
        [DllImport("coredll.dll")]
#endif
        public extern static int CallWindowProc(
            IntPtr lpPrevWndFunc, IntPtr hwnd, uint msg, uint wParam, int lParam);
    }

    class WndProcHooker
    {
        /// <summary>
        /// The callback used when a hooked window's message map contains the
        /// hooked message
        /// </summary>
        /// <param name="hwnd">The handle to the window for which the message
        /// was received</param>
        /// <param name="wParam">The message's parameters (part 1)</param>
        /// <param name="lParam">The message's parameters (part 2)</param>
        /// <param name="handled">The invoked function sets this to true if it
        /// handled the message. If the value is false when the callback
        /// returns, the next window procedure in the wndproc chain is
        /// called</param>
        /// <returns>A value specified for the given message in the MSDN
        /// documentation</returns>
        public delegate int WndProcCallback(
            IntPtr hwnd, uint msg, uint wParam, int lParam, ref bool handled);

        /// <summary>
        /// This is the global list of all the window procedures we have
        /// hooked. The key is an hwnd. The value is a HookedProcInformation
        /// object which contains a pointer to the old wndproc and a map of
        /// messages/callbacks for the window specified. Controls whose handles
        /// have been created go into this dictionary.
        /// </summary>
        private static Dictionary<IntPtr, HookedProcInformation> hwndDict =
            new Dictionary<IntPtr, HookedProcInformation>();

        /// <summary>
        /// See <see>hwndDict</see>. The key is a control and the value is a
        /// HookedProcInformation. Controls whose handles have not been created
        /// go into this dictionary. When the HandleCreated event for the
        /// control is fired the control is moved into <see>hwndDict</see>.
        /// </summary>
        private static Dictionary<Control, HookedProcInformation> ctlDict =
            new Dictionary<Control, HookedProcInformation>();

        /// <summary>
        /// Makes a connection between a message on a specified window handle
        /// and the callback to be called when that message is received. If the
        /// window was not previously hooked it is added to the global list of
        /// all the window procedures hooked.
        /// </summary>
        /// <param name="ctl">The control whose wndproc we are hooking</param>
        /// <param name="callback">The method to call when the specified
        /// message is received for the specified window</param>
        /// <param name="msg">The message we are hooking.</param>
        public static void HookWndProc(
            Control ctl, WndProcCallback callback, uint msg)
        {
            HookedProcInformation hpi = null;
            if (ctlDict.ContainsKey(ctl))
                hpi = ctlDict[ctl];
            else if (hwndDict.ContainsKey(ctl.Handle))
                hpi = hwndDict[ctl.Handle];
            if (hpi == null)
            {
                // We havne't seen this control before. Create a new
                // HookedProcInformation for it
                hpi = new HookedProcInformation(ctl,
                    new Win32.WndProc(WndProcHooker.WindowProc));
                ctl.HandleCreated += new EventHandler(ctl_HandleCreated);
                ctl.HandleDestroyed += new EventHandler(ctl_HandleDestroyed);
                ctl.Disposed += new EventHandler(ctl_Disposed);

                // If the handle has already been created set the hook. If it
                // hasn't been created yet, the hook will get set in the
                // ctl_HandleCreated event handler
                if (ctl.Handle != IntPtr.Zero)
                    hpi.SetHook();
            }

            // stick hpi into the correct dictionary
            if (ctl.Handle == IntPtr.Zero)
                ctlDict[ctl] = hpi;
            else
                hwndDict[ctl.Handle] = hpi;

            // add the message/callback into the message map
            hpi.messageMap[msg] = callback;
        }

        /// <summary>
        /// The event handler called when a control is disposed.
        /// </summary>
        /// <param name="sender">The object that raised this event</param>
        /// <param name="e">The arguments for this event</param>
        static void ctl_Disposed(object sender, EventArgs e)
        {
            Control ctl = sender as Control;
            if (ctlDict.ContainsKey(ctl))
                ctlDict.Remove(ctl);
            else
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// The event handler called when a control's handle is destroyed.
        /// We remove the HookedProcInformation from <see>hwndDict</see> and
        /// put it back into <see>ctlDict</see> in case the control get re-
        /// created and we still want to hook its messages.
        /// </summary>
        /// <param name="sender">The object that raised this event</param>
        /// <param name="e">The arguments for this event</param>
        static void ctl_HandleDestroyed(object sender, EventArgs e)
        {
            // When the handle for a control is destroyed, we want to
            // unhook its wndproc and update our lists
            Control ctl = sender as Control;
            if (hwndDict.ContainsKey(ctl.Handle))
            {
                HookedProcInformation hpi = hwndDict[ctl.Handle];
                UnhookWndProc(ctl, false);
            }
            else
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// The event handler called when a control's handle is created. We
        /// call SetHook() on the associated HookedProcInformation object and
        /// move it from <see>ctlDict</see> to <see>hwndDict</see>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ctl_HandleCreated(object sender, EventArgs e)
        {
            Control ctl = sender as Control;
            if (ctlDict.ContainsKey(ctl))
            {
                HookedProcInformation hpi = ctlDict[ctl];
                hwndDict[ctl.Handle] = hpi;
                ctlDict.Remove(ctl);
                hpi.SetHook();
            }
            else
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// This is a generic wndproc. It is the callback for all hooked
        /// windows. If we get into this function, we look up the hwnd in the
        /// global list of all hooked windows to get its message map. If the
        /// message received is present in the message map, its callback is
        /// invoked with the parameters listed here.
        /// </summary>
        /// <param name="hwnd">The handle to the window that received the
        /// message</param>
        /// <param name="msg">The message</param>
        /// <param name="wParam">The message's parameters (part 1)</param>
        /// <param name="lParam">The messages's parameters (part 2)</param>
        /// <returns>If the callback handled the message, the callback's return
        /// value is returned form this function. If the callback didn't handle
        /// the message, the message is forwarded on to the previous wndproc.
        /// </returns>
        private static int WindowProc(
            IntPtr hwnd, uint msg, uint wParam, int lParam)
        {
            if (hwndDict.ContainsKey(hwnd))
            {
                HookedProcInformation hpi = hwndDict[hwnd];
                if (hpi.messageMap.ContainsKey(msg))
                {
                    WndProcCallback callback = hpi.messageMap[msg];
                    bool handled = false;
                    int retval = callback(hwnd, msg, wParam, lParam, ref handled);
                    if (handled)
                        return retval;
                }

                // if we didn't hook the message passed or we did, but the
                // callback didn't set the handled property to true, call
                // the original window procedure
                return hpi.CallOldWindowProc(hwnd, msg, wParam, lParam);
            }

            System.Diagnostics.Debug.Assert(
                false, "WindowProc called for hwnd we don't know about");
            return Win32.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        /// <summary>
        /// This method removes the specified message from the message map for
        /// the specified hwnd.
        /// </summary>
        /// <param name="ctl">The control whose message we are unhooking
        /// </param>
        /// <param name="msg">The message no longer want to hook</param>
        public static void UnhookWndProc(Control ctl, uint msg)
        {
            // look for the HookedProcInformation in the control and hwnd
            // dictionaries
            HookedProcInformation hpi = null;
            if (ctlDict.ContainsKey(ctl))
                hpi = ctlDict[ctl];
            else if (hwndDict.ContainsKey(ctl.Handle))
                hpi = hwndDict[ctl.Handle];

            // if we couldn't find a HookedProcInformation, throw
            if (hpi == null)
                throw new ArgumentException("No hook exists for this control");

            // look for the message we are removing in the messageMap
            if (hpi.messageMap.ContainsKey(msg))
                hpi.messageMap.Remove(msg);
            else
                // if we couldn't find the message, throw
                throw new ArgumentException(
                    string.Format(
                        "No hook exists for message ({0}) on this control",
                         msg));
        }

        /// <summary>
        /// Restores the previous wndproc for the specified window.
        /// </summary>
        /// <param name="ctl">The control whose wndproc we no longer want to
        /// hook</param>
        /// <param name="disposing">if true we remove don't readd the
        /// HookedProcInformation
        /// back into ctlDict</param>
        public static void UnhookWndProc(Control ctl, bool disposing)
        {
            HookedProcInformation hpi = null;
            if (ctlDict.ContainsKey(ctl))
                hpi = ctlDict[ctl];
            else if (hwndDict.ContainsKey(ctl.Handle))
                hpi = hwndDict[ctl.Handle];

            if (hpi == null)
                throw new ArgumentException("No hook exists for this control");

            // If we found our HookedProcInformation in ctlDict and we are
            // disposing remove it from ctlDict
            if (ctlDict.ContainsKey(ctl) && disposing)
                ctlDict.Remove(ctl);

            // If we found our HookedProcInformation in hwndDict, remove it
            // and if we are not disposing stick it in ctlDict
            if (hwndDict.ContainsKey(ctl.Handle))
            {
                hpi.Unhook();
                hwndDict.Remove(ctl.Handle);
                if (!disposing)
                    ctlDict[ctl] = hpi;
            }
        }

        /// <summary>
        /// This class remembers the old window procedure for the specified
        /// window handle and also provides the message map for the messages
        /// hooked on that window.
        /// </summary>
        class HookedProcInformation
        {
            /// <summary>
            /// The message map for the window
            /// </summary>
            public Dictionary<uint, WndProcCallback> messageMap;

            /// <summary>
            /// The old window procedure for the window
            /// </summary>
            private IntPtr oldWndProc;

            /// <summary>
            /// The delegate that gets called in place of this window's
            /// wndproc.
            /// </summary>
            private Win32.WndProc newWndProc;

            /// <summary>
            /// Control whose wndproc we are hooking
            /// </summary>
            private Control control;

            /// <summary>
            /// Constructs a new HookedProcInformation object
            /// </summary>
            /// <param name="ctl">The handle to the window being hooked</param>
            /// <param name="wndproc">The window procedure to replace the
            /// original one for the control</param>
            public HookedProcInformation(Control ctl, Win32.WndProc wndproc)
            {
                control = ctl;
                newWndProc = wndproc;
                messageMap = new Dictionary<uint, WndProcCallback>();
            }

            /// <summary>
            /// Replaces the windows procedure for <see>control</see> with the
            /// one specified in the constructor.
            /// </summary>
            public void SetHook()
            {
                IntPtr hwnd = control.Handle;
                if (hwnd == IntPtr.Zero)
                    throw new InvalidOperationException(
                        "Handle for control has not been created");

                oldWndProc = Win32.SetWindowLong(hwnd, Win32.GWL_WNDPROC,
                    Marshal.GetFunctionPointerForDelegate(newWndProc));
            }

            /// <summary>
            /// Restores the original window procedure for the control.
            /// </summary>
            public void Unhook()
            {
                IntPtr hwnd = control.Handle;
                if (hwnd == IntPtr.Zero)
                    throw new InvalidOperationException(
                        "Handle for control has not been created");

                Win32.SetWindowLong(hwnd, Win32.GWL_WNDPROC, oldWndProc);
            }

            /// <summary>
            /// Calls the original window procedure of the control with the
            /// arguments provided.
            /// </summary>
            /// <param name="hwnd">The handle of the window that received the
            /// message</param>
            /// <param name="msg">The message</param>
            /// <param name="wParam">The message's arguments (part 1)</param>
            /// <param name="lParam">The message's arguments (part 2)</param>
            /// <returns>The value returned by the control's original wndproc
            /// </returns>
            public int CallOldWindowProc(
                    IntPtr hwnd, uint msg, uint wParam, int lParam)
            {
                return Win32.CallWindowProc(
                    oldWndProc, hwnd, msg, wParam, lParam);
            }
        }
    }
 * 
 * Form1 - Load
             WndProcHooker wphF = new WndProcHooker();
            WndProcHooker.HookWndProc(this.tcMain, OnKeyDown, W32.WM_KEYDOWN);
* 
         private int OnKeyDown(IntPtr hwnd, uint msg, uint wParam, int lParam, ref bool handled)
        {
            int ret = 0;
            switch (msg)
            {
                case W32.WM_SCANNED:
                    break;
                case W32.WM_KEYDOWN:
                    switch (wParam)
                    {
                        case W32.VK_SCAN:
                            break;
                        case W32.VK_HOME:
//                            CreateMMenu();
                            //handled = true;
                            break;
                    }
                    break;
                case W32.WM_KEYUP:
                    break;
            }
            return (ret);
        }
* 
 * 
 * 
 * 
 * 
 * 
 */
    #endregion




}

