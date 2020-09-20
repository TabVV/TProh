using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;

using SavuSocket;
using ScannerAll;
using PDA.OS;
using PDA.Service;

using FRACT = System.Decimal;


namespace Proh
{
    public partial class MainF : Form
    {
        // ���� ������� ����� � ���������
        private bool b1stEner = true;

        // ��� ��������� ������ ����������
        private void EnterInDoc(){
            if (b1stEner == true)
            {
                b1stEner = false;
                //xNSI.ChgGridStyle(NSI.TBD_DOC, NSI.GDOC_INV);
                lInfDocLeft.Text = (xSm.sUser == AppC.SUSER)?"Admin":xSm.sUser;
                xCDoc = new CurDoc(AppC.F_ADD_REC);
                if (xSm.nDocs > 0)
                {// ���� ������ ��� ���������
                    RestShowDoc(false);
                }
            }
            if (bEditMode == false)            // � ������ ���������
            {
                StatAllDoc();
                if (xCDoc.drCurRow != null)
                {// ���� ������ ��� �����������
                    //xCDoc.drCurRow["MEST"] = TotMest(NSI.REL2TTN);
                    //dgDoc.Focus();
                }
            }
        }


        // ����� ���������� �� ���� ����������
        private void StatAllDoc()
        {
            // ����� ����������
            int nR = xNSI.DT[NSI.TBD_DOC].dt.Rows.Count;
            string sRf = String.Format("SOURCE={0}", NSI.DOCSRC_UPLD);
            DataView dv = new DataView(xNSI.DT[NSI.TBD_DOC].dt, sRf, "", DataViewRowState.CurrentRows);
            int nDetIn = dv.Count;
            int nDetOut = nR - nDetIn;

            lInfDocAll.Text = "��� " + nR.ToString() + "(" + nDetIn.ToString() + "/" +
                nDetOut.ToString() + ")";
            if (nR > 0)
                btReqInf.Enabled = true;

            //if (xPars.parDocControl == true)
            //    tDocCtrlState.Text = "�";
            //else
            //    tDocCtrlState.Text = "";
        }



        // ��������� ������������ ������������
        private void OnScan(object sender, BarcodeScannerEventArgs e)
        {
            if (e.nID != BCId.NoData)
            {
                PSC_Types.ScDat sc = new PSC_Types.ScDat(e);

                if (bGoodAvtor == false)
                {// ������� ����������� ����� ��������
                    if ( (e.nID == BCId.Code128) && (e.Data.Length == 10) &&
                         (e.Data.Substring(0, 2) == "99") && (e.Data.Substring(2, 2) == "10") )
                    {
                        fAv.SetUserID(e.Data.Substring(4, 6));
                    }
                }
                else
                {
                    if (e.nID == BCId.Code128)
                    {
                        ProceedScan128(e);
                    }

                    
                    if (e.nID == BCId.DataMatrix)
                    {
                        //if (tcMain.SelectedIndex != PG_DOC)
                        //    tcMain.SelectedIndex = PG_DOC;
                        if (Read2D(e) == AppC.RC_OK)
                        {
                            NewTTNScanned(sLastDoc, 2);
                        }
                    }
                }
            }
        }
         
        //��������� ������������ Code128
        private void ProceedScan128(BarcodeScannerEventArgs e)
        {
            string sNomDoc;

            if (e.Data.Length == 14)
            {// ������� ���� ��� ���
                sNomDoc = e.Data.Substring(7);

                if (bEditMode == false)
                {// ���� ������ � ��������
                    if (tcMain.SelectedIndex != PG_DIR)
                        tcMain.SelectedIndex = PG_DIR;
                    xAvt.Putev = sNomDoc;
                    BeginEditAvt(tPutList, sNomDoc);
                }
                else
                {// ���� ��� �������������
                    if (tPutList.Focused == true)
                    {
                        if (tcMain.SelectedIndex != PG_DIR)
                            tcMain.SelectedIndex = PG_DIR;
                        xAvt.Putev = sNomDoc;
                        tPutList.DataBindings[0].ReadValue();
                        //ServClass.TryEditNextFiled(tPutList, AppC.CC_NEXT, aEdVvod);
                        aEdVvod.TryNext(AppC.CC_NEXT);
                    }
                    else
                        NewTTNScanned(sNomDoc, 1);
                }
            }
            else if (e.Data.Length == 12)
            {// ��������, �������
                if (e.Data.Substring(0, 3) == "778")
                {
                    //tPropusk.Text = e.Data.Substring(3);
                    tPropusk.Text = e.Data.Substring(3);
                }
            }

        }



        // ��������� ������������ �� ������ ����������
        //private CurLoad TransDocCode(ref PSC_Types.ScDat s)
        //{
        //    bool ret = false;
        //    CurLoad xL = null;
        //    if (true)
        //    {
        //        string sS = s.s;
        //        int i, nLen;
        //        xL = new CurLoad(AppC.UPL_FLT);

        //        ret = true;
        //        try
        //        {
        //            if (s.ci == ScannerAll.BCId.Code128)
        //            {
        //                nLen = sS.Length;
        //                switch (nLen)
        //                {
        //                    case 14:                            // � ���������
        //                        ret = false;
        //                        if (sS.Substring(0, 3) == "821")
        //                        {// ��� ����� - 821...
        //                            i = int.Parse(sS.Substring(7, 7));
        //                            if (i > 0)
        //                            {
        //                                xL.xLP.sNomDoc = i.ToString();
        //                                ret = true;
        //                            }
        //                            else
        //                                xL.xLP.sNomDoc = "";
        //                        }
        //                        xL.xLP.nTypD = AppC.TYPD_VPER;
        //                        break;
        //                    case 34:                            // �������� �� �������� � �.�.
        //                        // ��������� ���� �� ��������� (����� - 34)
        //                        // ���� - ������ (6)
        //                        // ����� - (4)
        //                        // ����� - (3)
        //                        // ������� - (3)
        //                        // ���������� - (4)
        //                        // ���������� - (4)
        //                        // ��� ��������� - (2)
        //                        // � ��������� - (8)
        //                        xL.xLP.dDatDoc = DateTime.ParseExact(sS.Substring(0, 6), "yyMMdd", null);
        //                        // ����� �����
        //                        i = int.Parse(sS.Substring(6, 1));
        //                        if (i > 0)
        //                            xL.xLP.sSmena = sS.Substring(7, i);
        //                        else
        //                            xL.xLP.sSmena = "";

        //                        i = int.Parse(sS.Substring(10, 3));
        //                        if (i > 0)
        //                            xL.xLP.nSklad = i;
        //                        else
        //                            xL.xLP.nSklad = AppC.EMPTY_INT;

        //                        i = int.Parse(sS.Substring(13, 3));
        //                        if (i > 0)
        //                            xL.xLP.nUch = i;
        //                        else
        //                            xL.xLP.nUch = AppC.EMPTY_INT;

        //                        i = int.Parse(sS.Substring(20, 4));
        //                        if (i > 0)
        //                            xL.xLP.nPol = i;
        //                        else
        //                            xL.xLP.nPol = AppC.EMPTY_INT;

        //                        i = int.Parse(sS.Substring(24, 2));
        //                        if (i > 0)
        //                            xL.xLP.nTypD = i;
        //                        else
        //                            xL.xLP.nTypD = AppC.EMPTY_INT;

        //                        i = int.Parse(sS.Substring(26, 8));
        //                        if (i > 0)
        //                        {
        //                            xL.xLP.sNomDoc = i.ToString();
        //                            ret = true;
        //                        }
        //                        else
        //                            xL.xLP.sNomDoc = "";
        //                        break;
        //                    default:
        //                        ret = false;
        //                        break;
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            ret = false;
        //        }

        //    }
        //    if (ret == false)
        //    {
        //        xL = null;
        //    }
        //    return (xL);
        //}



        // ��������� ������� � ������ �� ������
        private bool Doc_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;

            if (nFunc > 0)
            {


                switch (nFunc)
                {
                    case AppC.F_UPLD_DOC:
                        GetAvtInf(AppC.COM_ZCONTR);
                        break;
                }



                if (bEditMode == false)
                {
                    switch (nFunc)
                    {
                        case PDA.Service.AppC.F_ADD_REC:            // ���������� �����
                            AddOrChangeDoc(PDA.Service.AppC.F_ADD_REC);
                            ret = true;
                            break;
                        case AppC.F_CHG_REC:            // �������������
                            if (xCDoc.drCurRow != null)
                                AddOrChangeDoc(AppC.F_CHG_REC);
                            ret = true;
                            break;
                        case AppC.F_DEL_ALLREC:         // �������� ����
                        case AppC.F_DEL_REC:            // ��� ������
                            DelDoc(nFunc);
                            StatAllDoc();
                            ret = true;
                            break;
                        //case AppC.F_CHG_GSTYLE:           // ����� �����, ������
                        //    //ChgDocGridStyle(1);
                        //    ret = true;
                        //    break;
                        //case AppC.F_TOT_MEST:
                        //    // ����� ���� �� ���������/������
                        //    //ShowTotMest();
                        //    ret = true;
                        //    break;
                        //case AppC.F_CTRLDOC:
                        //    // �������� �������� ���������
                        //    //ControlDocs(AppC.F_INITREG, null);
                        //    ret = true;
                        //    break;
                        //case AppC.F_CHGSCR:
                        //    // ����� ������
                        //    //xScrDoc.NextReg();
                        //    ret = true;
                        //    break;
                    }
                }
            }
            else
            {

                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        Show2DResult(REG_2D_CLOSE, "");
                        xSer2D = new Ser2DSym(xNSI);
                        dgDoc.Focus();
                        ret = true;
                        break;
                    case W32.VK_ENTER:
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);

        }



        // �������� ���������� ����� �������
        private bool VerifyPars(DocPars xP, int nF, ref object xErr)
        {
            bool ret = false;
            //string sE = "";

            ////if ((xP.nTypD != AppC.EMPTY_INT) && (xP.dDatDoc != DateTime.MinValue))
            //    // ��� ���� ����� ����������
            //if (xP.nTypD != AppC.EMPTY_INT)
            //{// ������� ������
            //    if (nF == AppC.F_LOAD_DOC)
            //    {
            //        if (xP.sNomDoc != "")
            //            ret = true;
            //        else
            //        {
            //            sE = "� ��������� �� ������!";
            //            xErr = tNom_p;
            //        }
            //    }
            //    else
            //    {
            //        if (xP.nSklad != AppC.EMPTY_INT)
            //        {// ����� �������

            //            switch (nF)
            //            {
            //                case AppC.F_ADD_REC:
            //                case AppC.F_CHG_REC:
            //                    if (xP.nTypD != AppC.TYPD_INV)
            //                    {// ���� �� �������������� - ��������� ������

            //                        switch (xP.nTypD)
            //                        {
            //                            case AppC.TYPD_VPER:        // ���������� �����������
            //                                if (xP.nPol != AppC.EMPTY_INT)
            //                                {
            //                                    if (xP.sNomDoc != "")
            //                                        ret = true;
            //                                    else
            //                                    {
            //                                        sE = "� ��������� �� ������!";
            //                                        xErr = tNom_p;
            //                                    }
            //                                }
            //                                else
            //                                {
            //                                    sE = "���������� �� ������!";
            //                                    xErr = DocPars.tKPost;
            //                                }
            //                                break;
            //                        }
            //                    }
            //                    else
            //                        ret = true;
            //                    break;
            //                case AppC.F_UPLD_DOC:
            //                case AppC.F_LOAD_DOC:
            //                    if (xP.sSmena != "")
            //                        ret = true;
            //                    else
            //                    {
            //                        sE = "����� �� �������!";
            //                        xErr = null;
            //                    }
            //                    break;

            //            }
            //        }
            //        else
            //        {
            //            sE = "����� �� ������!";
            //            xErr = DocPars.tKSkl;
            //        }
            //    }
            //}
            //else
            //{
            //    sE = "��������� ���!";
            //    xErr = DocPars.tDate;
            //}

            //if (ret == false)
            //{
            //    ServClass.ErrorMsg(sE);
            //}
            return (ret);
        }


        // ������� � ����� ���������
        private void RestShowDoc(bool bGoodBefore)
        {
            //tStat_Reg.Text = "��������";
            if (bGoodBefore == false)
            {// ���������� �������� ���������, ���������� ������ (���� ����)
                DataView dvMaster = ((DataTable)dgDoc.DataSource).DefaultView;

                if (dvMaster.Count > 0)
                {// ���� ������ ��� ���������
                    xCDoc.drCurRow = dvMaster[dgDoc.CurrentRowIndex].Row;
                    xNSI.InitCurDoc(xCDoc);
                }
                else
                    xCDoc.xDocP = new DocPars(AppC.F_ADD_REC);
                SetParFields(xCDoc.xDocP);
            }
            dgDoc.Focus();
        }



        /// *** ������� ������ � �����������
        /// 

        // ���������� ����� ��� ��������� ������
        // nReg - ��������� �����
        private void AddOrChangeDoc(int nFunc)
        {
            CTRL1ST FirstC = CTRL1ST.START_EMPTY;

            if (nFunc == PDA.Service.AppC.F_ADD_REC)
                {// ���� � ����� ���������� ����� ������
                    //tStat_Reg.Text = "�����";
                    xCDoc.xDocP = new DocPars(AppC.F_ADD_REC);
                }
                else
                {// ���� � ����� ������������� ������
                    //tStat_Reg.Text = "����-��";
                    FirstC = CTRL1ST.START_AVAIL;
                }
                EditPars(nFunc, xCDoc.xDocP, FirstC, VerifyDoc, EditFieldsIsOver);
        }

        // �������� ��������� ��������
        private AppC.VerRet VerifyDoc()
        {
            AppC.VerRet 
                v;
            v.nRet = AppC.RC_OK;
            object xErr = null;
            bool bRet = VerifyPars(xCDoc.xDocP, nCurFunc, ref xErr);
            if (bRet != true)
                v.nRet = AppC.RC_CANCEL;
            else
                bQuitEdPars = true;
            v.cWhereFocus = (Control)xErr;
            return (v);
        }

        private void EditFieldsIsOver(int RC, int nF)
        {
            bool bRet = false;          // ���������� ������
            if (RC == AppC.RC_OK)
            {
                switch (nF)
                {
                    case PDA.Service.AppC.F_ADD_REC:
                        bRet = xNSI.AddDocRec(xCDoc, dgDoc);
                        break;
                    case AppC.F_CHG_REC:
                        bRet = xNSI.ChgDocRec(xCDoc);
                        break;
                }
            }
            RestShowDoc(bRet);
        }


        // �������� ��������� (��)
        private void DelDoc(int nReg)
        {
            if (xCDoc.drCurRow != null)
            {
                if (nReg == AppC.F_DEL_REC)
                {// �������� ���������
                    xNSI.DT[NSI.TBD_DOC].dt.Rows.Remove( xCDoc.drCurRow );

                    DataView dvMaster = ((DataTable)dgDoc.DataSource).DefaultView;
                    if (dvMaster.Count > 0)
                        xCDoc.drCurRow = dvMaster[dgDoc.CurrentRowIndex].Row;
                    else
                        xCDoc.drCurRow = null;
                }
                else
                {
                    DialogResult dr = MessageBox.Show("�������� �������� ���� (Enter)?\r\n(ESC) - ��� ������� ��� ��������",
                        "��������� ��� ������!",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (dr != DialogResult.OK)
                    {
                        //xNSI.DT[NSI.I_TZVK].dt.Rows.Clear();
                        //xNSI.DT[NSI.I_TFAKT].dt.Rows.Clear();
                        xNSI.DT[NSI.TBD_DOC].dt.Rows.Clear();
                        xCDoc.drCurRow = null;
                    }
                }
                RestShowDoc(false);

                //if (xCDoc.drCurRow != null)
                //{
                //    RestShowDoc(false);
                //}
                //else
                //{// ��-��������, ������� ���������, ���� �� ������
                //    //AddOrChangeDoc(AppC.F_ADD_REC);
                //}
            }
        }




        // ���������� ����� ������
        private void dgDoc_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid dg = (DataGrid)sender;

            DataView dvMaster = ((DataTable)dg.DataSource).DefaultView;
            DataRow dr = dvMaster[dg.CurrentRowIndex].Row;

            if (xCDoc.drCurRow != dr)
            {// ��������� ������

                xCDoc.drCurRow = dr;
                xNSI.InitCurDoc(xCDoc);
                SetParFields(xCDoc.xDocP);
            }
        }


        // ����� ������� �������� ���������
        private void ChgDocSourceState()
        {
            if (xCDoc.drCurRow != null)
            {// ���� ������
                if ((int)xCDoc.drCurRow["SOURCE"] == NSI.DOCSRC_UPLD)
                {
                    xCDoc.drCurRow["SOURCE"] = (xCDoc.nStrokZ == 0)?NSI.DOCSRC_CRTD:NSI.DOCSRC_LOAD;
                }
            }
        }

    }
}
