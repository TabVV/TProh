using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.IO;
using System.Threading;

using SavuSocket;
using ScannerAll;
using PDA.Service;

namespace Proh
{
    public delegate void LoadFromSrv(Stream s, Dictionary<string, string> aC, DataSet ds,
                                     ref string sE, int nRetSrv);

    //public delegate void LoadFromSrv(SocketStream s, Dictionary<string, string> aC, DataSet ds,
                                     //ref string sE, int nRetSrv);
    public delegate void OperSocket();

    public partial class MainF : Form
    {
        private SocketStream ssWrite;

        // Разбор команды от сервера
        private Dictionary<string,string> SrvCommParse(string sC, char[] aC)
        {
            int j = 0;
            string k, v;
            Dictionary<string, string> dicSrvComm = new Dictionary<string,string>();

            //string[] saPars = sC.Split(new char[]{';'});
            string[] saPars = sC.Split(aC);
            for (int i = 0; i < saPars.Length; i++)
            {
                if (saPars[i] != "")
                {
                    j = saPars[i].IndexOf('=');
                    if (j > 0)
                    {
                        //dicSrvComm.Add(saPars[i].Substring(0, j).Trim(), saPars[i].Substring(j + 1).Trim());
                        k = saPars[i].Substring(0, j).Trim();
                        try
                        {
                            v = saPars[i].Substring(j + 1).Trim();
                        }
                        catch { v = ""; }
                        dicSrvComm.Add(k, v);


                    }
                }
            }
            return(dicSrvComm);
        }

        
        // формирование команды на выгрузку
        private byte[] SetUpLoadCommand(int nComCode, string sP, string sMD5)
        {
            string s,
                sCom = "COM=" + AppC.saComms[nComCode],
                sTz = ";";
            sCom += sTz + "KP=" + xSm.sUser + sTz;

            s = String.Format("DEVID={0},PTVL={1},IO={2},TG={3},CEL={4}", xPars.DevID, xAvt.Putev,
                xAvt.DirMove, xAvt.GruzType, xAvt.Purp);

            //s = String.Format("DEVID={0},PTVL={1},IO={3},TG={4},CEL={2}", xPars.DevID, xAvt.Putev,
            //    cbPurp.SelectedValue, xAvt.DirMove, xAvt.GruzType);


            switch (nComCode)
            {
                case AppC.COM_ZPL:
                    sP = "PAR=(" + s + ");";
                    break;
                case AppC.COM_ZSPR:
                    sP = "PAR=" + sP + sTz + sMD5;
                    break;
                case AppC.COM_ZCONTR:
                    sP = "PARX=" + sP + sTz;
                    break;
                case AppC.COM_ZOPEN:
                    DateTime dtn = DateTime.Now;
                    sCom += "CDT=" + dtn.ToString("yyyyMMdd") + sTz + 
                        "CTM=" + dtn.ToString("HH:mm:ss") + sTz;
                    if (xAvt.Propusk.Length > 0)
                        s += ",PRPSK=" + xAvt.Propusk.Trim();
                    sP = "PAR=(" + s + "," + sP + ");";
                    break;
                default:
                    sCom = "ERR";
                    throw new SystemException("Неизвестная команда");
            }

            sCom +=  sP + Encoding.UTF8.GetString(AppC.baTermCom, 0, AppC.baTermCom.Length);

            byte [] baCom = Encoding.UTF8.GetBytes(sCom);
            return (baCom);
        }






        //private DataRow[] PrepDataArrForUL(int nReg)
        //{
        //    int nRet = AppC.RC_OK;
        //    string sRf = "";
        //    DataRow[] ret = null;

        //    if (nReg == AppC.UPL_CUR)
        //    {
        //        if ((int)xCDoc.drCurRow["SOURCE"] == NSI.DOCSRC_UPLD)
        //        {
        //            string sErr = "Уже выгружен!";
        //            DialogResult dr;
        //            dr = MessageBox.Show("Отменить выгрузку (Enter)?\r\n (ESC) - выгрузить повторно",
        //            sErr,
        //            MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
        //            MessageBoxDefaultButton.Button1);
        //            if (dr == DialogResult.OK)
        //            {
        //                nRet = AppC.RC_CANCEL;
        //            }
        //            else
        //                nRet = AppC.RC_OK;
        //        }
        //        if (nRet == AppC.RC_OK)
        //        {
        //            ret = new DataRow[] { xCDoc.drCurRow };
        //        }
        //    }
        //    else if (nReg == AppC.UPL_ALL)
        //    {
        //        // фильтр - текущий для Grid документов + невыгруженные
        //        sRf = xNSI.DT[NSI.TBD_DOC].dt.DefaultView.RowFilter;
        //        if (sRf != "")
        //        {
        //            sRf = "(" + sRf + ")AND";
        //        }
        //        sRf += String.Format("(SOURCE<>{0})", NSI.DOCSRC_UPLD);
        //        ret = (DataRow[])PrepForAll(sRf, false);
        //    }
        //    else if (nReg == AppC.UPL_FLT)
        //    {
        //        //sRf = FiltForDoc(xCUpLoad.SetFiltInRow(xNSI));
        //        sRf = xCUpLoad.SetFiltInRow();
        //        ret = (DataRow[])PrepForAll(sRf, false);
        //    }
        //    return (ret);
        //}





        // формирование DataSet для отгрузки данных
        //private DataSet PrepDataSetForUL(int nReg)
        //{
        //    int nRet = AppC.RC_OK;
        //    string sRf = "";
        //    DataSet ret = null;

        //    if (nReg == AppC.UPL_CUR)
        //    {
        //        if ((int)xCDoc.drCurRow["SOURCE"] == NSI.DOCSRC_UPLD)
        //        {
        //            string sErr = "Уже выгружен!";
        //            DialogResult dr;
        //            dr = MessageBox.Show("Отменить выгрузку (Enter)?\r\n (ESC) - выгрузить повторно",
        //            sErr,
        //            MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
        //            MessageBoxDefaultButton.Button1);
        //            if (dr == DialogResult.OK)
        //            {
        //                nRet = AppC.RC_CANCEL;
        //            }
        //            else
        //                nRet = AppC.RC_OK;
        //        }
        //        if (nRet == AppC.RC_OK)
        //        {
        //            DataRow[] drA = { xCDoc.drCurRow };
        //            ret = xNSI.MakeWorkDataSet(xNSI.DT[NSI.TBD_DOC].dt, xNSI.DT[NSI.I_TFAKT].dt, drA);
        //        }
        //    }
        //    else if (nReg == AppC.UPL_ALL)
        //    {
        //        // фильтр - текущий для Grid документов + невыгруженные
        //        sRf = xNSI.DT[NSI.TBD_DOC].dt.DefaultView.RowFilter;
        //        if (sRf != "")
        //        {
        //            sRf = "(" + sRf + ")AND";
        //        }
        //        sRf += String.Format("(SOURCE<>{0})", NSI.DOCSRC_UPLD);
        //        ret = (DataSet)PrepForAll(sRf, true);
        //    }
        //    else if (nReg == AppC.UPL_FLT)
        //    {
        //        //sRf = FiltForDoc(xCUpLoad.SetFiltInRow(xNSI));
        //        sRf = xCUpLoad.SetFiltInRow();
        //        ret = (DataSet)PrepForAll(sRf, true);
        //    }
        //    return (ret);
        //}

        //private object PrepForAll(string sRf, bool bNeedDataSet)
        //{
        //    object ret = null;

        //    // сортировка - ???
        //    string sSort = "";

        //    // все неотгруженные документы
        //    DataView dv = new DataView(xNSI.DT[NSI.TBD_DOC].dt, sRf, sSort, DataViewRowState.CurrentRows);
        //    DataRow[] drA = new DataRow[dv.Count];
        //    xCUpLoad.naComms = new List<int>();
        //    for (int i = 0; i < dv.Count; i++)
        //    {
        //        drA.SetValue(dv[i].Row, i);
        //        xCUpLoad.naComms.Add( (int)drA[i]["TD"]);

        //    }
        //    if (drA.Length > 0)
        //    {
        //            ret = drA;
        //    }
        //    return (ret);

        //}




        // сохранение документов в XML
/*
        private string UpLoadXML(int nComm)
        {
            int i,
                nCommLen = 0;
            byte[] baCom,
                baAns;
            string sC,
                sErr = "";
            DataSet dsTrans;
            DataRow[] drAUpL = null;
            Dictionary<string, string> aComm;
            System.IO.Stream stm;

            byte[] baBuf = new byte[1024];
            try
            {

                //dsTrans = PrepDataSetForUL(xCUpLoad.ilUpLoad.CurReg);
                //sErr = "(" + tSrvParServer.Text + ")Ошибка передачи - ";

                drAUpL = PrepDataArrForUL(xCUpLoad.ilUpLoad.CurReg);
                if (drAUpL != null)
                {
                    baCom = SetUpLoadCommand(nComm, "", "");

                    for (i = 0; i < drAUpL.Length; i++)
                    {
                        ssWrite = new SocketStream(tSrvParServer.Text, int.Parse(tSrvParServPort.Text));
                        stm = ssWrite.Connect();

                        dsTrans = xNSI.MakeWorkDataSet(xNSI.DT[NSI.TBD_DOC].dt,
                                  xNSI.DT[NSI.I_TFAKT].dt, new DataRow[] {drAUpL[i]});


                        stm.Write(baCom, 0, baCom.Length);
                        stm.Write(AppC.baTermCom, 0, AppC.baTermCom.Length);

                        dsTrans.WriteXml(stm, XmlWriteMode.IgnoreSchema);

                        stm.Write(AppC.baTermMsg, 0, AppC.baTermMsg.Length);

                        //nRec = stm.Read(baBuf, 0, baBuf.Length);


                        nCommLen = 0;
                        baAns = ReadAnswerCommand(stm, ref nCommLen);
                        stm.Close();
                        ssWrite.Disconnect();

                        sC = Encoding.UTF8.GetString(baAns, 0, nCommLen - AppC.baTermCom.Length);
                        aComm = SrvCommParse(sC);

                        if (aComm["COM"] == AppC.saComms[nComm])
                        {
                            if (int.Parse(aComm["RET"]) == 0)
                            {
                                xNSI.SetRecState(dsTrans);
                                if ((i + 1) >= drAUpL.Length)
                                    sErr += "OK - Передача завершена";

                            }
                            else
                            {
                                sErr += drAUpL[i]["KEKS"].ToString() + "-" + aComm["MSG"] + "\n";
                            }
                        }

                    }
                }
                else
                {
                    sErr = "Нет данных для передачи";
                }
            }
            catch (Exception ex)
            {
                sErr = "(" + tSrvParServer.Text + ")Ошибка передачи";
                //sErr = ex.Message;
            }
            //MessageBox.Show(sErr, sRez);
            return (sErr);
        }
*/


        //private string UpLoadDoc(int nComm, ref int nR)
        //{
        //    int i,
        //        nRet = AppC.RC_OK;
        //    string sErr = "";
        //    DataRow[] drAUpL = null;

        //    try
        //    {
        //        drAUpL = PrepDataArrForUL(xCUpLoad.ilUpLoad.CurReg);
        //        if (drAUpL != null)
        //        {
        //            LoadFromSrv dgL = null;
        //            for (i = 0; i < drAUpL.Length; i++)
        //            {
        //                xCUpLoad.xCur = new CurDoc(AppC.UPL_CUR);
        //                xCUpLoad.xCur.drCurRow = drAUpL[i];
        //                xNSI.InitCurDoc(xCUpLoad.xCur);
        //                //tbPanP1.Text = xCUpLoad.xCur.xDocP.sNomDoc;
        //                //tbPanP1.Refresh();
        //                xFPan.UpdateReg(xCUpLoad.xCur.xDocP.sNomDoc);
        //                sErr = ExchgSrv(nComm, "", "", dgL, null, ref nRet);
        //                if (nRet != AppC.RC_OK)
        //                    break;
        //            }
        //            sErr += "(" + drAUpL.Length.ToString() + ")";
        //        }
        //        else
        //        {
        //            nRet = AppC.RC_NODATA;
        //            sErr = "Нет данных для передачи";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        nRet = AppC.RC_NODATA;
        //        sErr = "Ошибка подготовки";
        //    }
        //    nR = nRet;
        //    return (sErr);
        //}

        //private string FiltForDoc(DataRow drZ)
        //{
        //    int n;
        //    string s;

        //    string sF = String.Format("(TD={0}) AND (DT={1}) AND (SYSN={2})", drZ["TD"], drZ["DT"], drZ["SYSN"]);


        //    s = "AND(ISNULL(KPP1,-1)=-1)";
        //    try
        //    {
        //        n = (int)drZ["KPP1"];
        //        if (n > 0)
        //        {
        //            s = "AND(KPP1=" + n.ToString() + ")";
        //        }
        //        else
        //            drZ["KPP1"] = System.DBNull.Value;
        //    }
        //    catch { s = ""; }
        //    finally
        //    {
        //        sF += s;
        //    }

        //    s = "AND(ISNULL(NUCH,-1)=-1)";
        //    try
        //    {
        //        n = (int)drZ["NUCH"];
        //        if (n > 0)
        //        {
        //            s = "AND(NUCH=" + n.ToString() + ")";
        //        }
        //        else
        //            drZ["NUCH"] = System.DBNull.Value;
        //    }
        //    catch { s = ""; }
        //    finally
        //    {
        //        sF += s;
        //    }

        //    return ("(" + sF + ")");
        //}




        // чтение ответа
        //private byte[] ReadAnswerCommand(System.IO.Stream stm, ref int nLen)
        //{
        //    int i, n1Byte;
        //    bool bEnd = false;
        //    byte[] baCom = new byte[1024];
        //    nLen = 0;
        //    i = 0;
        //    try
        //    {
        //        n1Byte = stm.ReadByte();
        //        while ((n1Byte != -1) && (bEnd == false))
        //        {
        //            i++;
        //            baCom[i - 1] = (byte)n1Byte;
        //            bEnd = false;
        //            if ((i >= AppC.baTermCom.Length) && (baCom[i - 1] == AppC.baTermCom[AppC.baTermCom.Length - 1]))
        //            {
        //                bEnd = true;
        //                for (int j = 0; j < AppC.baTermCom.Length; j++)
        //                {
        //                    if (baCom[i - 1 - j] != AppC.baTermCom[AppC.baTermCom.Length - 1 - j])
        //                    {
        //                        bEnd = false;
        //                        break;
        //                    }
        //                }
        //            }
        //            if (bEnd != true)
        //                n1Byte = stm.ReadByte();
        //        }
        //        if (bEnd == true)
        //            nLen = i;

        //    }
        //    catch { }
        //    return (baCom);
        //}




        // обмен данными с сервером в формате XML
        // nCom - номер команды
        // sPar1



        // чтение ответа
        private byte[] ReadAnswerCommand(System.IO.Stream stm, ref int nLen)
        {
            int i, n1Byte;
            bool bEnd = false;
            byte[] baCom = new byte[1024];
            nLen = 0;
            i = 0;
            try
            {
                n1Byte = stm.ReadByte();
                while ((n1Byte != -1) && (bEnd == false))
                {
                    i++;
                    baCom[i - 1] = (byte)n1Byte;
                    bEnd = false;
                    if ((i >= AppC.baTermCom.Length) && (baCom[i - 1] == AppC.baTermCom[AppC.baTermCom.Length - 1]))
                    {
                        bEnd = true;
                        for (int j = 0; j < AppC.baTermCom.Length; j++)
                        {
                            if (baCom[i - 1 - j] != AppC.baTermCom[AppC.baTermCom.Length - 1 - j])
                            {
                                bEnd = false;
                                break;
                            }
                        }
                    }
                    if (bEnd != true)
                        n1Byte = stm.ReadByte();
                }
                if (bEnd == true)
                    nLen = i;

            }
            catch { }
            return (baCom);
        }


        private string ExchgSrv(int nCom, string sPar1, string sDop, 
            LoadFromSrv dgRead, DataSet dsTrans,  ref int ret)
        {
            string sC;
            string sAdr = xPars.sHostSrv + ":" + xPars.nSrvPort.ToString();
            string sErr = sAdr + "-нет связи с сервером";
            int nRetSrv;
            byte[] bAns = { };

            //SocketStream.ASRWERROR nRErr;

            System.IO.Stream stm = null;

            ret = 0;
            try
            {
                ssWrite = new SocketStream(xPars.sHostSrv, xPars.nSrvPort);
                if (!TestConnection())
                {
                    //throw new System.Net.Sockets.SocketException(11053);
                }

                stm = ssWrite.Connect();

                // поток создан, отправка команды
                sErr = sAdr + "-команды не отправлена";
                byte[] baCom = SetUpLoadCommand(nCom, sPar1, sDop);

                // 20 секунд на запись команды
                ssWrite.ASWriteS.TimeOutWrite = 1000 * 20;
                ssWrite.ASWriteS.BeginAWrite(baCom, baCom.Length);

                sErr = sAdr + "-ошибка завершения";
                // 10 секунд на запись терминатора сообщения
                ssWrite.ASWriteS.TimeOutWrite = 1000 * 10;
                // терминатор сообщения
                ssWrite.ASWriteS.BeginAWrite(AppC.baTermMsg, AppC.baTermMsg.Length);

                sErr = sAdr + "-ошибка чтения";
/*
                // 120 секунд на чтение ответа
                ssWrite.ASReadS.TimeOutRead = 1000 * 120;
                ssWrite.ASReadS.TermMsg = AppC.baTermCom;
                nRErr = ssWrite.ASReadS.BeginARead();
                switch (nRErr)
                {
                    case SocketStream.ASRWERROR.RET_FULLBUF:   // переполнение буфера
                        sErr = " длинная команда";
                        throw new System.Net.Sockets.SocketException(10061);
                    case SocketStream.ASRWERROR.RET_FULLMSG:   // сообщение полностью получено
                        sC = ssWrite.ASReadS.GetMsg();
                        break;
                    default:
                        throw new System.Net.Sockets.SocketException(10061);
                }
*/

                //============
                int nCommLen = 0;
                bAns = ReadAnswerCommand(stm, ref nCommLen);
                sC = Encoding.UTF8.GetString(bAns, 0, nCommLen - AppC.baTermCom.Length);
                //================


                Dictionary<string, string> aComm = SrvCommParse(sC, new char[] { ';' });
                nRetSrv = int.Parse(aComm["RET"]);

                if (aComm["COM"] == AppC.saComms[nCom])
                {
                    if (nRetSrv == AppC.RC_OK)
                    {
                        sErr = "OK";
                        if (dgRead != null)
                            dgRead(stm, aComm, dsTrans, ref sErr, nRetSrv);
                        else
                        {//
                            //sErr = sAdr + "-завершение";
                            //// 5 секунд на чтение ответа
                            //ssWrite.ASReadS.TimeOutRead = 1000 * 5;
                            //ssWrite.ASReadS.TermMsg = AppC.baTermMsg;
                            //nRErr = ssWrite.ASReadS.BeginARead(256);
                            //switch (nRErr)
                            //{
                            //    case SocketStream.ASRWERROR.RET_FULLBUF:   // переполнение буфера
                            //        sErr = " длинная команда";
                            //        throw new System.Net.Sockets.SocketException(10061);
                            //    case SocketStream.ASRWERROR.RET_FULLMSG:   // сообщение полностью получено
                            //        break;
                            //    default:
                            //        throw new System.Net.Sockets.SocketException(10061);
                            //}
                        }
                    }
                    else
                    {
                        if (aComm["MSG"] != "")
                            sErr = sAdr + " - ошибка:\r\n" + aComm["MSG"];
                        else
                            sErr = sAdr + "\r\n Отложено выполнение";
                    }
                }
                ret = nRetSrv;

            }
            catch (Exception e)
            {
                sC = e.Message;
                ret = 3;
            }
            finally
            {
                ssWrite.Disconnect();
            }
            return (sErr);


        }

        private string ShMove(string sHostSh, int nPortSh, string sLogin, string sPass,
            string sDevID, string sOper, ref int nRet)
        {

            string sC;
            string sAdr = sHostSh + ":" + nPortSh.ToString();
            string sErr = sAdr + "-нет связи с сервером";
            SocketStream.ASRWERROR nRErr;
            byte[] baCom;

            SocketStream ssSH = null;
            System.IO.Stream stm = null;

            try
            {
                Encoding enc866 = Encoding.Default;

                ssSH = new SocketStream(sHostSh, nPortSh);
                if (!TestConnection())
                {
                    //throw new System.Net.Sockets.SocketException(11053);
                }
                stm = ssSH.Connect();

                // поток создан, отправка команды
                sErr = sAdr + "-команда не отправлена";
                string sCom = "100|" + sLogin + "|" + sPass + "|" + sDevID + "|" + sOper;
                baCom = enc866.GetBytes(sCom);

                // 10 секунд на запись команды
                ssSH.ASWriteS.TimeOutWrite = 1000 * 10;
                ssSH.ASWriteS.BeginAWrite(baCom, baCom.Length);

                sErr = sAdr + "-ошибка чтения";
                // 20 секунд на чтение ответа
                ssSH.ASReadS.TimeOutRead = 1000 * 30;
                ssSH.ASReadS.MsgEncoding = Encoding.Default;
                //ssSH.ASReadS.TermMsg = enc866.GetBytes("1|");
                ssSH.ASReadS.TermDat = enc866.GetBytes("1|");

                nRErr = ssSH.ASReadS.BeginARead(256);
                switch (nRErr)
                {
                    case SocketStream.ASRWERROR.RET_FULLBUF:   // переполнение буфера
                        sErr = " длинная команда";
                        throw new System.Net.Sockets.SocketException(10061);
                    case SocketStream.ASRWERROR.RET_FULLMSG:   // сообщение полностью получено
                        sC = ssSH.ASReadS.GetMsg();
                        break;
                    default:
                        throw new System.Net.Sockets.SocketException(10061);
                }

                sErr = " ошибочный ответ";
                string[] saReply = sC.Split(new char[] { '|' });
                nRet = int.Parse(saReply[2]);
                switch (nRet)
                {
                    case 0:
                        sErr = "OK";
                        break;
                    case -1:
                        sErr = " Неверный ID";
                        break;
                    case -2:
                        sErr = " Неверный User/Pass";
                        break;
                    case -3:
                        sErr = " Ошибка устройства";
                        break;
                    case -4:
                        sErr = " Нет прав";
                        break;
                    default:
                        sErr = " Неизвестная ошибка";
                        break;
                }


            }
            catch (Exception e)
            {
                sC = e.Message;
                nRet = 3;
            }
            finally
            {
                ssSH.Disconnect();
            }
            return (sErr);
        }

        private bool TestConnection()
        {
            bool ret = true;
            WiFiStat.CONN_TYPE cT = xBCScanner.WiFi.ConnectionType();

            if (cT == WiFiStat.CONN_TYPE.NOCONNECTIONS)
            {
                bool bHidePan = false;

                if (!xFPan.IsShown)
                {
                    xFPan.ShowP(6, 50, "Переподключение к сети", "Wi-Fi");
                    bHidePan = true;
                }

                Cursor crsOld = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                xFPan.RegInf = "Переподключение Wi-Fi...";
                ret = xBCScanner.WiFi.ResetWiFi(2);
                if (ret)
                {
                    xFPan.RegInf = "IP получен:" + xBCScanner.WiFi.IPCurrent;
                }
                else
                {
                    xFPan.RegInf = "Wi-Fi недоступен...";
                }
                if (bHidePan)
                    xFPan.HideP();

                Cursor.Current = crsOld;
            }
            return (ret);
        }

        private void WDebug()
        {
            if (swProt != null)
            {
            }
        }



    }
}
