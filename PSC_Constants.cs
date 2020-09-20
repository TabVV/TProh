using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;

using PDA.Service;

using FRACT = System.Decimal;


namespace PDA.Service
{
    public partial class AppC
    {
        // Общий код для авторизации
        internal const string GUEST = "1";

        public const int RC_NOID = 401;         // не указан ID описания
        public const int RC_BADID = 20;         // не распознан ID серии (нет описания)

        public const int F_CHGZMK = 63;         // смена замка
        public const int F_OPENSH = 71;         // поднять шлагбаум на проходной
        public const int F_CLOSESH = 72;        // поднять шлагбаум на проходной

        // Номера допустимых команд
        internal const int COM_ZSPR     = 0;    // загрузка справочников
        internal const int COM_ZPL      = 1;    // загрузка сведений по № путевого
        internal const int COM_ZCONTR   = 2;    // контроль на выезде
        internal const int COM_ZOPEN    = 3;    // поднять шлагбаум
        internal const int COM_ZCLOSE   = 4;    // закрыть шлагбаум
        // символьные значения команд
        internal static string[] saComms = { "ZSPR", "ZPL", "ZCONTR", "ZOPEN", "ZCLOSE" };


        // Терминатор для команды
        internal static byte[] baTermCom = { 13, 10 };
        // Терминатор для передаваемых данных
        internal static byte[] baTermMsg = { 13, 10, 0x2E, 13, 10 };

        // строк в окне Help
        internal const int HELPLINES = 19;
        // строк в окне доп.информации
        internal const int INFLINES = 17;

        // типы документов
        public const int TYPD_SAMV = 0;             // отгрузка-самовывоз
        public const int TYPD_СVYV = 1;             // отгрузка-центровывоз
        public const int TYPD_SVOD = 2;             // отгрузка по своду
        public const int TYPD_VPER = 3;             // внутреннее перемещение
        public const int TYPD_AKT_SPIS = 4;         // акт списания
        public const int TYPD_INV = 7;             // инвентаризация
        public const int TYPD_VZV_TTN = 10;         // возврат по накладной
        public const int TYPD_VZV_SVOD = 12;        // возврат по своду

        internal const int DIR_IN = 1;
        internal const int DIR_OUT = 2;

        internal const int GRUZ_EMPTY = 1;
        internal const int GRUZ_MAIN = 2;

    }



}

namespace Proh
{
   

    // настройки программы
    public sealed class AppPars
    {

        //===***===
        // Путь к резервной копии
        private string m_AppStore;
        // Путь к НСИ
        private string m_NSIPath;
        // Путь к данным
        private string m_DataPath;

        // HOST-name сервера
        private string m_Host;
        // № порта сервера (обмен данными)
        private int m_SrvPort;
        // № порта сервера (обмен сообщениями)
        private int m_SrvPortM;
        // Вкл/выкл обмен сообщениями с сервером
        private bool m_WaitSock;

        // HOST-name сервера шлагбаума
        private string m_HostSh;
        // № порта сервера шлагбаума
        private int m_SrvPortSh;

        // Смен в сутки
        private int m_Smennost;


        private bool 
            m_OpenSrv,                  // Команда Открыть - через сервер
            m_CloseSrv,                 // Команда Закрыть - через сервер
            m_ChkPrmOut = true,         // Проверка пропуска на выезде
            m_2DFull;                   // Чтение полностью всей серии 2D

        // Login для доступа к серверу шлагбаумов
        private string m_LoginSh = "test";
        // Pass для доступа к серверу шлагбаумов
        private string m_PassSh = "TseT";
        // DevID шлагбаума
        private string m_DevID = "SHLG";

        //-----*****-----*****-----
        // Текущее поле (индекс)
        private int m_CurField;

        /// Панель ввода
        /// 

        // таблица с параметрами
        private static string sFilePars = "TermPars.xml";

        public AppPars()
        {
            m_AppStore = @"\BACKUP\OAO_SP\TermMC";
            m_NSIPath = @"\BDDOC\";
            m_DataPath = @"\BDDOC\";

            m_Host = "BPR_SERV3";
            m_SrvPort = 11010;
            m_SrvPortM = 11001;
            m_WaitSock = false;
            m_HostSh = "10.0.0.213";
            m_SrvPortSh = 1235;
            Smennost = 2;

            m_OpenSrv = true;
            m_CloseSrv = false;
            m_2DFull = false;

            CurField = 0;
        }

        //---***---***---
        #region Общие параметры 

        // Путь к резервной копии
        public string sAppStore
        {
            get { return m_AppStore; }
            set { m_AppStore = value; }
        }
        // Путь к НСИ
        public string sNSIPath
        {
            get { return m_NSIPath; }
            set { m_NSIPath = value; }
        }
        // Путь к данным
        public string sDataPath
        {
            get { return m_DataPath; }
            set { m_DataPath = value; }
        }

        // HOST-name сервера
        public string sHostSrv
        {
            get { return m_Host; }
            set { m_Host = value; }
        }
        // № порта сервера (обмен данными)
        public int nSrvPort
        {
            get { return m_SrvPort; }
            set { m_SrvPort = value; }
        }
        // № порта сервера (обмен сообщениями)
        public int nSrvPortM
        {
            get { return m_SrvPortM; }
            set { m_SrvPortM = value; }
        }
        // Вкл/выкл обмен сообщениями с сервером
        public bool bWaitSock
        {
            get { return m_WaitSock; }
            set { m_WaitSock = value; }
        }
        // Сменность в сутки
        public int Smennost
        {
            get { return m_Smennost; }
            set { m_Smennost = value; }
        }
        // HOST-name сервера шлагбаума
        public string sHostSrvSh
        {
            get { return m_HostSh; }
            set { m_HostSh = value; }
        }
        // № порта сервера шлагбаума
        public int nSrvPortSh
        {
            get { return m_SrvPortSh; }
            set { m_SrvPortSh = value; }
        }
       
        #endregion
        //-----*****-----*****-----
        #region Параметры ввода
        // Параметы ввода данных для полей
        //public FieldDef[] aFields = new FieldDef[5];

        // Текущее поле
        public int CurField
        {
            get { return m_CurField; }
            set { m_CurField = value; }
        }

        // Команда Открыть - через сервер
        public bool bOpenSrv
        {
            get { return m_OpenSrv; }
            set { m_OpenSrv = value; }
        }
        // Команда Закрыть - через сервер
        public bool bCloseSrv
        {
            get { return m_CloseSrv; }
            set { m_CloseSrv = value; }
        }
        // Чтение полностью всей серии 2D
        public bool b2DFull
        {
            get { return m_2DFull; }
            set { m_2DFull = value; }
        }

        // Login для доступа к серверу шлагбаумов
        public string LoginSh
        {
            get { return m_LoginSh.Trim(); }
            set { m_LoginSh = value; }
        }
        // Pass для доступа к серверу шлагбаумов
        public string PassSh
        {
            get { return m_PassSh.Trim(); }
            set { m_PassSh = value; }
        }
        // DevID шлагбаума
        public string DevID
        {
            get { return m_DevID.Trim(); }
            set { m_DevID = value; }
        }

        // Контроль пропуска на выезде
        public bool CheckPermitOnOut
        {
            get { return m_ChkPrmOut; }
            set { m_ChkPrmOut = value; }
        }


        #endregion

        public static object InitPars(string sPath)
        {
            int nRet = AppC.RC_OK;
            object xx = null;

            sFilePars = sPath + "\\" + sFilePars;

            nRet = Srv.ReadXMLObj(typeof(AppPars), out xx, sFilePars);
            AppPars xNew = (AppPars)xx;
            if (nRet != AppC.RC_OK)
            {
                if (xNew == null)
                {
                    xNew = new AppPars();
                    SavePars(xNew);
                }
            }
            return (xNew);
        }

        public static int SavePars(AppPars x)
        {
            return (Srv.WriteXMLObjTxt(typeof(AppPars), x, sFilePars));
        }


    }

    // параметры текущей смены (пользователя)
    public class Smena
    {
        private static string sXML = "CS.XML";          // имя файла с настройками пользователь/смена

        private int m_Sklad;                            // код склада
        private int m_Uch;                              // код участка
        private DateTime m_Date;                        // дата по умолчанию
        private int m_Smena;                            // текущая смена

        private string 
            m_SDate = "";                               // дата по умолчанию (символьная)

        // права пользователей
        public enum USERRIGHTS: int
        {
            USER_KLAD   = 1,                            // кладовщик
            USER_BOSS_SMENA   = 10,                     // начальник смены
            USER_BOSS_SKLAD   = 100,                    // начальник склада
            USER_ADMIN   = 1000,                        // начальник смены
            USER_SUPER = 2000                           // наверное, Толик
        }


        public string sUser = "";                       // код пользователя
        public string sUName = "";                      // ФИО
        public USERRIGHTS 
            urCur = USERRIGHTS.USER_KLAD;               // текущие права    

        // дата-время последней загрузки всех справочников
        public DateTime dtLoadNS;

        public DateTime dBeg;                           // начало смены
        public DateTime dEnd;                           // окончание смены
        public int nLogins;

        public int nStatus;

        public int nDocs = 0;

        // код склада
        public int nSklad
        {
            get { return m_Sklad; }
            set { m_Sklad = value; }
        }
        // код участка
        public int nUch
        {
            get { return m_Uch; }
            set { m_Uch = value; }
        }
        // смена
        public int CurSmena
        {
            get { return m_Smena; }
            set { m_Smena = value; }
        }

        // Дата
        public DateTime CurDate
        {
            get { return m_Date; }
            set { m_Date = value; }
        }

        // дата документов (символьная)
        public string DocDate
        {
            get { return m_SDate; }
            set
            {
                try
                {
                    CurDate = DateTime.ParseExact(value, "dd.MM.yy", null);
                }
                catch
                {
                    CurDate = DateTime.Now;
                }
                m_SDate = CurDate.ToString("dd.MM.yy");
            }
        }


        public static int ReadSm(ref Smena xS, string sPath)
        {
            object x;
            int nRet = Srv.ReadXMLObj(typeof(Smena), out x, sPath);
            if (nRet == AppC.RC_OK)
            {
                xS = (Smena)x;
                xS.sUName = xS.sUser = "";
            }
            else
                xS = new Smena();

            return (nRet);
        }

        public int SaveCS(string sP, int nD)
        {
            //xSm.nDocs = DT[NSI.TBD_DOC].dt.Rows.Count;
            //Srv.WriteXMLObjTxt(typeof(Smena), xSm, xP.sDataPath + sP_CS);
            //dsM.WriteXml(xP.sDataPath + sP_CSDat);


            this.nDocs = nD;
            return (Srv.WriteXMLObjTxt(typeof(Smena), this, sP + sXML));
        }


    }


    public class AvtoCtrl
    {
        // Направление движения (въезд/выезд)
        private int m_InOut = AppC.DIR_IN;

        // Груженый или нет
        private int m_Empty = AppC.GRUZ_EMPTY;

        // Цель перемещения
        private int m_Purp;

        // № тягача/полуприцепа
        private string m_Avto = "";

        // № путевого
        private string m_NPutev = "";

        // Масса
        private FRACT m_Massa = 0;

        // Мест
        private int m_Mest = 0;

        // Сумма
        private FRACT m_Summa = 0;

        // № документов
        private string m_TTN;

        // количество документов
        private int m_KolTTN = 0;

        // количество документов
        private int m_KolTTN_EBD = 0;

        // № путевого для которого делали загрузку
        private string m_LoadPL = "";

        // № пропуска
        private string m_NPropusk = "";


        //--------------------------
        // Время въезд
        private DateTime m_DateIn;

        // Время выезд
        private DateTime m_DateOut;

        public AvtoCtrl(int DirM)
        {
            m_InOut = DirM;
        }


        //=================================
        public bool bCanOpen = true;

        public string PLWasLoad
        {
            get { return m_LoadPL; }
            set { m_LoadPL = value; }
        }

        // параметры с сервера
        public string SrvAns = "";
        public string SrvCTM = "";

        // Направление движения
        public int DirMove
        {
            get { return m_InOut; }
            set { m_InOut = value; }
        }

        // Тип груза
        public int GruzType
        {
            get { return m_Empty; }
            set { m_Empty = value; }
        }

        // Цель перемещения
        public int Purp
        {
            get { return m_Purp; }
            set { m_Purp = value; }
        }

        // Тягач
        public string Avto
        {
            get { return m_Avto; }
            set { m_Avto = value; }
        }

        // № путевого
        public string Putev
        {
            get { return m_NPutev; }
            set { m_NPutev = value; }
        }

        // Масса
        public FRACT Massa
        {
            get { return m_Massa; }
            set { m_Massa = value; }
        }

        // Мест
        public int Mest
        {
            get { return m_Mest; }
            set { m_Mest = value; }
        }

        // Сумма
        public FRACT Summa
        {
            get { return m_Summa; }
            set { m_Summa = value; }
        }

        // № документов
        public string TTN
        {
            get { return m_TTN; }
            set { m_TTN = value; }
        }

        // количество документов (введено/отсканировано)
        public int KolTTN
        {
            get { return m_KolTTN; }
            set { m_KolTTN = value; }
        }

        // количество документов (дал сервер)
        public int KolTTN_EBD
        {
            get { return m_KolTTN_EBD; }
            set { m_KolTTN_EBD = value; }
        }

        // время выезда
        public DateTime TimeOut
        {
            get { return m_DateOut; }
            set { m_DateOut = value; }
        }

        // № пропуска
        public string Propusk
        {
            get { return m_NPropusk; }
            set { m_NPropusk = value; }
        }


        // очистка для новой машины
        public void ClearAvt(int DM, Control x)
        {
            DirMove = DM;
            Avto = "";
            if (x == null)
                Putev = "";
            Massa = 0;
            Mest = 0;
            Summa = 0;
            TTN = "";
            Propusk = "";

            KolTTN = 0;
            KolTTN_EBD = 0;

            bCanOpen = true;
            PLWasLoad = "";
        }

    }

    // поля документа
    public class DocPars
    {
        public static TextBox tKTyp = null;    // 
        public static Label tNTyp = null;    // 

        public static TextBox tDate = null;    // 

        public static TextBox tKPost = null;    // 

        public static TextBox tNDoc = null;    // 

        public int nTypD;               // тип документа (код)
        public string sTypD;            // тип документа (наименование)

        public DateTime dDatDoc;        // дата документа

        public long nPost;               // код поставщика
        public string sPost;             // наименование поставщика

        public string sNomDoc;          // № документа

        public DocPars(int nReg)
        {
            switch(nReg){
                case AppC.F_ADD_REC:
                    nTypD = AppC.TYPD_INV;
                    break;
                case AppC.F_UPLD_DOC:
                case AppC.F_LOAD_DOC:
                    nTypD = AppC.TYPD_VPER;
                    break;
                default:
                    sNomDoc = "";
                    break;
            }
            //dDatDoc = xS.CurDate;
        }

        // наименование типа документа
        public static string TypName(ref int nTD)
        {
            string s = "Неизвестно";
            switch (nTD)
            {
                case AppC.TYPD_SAMV:
                    s = "Самовывоз";
                    break;
                case AppC.TYPD_СVYV:
                    s = "Центровывоз";
                    break;
                case AppC.TYPD_VPER:
                    s = "В_Перемещение";
                    break;
                case AppC.TYPD_SVOD:
                    s = "Свод";
                    break;
                case AppC.TYPD_AKT_SPIS:
                    s = "Акт списания";
                    break;
                case AppC.TYPD_INV:
                    s = "Инвентаризация";
                    break;
                case AppC.TYPD_VZV_TTN:
                    s = "Возврат-ТТН";
                    break;
                case AppC.TYPD_VZV_SVOD:
                    s = "Возврат-свод";
                    break;
                case AppC.EMPTY_INT:
                    s = "";
                    break;
                default:
                    nTD = AppC.EMPTY_INT;
                    break;
            }
            return (s);
        }
    }


    // текущий документ
    public class CurDoc
    {
        public DataRow drCurRow = null; // текущая строка в таблице

        public int nId;                 // код документа

        public bool bSpecCond;          // особые условия для детальных строк

        public int nDocSrc;             // происхождение документа (загружен или введен)
        public int nStrokZ;             // строк в заявке
        public int nStrokV;             // строк введено

        public DocPars xDocP;

        public byte[] From2D;
        public int nHead2D;
        public int nFull2D;
        public long nIDSer;

        public CurDoc(int nReg){
            switch (nReg)
            {
                case AppC.F_ADD_REC:
                    // создание нового документа
                    //nCurRec = AppC.EMPTY_INT;
                    nId = AppC.EMPTY_INT;
                    break;
                default:
                    break;
            }

            //nDt = NSI.I_DOCOUT;
            xDocP = new DocPars(nReg);

            bSpecCond = true;
            nStrokZ = 0;
        }
    }

    // текущая загрузка
    public class CurLoad
    {
        //режим загрузки
        public IntRegsAvail ilLoad;

        // параметры фильтра
        public DocPars xLP;

        // результат загрузки
        public DataSet dsZ;
        // символьное выражение фильтра
        public string sFilt;

        public CurLoad()
            : this(AppC.UPL_CUR) {}
        public CurLoad(int nRegLoad)
        {
            xLP = new DocPars(AppC.F_LOAD_DOC);
            ilLoad = new IntRegsAvail(nRegLoad);
        }
    }

    // доступные значения режимов
    public class IntRegsAvail
    {
        private struct RegAttr
        {
            public int RegValue;
            public string RegName;
            public bool bRegAvail;

            public RegAttr(int RV, string RN, bool RA)
            {
                RegValue = RV;
                RegName = RN;
                bRegAvail = RA;
            }
        }

        private List<RegAttr> lRegs;
        private int nI;

        public IntRegsAvail() : this(AppC.UPL_CUR) { }

        public IntRegsAvail(int nSetCur)
        {
            lRegs = new List<RegAttr>(5);
            lRegs.Add(new RegAttr(AppC.UPL_CUR, "Текущий", true));
            lRegs.Add(new RegAttr(AppC.UPL_ALL, "Все", false));
            lRegs.Add(new RegAttr(AppC.UPL_FLT, "По фильтру", false));

            nI = 0;
            CurReg = nSetCur;
        }

        // поиск по заданному значению
        private int FindByVal(int V)
        {
            int ret = -1;
            int nK = 0;
            foreach (RegAttr ra in lRegs)
            {
                if (ra.RegValue == V)
                {
                    ret = nK;
                    break;
                }
                nK++;
            }
            return (ret);
        }

        // Текущий режим
        public int CurReg {
            get { return (lRegs[nI].RegValue); }
            set
            {
                int nK = FindByVal(value);
                if (nK >= 0)
                    nI = nK;
            }
        }

        // Наименование текущего режима
        public string CurRegName
        {
            get { return (lRegs[nI].RegName); }
        }

        // установить доступность текущего режима
        public bool CurRegAvail
        {
            get { return (lRegs[nI].bRegAvail); }
            set { 
                RegAttr ra = lRegs[nI];
                ra.bRegAvail = value;
                lRegs[nI] = ra;
            }
        }

        // установить следующий/предыдущий доступные режимы
        public string NextReg(bool bUp)
        {
            int nK;

            if (bUp == true)
            {// выбор следующего
                nK = (nI == lRegs.Count - 1) ? 0: nI + 1;
                while ((nK < lRegs.Count) && (nK != nI))
                {
                    if (lRegs[nK].bRegAvail == true)
                    {
                        nI = nK;
                        break;
                    }
                    nK++;
                    if (nK == lRegs.Count)
                        nK = 0;
                }
            }
            else
            {
                nK = (nI == 0)? lRegs.Count - 1 : nI - 1;
                while ((nK >= 0) && (nK != nI))
                {
                    if (lRegs[nK].bRegAvail == true)
                    {
                        nI = nK;
                        break;
                    }
                    if (nK == 0)
                        nK = lRegs.Count - 1;
                    else
                        nK--;
                }
            }

            return (lRegs[nI].RegName);
        }

        // флаг доступности для всех
        public void SetAllAvail(bool bFlag)
        {
            for (int i = 0; i < lRegs.Count; i++ )
            {
                RegAttr ra = lRegs[i];
                ra.bRegAvail = bFlag;
                lRegs[i] = ra;
            }
        }

        // Установить доступность конкретному
        public bool SetAvail(int nReg, bool v)
        {
            bool ret = false;
            int nK = FindByVal(nReg);
            if (nK >= 0)
            {
                RegAttr ra = lRegs[nK];
                ra.bRegAvail = v;
                lRegs[nK] = ra;
                ret = true;
            }
            return (ret);
        }


    }

    // текущая выгрузка
    public class CurUpLoad
    {
        //режим выгрузки
        public IntRegsAvail ilUpLoad;

        // параметры фильтра
        public DocPars xLP;

        // параметры команды
        public string sComPar;

        public CurDoc xCur;

        public List<int> naComms;

        public CurUpLoad()
            : this(AppC.UPL_CUR) {}

        public CurUpLoad(int nRegUpl)
        {
            xLP = new DocPars(AppC.F_UPLD_DOC);
            ilUpLoad = new IntRegsAvail(nRegUpl);
        }

        //public DataRow SetFiltInRow(NSI xNSI)
        public string SetFiltInRow()
        {

            string sF = String.Format("(TD={0}) AND (DT={1}) AND (KPP1={2})",
                xLP.nTypD, xLP.dDatDoc.ToString("yyyyMMdd"), xLP.nPost);

            return ("(" + sF + ")");
        }

    }

    public class ScanDat
    {
        // результаты сканирования
        public ScannerAll.BCId ci;      // тип штрих-кода
        public string s;                // штрих-код
        public DateTime dScan;          // дата-время сканирования

        public ScanDat(ScannerAll.BarcodeScannerEventArgs e)
        {
            ci = e.nID;
            s = e.Data;
            dScan = DateTime.Now;

        }

    }

    public sealed class PSC_Types
    {

        public struct ScDat
        {
            // результаты сканирования
            public ScannerAll.BCId ci;      // тип штрих-кода
            public string s;                // штрих-код
            public DateTime dScan;          // дата-время сканирования

            // выдрали из штрих-кода
            public int nParty;              // партия
            public string sDataIzg;         // дата изготовления (символьно)
            public DateTime dDataIzg;       // дата изготовления
            public FRACT fEmk;              // емкость в штуках (для штучного) или 
                                            // вес упаковки (для весового); 0 - единичный товар

            public int nTara;               // краткий код тары(N(2))
            public FRACT fVes;              // вес
            public int nMestPal;            // количество мест на палетте

            // tmc
            public int nSysN;




            public int nMest;               // количество мест
            public FRACT fVsego;            // всего штук /вес
            public int nEmkSht;             // емкость в штуках (для весового)


            // будет нужно -???
            //public int nKolSht;             // количество (штуки)
            //public float nKolVes;           // количество (вес)
            // будет нужно -???

            public bool bFindNSI;           // удалось найти в НСИ

            //--- накопленные данные
            public FRACT fKolE_alr;         // уже введено единиц данного кода (мест = 0)
            public int nKolM_alr;           // уже введено мест данного кода
            public FRACT fMKol_alr;         // уже введено количество продукции (мест != 0)
            //--- накопленные данные (точное совпадение)
            public FRACT fKolE_alrT;        // уже введено единиц данного кода (мест = 0)
            public int nKolM_alrT;          // уже введено мест данного кода
            public FRACT fMKol_alrT;        // уже введено количество продукции (мест != 0)

            //--- заявка - накопленные данные
            public FRACT fKolE_zvk;         // единиц данного кода всего
            public FRACT fKolE_zvk_ost;     // единиц данного кода еще осталось (мест = 0) по заявке
            public int nKolM_zvk;           // мест данного кода  по заявке
            public int nKolM_zvk_ost;       // мест данного кода  еще осталось по заявке
            public FRACT fMKol_zvk;         // количество (мест != 0)

            // адреса
            public System.Data.DataRow drEd;            // куда суммировать единицы в ТТН
            public System.Data.DataRow drMest;          // куда суммировать места в ТТН

            // строки из заявки
            public System.Data.DataRow drTotKey;        // заявка на места с конкретной партией
            public System.Data.DataRow drPartKey;       // заявка на места с любой партией
            public System.Data.DataRow drTotKeyE;       // заявка на единички с конкретной партией
            public System.Data.DataRow drPartKeyE;      // заявка на единички с любой партией

            public System.Data.DataRow drMC;            // строка в справочнике матценностей
            // из справочника матценностей
            public string sKMC;             // полный код
            public int nKrKMC;              // краткий код
            public string sN;               // наименование
            public int nSrok;               // срок реализации (часы)
            public bool bVes;               // признак весового
            public string sEAN;             // EAN-код продукции
            public string sGrK;             // групповой код продукции
            public FRACT fEmk_s;            // для восстановления емкости при переключениях мест=0
            public string sParty;

            // происхождение строки
            //public NSI.DESTINPROD nDest;                // что из заявки закрывается (общая или точная часть)

            // результат контроля по данному коду-емкости
            public int nDocCtrlResult;

            public ScDat(ScannerAll.BarcodeScannerEventArgs e)
            {
                ci = e.nID;                         // тип штрих-кода
                s = e.Data;                         // штрих-код
                dScan = DateTime.MinValue;

                nParty = AppC.EMPTY_INT;
                sDataIzg = "";
                dDataIzg = DateTime.MinValue;

                nMest = 0;
                nMestPal = 0;
                fEmk = 0;
                fVsego = 0;
                fVes = 0;

                nEmkSht = AppC.EMPTY_INT;

                bFindNSI = false;

                drEd = null;
                drMest = null;
                drMC = null;

                fKolE_alr = 0;
                nKolM_alr = 0;
                fMKol_alr = 0;

                fKolE_alrT = 0;
                nKolM_alrT = 0;
                fMKol_alrT = 0;

                fKolE_zvk = 0;       // единиц данного кода всго
                fKolE_zvk_ost = 0;   // единиц данного кода еще осталось (мест = 0) по заявке
                nKolM_zvk = 0;       // мест данного кода  по заявке
                nKolM_zvk_ost = 0;   // мест данного кода  еще осталось по заявке
                fMKol_zvk = 0;       // количество (мест != 0)

                drTotKey = null;     // заявка на места с конкретной партией
                drPartKey = null;    // заявка на места с любой партией
                drTotKeyE = null;    // заявка на единички с конкретной партией
                drPartKeyE = null;   // заявка на единички с любой партией

                sKMC = "";
                nKrKMC = AppC.EMPTY_INT;
                sN = "<Неизвестно>";
                nSrok = 0;
                nTara = 0;
                bVes = false;
                sEAN = "";
                sGrK = "";
                fEmk_s = 0;
                sParty = "";

                //nDest = NSI.DESTINPROD.GENCASE;
                nDocCtrlResult = AppC.RC_CANCEL;


                nSysN = 0;
            }

            // получить данные из справочника по EAN или коду
            public bool GetFromNSI(string s, DataRow dr)
            {
                bFindNSI = false;
                
                if (dr != null)
                {
                    drMC = dr;
                    sN = dr["SNM"].ToString();
                    sKMC = (string)dr["KMT"];
                    nKrKMC = int.Parse(sKMC);
                    bFindNSI = true;

                    bVes = false;

                }
                else
                {
                    sN = sKMC + "-нет в справочнике";
                }
                return (bFindNSI);
            }
        }


        public struct FuncKey
        {
            public int nF;
            public int nKeyValue;
            public Keys kMod;
            public FuncKey(int f, int v, Keys m)
            {
                nF = f;
                nKeyValue = v;
                kMod = m;
            }
        }
    }


    public class KMTScan
    {
        public int nType;
        public int nSysN;
        public string sKMT;
        public int nParty;
        public DateTime dV;
        public DateTime dG;
        public FRACT fEmk;
        public string sN;

        public KMTScan(string s)
        {
            try
            {
                nType = int.Parse(s.Substring(0, 2));
                nSysN = int.Parse(s.Substring(2, 9));
                sKMT = s.Substring(11, 10);
                nParty = int.Parse(s.Substring(21, 4));
                dV = DateTime.ParseExact(s.Substring(25, 6), "yyMMdd", null);
                fEmk = FRACT.Parse(s.Substring(31, 7));
            }
            catch { }
        }
        public KMTScan(DataRow dr)
        {
            try
            {
                nType = 11;
                sKMT = (string)dr["KMT"];
                sN = (string)dr["SNM"];
                fEmk = (FRACT)dr["EMK"];
                nSysN = (int)dr["SYSN"];
                nParty = (int)dr["NP"];
                dV = DateTime.ParseExact((string)dr["DVR"], "yyyyMMdd", null);
            }
            catch { }
        }


    }
}
