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
        // ����� ��� ��� �����������
        internal const string GUEST = "1";

        public const int RC_NOID = 401;         // �� ������ ID ��������
        public const int RC_BADID = 20;         // �� ��������� ID ����� (��� ��������)

        public const int F_CHGZMK = 63;         // ����� �����
        public const int F_OPENSH = 71;         // ������� �������� �� ���������
        public const int F_CLOSESH = 72;        // ������� �������� �� ���������

        // ������ ���������� ������
        internal const int COM_ZSPR     = 0;    // �������� ������������
        internal const int COM_ZPL      = 1;    // �������� �������� �� � ��������
        internal const int COM_ZCONTR   = 2;    // �������� �� ������
        internal const int COM_ZOPEN    = 3;    // ������� ��������
        internal const int COM_ZCLOSE   = 4;    // ������� ��������
        // ���������� �������� ������
        internal static string[] saComms = { "ZSPR", "ZPL", "ZCONTR", "ZOPEN", "ZCLOSE" };


        // ���������� ��� �������
        internal static byte[] baTermCom = { 13, 10 };
        // ���������� ��� ������������ ������
        internal static byte[] baTermMsg = { 13, 10, 0x2E, 13, 10 };

        // ����� � ���� Help
        internal const int HELPLINES = 19;
        // ����� � ���� ���.����������
        internal const int INFLINES = 17;

        // ���� ����������
        public const int TYPD_SAMV = 0;             // ��������-���������
        public const int TYPD_�VYV = 1;             // ��������-�����������
        public const int TYPD_SVOD = 2;             // �������� �� �����
        public const int TYPD_VPER = 3;             // ���������� �����������
        public const int TYPD_AKT_SPIS = 4;         // ��� ��������
        public const int TYPD_INV = 7;             // ��������������
        public const int TYPD_VZV_TTN = 10;         // ������� �� ���������
        public const int TYPD_VZV_SVOD = 12;        // ������� �� �����

        internal const int DIR_IN = 1;
        internal const int DIR_OUT = 2;

        internal const int GRUZ_EMPTY = 1;
        internal const int GRUZ_MAIN = 2;

    }



}

namespace Proh
{
   

    // ��������� ���������
    public sealed class AppPars
    {

        //===***===
        // ���� � ��������� �����
        private string m_AppStore;
        // ���� � ���
        private string m_NSIPath;
        // ���� � ������
        private string m_DataPath;

        // HOST-name �������
        private string m_Host;
        // � ����� ������� (����� �������)
        private int m_SrvPort;
        // � ����� ������� (����� �����������)
        private int m_SrvPortM;
        // ���/���� ����� ����������� � ��������
        private bool m_WaitSock;

        // HOST-name ������� ���������
        private string m_HostSh;
        // � ����� ������� ���������
        private int m_SrvPortSh;

        // ���� � �����
        private int m_Smennost;


        private bool 
            m_OpenSrv,                  // ������� ������� - ����� ������
            m_CloseSrv,                 // ������� ������� - ����� ������
            m_ChkPrmOut = true,         // �������� �������� �� ������
            m_2DFull;                   // ������ ��������� ���� ����� 2D

        // Login ��� ������� � ������� ����������
        private string m_LoginSh = "test";
        // Pass ��� ������� � ������� ����������
        private string m_PassSh = "TseT";
        // DevID ���������
        private string m_DevID = "SHLG";

        //-----*****-----*****-----
        // ������� ���� (������)
        private int m_CurField;

        /// ������ �����
        /// 

        // ������� � �����������
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
        #region ����� ��������� 

        // ���� � ��������� �����
        public string sAppStore
        {
            get { return m_AppStore; }
            set { m_AppStore = value; }
        }
        // ���� � ���
        public string sNSIPath
        {
            get { return m_NSIPath; }
            set { m_NSIPath = value; }
        }
        // ���� � ������
        public string sDataPath
        {
            get { return m_DataPath; }
            set { m_DataPath = value; }
        }

        // HOST-name �������
        public string sHostSrv
        {
            get { return m_Host; }
            set { m_Host = value; }
        }
        // � ����� ������� (����� �������)
        public int nSrvPort
        {
            get { return m_SrvPort; }
            set { m_SrvPort = value; }
        }
        // � ����� ������� (����� �����������)
        public int nSrvPortM
        {
            get { return m_SrvPortM; }
            set { m_SrvPortM = value; }
        }
        // ���/���� ����� ����������� � ��������
        public bool bWaitSock
        {
            get { return m_WaitSock; }
            set { m_WaitSock = value; }
        }
        // ��������� � �����
        public int Smennost
        {
            get { return m_Smennost; }
            set { m_Smennost = value; }
        }
        // HOST-name ������� ���������
        public string sHostSrvSh
        {
            get { return m_HostSh; }
            set { m_HostSh = value; }
        }
        // � ����� ������� ���������
        public int nSrvPortSh
        {
            get { return m_SrvPortSh; }
            set { m_SrvPortSh = value; }
        }
       
        #endregion
        //-----*****-----*****-----
        #region ��������� �����
        // �������� ����� ������ ��� �����
        //public FieldDef[] aFields = new FieldDef[5];

        // ������� ����
        public int CurField
        {
            get { return m_CurField; }
            set { m_CurField = value; }
        }

        // ������� ������� - ����� ������
        public bool bOpenSrv
        {
            get { return m_OpenSrv; }
            set { m_OpenSrv = value; }
        }
        // ������� ������� - ����� ������
        public bool bCloseSrv
        {
            get { return m_CloseSrv; }
            set { m_CloseSrv = value; }
        }
        // ������ ��������� ���� ����� 2D
        public bool b2DFull
        {
            get { return m_2DFull; }
            set { m_2DFull = value; }
        }

        // Login ��� ������� � ������� ����������
        public string LoginSh
        {
            get { return m_LoginSh.Trim(); }
            set { m_LoginSh = value; }
        }
        // Pass ��� ������� � ������� ����������
        public string PassSh
        {
            get { return m_PassSh.Trim(); }
            set { m_PassSh = value; }
        }
        // DevID ���������
        public string DevID
        {
            get { return m_DevID.Trim(); }
            set { m_DevID = value; }
        }

        // �������� �������� �� ������
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

    // ��������� ������� ����� (������������)
    public class Smena
    {
        private static string sXML = "CS.XML";          // ��� ����� � ����������� ������������/�����

        private int m_Sklad;                            // ��� ������
        private int m_Uch;                              // ��� �������
        private DateTime m_Date;                        // ���� �� ���������
        private int m_Smena;                            // ������� �����

        private string 
            m_SDate = "";                               // ���� �� ��������� (����������)

        // ����� �������������
        public enum USERRIGHTS: int
        {
            USER_KLAD   = 1,                            // ���������
            USER_BOSS_SMENA   = 10,                     // ��������� �����
            USER_BOSS_SKLAD   = 100,                    // ��������� ������
            USER_ADMIN   = 1000,                        // ��������� �����
            USER_SUPER = 2000                           // ��������, �����
        }


        public string sUser = "";                       // ��� ������������
        public string sUName = "";                      // ���
        public USERRIGHTS 
            urCur = USERRIGHTS.USER_KLAD;               // ������� �����    

        // ����-����� ��������� �������� ���� ������������
        public DateTime dtLoadNS;

        public DateTime dBeg;                           // ������ �����
        public DateTime dEnd;                           // ��������� �����
        public int nLogins;

        public int nStatus;

        public int nDocs = 0;

        // ��� ������
        public int nSklad
        {
            get { return m_Sklad; }
            set { m_Sklad = value; }
        }
        // ��� �������
        public int nUch
        {
            get { return m_Uch; }
            set { m_Uch = value; }
        }
        // �����
        public int CurSmena
        {
            get { return m_Smena; }
            set { m_Smena = value; }
        }

        // ����
        public DateTime CurDate
        {
            get { return m_Date; }
            set { m_Date = value; }
        }

        // ���� ���������� (����������)
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
        // ����������� �������� (�����/�����)
        private int m_InOut = AppC.DIR_IN;

        // �������� ��� ���
        private int m_Empty = AppC.GRUZ_EMPTY;

        // ���� �����������
        private int m_Purp;

        // � ������/�����������
        private string m_Avto = "";

        // � ��������
        private string m_NPutev = "";

        // �����
        private FRACT m_Massa = 0;

        // ����
        private int m_Mest = 0;

        // �����
        private FRACT m_Summa = 0;

        // � ����������
        private string m_TTN;

        // ���������� ����������
        private int m_KolTTN = 0;

        // ���������� ����������
        private int m_KolTTN_EBD = 0;

        // � �������� ��� �������� ������ ��������
        private string m_LoadPL = "";

        // � ��������
        private string m_NPropusk = "";


        //--------------------------
        // ����� �����
        private DateTime m_DateIn;

        // ����� �����
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

        // ��������� � �������
        public string SrvAns = "";
        public string SrvCTM = "";

        // ����������� ��������
        public int DirMove
        {
            get { return m_InOut; }
            set { m_InOut = value; }
        }

        // ��� �����
        public int GruzType
        {
            get { return m_Empty; }
            set { m_Empty = value; }
        }

        // ���� �����������
        public int Purp
        {
            get { return m_Purp; }
            set { m_Purp = value; }
        }

        // �����
        public string Avto
        {
            get { return m_Avto; }
            set { m_Avto = value; }
        }

        // � ��������
        public string Putev
        {
            get { return m_NPutev; }
            set { m_NPutev = value; }
        }

        // �����
        public FRACT Massa
        {
            get { return m_Massa; }
            set { m_Massa = value; }
        }

        // ����
        public int Mest
        {
            get { return m_Mest; }
            set { m_Mest = value; }
        }

        // �����
        public FRACT Summa
        {
            get { return m_Summa; }
            set { m_Summa = value; }
        }

        // � ����������
        public string TTN
        {
            get { return m_TTN; }
            set { m_TTN = value; }
        }

        // ���������� ���������� (�������/�������������)
        public int KolTTN
        {
            get { return m_KolTTN; }
            set { m_KolTTN = value; }
        }

        // ���������� ���������� (��� ������)
        public int KolTTN_EBD
        {
            get { return m_KolTTN_EBD; }
            set { m_KolTTN_EBD = value; }
        }

        // ����� ������
        public DateTime TimeOut
        {
            get { return m_DateOut; }
            set { m_DateOut = value; }
        }

        // � ��������
        public string Propusk
        {
            get { return m_NPropusk; }
            set { m_NPropusk = value; }
        }


        // ������� ��� ����� ������
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

    // ���� ���������
    public class DocPars
    {
        public static TextBox tKTyp = null;    // 
        public static Label tNTyp = null;    // 

        public static TextBox tDate = null;    // 

        public static TextBox tKPost = null;    // 

        public static TextBox tNDoc = null;    // 

        public int nTypD;               // ��� ��������� (���)
        public string sTypD;            // ��� ��������� (������������)

        public DateTime dDatDoc;        // ���� ���������

        public long nPost;               // ��� ����������
        public string sPost;             // ������������ ����������

        public string sNomDoc;          // � ���������

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

        // ������������ ���� ���������
        public static string TypName(ref int nTD)
        {
            string s = "����������";
            switch (nTD)
            {
                case AppC.TYPD_SAMV:
                    s = "���������";
                    break;
                case AppC.TYPD_�VYV:
                    s = "�����������";
                    break;
                case AppC.TYPD_VPER:
                    s = "�_�����������";
                    break;
                case AppC.TYPD_SVOD:
                    s = "����";
                    break;
                case AppC.TYPD_AKT_SPIS:
                    s = "��� ��������";
                    break;
                case AppC.TYPD_INV:
                    s = "��������������";
                    break;
                case AppC.TYPD_VZV_TTN:
                    s = "�������-���";
                    break;
                case AppC.TYPD_VZV_SVOD:
                    s = "�������-����";
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


    // ������� ��������
    public class CurDoc
    {
        public DataRow drCurRow = null; // ������� ������ � �������

        public int nId;                 // ��� ���������

        public bool bSpecCond;          // ������ ������� ��� ��������� �����

        public int nDocSrc;             // ������������� ��������� (�������� ��� ������)
        public int nStrokZ;             // ����� � ������
        public int nStrokV;             // ����� �������

        public DocPars xDocP;

        public byte[] From2D;
        public int nHead2D;
        public int nFull2D;
        public long nIDSer;

        public CurDoc(int nReg){
            switch (nReg)
            {
                case AppC.F_ADD_REC:
                    // �������� ������ ���������
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

    // ������� ��������
    public class CurLoad
    {
        //����� ��������
        public IntRegsAvail ilLoad;

        // ��������� �������
        public DocPars xLP;

        // ��������� ��������
        public DataSet dsZ;
        // ���������� ��������� �������
        public string sFilt;

        public CurLoad()
            : this(AppC.UPL_CUR) {}
        public CurLoad(int nRegLoad)
        {
            xLP = new DocPars(AppC.F_LOAD_DOC);
            ilLoad = new IntRegsAvail(nRegLoad);
        }
    }

    // ��������� �������� �������
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
            lRegs.Add(new RegAttr(AppC.UPL_CUR, "�������", true));
            lRegs.Add(new RegAttr(AppC.UPL_ALL, "���", false));
            lRegs.Add(new RegAttr(AppC.UPL_FLT, "�� �������", false));

            nI = 0;
            CurReg = nSetCur;
        }

        // ����� �� ��������� ��������
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

        // ������� �����
        public int CurReg {
            get { return (lRegs[nI].RegValue); }
            set
            {
                int nK = FindByVal(value);
                if (nK >= 0)
                    nI = nK;
            }
        }

        // ������������ �������� ������
        public string CurRegName
        {
            get { return (lRegs[nI].RegName); }
        }

        // ���������� ����������� �������� ������
        public bool CurRegAvail
        {
            get { return (lRegs[nI].bRegAvail); }
            set { 
                RegAttr ra = lRegs[nI];
                ra.bRegAvail = value;
                lRegs[nI] = ra;
            }
        }

        // ���������� ���������/���������� ��������� ������
        public string NextReg(bool bUp)
        {
            int nK;

            if (bUp == true)
            {// ����� ����������
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

        // ���� ����������� ��� ����
        public void SetAllAvail(bool bFlag)
        {
            for (int i = 0; i < lRegs.Count; i++ )
            {
                RegAttr ra = lRegs[i];
                ra.bRegAvail = bFlag;
                lRegs[i] = ra;
            }
        }

        // ���������� ����������� �����������
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

    // ������� ��������
    public class CurUpLoad
    {
        //����� ��������
        public IntRegsAvail ilUpLoad;

        // ��������� �������
        public DocPars xLP;

        // ��������� �������
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
        // ���������� ������������
        public ScannerAll.BCId ci;      // ��� �����-����
        public string s;                // �����-���
        public DateTime dScan;          // ����-����� ������������

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
            // ���������� ������������
            public ScannerAll.BCId ci;      // ��� �����-����
            public string s;                // �����-���
            public DateTime dScan;          // ����-����� ������������

            // ������� �� �����-����
            public int nParty;              // ������
            public string sDataIzg;         // ���� ������������ (���������)
            public DateTime dDataIzg;       // ���� ������������
            public FRACT fEmk;              // ������� � ������ (��� ��������) ��� 
                                            // ��� �������� (��� ��������); 0 - ��������� �����

            public int nTara;               // ������� ��� ����(N(2))
            public FRACT fVes;              // ���
            public int nMestPal;            // ���������� ���� �� �������

            // tmc
            public int nSysN;




            public int nMest;               // ���������� ����
            public FRACT fVsego;            // ����� ���� /���
            public int nEmkSht;             // ������� � ������ (��� ��������)


            // ����� ����� -???
            //public int nKolSht;             // ���������� (�����)
            //public float nKolVes;           // ���������� (���)
            // ����� ����� -???

            public bool bFindNSI;           // ������� ����� � ���

            //--- ����������� ������
            public FRACT fKolE_alr;         // ��� ������� ������ ������� ���� (���� = 0)
            public int nKolM_alr;           // ��� ������� ���� ������� ����
            public FRACT fMKol_alr;         // ��� ������� ���������� ��������� (���� != 0)
            //--- ����������� ������ (������ ����������)
            public FRACT fKolE_alrT;        // ��� ������� ������ ������� ���� (���� = 0)
            public int nKolM_alrT;          // ��� ������� ���� ������� ����
            public FRACT fMKol_alrT;        // ��� ������� ���������� ��������� (���� != 0)

            //--- ������ - ����������� ������
            public FRACT fKolE_zvk;         // ������ ������� ���� �����
            public FRACT fKolE_zvk_ost;     // ������ ������� ���� ��� �������� (���� = 0) �� ������
            public int nKolM_zvk;           // ���� ������� ����  �� ������
            public int nKolM_zvk_ost;       // ���� ������� ����  ��� �������� �� ������
            public FRACT fMKol_zvk;         // ���������� (���� != 0)

            // ������
            public System.Data.DataRow drEd;            // ���� ����������� ������� � ���
            public System.Data.DataRow drMest;          // ���� ����������� ����� � ���

            // ������ �� ������
            public System.Data.DataRow drTotKey;        // ������ �� ����� � ���������� �������
            public System.Data.DataRow drPartKey;       // ������ �� ����� � ����� �������
            public System.Data.DataRow drTotKeyE;       // ������ �� �������� � ���������� �������
            public System.Data.DataRow drPartKeyE;      // ������ �� �������� � ����� �������

            public System.Data.DataRow drMC;            // ������ � ����������� ������������
            // �� ����������� ������������
            public string sKMC;             // ������ ���
            public int nKrKMC;              // ������� ���
            public string sN;               // ������������
            public int nSrok;               // ���� ���������� (����)
            public bool bVes;               // ������� ��������
            public string sEAN;             // EAN-��� ���������
            public string sGrK;             // ��������� ��� ���������
            public FRACT fEmk_s;            // ��� �������������� ������� ��� ������������� ����=0
            public string sParty;

            // ������������� ������
            //public NSI.DESTINPROD nDest;                // ��� �� ������ ����������� (����� ��� ������ �����)

            // ��������� �������� �� ������� ����-�������
            public int nDocCtrlResult;

            public ScDat(ScannerAll.BarcodeScannerEventArgs e)
            {
                ci = e.nID;                         // ��� �����-����
                s = e.Data;                         // �����-���
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

                fKolE_zvk = 0;       // ������ ������� ���� ����
                fKolE_zvk_ost = 0;   // ������ ������� ���� ��� �������� (���� = 0) �� ������
                nKolM_zvk = 0;       // ���� ������� ����  �� ������
                nKolM_zvk_ost = 0;   // ���� ������� ����  ��� �������� �� ������
                fMKol_zvk = 0;       // ���������� (���� != 0)

                drTotKey = null;     // ������ �� ����� � ���������� �������
                drPartKey = null;    // ������ �� ����� � ����� �������
                drTotKeyE = null;    // ������ �� �������� � ���������� �������
                drPartKeyE = null;   // ������ �� �������� � ����� �������

                sKMC = "";
                nKrKMC = AppC.EMPTY_INT;
                sN = "<����������>";
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

            // �������� ������ �� ����������� �� EAN ��� ����
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
                    sN = sKMC + "-��� � �����������";
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
