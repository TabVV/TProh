using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.IO;

using PDA.OS;
using PDA.Service;

using SkladAll;

namespace Proh
{
    public partial class MainF : Form
    {
        
        private string nCurNsi = "";
        private Dictionary<string, NSI.TableDef>.Enumerator enuNSI;

        // при активации панели НСИ
        private void EnterInNSI()
        {
            if (nCurNsi == "")
            {
                enuNSI = xNSI.DT.GetEnumerator();
                ChangeCurNSI();
            }
            dgMC.Focus();
        }

        // вывод статистики по справочнику
        private void ShowNSIStat()
        {
            if (nCurNsi != "")
            {
                lNsiInf.Text = xNSI.DT[nCurNsi].Text;
                tNsiInf.Text = "Записей - " + xNSI.DT[nCurNsi].dt.Rows.Count.ToString();
            }
            else
            {
                lNsiInf.Text = "";
                tNsiInf.Text = "Записей";
            }
        }

        // смена текущего справочника
        private void ChangeCurNSI()
        {
            bool bFound = false;

            while (enuNSI.MoveNext())
            {
                if ((enuNSI.Current.Value.nType & NSI.TBLTYPE.NSI) == NSI.TBLTYPE.NSI)
                {
                    nCurNsi = enuNSI.Current.Key;
                    bFound = true;
                    break;
                }
            }
            if (bFound)
            {
                dgMC.DataSource = xNSI.DT[nCurNsi].dt;
                dgMC.Refresh();
                ShowNSIStat();
            }
            else
            {
                enuNSI = xNSI.DT.GetEnumerator();
                ChangeCurNSI();
            }

        }

        // обработка клавиш на панели НСИ
        private bool NSI_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;

            if (nFunc > 0)
            {
                switch (nFunc)
                {
                    case PDA.Service.AppC.F_LOAD_DOC:                       // загрузка справочников
                        tNsiInf.Text = "Идет загрузка...";
                        object xDSrc = dgMC.DataSource;
                        dgMC.DataSource = null;
                        //LoadAllNSISrv(AppC.EMPTY_INT, null, false);
                        dgMC.DataSource = xDSrc;
                        ShowNSIStat();
                        ret = true;
                        break;
                }
            }

            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        tcMain.SelectedIndex = 0;
                        ret = true;
                        break;
                    case W32.VK_ENTER:                      // следующий справочник
                        ChangeCurNSI();
                        ret = true;
                        break;
                }
            }
            e.Handled = ret;

            return (ret);
        }


        class LoadNSISrv
        {
            private NSI xNSI;
            private string sTName;
            private DataTable dt;
            private bool bMD_5;
            private string sMD5Old = "";
            private DataRow drMD5;

            public string sDop = "";
            public LoadFromSrv dgL;

            public LoadNSISrv(NSI x_NSI, string sTName_Ind, bool bMD5)
            {
                xNSI = x_NSI;
                sTName = sTName_Ind;
                dt = xNSI.DT[sTName].dt;
                bMD_5 = bMD5;
                object xSearch = sTName;
                drMD5 = xNSI.DT[NSI.BD_TINF].dt.Rows.Find(xSearch);
                if (drMD5 != null)
                {
                    sMD5Old = (string)drMD5["MD5"];
                    sDop = (bMD5 == true) ? "MD5=" + sMD5Old + ";" : "";
                }
                dgL = new LoadFromSrv(NsiFromSrv);
                
            }

            //private void NsiFromSrv(Stream stmX, Dictionary<string, string> aC, DataSet ds, 
            //    ref string sErr, int nRetSrv)
            //{
            //    string sMD5New = aC["MD5"];
            //    string sP = xNSI.sPathNSI + xNSI.DT[sTName].sXML;

            //    DataTable dtAdded = null;
            //    int nAddedCount = 0;

            //    if ((bMD_5 == true) && (sMD5New == sMD5Old))
            //        sErr = "OK-No Load";
            //    else
            //    {
            //        sErr = "Ошибка чтения XML";
            //        string sXMLFile = "";
            //        int nFileSize = ServClass.ReadXMLWrite2File(stmX, ref sXMLFile);
            //        switch (sTName)
            //        {
            //            case NSI.I_TMT:
            //                dtAdded = dt.Clone();
            //                dtAdded.BeginLoadData();

            //                foreach (DataRow dr in dt.Rows)
            //                {
            //                    if ( (System.DBNull.Value != dr["SOURCE"]) && 
            //                         (NSI.DOCSRC_ZVKLOAD == (int)dr["SOURCE"]) )
            //                    {// добавить в буфер для восстановления
            //                        dtAdded.ImportRow(dr);
            //                    }
            //                }
            //                dtAdded.EndLoadData();
            //                nAddedCount = dtAdded.Rows.Count;
            //                break;
            //        }

            //        sErr = "Ошибка загрузки XML";
            //        dt.BeginLoadData();
            //        dt.Clear();
            //        System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
            //        dt.ReadXml(xmlRd);
            //        xmlRd.Close();
            //        if (nAddedCount > 0)
            //        {
            //            dt.Merge(dtAdded);
            //        }

            //        dt.EndLoadData();

            //        sErr = "Ошибка сохранения XML";
            //        //File.Delete( sP );
            //        //File.Move(sXMLFile, sP);
            //        dt.WriteXml(sP);

            //        xNSI.DT[NSI.BD_TINF].dt.Rows[sTName]["MD5"] = sMD5New;
            //        sErr = "OK";
            //    }
            //}

            private void NsiFromSrv(Stream stmX, Dictionary<string, string> aC, DataSet ds,
                ref string sErr, int nRetSrv)
            {
                string sMD5New = aC["MD5"];
                string sP = xNSI.sPathNSI + xNSI.DT[sTName].sXML;

                //DataTable dtAdded = null;
                //int nAddedCount = 0;

                if ((bMD_5 == true) && (sMD5New == sMD5Old))
                    sErr = "OK-No Load";
                else
                {
                    sErr = "Ошибка чтения XML";
                    string sXMLFile = "";
                    int nFileSize = ServClass.ReadXMLWrite2File(stmX, ref sXMLFile);

                    sErr = "Ошибка загрузки XML";
                    dt.BeginLoadData();
                    dt.Clear();
                    System.Xml.XmlReader xmlRd = System.Xml.XmlReader.Create(sXMLFile);
                    dt.ReadXml(xmlRd);
                    xmlRd.Close();
                    //if (nAddedCount > 0)
                    //  dt.Merge(dtAdded);

                    dt.EndLoadData();

                    sErr = "Ошибка сохранения XML";
                    //File.Delete( sP );
                    //File.Move(sXMLFile, sP);
                    dt.WriteXml(sP);

                    if (drMD5 != null)
                        drMD5["MD5"] = sMD5New;
                    sErr = "OK";
                }
            }



        }

        // загрузка справочников с сервера
        private int LoadAllNSISrv(string[] aI, bool bMD5, bool xShow)
        {
            int nErr = 0, nGood = 0, nRet = 0, tc1 = Environment.TickCount;
            string i, sFull = "", sT = "";
            int j;

            xNSI.dsNSI.EnforceConstraints = false;

            if (aI.Length == 0)
            {
                List<string> lT = new List<string>();

                foreach (KeyValuePair<string, NSI.TableDef> td in xNSI.DT)
                {
                    if (((td.Value.nType & NSI.TBLTYPE.NSI) == NSI.TBLTYPE.NSI) &&
                        ((td.Value.nType & NSI.TBLTYPE.LOAD) == NSI.TBLTYPE.LOAD))   // НСИ загружаемое
                        lT.Add(td.Key);
                }
                lT.CopyTo(aI);
            }

            for (j = 0; j < aI.Length; j++)
            {
                nRet = 0;
                i = aI[j];
                LoadNSISrv lnsi = new LoadNSISrv(xNSI, i, bMD5);
                sT = ExchgSrv(AppC.COM_ZSPR, i, lnsi.sDop, lnsi.dgL, null, ref nRet);
                if (nRet == 0)
                    nGood++;
                else
                    nErr++;
                sT = xNSI.DT[i].Text + "..." + sT + "\r\n";
                sFull += sT;

                if (xShow == true)
                    xFPan.UpdateReg(sT);
            }
            try
            {
                xNSI.dsNSI.EnforceConstraints = true;
            }
            catch
            {
                nErr = 1;
            }
            if (nErr == 0)
            {
                xSm.dtLoadNS = DateTime.Now;
            }

            sT = Srv.TimeDiff(tc1, Environment.TickCount);
            if (bMD5 == false)
                MessageBox.Show(sFull, "Время-" + sT);
            return (nRet);
        }


        private void LoadNsiMenu(bool bMD5)
        {
            //string sHLine;

            //pnLoadDoc.Left = dgDoc.Left + 5;
            //pnLoadDoc.Top = dgDoc.Top + 25;
            //sHLine = (bMD5 == true) ? "Проверка" : "Загрузка";

            //lFuncNamePan.Text = sHLine + " справочников";
            //lpnLoadInf.Text = "Соединение с сервером";
            //pnLoadDoc.Enabled = true; pnLoadDoc.Visible = true;
            //pnLoadDoc.Refresh();

            xFPan.ShowP(6, 45, "Обновление НСИ", (bMD5 == true) ? "Проверка" : "Загрузка");
            xFPan.UpdateHelp("Соединение с сервером");

            Cursor crsOld = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            LoadTInf();

            LoadAllNSISrv(new string[] { NSI.NS_USER, NSI.NS_POST, NSI.NS_SHLG, NSI.NS_PURP }, bMD5, true);
            Cursor.Current = crsOld;

            SaveTInf();
            xFPan.HideP();
            //pnLoadDoc.Visible = false; pnLoadDoc.Enabled = false;
            //pnLoadDoc.Left = 350;

        }

        private void LoadTInf()
        {
            xNSI.LoadLocNSI(new string[] { NSI.BD_TINF }, NSI.LOAD_ANY);

            if (xNSI.DT[NSI.BD_TINF].nErr != 0)
            {// загрузить не удалось
                foreach (KeyValuePair<string, NSI.TableDef> td in xNSI.DT)
                {
                    if (((td.Value.nType & NSI.TBLTYPE.NSI) == NSI.TBLTYPE.NSI) &&
                        ((td.Value.nType & NSI.TBLTYPE.LOAD) == NSI.TBLTYPE.LOAD))   // НСИ загружаемое
                    {
                        DataRow dr = xNSI.DT[NSI.BD_TINF].dt.NewRow();
                        dr["DT_NAME"] = td.Key;
                        dr["MD5"] = "12345678901234567890123456789012";
                        xNSI.DT[NSI.BD_TINF].dt.Rows.Add(dr);
                    }
                }
            }
        }

        private void SaveTInf()
        {
            xNSI.DT[NSI.BD_TINF].dt.WriteXml(xNSI.sPathNSI + xNSI.DT[NSI.BD_TINF].sXML);
            xNSI.DT[NSI.BD_TINF].dt.Clear();
            xNSI.DT[NSI.BD_TINF].nState = NSI.DT_STATE_INIT;
        }



    }
}
