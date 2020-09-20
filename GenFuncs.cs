using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;

using PDA.OS;
using PDA.Service;
using FRACT = System.Decimal;

namespace Proh
{
    public partial class MainF : Form
    {
        // текущий объект загрузки документов
        public CurLoad xCLoad = null;

        // объект текущей выгрузки
        private CurUpLoad xCUpLoad = null;


        // строк в информационном окне для листания
        public int nHelpPeriod = AppC.HELPLINES;
        private int nHelpInd = 0;
        // текущая инфо
        private object xInf;

        // вывод панели c окном помощи
        private void ShowInf(object xInfP)
        {
            if (xInfP != null)
                xInf = xInfP;

            if (xInf == null)
                return;

            // центровка панели
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            if (screen.Width > 240)
            {
                pnHelp.Width = screen.Width;
                tMainHelp.Width = screen.Width - 4;
                lMainHelp.Left = (screen.Width - lMainHelp.Width) / 2;
            }
            pnHelp.Location = new Point((screen.Width - pnHelp.Width) / 2,
                (screen.Height - pnHelp.Height) / 2);

            nHelpPeriod = AppC.HELPLINES;
            nHelpInd = 0;

            tHeadHelp.Visible = false;
            tMainHelp.Location = new Point(2, 3);
            tMainHelp.Height = 296;

            if (xInf.GetType().IsArray == true)
                tMainHelp.Text = ((string[])xInf)[0];
            else
                tMainHelp.Text = NextInfPart();

            pnHelp.Visible = true;
            ehCurrFunc += new CurrFuncKeyHandler(HelpKeyDown);
            tFiction.Focus();
        }

        private string NextInfPart()
        {
            string sRet = "";
            int nStart = (nHelpInd * nHelpPeriod),
                nEnd;
               
            if (nStart >= ((List<string>)xInf).Count)
            {
                nHelpInd = 0;
                nStart = 0;
            }
            nEnd = Math.Min(((List<string>)xInf).Count, nStart + nHelpPeriod);
                
            for (int i = nStart; i < nEnd; i++)
                sRet += ((List<string>)xInf)[i] + "\r\n";
            if (sRet.Length > 0)
                sRet = sRet.Remove(sRet.Length - 2, 2);
            return (sRet);
        }


        // вывод панели c окном помощи
        private void ShowHelp()
        {
            ShowInf(xFuncs.GetFHelp());
        }

        private bool HelpKeyDown(int nFunc, KeyEventArgs e)
        {
            float fsize;
            bool bKeyHandled = true, 
                bCloseHelp = false;
            string sH = "";

            if (nFunc > 0)
            {
                bCloseHelp = true;
                if (nFunc != PDA.Service.AppC.F_HELP)
                    bKeyHandled = false;
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        bCloseHelp = true;
                        break;
                    case W32.VK_ENTER:
                        nHelpInd++;
                        if (xInf.GetType().IsArray == true)
                        {
                            if (nHelpInd == ((string[])xInf).Length)
                                nHelpInd = 0;
                            sH = ((string[])xInf)[nHelpInd];
                        }
                        else
                            sH = NextInfPart();

                        tMainHelp.Text = sH;
                        break;
                }
            }
            if (bCloseHelp == true)
            {
                Control cOldCtrl = null;
                ehCurrFunc -= HelpKeyDown;
                pnHelp.Visible = false;
                pnHelp.Location = new Point(400, 50);
                fsize = 9.0F;
                tMainHelp.Font = new Font(tMainHelp.Font.Name, fsize, tMainHelp.Font.Style);
                switch (tcMain.SelectedIndex)
                {
                    default:
                        cOldCtrl = dgDoc;
                        break;
                }
                cOldCtrl.Focus();
            }
            else
                tFiction.Focus();
            return (bKeyHandled);
        }

        private void CheckNSIState()
        {
            if (xSm.dtLoadNS.Date < DateTime.Now.Date)
            {
                LoadNsiMenu(true);
            }
        }

        private void pnLoadDoc_EnabledChanged(object sender, EventArgs e)
        {
            xBCScanner.WiFi.ShowWiFi(pnLoadDoc);
        }

        private int GetAvtInf(int nCom)
        {
            int i,
                nRet = AppC.RC_OK;
            string sErr, sSys = "";
            List<DataRow> aDR = new List<DataRow>();
            DataView dv = null; 
            LoadFromSrv dgL = null;

            if (xCUpLoad == null)
            {
                xCUpLoad = new CurUpLoad();
                if (xCLoad != null)
                    xCUpLoad.xLP = xCLoad.xLP;
                xDP = xCUpLoad.xLP;
            }
            xCUpLoad.ilUpLoad.SetAllAvail(true);
            xCUpLoad.ilUpLoad.CurReg = AppC.UPL_ALL;

            switch (nCom)
            {
                case AppC.COM_ZOPEN:
                    sErr = "";
                    foreach (object s in lbTTN.Items)
                    {
                        sErr += (string)s + " ";
                    }
                    sSys = String.Format("AVT={4},MASSA={0},SUMMA={1},MEST={2},TTN={3}", 
                        xAvt.Massa, xAvt.Summa, xAvt.Mest, sErr, xAvt.Avto);
                    dgL = new LoadFromSrv(AvtFromSrv);
                    break;
                case AppC.COM_ZPL:
                    dgL = new LoadFromSrv(InfoFromPL);
                    break;
                case AppC.COM_ZCONTR:
                    // по всем невыгруженным документам
                    dv = new DataView(xNSI.DT[NSI.TBD_DOC].dt, 
                        String.Format("SOURCE={0}", NSI.DOCSRC_CRTD), "", DataViewRowState.CurrentRows);
                    if (dv.Count <= 0)
                    {
                        ServClass.ErrorMsg("Нет данных !");
                        return (AppC.RC_OK);
                    }
                    for (i = 0; i < dv.Count; i++)
                    {
                        if (i > 0)
                            sSys += ",";
                        sSys += dv[i].Row["IDSER2D"].ToString();
                        aDR.Add(dv[i].Row);
                    }
                    xCUpLoad.sComPar = sSys;
                    dgL = new LoadFromSrv(AvtFromSrv);

                    break;

            }

            xFPan.ShowP(6, 60, "Запрос данных", xCUpLoad.ilUpLoad.CurRegName);
            xFPan.UpdateHelp("Обмен с сервером...");

            Cursor crsOld = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            sErr = ExchgSrv(nCom, sSys, "", dgL, null, ref nRet);
            
            Cursor.Current = crsOld;
            xFPan.HideP();

            if (nRet == AppC.RC_OK)
            {
            }
            else
                MessageBox.Show("Результат - " + sErr, "Код - " + nRet.ToString());


            if (tcMain.SelectedIndex != PG_DIR)
                tcMain.SelectedIndex = PG_DIR;

            switch (nCom)
            {
                case AppC.COM_ZPL:
                    break;
                case AppC.COM_ZCONTR:
                    for (i = 0; i < dv.Count; i++)
                    {
                        //aDR[i]["SOURCE"] = NSI.DOCSRC_UPLD;
                    }
                    break;
            }



            return (nRet);
        }




        private void InfoFromPL(Stream stmX, Dictionary<string, string> aC, DataSet ds,
            ref string sErr, int nRetSrv)
        {
            DataTable dt = xNSI.DT[NSI.TBD_TTN].dt;

            // время с сервера
            xAvt.SrvCTM = aC["CTM"].Trim();
            // сохранить параметры для разбора
            xAvt.SrvAns = aC["PAR"].Trim();
            xAvt.SrvAns = xAvt.SrvAns.Substring(1, xAvt.SrvAns.Length - 2);
            xAvt.KolTTN_EBD = 0;

            sErr = "Ошибка чтения XML";
            string sXMLFile = "";
            int nFileSize = ServClass.ReadXMLWrite2File(stmX, ref sXMLFile);

            sErr = "Ошибка загрузки XML";

            dt.BeginLoadData();
            dt.Clear();
            System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
            dt.ReadXml(xmlRd);
            xmlRd.Close();
            xAvt.KolTTN_EBD = dt.Rows.Count; ;

            dt.EndLoadData();
            sErr = "OK";
        }



        private void AvtFromSrv(System.IO.Stream stmX, Dictionary<string, string> aC, DataSet ds, 
            ref string sErr, int nRetSrv)
        {
            // время с сервера
            xAvt.SrvCTM = aC["CTM"].Trim();
            // сохранить параметры для разбора
            xAvt.SrvAns = aC["PAR"].Trim();
            xAvt.SrvAns = xAvt.SrvAns.Substring(1, xAvt.SrvAns.Length - 2);
            sErr = "OK";
        }

        private int ShlgbMove(int nFunc, string sDevID, string sDevLogin, string sDevPass)
        {
            int nRet = AppC.RC_OK;
            string sMsg = "Open ";
            string cFunc = (nFunc == AppC.F_OPENSH) ? "1" : "2";

            //sMsg += ShMove(xPars.sHostSrvSh.Trim(), xPars.nSrvPortSh, xPars.LoginSh, xPars.PassSh, 
            //    xPars.DevID, cFunc, ref nRet);
            sMsg += ShMove(xPars.sHostSrvSh.Trim(), xPars.nSrvPortSh, sDevLogin, sDevPass,
                sDevID, cFunc, ref nRet);
            Thread.Sleep(1000 * 2);
            if (nRet != AppC.RC_OK)
            {
                // с новыми сокетами вобще-то ошибка...
                // MessageBox.Show(sMsg);
            }
            return (nRet);
        }



    }
}
