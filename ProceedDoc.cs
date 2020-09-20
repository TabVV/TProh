using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Data;

using ScannerAll;
using PDA.OS;
using PDA.Service;

namespace Proh
{
    public partial class MainF : Form
    {

        /*
         * 
// ������� � ������� ������
 IL_MAX  =  6;

 IL_KOD   = 0;
 IL_NAME  = 1;
 IL_FUNC  = 2;
 IL_TABLE = 3;
 IL_ID    = 4;
 IL_MEMO  = 5;

// ������� � ������� ���������
 IE_MAX  = 15;
{
 IE_TYPE = 0;
 IE_TYPD = 1;
 IE_LEN  = 2;
 IE_TOCH = 3;
 IE_TYPI = 4;
 IE_LENI = 5;
 IE_FLD  = 6;
 IE_OUT  = 7;
 IE_FMT  = 8;
 IE_MAC  = 9;
}
 IE_KOD  = 0;     //!!!
 IE_NPP  = 1;     //!!!
 IE_TYPE = 2;
 IE_NAME = 3;     //!!!
 IE_TYPD = 4;
 IE_LEN  = 5;
 IE_TOCH = 6;
 IE_TYPI = 7;
 IE_LENI = 8;
 IE_FLD  = 9;
 IE_BC   = 10;    //!!!
 IE_MEM  = 11;    //!!!
 IE_FMT  = 12;
 IE_OUT  = 13;
 IE_MAC  = 14;
         */

        // ���� ���������
        public enum ELTYPES : int
        {
            TE_DAT = 1,         // ������
            TE_MACRO = 2,       // ������
            TE_NREC = 4,        // ���������� �������
            TE_EANCL = 5,       // ����� ���������� ����� EAN
            TE_EANC = 6,        // ���������� ����� EAN
            TE_EANV = 7,        // ���������� ����� EAN
            TE_RINT = 11,       // ������ �������� (0 - ����������, 1 - ��������)
            TE_RZIP = 12,       // ����� ��������� (0 - ��������, 1 - �������)
            TE_RMULT = 21,      // ����� ��������� ����������� (0 - ��������)
            TE_ID = 41,         // ID ������������������ 2d-��������
            TE_IDSEQ = 42,      // ID ������������������ 2d-��������
            TE_IDDEF = 43,      // ID ��������
            TE_I2D = 50,        // ���������� ����� 2D-������� (1, 2, ...)
            TE_HLEN = 51,       // ����� ��������� 2D-�������(����)
            TE_TOT2D = 52,      // ����� 2D-�������� �� ����
            TE_SZ2D = 53,       // ������ ������ ������� (����) - ��������� + ������
            TE_SZDAT = 54       // ������ ����� (����) - ������ ������
        }

        // ���� ������
        public const char T_E = ' ';        // ������
        public const char T_N = 'N';        // ��������
        public const char T_C = 'C';        // ����������
        public const char T_D = 'D';        // ����
        public const char T_I = 'I';        // �����
        public const char T_S = 'S';        // ��������� ��������
        public const char T_2 = 'D';        // ������� ��������

        // ��������� ������ 2D
        public enum READ2D_STATE : int
        {
            R_WAIT_SER = 0,                 // �������� �����
            R_WAIT_NEXT = 1,                // �������� ������� �����
            R_WAIT_IN = 2,                  // �������� ��������� �������
            R_FULL_SER = 10,                // ����� �������
            R_SAME_DOC = 20,                // ��������� ������
            R_BAD_KEYS = 21,                // �������� ��������� �� �������
            R_QUIT_SER = 50                 // ��������� ������ �����
        }

        public class MacroDef
        {
            public MacroDef(string sN)
            {
                sName = sN;
            }
            public string sName;
            public string sVal = "";
            public object xD = null;
        }

        // �������� ������ �������� (������) �������� 2D
        public class ElemDef
        {
            public ElemDef(int n, string s, ELTYPES t, string td, int d)
            {
                nNPP = n;
                sNAME = s.Trim();
                nTYPE = t;
                cTYPD = td.Trim()[0];
                nLEN = d;
            }

            public string sKOD = "";        //!!!
            public int nNPP;     //!!!
            public ELTYPES nTYPE;
            public string sNAME = "";     //!!!
            public char cTYPD = T_E;
            public int nLEN;
            public int nTOCH;
            public char cTYPI = T_E;
            public int nLENI;
            public string sFLD = "";
            public string sBC = "";    //!!!
            public int nMacInd = -1;    //!!!
            public string sFMT = "";
            public bool bOUT;
            public bool bMAC = false;


        }

        public class ListMac : List<MacroDef>
        {
            private string StrByName(string N)
            {
                int i;
                string s = "";
                for (i = 0; i < this.Count; i++){
                    if (this[i].sName == N)
                    {
                        s = this[i].sVal;
                        break;
                    }
                }
                return (s);
            }

            public object ValByName(string N)
            {
                int i;
                object s = null;
                for (i = 0; i < this.Count; i++)
                {
                    if (this[i].sName == N)
                    {
                        s = this[i].xD;
                        break;
                    }
                }
                return (s);
            }

            public string this[string name]
            {
                get{ return (StrByName(name)); }
            }

        }

        // �������� ������ ������
        public class LevDef
        {
            public LevDef(int nL, string sN)
            {
                nLev = nL;
                sNAME = sN;
                lMac = new ListMac();
            }

            public int    nLev;     //!!!
            public string sNAME;     //!!!
            public string sFUNC;
            public string sTABLE;
            public ListMac lMac;
            public string sMEM;    //!!!
        }

        // �������� ��������� 2D-����
        public class DefOneLev
        {
            public object xDeepLev;
            public List<ElemDef> aElems;
            public LevDef stLev;
        }

        public class Def2Data
        {
            public List<DefOneLev> lDef;
            public string sID_OAO = "";
            public byte[] bID_Def;
            public int n;
        }

        public class One2DSym
        {
            public string sID_Global = "";      // ID ���������/��������
            public long nID_Serial = -1;        // ID ������������������(�����)
            public int nMaxSer = -1;            // ����� �������� � ������������������
            public int nInSer = -1;             // � � ������������������
            public int nSymLen = -1;            // ����� �������
            public int nHeadLen = -1;           // ����� ���������
            public int nDataLen = -1;           // ����� ������
            public int nFileLen = -1;           // ����� ���� �����
            public bool bBin = false;           // ����� ������������� (���/bin)
            public bool bZip = false;           // ���� ��������� (��/���)
            public bool bMult = false;          // ����� ��������� (��/���)

            public byte[] buf = null;
            public int nL = -1;
        }


        // ���������� ����� 2D
        public class Ser2DSym
        {
            private NSI xNSI;
            // � ����� ��������� �������� �����
            public Def2Data x�Def2D;
            // ������� ������
            One2DSym xS;

            public Ser2DSym(NSI n)
            {
                lSym = new List<One2DSym>(0);
                lSavedNs = new List<int>(0);
                nSaved2D = 0;
                xNSI = n;
                bRewrite = false;
            }

            public List<One2DSym> lSym;
            public List<int> lSavedNs;          // ������ ����������� ��������
            public READ2D_STATE State =         // ������� ��������� ����������
                READ2D_STATE.R_WAIT_SER;

            public bool bRewrite = false;       // ������� ����������
            public DataRow drRew;               // ����� ������ ����������
            public CurDoc xD;                   // �������� ��� ������ � ������

            public string sID_Global;           // ������� ID ���������
            public byte[] bID_Global;           // ������� ID ���������

            public long nID_Serial;             // ������� ID �����
            public int nMaxSer = -1;            // ����� �������� � ������������������
            public int nFileLen;                // ����� ���� �����

            public int nHeadLen;                // ����� ���������

            public bool bBin = false;           // ����� ������������� (���/bin)
            public bool bZip = false;           // ���� ��������� (��/���)
            public bool bMult = false;          // ����� ��������� (��/���)

            public int nSaved2D;                // ��� ��������� �� ������������������
            public int nSavedBytes;             // ��� ��������� �� �����

            public byte[] FullBuf;

            private int AddFileLen(int nLenWas)
            {
                int nRet = xS.nFileLen;
                if (nRet == -1)
                    nRet = nLenWas + (xS.nSymLen - xS.nHeadLen);
                return (nRet);
            }

            // ������ ����� �������� ?
            private READ2D_STATE TestIsSame()
            {
                READ2D_STATE nRet = READ2D_STATE.R_WAIT_NEXT;
                long nTyp,
                    nKsk,
                    nDoc;
                DateTime d;
                string sD;

                try
                {
                    nTyp = long.Parse(x�Def2D.lDef[0].stLev.lMac["TDOC"]);
                    d = (DateTime)x�Def2D.lDef[0].stLev.lMac.ValByName("DDOC");
                    sD = d.ToString("yyyyMMdd");
                    nKsk = long.Parse(x�Def2D.lDef[0].stLev.lMac["KSK"]);
                    nDoc = (long)x�Def2D.lDef[0].stLev.lMac.ValByName("NDOC");
                    DataView dv = new DataView(xNSI.DT[NSI.TBD_DOC].dt,
                        String.Format("(SYSN={0})AND(TD={1})AND(KPP1={2})AND(DT={3})",
                                      nDoc, nTyp, nKsk, sD), "TD", DataViewRowState.CurrentRows);
                    if (dv.Count > 0)
                    {
                        nRet = READ2D_STATE.R_SAME_DOC;
                        drRew = dv[0].Row;
                    }
                    xD.xDocP.nTypD = (int)nTyp;
                    xD.xDocP.dDatDoc = d;
                    xD.xDocP.nPost = (long)nKsk;
                    xD.xDocP.sNomDoc = nDoc.ToString();
                    xD.nIDSer = nID_Serial;
                }
                catch
                {
                    nRet = READ2D_STATE.R_BAD_KEYS;
                }
                return (nRet);
            }


            // �������� ���������� ������
            public int AddNew(byte[] b, int nL, bool b2DFull)
            {
                int nOfs,
                    nRet = AppC.RC_OK;

                xS = new One2DSym();
                xS.buf = b;
                xS.nL = nL;

                nOfs = 0;
                // ������� ��������� ���� �� ���������
                ParseLev(b, ref nOfs, x�Def2D.lDef[0], 0);
                switch (State)
                {
                    case READ2D_STATE.R_WAIT_SER:
                        nID_Serial = xS.nID_Serial;
                        if (b2DFull == false)
                        {
                            xS.nMaxSer = 1;
                            xS.nInSer = 1;
                        }

                        nMaxSer = xS.nMaxSer;
                        nFileLen = AddFileLen(0);
                        nSaved2D = 1;
                        lSavedNs.Add(xS.nInSer);
                        lSym.Add(xS);
                        xD = new CurDoc(AppC.F_ADD_REC);
                        State = TestIsSame();
                        switch (State)
                        {
                            case READ2D_STATE.R_SAME_DOC:
                                ServClass.PlayMelody(W32.MB_3GONG_EXCLAM);
                                DialogResult dr = MessageBox.Show("�������� ����(Enter)?\r\n(ESC) - ����������",
                                    "�������� ����������!",
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                if (dr != DialogResult.OK)
                                {
                                    bRewrite = true;
                                    State = READ2D_STATE.R_WAIT_NEXT;
                                }
                                break;
                            case READ2D_STATE.R_WAIT_NEXT:
                                break;
                        }
                        break;
                    case READ2D_STATE.R_WAIT_NEXT:
                        if (nID_Serial == xS.nID_Serial)
                        {
                            if (!lSavedNs.Contains(xS.nInSer))
                            {
                                nSaved2D++;
                                lSavedNs.Add(xS.nInSer);
                                lSym.Add(xS);
                                nFileLen = AddFileLen(nFileLen);
                            }
                        }
                        break;
                }
                if (State == READ2D_STATE.R_WAIT_NEXT)
                {
                    if ((nSaved2D == nMaxSer) || (b2DFull == false))
                    {
                        State = READ2D_STATE.R_FULL_SER;
                        PrepBinDat(true, b2DFull);
                    }
                }

                return (nRet);
            }

            // ���������� ��� �����������
            private void PrepBinDat(bool bStorHead, bool b2DFull)
            {
                int j, k,
                    nOfs = 0;

                if (b2DFull == false)
                {
                }

                for (int i = 0; i < lSym.Count; i++)
                {
                    //j = (b2DFull == true) ? lSavedNs.IndexOf(i + 1) : 0;
                    j = lSavedNs.IndexOf(i + 1);
                    if (i == 0)
                    {
                        xD.nHead2D = lSym[j].nHeadLen;
                        FullBuf = new byte[nFileLen + lSym[j].nHeadLen];
                        for (k = 0; k < lSym[j].nHeadLen; k++)
                            FullBuf[nOfs++] = lSym[j].buf[k];
                    }
                    for (k = lSym[j].nHeadLen; k < lSym[j].nSymLen; k++)
                    {
                        FullBuf[nOfs] = lSym[j].buf[k];
                        nOfs++;
                    }
                }
                xD.nFull2D = xD.nHead2D + nFileLen;
                xD.From2D = new byte[xD.nFull2D];
                FullBuf.CopyTo(xD.From2D, 0);

            }

            // �������� ��� �������� ������
            public int ParseLev(byte[] b, ref int nOfs, DefOneLev xDLev, int nLev)
            {
                int i, j,
                    nRet = AppC.RC_OK;
                string sE;
                object x;

                for (i = 0; i < xDLev.aElems.Count; i++)
                {
                    sE = ParseOneEl(b, ref nOfs, xDLev.aElems[i], out x);
                    if (nLev == 0)
                    {// ������ ���������� ������
                        SetSerPars(xDLev.aElems[i].nTYPE, sE, x);
                        if (xDLev.aElems[i].bMAC == true)
                        {
                            j = xDLev.aElems[i].nMacInd;
                            xDLev.stLev.lMac[j].sVal = sE;
                            xDLev.stLev.lMac[j].xD = x;
                        }
                    }
                }

                return (nRet);
            }

            private void SetSerPars(ELTYPES nType, string s, object x)
            {
                long l;

                switch (nType)
                {
                    case ELTYPES.TE_ID:
                        xS.sID_Global = s;
                        break;
                    case ELTYPES.TE_IDSEQ:
                        l = long.Parse(s);
                        xS.nID_Serial = l;
                        break;
                    case ELTYPES.TE_RINT:
                        xS.bBin = (int.Parse(s) > 0) ? true : false;
                        break;
                    case ELTYPES.TE_RZIP:
                        xS.bZip = (int.Parse(s) > 0) ? true : false;
                        break;
                    case ELTYPES.TE_RMULT:
                        xS.bMult = (int.Parse(s) > 0) ? true : false;
                        break;
                    case ELTYPES.TE_TOT2D:
                        xS.nMaxSer = int.Parse(s);
                        break;
                    case ELTYPES.TE_I2D:
                        xS.nInSer = int.Parse(s);
                        break;
                    case ELTYPES.TE_HLEN:
                        xS.nHeadLen = int.Parse(s);
                        break;
                    case ELTYPES.TE_SZ2D:
                        xS.nSymLen = int.Parse(s);
                        break;
                    case ELTYPES.TE_SZDAT:
                        xS.nFileLen = int.Parse(s);
                        break;
                }

            }



            // �������� ��� �������� ������
            public string ParseOneEl(byte[] b, ref int nOfs, ElemDef xE, out object x)
            {
                DateTime d;
                string s = "";
                long i64;
                double dBl;

                x = null;

                switch (xE.cTYPD)
                {
                    case T_N:       // �����
                        if ((xE.cTYPI != T_E) && (bBin == true))
                        {// �������� �������������
                        }
                        else
                        {// ���������� ������������� �����
                            if (xE.nTOCH == 0)
                            {
                                s = Encoding.ASCII.GetString(b, nOfs, xE.nLEN);
                                nOfs = nOfs + xE.nLEN;
                                i64 = long.Parse(s);
                                x = i64;
                            }
                            else
                            {
                                s = Encoding.ASCII.GetString(b, nOfs, xE.nLEN - 1);
                                s.Insert(xE.nLEN - xE.nTOCH, ".");
                                dBl = double.Parse(s);
                                s = dBl.ToString();
                                nOfs = nOfs + xE.nLEN - 1;
                                x = dBl;
                            }
                        }
                        break;
                    case T_C:       // ���������� ������
                        if ((xE.cTYPI != T_E) && (bBin == true))
                        {// ����������� �������������
                        }
                        else
                        {// ���������� �������������
                            s = Encoding.ASCII.GetString(b, nOfs, xE.nLEN);
                            nOfs = nOfs + xE.nLEN;
                        }
                        break;
                    case T_D:       // ����
                        if ((xE.cTYPI != T_E) && (bBin == true))
                        {// ����������� �������������
                            d = new DateTime(b[nOfs + 2], b[nOfs + 1], b[nOfs]);
                            nOfs = nOfs + 3;
                        }
                        else
                        {// ���������� �������������
                            s = Encoding.ASCII.GetString(b, nOfs, 6);
                            d = DateTime.ParseExact(s, "yyMMdd", null);
                            nOfs = nOfs + 6;
                        }
                        if (xE.sFMT.Length > 0)
                            s = d.ToString(xE.sFMT);
                        else
                            s = d.ToString("dd.MM.yyyy");
                        x = d;
                        break;
                }


                return (s);
            }

        }




        private string sLevSign = @"---";
        private char[] aSep = {'|'};

        public const int R_BADID = 300;

        private Ser2DSym xSer2D = null;
        private List<string> lDoc;
        private List<Def2Data> xAllDef;

        private string sLastDoc = "";



        private int Read2D(BarcodeScannerEventArgs e)
        {
            int nRet =AppC.RC_OK,
                nLev;
            string s;



            s = e.Data;

            if (xAllDef == null)
            {
                DirectoryInfo di = new DirectoryInfo(xPars.sDataPath);
                FileInfo[] fi = di.GetFiles("*.def");
                Def2Data xOneDef;

                xAllDef = new List<Def2Data>();

                for (int i = 0; i < fi.Length; i++)
                {
                    xOneDef = new Def2Data();
                    nRet = Read2DDef(fi[i].FullName, Encoding.GetEncoding(1251), xOneDef);
                    if (nRet == AppC.RC_OK)
                    {
                        nLev = 0;
                        xOneDef.lDef = CreateArrOp(lDoc, ref nLev, 0);
                        xAllDef.Add(xOneDef);
                    }
                    else
                    {
                        ServClass.ErrorMsg(fi[i].FullName + "-������ � ��������!");
                    }
                }

                xSer2D = new Ser2DSym(xNSI);
            }

            nRet = New2D(e.BinData, e.LenBinData);
            s = "";
            if (nRet == AppC.RC_OK)
            {
                switch (xSer2D.State)
                {
                    case READ2D_STATE.R_WAIT_SER:
                        s = "������� - <" + sLastDoc + ">";
                        break;
                    case READ2D_STATE.R_WAIT_NEXT:
                        break;
                }
            }
            //Show2DResult(REG_2D_SHOW, s);
            return (nRet);

        }
        private int Parse1stLine(string sF, Def2Data xDef)
        {
            int nRet = AppC.RC_OK;
            try
            {
                Dictionary<string, string> aPars = SrvCommParse(sF, new char[] { '|' });
                xDef.sID_OAO = aPars["IDDEF"];
                xDef.bID_Def = Encoding.UTF8.GetBytes(xDef.sID_OAO);
            }
            catch
            {
                nRet = AppC.RC_NOID;
                xDef.sID_OAO = "";
            }

            return (nRet);
        }

        private int Read2DDef(string sF, Encoding enc, Def2Data xDef)
        {
            int nRet = AppC.RC_OK;
            try
            {
                using (StreamReader sr = new StreamReader(sF, enc))
                {
                    String l;
                    lDoc = new List<string>();
                    while (((l = sr.ReadLine()) != null) && (nRet == AppC.RC_OK))
                    {
                        if (l.Substring(0, 2) != @"//")
                        {
                            if ((lDoc.Count == 0) && (l.IndexOf(sLevSign) < 0))
                                nRet = Parse1stLine(l, xDef);
                            else
                                lDoc.Add(l);
                        }
                    }
                }
                if (xDef.sID_OAO == "")
                {
                    nRet = AppC.RC_NOID;
                }
            }
            catch
            {
                nRet = AppC.RC_NOFILE;
            }

            return (nRet);
        }

        private int GetLev(string s, out string[] aF)
        {
            int nRet = -1;
            try
            {
                if (s[0] == ';')
                    s = s.Substring(1);
                aF = s.Split(aSep);
                nRet = int.Parse(aF[0]);
            }
            catch
            {
                throw new SystemException("�������� ��������:" + s);
            }
            return (nRet);
        }

        private List<DefOneLev> CreateArrOp(List<string> lD, ref int iL, int nCurLev)
        {
            int nLev, j,
                nEl;
            string s,
                sField;
            string[] aFields;
            DefOneLev xLev;
            List<string> lM = new List<string>();

            List<DefOneLev> xRet = new List<DefOneLev>(0);

            while (iL < lD.Count)
            {
                nLev = GetLev(lD[iL], out aFields);
                if (nLev >= nCurLev)
                {
                    if (nLev == nCurLev)
                    {
                        xLev = new DefOneLev();
       
                        // 1-� ������ ������, �������� ����������� (��� ���������� ������)
                        xLev.xDeepLev = null;
       
                        // 2-� ������ - �������� ��������� ��������� ������
                        xLev.aElems = new List<ElemDef>(0);
       
                        // 3-� ������ - ������ ������
                        // ��� ������
                        // ������������ ������
                        xLev.stLev = new LevDef(nLev, aFields[1]);

                        // ������� Skip ��� ������
                        xLev.stLev.sFUNC = (aFields.Length >= 3)?aFields[2]:"";
       
                        // ��� ������� ������
                        xLev.stLev.sTABLE = (aFields.Length >= 4)?aFields[3]:"";
       
                        // ID ������

                        // MEMO ������ ���� �������� � ���������
     
                        nEl = 0;
       
                        sField = "";
       
                        iL = iL + 1;
                        while (( (iL < lD.Count) && 
                            (ParseOneEl(lD[iL], xLev.aElems, ref nEl, out sField, lM) == AppC.RC_OK)) == true) {
                            if (nEl < 0)
                                throw new SystemException( "������������� ��������, ������� " + lD[iL]);

                            if (sField != "")
                            {
                                // �������� ��������� ���
                                sField = "";
                            }

                            j = xLev.aElems.Count - 1;
                            s = xLev.aElems[j].sBC;
                            if (xLev.aElems[j].nTYPE == ELTYPES.TE_MACRO)
                            {// ������ ����������� �� ������ �������
                                xLev.stLev.lMac.Add(new MacroDef(s));
                                xLev.aElems[j].nMacInd = xLev.stLev.lMac.Count - 1;
                                xLev.aElems[j].bMAC = true;
                            }
         
                            iL++;
                        }

                        if (nEl > 0)
                            xRet.Add(xLev);
                    }
                    else
                    {
                        // ���������� ������ �������
                        nEl = xRet.Count - 1;
                        xRet[nEl].xDeepLev = CreateArrOp(lD, ref iL, nLev);
                    }
                }
                // iL ���������� � IF
            }

            return (xRet);
        }

        private int ParseOneEl(string sD, List<ElemDef> lE, ref int nEl, out string sF, List<string> lM)
        {
            bool bOut;
            string[] aF;
            int i,
                nL,
                nRet = AppC.RC_CANCEL;
            string s;
            ElemDef x;

            sF = "";
            if (sD.IndexOf(sLevSign) < 0)
            {
                // ��� ������������� ���������
                try
                {

                    bOut = true;                            // ������� ������������ � ������

                    if (sD[0] == ';')
                    {
                        sD = sD.Substring(1);
                        bOut = false;
                    }

                    aF = sD.Split(aSep);
                    nL = aF.Length - 1;

                    if (aF[nL].Length >= 2)
                    {
                        i = aF[nL].IndexOf("//");
                        if (i > 0)
                            aF[nL] = aF[nL].Substring(0, i).Trim();
                        if (aF[nL].Length > 0)
                            nL++;
                    }


                    if (nL < 4)
                        throw new SystemException("�������� ��������:" + sD);

                    int iT = int.Parse(aF[1]);
                    x = new ElemDef(nEl, aF[0], (ELTYPES)iT, aF[2], int.Parse(aF[3]));
                    // ��������
                    x.nTOCH = (nL >= 5) ? int.Parse(aF[4]) : 0;
                    // ���������� ���
                    if (nL >= 6)
                    {
                        s = aF[5].Trim();
                        if (s.Length > 0)
                            x.cTYPI = s[0];
                    }

                    // ����� (�����.)
                    x.nLENI = (nL >= 7) ? int.Parse(aF[6]) : 0;

                    // ��� ����
                    if (nL >= 8)
                    {
                        s = aF[7].Trim();
                        if (s.Length > 0)
                        {
                            x.sFLD = s;
                            sF = s;
                        }
                    }

                    // ��������� (������� �������� �����)
                    if (nL >= 9)
                    {
                        s = aF[8].Trim();
                        if (s.Length > 0)
                        {
                            x.sBC = s;
                        }
                    }

                    // ������ �������������
                    x.sFMT = (nL >= 10) ? aF[9].Trim() : "";
                    if (nL >= 10)
                    {
                        s = aF[9].Trim();
                        if (s.Length > 0)
                            x.sFMT = s;
                    }

                    // ����������� � �������� �����
                    x.bOUT = bOut;

                    lE.Add(x);


                    nEl = nEl + 1;



                    nRet = AppC.RC_OK;
                }
                catch
                {
                    nEl = -1;
                }
            }
            return( nRet );
        }

        private bool IsGood2D(byte[] b, int nLen)
        {
            bool bRet = AppC.RC_CANCELB;
            Def2Data xDef = null;
            int i, iM, j;

            if (xSer2D.State == READ2D_STATE.R_WAIT_SER)
            {// �������� ��� �� �������������
                iM = xAllDef.Count;
            }
            else
            {// ������ �� �������� ��� � ���������� 2D
                xDef = xSer2D.x�Def2D;
                iM = 1;
            }

            for (i = 0; i < iM; i++)
            {
                if ((i > 0) || (xDef == null))
                    xDef = xAllDef[i];
                if (xDef.bID_Def.Length <= nLen)
                {
                    bRet = AppC.RC_OKB;
                    for (j = 0; j < xDef.bID_Def.Length; j++)
                        if (b[j] != xDef.bID_Def[j])
                        {
                            bRet = AppC.RC_CANCELB;
                            break;
                        }
                    if (bRet == AppC.RC_OKB)
                    {
                        if (xSer2D.State == READ2D_STATE.R_WAIT_SER)
                            xSer2D.x�Def2D = xDef;
                        break;
                    }
                }
            }

            return (bRet);
        }

        private int New2D(byte[] b, int nLen)
        {
            int nRet = AppC.RC_OK;

            if (IsGood2D(b, nLen))
            {
                xSer2D.AddNew(b, nLen, xPars.b2DFull);
                if (xPars.b2DFull == false)
                    xSer2D.State = READ2D_STATE.R_FULL_SER;
                switch (xSer2D.State)
                {
                    case READ2D_STATE.R_SAME_DOC:
                    case READ2D_STATE.R_BAD_KEYS:
                        xSer2D = new Ser2DSym(xNSI);
                        break;
                    case READ2D_STATE.R_FULL_SER:
                        if (xSer2D.bRewrite == false)
                        {// ������� �����
                            xNSI.AddDocRec(xSer2D.xD, dgDoc);
                            StatAllDoc();
                        }
                        else
                        {
                            xNSI.UpdateDocRec(xSer2D.drRew, xSer2D.xD);
                        }
                        sLastDoc = xSer2D.xD.xDocP.sNomDoc;
                        xSer2D = new Ser2DSym(xNSI);
                        break;
                }
            }
            else
                nRet = AppC.RC_BADID;

            return (nRet);
        }


        private const int REG_2D_SHOW = 1;
        private const int REG_2D_CLOSE = 2;

        private int nCur2DShow = REG_2D_CLOSE;

        private void Show2DResult(int nReg, string sHelp)
        {
            int i;
            string s, s1;

            if (nReg == REG_2D_SHOW)
            {// ���� ��������
                s = "�������� - <";
                for (i = 0; i < xSer2D.nMaxSer; i++)
                {
                    if (xSer2D.lSavedNs.IndexOf(i + 1) < 0)
                        s += (i + 1).ToString() + ",";
                    else
                        s += "+,";
                }
                if (s[s.Length - 1] == ',')
                    s = s.Remove(s.Length - 1, 1);
                if (nCur2DShow == REG_2D_CLOSE)
                {// �������� ������
                    nCur2DShow = REG_2D_SHOW;
                    xFPan.ShowP(dgDoc.Left + 4, dgDoc.Top + 12, "���� 2D-������", s + ">");
                }
                else
                    xFPan.UpdateReg(s + ">");


                s = "����� - " + ((xSer2D.nMaxSer > 0) ? xSer2D.nMaxSer.ToString() : "...");
                s1 = ", ��������� - " + ((xSer2D.lSavedNs.Count > 0) ? 
                                         xSer2D.lSavedNs[xSer2D.lSavedNs.Count - 1].ToString() : "...");
                s = (sHelp == "") ? s + s1 : sHelp;
                xFPan.UpdateHelp(s);
            }
            if (nReg == REG_2D_CLOSE)
            {
                xFPan.HideP();
                nCur2DShow = REG_2D_CLOSE;
            }
        }



    }
}
