using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using SavuSocket;
using ScannerAll;

using FRACT = System.Decimal;


namespace SkladDoc
{
    public partial class MainF : Form
    {
        // ������� ������� ������ � �����������
        private int nCurMsgFunc = AppC.DT_SHOW;

        // ������� ������ � ����
        private DataRow drCurMsg = null;

        // ��� ��������� ������ ����������
        private void EnterInMsg(){
            if (nCurMsgFunc == AppC.DT_SHOW)            // � ������ ���������
            {
                    dgMsg.Focus();
            }
        }


        // ���������� ����� ������
        private void dgMsg_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid dg = (DataGrid)sender;

            DataView dvMaster = ((DataTable)dg.DataSource).DefaultView;
            DataRow dr = dvMaster[dg.CurrentRowIndex].Row;
            if (dr != drCurMsg)
            {
                drCurMsg = dr;
                tbMsg.Text = (string)dr["MSG"];
            }

        }



        // ��������� ������� � ������ �� ������
        private bool Msg_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;

            if (nFunc > 0)
            {
                if (bEditMode == false)
                {
                    switch (nFunc)
                    {
                        case AppC.F_FLTVYP:             // ������ �� �������
                            SetFltMsgState(false);
                            ret = true;
                            break;
                        case AppC.F_CHGSCR:
                            break;
                    }
                }
            }
            else
            {
                if (bEditMode == false)
                {// ������ � ������ ���������
                    switch (e.KeyValue)
                    {
                        case W32.VK_ENTER:
                            ProceedMsg();
                            ret = true;
                            break;
                    }
                }
            }
            e.Handled |= ret;
            return (ret);

        }


        /// *** ������� ������ � �����������


        private void SetFltMsgState(bool bSetNow)
        {
            string sF = xNSI.DT[NSI.TBD_SMSG].sTFilt;
            if ((bSetNow == true) && (sF != ""))
                return;
            if (sF == "")
            {
                ServClass.ErrorMsg("Set Filter");
                sF = String.Format("STATE<>{0}", (int)AppC.MSGSTATE.PROCEED);
            }
            else
            {
                ServClass.ErrorMsg("Reset Filter");
                sF = "";
            }
            ((DataTable)dgMsg.DataSource).DefaultView.RowFilter = sF;
            xNSI.DT[NSI.TBD_SMSG].sTFilt = sF;


        }

        private void ProceedMsg()
        {
            if (drCurMsg != null)
            {
                drCurMsg["STATE"] = AppC.MSGSTATE.PROCEED;
                drCurMsg["EXPR_STS"] = "���������";
                ServClass.ErrorMsg("Set State");
            }
        }




    }
}
