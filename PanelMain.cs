using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Reflection;

using ScannerAll;
using PDA.OS;
using PDA.Service;
using ExprDll;
using SavuSocket;
//using KBWait;

namespace Proh
{
    public partial class MainF : Form
    {

        // указатель на главное меню формы
        MainMenu mmSaved;

        // форма авторизации
        private Avtor fAv = null;

        // объект параметров
        private AppPars 
            xPars;

        // объект-сканер
        private BarcodeScanner xBCScanner = null;
        
        // тип терминала
        private ScannerAll.TERM_TYPE nTerminalType = TERM_TYPE.UNKNOWN;

        // словарь доустимых функций
        private FuncDic xFuncs;

        // обмен с сервером
        private List<string> lstSrvMsg = new List<string>();

        // класс работы со справочниками
        public NSI xNSI;

        // текущий сеанс
        public Smena 
            xSm;

        public FuncPanel xFPan;

        // текущие значения панели документов
        public CurDoc xCDoc = null;

        // индикатор батареи
        private BATT_INF xBBI;

        // индексы панелей
        public const int PG_DIR = 0;
        public const int PG_TTN = 1;
        public const int PG_DOC = 2;
        public const int PG_NSI = 3;
        public const int PG_PAR = 4;
        public const int PG_SRV = 5;

        // флаги для обработки клавиатурного ввода
        private bool bSkipChar = false;                 // не обрабатывать введенный символ

        // флаг режима редактирования
        public bool bEditMode = false;

        // обработчик клавиш для текущей функции
        delegate bool CurrFuncKeyHandler(int nF, KeyEventArgs e);
        CurrFuncKeyHandler ehCurrFunc = null;

        // флаг нормальной авторизации
        private bool bGoodAvtor = false;

        // файл отладки
        public static System.IO.StreamWriter swProt;

        public void InitializeDop(BarcodeScanner xSc, Size szBatt, Point ptBatt)
        {
            string sExePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string sExeDir = System.IO.Path.GetDirectoryName(sExePath);
            xBCScanner = xSc;
            if (xBCScanner != null)
            {
                nTerminalType = xBCScanner.nTermType;
                xBCScanner.BarcodeScan += new BarcodeScanner.BarcodeScanEventHandler(OnScan);

            }

            TimeSync.SyncAsync("10.0.0.221");

            // настройка выполняемых функций на клавиши конкретного терминала
            SetMainFuncDict(nTerminalType, sExeDir);

            xPars = (AppPars)AppPars.InitPars(sExeDir);
            SetBindAppPars();

            xNSI = new NSI(xPars.sNSIPath, xPars.sDataPath);
            xNSI.ConnDTGrid(new DataGrid[]{dgDoc, dgTTN, dgMC});

            Smena.ReadSm(ref xSm, xPars.sDataPath + "CS.XML");

            // создать индикатор батареи
            xBBI = new BATT_INF(tpInOut, szBatt, ptBatt);
            xBBI.BIFont = 8F;

            // инфо-панель
            xFPan = new FuncPanel(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (fAv == null)
                fAv = new Avtor(xNSI, xSm, xBCScanner, xPars);
            fAv.nCurReg = AppC.AVT_LOGON;
            SetEditMode(true);
            DialogResult dr = fAv.ShowDialog();
            SetEditMode(false);
            if (dr != DialogResult.OK)
                this.Close();
            else
            {
                swProt = File.CreateText("ProtTerm.txt");
                bGoodAvtor = true;
                InitPanels();
                //tStat_Reg.Text = "Склад " + xSm.nSklad.ToString() + " " + xSm.CurDate.ToString("dd.MM.yy") + 
                //    " См " + xSm.CurSmena.ToString();
                xNSI.TryRestoreUserDat(xSm, xPars, true);

                // привязка к НСИ
                SetBindDevPars();

                // на вкладку ввода
                tcMain.SelectedIndex = PG_DIR;

                // показать индикатор батареи
                xBBI.EnableShow = true;
                xNSI.ChgGridStyle(NSI.TBD_DOC, NSI.GDOC_INV);
            }
        }


        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (xBCScanner != null)
                xBCScanner.Dispose();
            // сохранение рабочих данных (если есть)
            if (bGoodAvtor == true)
            {
                //xNSI.SaveCS(xSm, xPars);
                xSm.SaveCS(xPars.sDataPath, xNSI.DT[NSI.TBD_DOC].dt.Rows.Count);
                xNSI.DSSave(xPars.sDataPath);
            }
            if (swProt != null)
                swProt.Close();
        }


        private void InitPanels()
        {
            // Ссылки на Controls формы для панели параметров (документы, загрузка,...)

            DocPars.tDate = this.tDateD_p;
            DocPars.tKTyp = this.tKT_p;
            DocPars.tNTyp = this.tNT_p;
            DocPars.tKPost = this.tKPost_p;
            DocPars.tNDoc = this.tNom_p;
        }


        // обработка смены вкладок
        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tcMain.SelectedIndex)
            {
                case PG_DIR:
                    EnterInDirect();
                    break;
                case PG_TTN:
                    EnterInTTN();
                    break;
                case PG_DOC:
                    EnterInDoc();
                    break;
                case PG_NSI:
                    EnterInNSI();
                    break;
                case PG_PAR:
                    EnterInPars();
                    break;
                case PG_SRV:
                    EnterInServ();
                    break;
            }
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            bool bAlreadyProceed = false,   // клавиша уже обработана
                bHandledKey = false;
            int nF = 0;

            nF = xFuncs.TryGetFunc(e);
            if (bEditMode == true)
            {
                if (((e.KeyValue >= W32.VK_D0) && (e.KeyValue <= W32.VK_D9)) ||
                    (e.KeyValue == W32.VK_PERIOD))
                    //if (((e.KeyValue >= W32.VK_D0) && (e.KeyValue <= W32.VK_D9)) ||
                    //    (e.KeyValue == W32.VK_BACK) || (e.KeyValue == W32.VK_PERIOD_D))
                        nF = 0;
            }

            if (ehCurrFunc != null)
            {// клавиши ловит одна из функций
                bAlreadyProceed = ehCurrFunc(nF, e);
            }
            else
            {
                // обработка функций и клавиш с учетом текущего Control
                if (tcMain.SelectedIndex == PG_DOC)
                    bAlreadyProceed = Doc_KeyDown(nF, e);
                else if (tcMain.SelectedIndex == PG_DIR)
                    bAlreadyProceed = Direct_KeyDown(nF, e);
                else if (tcMain.SelectedIndex == PG_TTN)
                    bAlreadyProceed = TTN_KeyDown(nF, e);
                else if (tcMain.SelectedIndex == PG_NSI)
                    bAlreadyProceed = NSI_KeyDown(nF, e);
                else if (tcMain.SelectedIndex == PG_PAR)
                    bAlreadyProceed = AppPars_KeyDown(nF, e);
                else if (tcMain.SelectedIndex == PG_SRV)
                    bAlreadyProceed = Service_KeyDown(nF, e);
            }

            if ((nF > 0) && (bAlreadyProceed == false))
            {// общая обработка функций
                bHandledKey = ProceedFunc(nF, e, sender);
            }

            // а здесь - только клавиши
            if ((bAlreadyProceed == false) && (bHandledKey == false))
            {
                switch (e.KeyValue)     // для всех вкладок
                {
                    case W32.VK_ENTER:
                        if (bEditMode == true)
                        {
                        }
                        e.Handled = true;
                        break;
                    case W32.VK_TAB:
                        break;
                }
            }
            bSkipChar = e.Handled;
        }


        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipChar == true)
            {
                e.Handled = true;
                bSkipChar = false;
            }
        }



        // обработка глобальных функций (все вкладки)
        private bool ProceedFunc(int nFunc, KeyEventArgs e, object sender)
        {
            bool ret = false;

            if (bEditMode == false)
            {// функции только для режима просмотра
                switch (nFunc)     // для всех вкладок
                {
                    case AppC.F_MENU:
                        CreateMMenu();              // главное меню
                        ret = true;
                        break;
                    //case AppC.F_UPLD_DOC:           // выгрузка документов
                    //    UploadDocs2Server(AppC.F_INITREG, e);
                    //    ret = true;
                    //    break;
                }
            }


            switch (nFunc)     // для всех вкладок
            {
                case AppC.F_HELP:
                    ShowHelp();
                    ret = true;
                    break;
                case AppC.F_VIEW_DOC:
                    // переход на вкладку Документы
                    if (tcMain.SelectedIndex != PG_DIR)
                        tcMain.SelectedIndex = PG_DIR;
                    ret = true;
                    break;
                case AppC.F_PREVPAGE:
                    // предыдущая страница
                    if (tcMain.SelectedIndex == 0)
                        tcMain.SelectedIndex = tcMain.TabPages.Count - 1;
                    else
                        tcMain.SelectedIndex--;
                    ret = true;
                    break;
                case AppC.F_NEXTPAGE:
                    // следующая страница
                    if (tcMain.SelectedIndex == (tcMain.TabPages.Count - 1))
                        tcMain.SelectedIndex = 0;
                    else
                        tcMain.SelectedIndex++;
                    ret = true;
                    break;
                case AppC.F_QUIT:
                    ExitApp();
                    //this.Close();
                    break;
            }
            e.Handled |= ret;
            return (ret);
        }





        private void CreateMMenu()
        {
            if (this.mmSaved == null)
            {
                // Пункты главного меню
                MenuItem miExch = new MenuItem();
                MenuItem miNsi = new MenuItem();
                MenuItem miServ = new MenuItem();

                miExch.Text = "&1 Документы";
                miNsi.Text = "&2 НСИ";
                miServ.Text = "&3 Сервис";

                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems.Add(new MenuItem());
                miExch.MenuItems[0].Text = "&1 Загрузка";
                miExch.MenuItems[0].Click += new EventHandler(MMenuClick_Load); ;

                miExch.MenuItems[1].Text = "&2 Выгрузка";
                miExch.MenuItems[1].Click += new EventHandler(MMenuClick_WriteSock); ;

                miExch.MenuItems[2].Text = "&3 НСИ";
                miExch.MenuItems[2].Click += new EventHandler(MMenuClick_LoadNSI);

                miExch.MenuItems[3].Text = "&4 Корректировка";

                miExch.MenuItems[4].Text = "&5 Удалить";
                miExch.MenuItems[4].Click += new EventHandler(MMenuClick_Delete); ;

                miExch.MenuItems[5].Text = "&6 Фильтр";
                miExch.MenuItems[6].Text = "-";

                miExch.MenuItems[7].Text = "&7 Выход";
                miExch.MenuItems[7].Click += new EventHandler(MMenuClick_Exit); ;


                // меню НСИ
                miNsi.MenuItems.Add(new MenuItem());
                miNsi.MenuItems[0].Text = "&1 Загрузка";
                miNsi.MenuItems[0].Click += new EventHandler(MMenuClick_LoadNSI);

                // меню сервисных функций
                miServ.MenuItems.Add(new MenuItem());
                miServ.MenuItems.Add(new MenuItem());
                miServ.MenuItems.Add(new MenuItem());

                miServ.MenuItems[0].Text = "&1 Выгрузка KeyDef";
                miServ.MenuItems[0].Click += new EventHandler(MMenuClick_LoadControl);
                miServ.MenuItems[1].Text = "&2 Получить время";
                miServ.MenuItems[1].Click += new EventHandler(MMenuClick_SyncTime);
                miServ.MenuItems[2].Text = "&3 Версия";
                miServ.MenuItems[2].Click += new EventHandler(MMenuClick_AppVer);


                // Create a MainMenu and assign MenuItem objects.
                MainMenu mainMenu1 = new MainMenu();
                mainMenu1.MenuItems.Add(miExch);
                mainMenu1.MenuItems.Add(miNsi);
                mainMenu1.MenuItems.Add(miServ);

                // Bind the MainMenu to Form1.
                this.mmSaved = mainMenu1;
            }
            if (this.Menu == null)
            {
                this.Menu = this.mmSaved;
#if DOLPH7850
                W32.SimulMouseClick(5, 315, this);
#else
                W32.SimulMouseClick(5, 5, this);
#endif
            }
            else
                this.Menu = null;
        }

        private void MMenuClick_Load(object sender, EventArgs e)
        {
            //LoadDocFromServer(AppC.F_INITREG, new KeyEventArgs(Keys.Enter));
            StatAllDoc();
            CreateMMenu();
        }

        private void MMenuClick_WriteSock(object sender, EventArgs e)
        {
            //UpLoadDoc(AppC.F_LOAD_DOC, 0);

            //UploadDocs2Server(AppC.F_INITREG, new KeyEventArgs(Keys.Enter));
            StatAllDoc();
            CreateMMenu();
        }


        private void MMenuClick_Delete(object sender, EventArgs e)
        {
            DelDoc(AppC.F_DEL_REC);
            StatAllDoc();
            CreateMMenu();
        }


        private void MMenuClick_Exit(object sender, EventArgs e)
        {
            ExitApp();
            //this.Close();
        }

        private void MMenuClick_LoadNSI(object sender, EventArgs e)
        {
            LoadNsiMenu(false);
            CreateMMenu();
        }

        // Выгрузить настройки клавиатуры
        private void MMenuClick_LoadControl(object sender, EventArgs e)
        {
            if (xSm.sUser == AppC.SUSER)
            {
                if (xFuncs.SaveKMap() == AppC.RC_OK)
                    MessageBox.Show("Файл выгружен");
            }
            else
                Srv.ErrorMsg("Только Admin!", true);
            CreateMMenu();
        }

        private void MMenuClick_SyncTime(object sender, EventArgs e)
        {
            //int t1 = Environment.TickCount;

            //DateTime ddtt = TimeSync.GetTime();

            //t1 = Environment.TickCount - t1;
            //TimeSpan tsDiff = new TimeSpan(0, 0, 0, 0, t1);

            string 
                sHead = String.Format("Текущее: {0}", DateTime.Now.TimeOfDay.ToString()),
                sAfter = "Ошибка синхронизации";

            Cursor.Current = Cursors.WaitCursor;
            //if (TimeSync.Sync(xPars.NTPSrv, 123, 10000, 3600))
            if (TimeSync.Sync("10.0.0.221", 123, 10000, 3600))
                    sAfter = String.Format("Новое время: {0}", DateTime.Now.TimeOfDay.ToString());
        
            MessageBox.Show(sAfter, sHead);
            Cursor.Current = Cursors.Default;



            CreateMMenu();
        }

        // Версия софтины
        private void MMenuClick_AppVer(object sender, EventArgs e)
        {
            AssemblyName xAN = Assembly.GetExecutingAssembly().GetName();
            //MessageBox.Show(String.Format("Версия ПО - {0}", xAN.Version.ToString()), "Информация");
            ServClass.ErrorMsg(String.Format("Версия ПО - {0}", xAN.Version.ToString()), "Информация", true);

            CreateMMenu();
        }



        private void ExitApp()
        {
            DialogResult dr = MessageBox.Show(" Выход ?  (Enter)\nпродолжить работу (ESC)",
                "Завершение работы", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dr == DialogResult.OK)
                this.Close();
        }


        private void dgDoc_LostFocus(object sender, EventArgs e)
        {
            //if ((nCurDocFunc == AppC.DT_SHOW) && (tcMain.SelectedIndex == PG_DOC))
            //    ToPageHeader(tpDoc);
        }

        private void ToPageHeader(TabPage pgT)
        {
            Control cTab0 = ServClass.GetPageControl(pgT, 0, null);
            cTab0.Focus();

            W32.keybd_event(W32.VK_SHIFT, W32.VK_SHIFT,  W32.KEYEVENTF_SILENT, 0);
            W32.keybd_event(W32.VK_TAB, W32.VK_TAB,  W32.KEYEVENTF_SILENT, 0);
            W32.keybd_event(W32.VK_TAB, W32.VK_TAB, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
            W32.keybd_event(W32.VK_SHIFT, W32.VK_SHIFT, W32.KEYEVENTF_KEYUP | W32.KEYEVENTF_SILENT, 0);
        }





        private void tFiction_GotFocus(object sender, EventArgs e)
        {
            int j, k;
            j = 7;
            k = 99;
            j = k - 5;
            k = j + 56;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BindingSource dt = (BindingSource)cbPurp.DataBindings["SelectedValue"].DataSource;
            dt.ResetBindings(true);
            
        }

        private void lbTTN_DataSourceChanged(object sender, EventArgs e)
        {
            ComboBox xC = (ComboBox)sender;
            int i = xC.Items.Count; ;
        }







        #region NOT_Used_yet

        /*
            // --- для частичной загрузки справочников
            //nnSS = xNSI;
            //thReadNSI = new Thread(new ThreadStart(ReadInThread));
            //thReadNSI.Start();

        private Thread thReadNSI;
        private void Form1_Activated(object sender, EventArgs e)
        {
            //fAv.ShowDialog();
            if (xNSI.DT[NSI.I_MC].nState == NSI.DT_STATE_INIT)
            {// справочник матценностей еще не грузили
                nnSS = xNSI;
                thReadNSI = new Thread(new ThreadStart(ReadInThread));
                thReadNSI.Start();
            }
        }
         * 
         private static NSI nnSS;
        private static void ReadInThread()
        {
            nnSS.LoadLocNSI(new int[] {}, 0);
        }
* 
 * 
 */

        #endregion


    }
}