using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using ScannerAll;
using PDA.OS;
using PDA.Service;

namespace Proh
{
    public partial class Avtor : Form
    {
        /// Еще:
/*
 *  - 
 */

        private const int REG_NEW = 0;          // начало новой смены
        private const int REG_INPROC = 1;       // смена уже начата
        private const int REG_CLOSE = 2;
        private const int REG_PARS = 3;

        //private ScannerAll.TERM_TYPE nTerminalType = TERM_TYPE.UNKNOWN;
        private ScannerAll.BarcodeScanner xBCS = null;
        private NSI xNSI;
        private AppPars xPars;

        // режим вызова: начало или завершение
        public int nCurReg = AppC.AVT_LOGON;

        // флаг смены имени пользователя User
        private bool bUserChanged = false;

        // не обрабатывать введенный символ
        private bool bSkipKey = false;          

        // текущая таблица переходов для редактирования
        private AppC.EditListC 
            aEdVvod;

        // текущая команда перехода в таблице переходов
        //private int 
        //    nCurEditCommand = -1;

        //private bool bMayQuit = false;

        public Smena 
            xSm, xOld;

        public Avtor(NSI x, Smena y, ScannerAll.BarcodeScanner xSc, AppPars xp)
        {
            InitializeComponent();
            xNSI = x;
            xSm  = y;
            xBCS = xSc;
            //nTerminalType = xSc.nTermType;
            xPars = xp;
            //bMayQuit = false;
        }

        // загрузка формы
        private void Avtor_Load(object sender, EventArgs e)
        {

            // центровка формы
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            this.Location = new Point((screen.Width - this.Width) / 2,
                (screen.Height - this.Height) / 2);

            SetBindSmenaPars();
            SetAvtFields();

            // установка делегата проверки корректности данных
            //ServClass.dgVerEd = new VerifyEditFields(Try2Start);

            // переход в режим редактирования
            CreateEdArrDet(nCurReg);
        }

        private void SetBindSmenaPars()
        {
            Binding bi;

            bi = new Binding("Text", xSm, "nSklad");
            tDefKSKL.DataBindings.Add(bi);
            bi = new Binding("Text", xSm, "DocDate");
            tDefDateDoc.DataBindings.Add(bi);
            bi = new Binding("Text", xSm, "CurSmena");
            tDefSmena.DataBindings.Add(bi);
        }


        // сброс/установка полей ввода/вывода
        private void SetAvtFields()
        {
            tUser.Text = xSm.sUName;
            tPass.Text = "";
            tDefNameSkl.Text = xNSI.GetNameSPR(NSI.NS_POST, new object[] { xSm.nSklad }, "NAME").sName;
            tDefDateDoc.Text = DateTime.Now.ToString("dd.MM.yy");
        }


        // выделение всего поля при входе (текстовые поля)
        private void SelAllTextF(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        // установка флага смены пользователя
        private void tUser_TextChanged(object sender, EventArgs e)
        {
            string s = tUser.Text.Trim().ToUpper();
            bUserChanged = true;
            if ((s.Length <= AppC.SUSER.Length) && (s == AppC.SUSER.Substring(0, s.Length)))
                tUser.PasswordChar = '*';
            else
                tUser.PasswordChar = Char.MinValue; ;
        }

        // сброс флага смены пользователя
        private void tUser_Validated(object sender, EventArgs e)
        {
            bUserChanged = false;
        }

        // проверка имени пользователя
        private void tUser_Validating(object sender, CancelEventArgs e)
        {
            if (bUserChanged == true)
            {
                string s = tUser.Text.Trim().ToUpper();
                if (s.Length > 0)
                {
                    if ((s == AppC.SUSER) || (s == AppC.GUEST))
                    {// пароль не нужен
                        tUser.Text = (s == AppC.SUSER)?"Admin" : "Сотрудник";
                        //ServClass.ChangeEdArrDet(null, new Control[] { tPass }, aEdVvod);
                        aEdVvod.SetAvail(tPass, false);
                    }
                    else
                    {
                        try
                        {
                            //ServClass.ChangeEdArrDet(new Control[] { tPass }, null, aEdVvod);
                            aEdVvod.SetAvail(tPass, true);
                            NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_USER, new object[] { s }, "NMP");
                            if (zS.bFind == false)
                                e.Cancel = true;
                            else
                                tUser.Text = zS.sName;
                        }
                        catch
                        {
                            e.Cancel = true;
                        }
                    }
                }
                if (e.Cancel != true)
                {
                    xSm.sUser = s;
                    xSm.sUName = tUser.Text;
                    //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
                }
            }
        }

        // проверка пароля
        private void tPass_Validating(object sender, CancelEventArgs e)
        {
            if (xSm.sUser.Length > 0)
            {
                if (tPass.Text.Trim().Length > 0)
                    e.Cancel = !(ValidUser());
                //if (e.Cancel != true)
                //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
            }

            //string sP = tPass.Text.Trim();
            //if ((nCurReg == AppC.AVT_LOGON) && (sP.Length > 0) && (xSm.sUser.Length > 0))
            //    e.Cancel = !ValidUserPass(xSm.sUser, sP, "", "");



        }


        // проверка пароля для непустого пользователя
        //public bool ValidUserPass(string sUser, string sPass, string sTabN, string sUN)
        //{
        //    bool ret = false;
        //    int nRet = AppC.RC_OK;

        //    if (sUser.Length > 0)
        //    {
        //        if ((sUser != AppC.SUSER) && (sUser != AppC.GUEST))
        //        {
        //            try
        //            {
        //                if (sTabN.Length > 0)
        //                {// отсканирован штрихкод с табельным
        //                    ret = true;
        //                }
        //                else
        //                {
        //                    DataView dv = new DataView(xNSI.DT[NSI.NS_USER].dt,
        //                                    String.Format("KP='{0}'", sUser), "", DataViewRowState.CurrentRows);
        //                    if (dv.Count == 1)
        //                    {
        //                        if (sPass == (string)dv[0].Row["PP"])
        //                        {
        //                            ret = true;
        //                            sUN = (string)dv[0].Row["NMP"];
        //                            sTabN = (string)dv[0].Row["TABN"];
        //                        }
        //                    }
        //                }

        //                string sE = CheckUserLogin(sUser, sTabN, ref nRet);
        //                if (nRet != AppC.RC_OK)
        //                {
        //                    ret = false;
        //                    Srv.ErrorMsg(sE, true);
        //                }
        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            ret = true;
        //            sUN = (sUser == AppC.SUSER) ? "Admin" : "Работник склада";
        //        }
        //        if (ret)
        //        {
        //            xSm.sUser = sUser;
        //            xSm.sUName = sUN;
        //            xSm.sUserPass = sPass;
        //            xSm.sUserTabNom = sTabN;
        //            xSm.urCur = (sUser == AppC.SUSER) ? Smena.USERRIGHTS.USER_SUPER :
        //                Smena.USERRIGHTS.USER_KLAD;
        //        }
        //    }
        //    return (ret);
        //}




        // проверка пароля для непустого пользователя
        private bool ValidUser()
        {
            bool ret = false;
            if (xSm.sUser.Length > 0)
            {
                if ((xSm.sUser != AppC.SUSER) && (xSm.sUser != AppC.GUEST))
                {
                    try
                    {
                        xSm.urCur = Smena.USERRIGHTS.USER_KLAD;
                        NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_USER, new object[] { xSm.sUser }, "PP");
                        if (zS.bFind == true)
                        {
                            if (tPass.Text.Trim() == zS.sName)
                            {
                                ret = true;
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    ret = true;
                    xSm.urCur = (xSm.sUser == AppC.SUSER)?Smena.USERRIGHTS.USER_SUPER : Smena.USERRIGHTS.USER_KLAD;
                }
            }
            return (ret);
        }

        // проверка склада
        private void tDefKSKL_Validating(object sender, CancelEventArgs e)
        {
            int 
                nS = 0;
            if (tDefKSKL.Text.Trim().Length > 0)
            {
                try
                {
                    nS = int.Parse(tDefKSKL.Text);
                    NSI.RezSrch zS = xNSI.GetNameSPR(NSI.NS_POST, new object[] { nS }, "NAME");
                    tDefNameSkl.Text = zS.sName;
                    e.Cancel = ! zS.bFind;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
            {
                //DefDoc.nDefSklad = AppC.EMPTY_INT;
            }
            if ((true == e.Cancel) || (nS <= 0))
                tDefNameSkl.Text = "";
        }

        // проверка даты
        private void tDefDateDoc_Validating(object sender, CancelEventArgs e)
        {
            string sD = tDefDateDoc.Text.Trim();
            if (sD.Length > 0)
            {
                try
                {
                    sD = Srv.SimpleDateTime(sD);
                    //DateTime d = DateTime.ParseExact(sD, "dd.MM.yy", null);
                    //DefDoc.dDefDate = d;
                    ((TextBox)sender).Text = sD;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        // проверка смены
        private void tDefSmena_Validating(object sender, CancelEventArgs e)
        {
            string sSm = ((TextBox)sender).Text.Trim();
            if (sSm.Length > 0)
            {
                try
                {
                    int nS = int.Parse(sSm);
                    if (((nS >= 1) && (nS <= xPars.Smennost)) ||
                    (xSm.sUser == AppC.SUSER))
                    {
                    }
                    else
                        e.Cancel = true;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }





        // попытка начала смены
        private AppC.VerRet Try2Start()
        {
            AppC.VerRet 
                v;
            v.nRet = AppC.RC_CANCEL;
            v.cWhereFocus = null;
            bool bCanCont = false;

            if (xSm.sUser.Length > 0)
            {
                if ((xSm.sUser != AppC.SUSER) && (xSm.sUser != AppC.GUEST))
                {
                    bCanCont = ValidUser();
                }
                else
                {
                    bCanCont = true;
                    xSm.urCur = (xSm.sUser == AppC.SUSER) ? Smena.USERRIGHTS.USER_SUPER : Smena.USERRIGHTS.USER_KLAD;
                }
            }
            if (bCanCont == true)
            {
                v.nRet = AppC.RC_OK;
            }
            return (v);
        }
/*
        private void Try2BegSmena()
        {
            xSm.dBeg = DateTime.Now;
            xSm.nStatus = REG_INPROC;
            xSm.WriteCurSm(xSm, xPars.sDataPath);
        }
*/
        private void Avtor_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyValue)
            {
                case W32.VK_ENTER:
                    //nCurEditCommand = AppC.CC_NEXTOVER;
                    //panel1.Focus();
                    //if (bMayQuit == true)
                    //    QuitAvtor(DialogResult.OK);
                    e.Handled = true;

                    if (aEdVvod.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                        QuitAvtor(DialogResult.OK);
                    break;
                case W32.VK_ESC:
                    QuitAvtor(DialogResult.Abort);
                    break;
                case W32.VK_UP:
                    //nCurEditCommand = AppC.CC_PREV;
                    //panel1.Focus();
                    e.Handled = true;
                    aEdVvod.TryNext(AppC.CC_PREV);
                    break;
                case W32.VK_DOWN:
                    //nCurEditCommand = AppC.CC_NEXT;
                    //panel1.Focus();
                    e.Handled = true;
                    aEdVvod.TryNext(AppC.CC_NEXT);
                    break;
                case W32.VK_TAB:
                    //if (e.Shift == true)
                    //    nCurEditCommand = AppC.CC_PREV;
                    //else
                    //    nCurEditCommand = AppC.CC_NEXT;
                    //break;
                    e.Handled = true;
                    aEdVvod.TryNext((e.Shift == true)?AppC.CC_PREV:AppC.CC_NEXT);
                    break;
            }
            bSkipKey = e.Handled;
        }
        
        private void Avtor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (bSkipKey == true)
            {
                bSkipKey = false;
                e.Handled = true;
            }
        }

        // создание массива управления редактированием полей
        private void CreateEdArrDet(int nReg)
        {
            Control 
                xC = null;
            aEdVvod = new AppC.EditListC(Try2Start);

            switch (nReg)
            {
                case AppC.AVT_LOGON:            // начало новой смены
                    aEdVvod.AddC(tUser);
                    aEdVvod.AddC(tPass);
                    aEdVvod.AddC(tDefKSKL, !(xNSI.DT[NSI.NS_POST].nState == NSI.DT_STATE_INIT));    // склады еще не грузили ?
                    aEdVvod.AddC(tDefDateDoc);
                    aEdVvod.AddC(tDefSmena);
                    xC = tUser;
                    break;
                case AppC.AVT_LOGOFF:
                    break;
            }

            // фокус - на первый из доступных
            aEdVvod.SetCur(xC);
        }

        // выход из формы
        private void QuitAvtor(DialogResult ret)
        {
            this.DialogResult = ret;
            this.Close();
        }

        // авторизация через сканер
        public void SetUserID(string sID)
        {
            tUser.Text = int.Parse(sID).ToString();
            //ServClass.TryEditNextFiled((Control)tUser, AppC.CC_NEXT, aEdVvod);
            aEdVvod.TryNext(AppC.CC_NEXT);
        }


        #region Not_Used
        /*
        private Thread thReadNSI;               // догрузка справочников ()
        private static NSI nnSS;
        private static void ReadInThread()
        {
            nnSS.LoadLocNSI(new int[] { NSI.NS_USER, NSI.NS_SKLAD, NSI.NS_SUSK, NSI.I_SMEN }, 0);
        }

* 
 * 
 */
        #endregion

    }
}