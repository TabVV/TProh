using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;

using ScannerAll;
using PDA.OS;
using PDA.Service;

namespace Proh
{
    public partial class MainF : Form
    {
        //public event EventParsHandler ParsEvents;

        public delegate void EditParsOver(int nRetCode, int nFunc);


        // текущие параметры (при редактировании)
        private DocPars xDP;

        // текущая функция 
        private int nCurFunc;

        // текущая таблица переходов для редактирования
        private AppC.EditListC 
            aEdVvod;
        // текущая команда перехода в таблице переходов
        //private int nCurEditCommand = -1;

        // значения типа документа до редактирования
        private int nTypDOld;

        // флаг работы с параметрами
        private bool bWorkWithDocPars = false;

        // флаг завершения ввода
        private bool bQuitEdPars = false;
        EditParsOver dgOver;


        // предыдущий обработчик клавиатуры
        CurrFuncKeyHandler oldKeyH;



        // с какого поля начать: первого доступного или первого пустого
        public enum CTRL1ST : int
        {
            START_AVAIL = 1,
            START_EMPTY = 2,
            START_LAST  = 3,
            START_EX1 = -1,
            START_EX2 = -2,
            START_EX3 = -3,
            START_EX4 = -4,
            START_EX5 = -5,
            START_EX6 = -6,
            START_EX7 = -7,
            START_EX8 = -8,
            START_EX9 = -9,
            START_EX10 = -10
        }


        // вход в режим ввода/корректировки параметров
        public void EditPars(int nReg, DocPars x, CTRL1ST FirstEd, AppC.VerifyEditFields dgVer, EditParsOver dgEnd)
        {
            xDP = x;
            if (x != null)
            {
                bQuitEdPars = false;
                nCurFunc = nReg;
                bWorkWithDocPars = true;
                //ServClass.dgVerEd = new VerifyEditFields(dgVer);
                dgOver = new EditParsOver(dgEnd);
                SetParFields(xDP);
                //EnableDocF(x.nTypD, 1);
                SetEditMode(true);

                CreateEdArrPar(FirstEd, dgVer);

                oldKeyH = ehCurrFunc;
                ehCurrFunc = new CurrFuncKeyHandler(PPars_KeyDown);

//                WhereFocus(FirstEd);

            }
        }

        // сброс/установка полей ввода/вывода
        private void SetParFields(DocPars xDP)
        {
            int 
                n = xDP.nTypD;

            DocPars.tKTyp.Text = (n == AppC.EMPTY_INT) ? "" : xDP.nTypD.ToString();
            DocPars.tNTyp.Text = DocPars.TypName(ref n);
            xDP.sTypD = DocPars.tNTyp.Text;

            //tSm_p.Text = xDP.sSmena;
            tDateD_p.Text = DateTime.Now.ToString("dd.MM.yy");
            if (xDP.dDatDoc != DateTime.MinValue)
            {
                tDateD_p.Text = xDP.dDatDoc.ToString("dd.MM.yy");
            }
            tKPost_p.Text = "";
            tNPost_p.Text = "";
            if (xDP.nPost != AppC.EMPTY_INT)
            {
                tKPost_p.Text = xDP.nPost.ToString();
                tNPost_p.Text = xDP.nPost.ToString();

            }
            
            tNom_p.Text = xDP.sNomDoc;

        }

        // куда поставить фокус ввода на панели
        private Control Where1Empty(AppC.EditListC aEdVvod)
        {
            int i;
            Control cRet = null;
            for (i = 0; i < aEdVvod.Count; i++)
            {
                if (aEdVvod[i].Enabled)
                {
                    if (aEdVvod[i].Text.Length == 0)
                    {
                        cRet = aEdVvod[i];
                        break;
                    }
                }
            }

            return (cRet);
        }

        // завершение режима ввода/корректировки параметров
        public void EndEditPars(int nKey)
        {
            int nRet = (nKey == W32.VK_ENTER) ? AppC.RC_OK : AppC.RC_CANCEL;
            ehCurrFunc -= PPars_KeyDown;
            ehCurrFunc = oldKeyH;

            for (int i = 0; i < aEdVvod.Count; i++)
            {
                aEdVvod[i].Enabled = false;
            }

            bWorkWithDocPars = false;
            SetEditMode(false);
            dgOver(nRet, nCurFunc);
        }

        // проверка даты
        private void tDateD_pValidating(object sender, CancelEventArgs e)
        {
            string sD = ((TextBox)sender).Text.Trim();
            if (sD.Length > 0)
            {
                try
                {
                    sD = Srv.SimpleDateTime(sD);
                    DateTime d = DateTime.ParseExact(sD, "dd.MM.yy", null);
                    xDP.dDatDoc = d;
                    ((TextBox)sender).Text = sD;
                }
                catch
                {
                    e.Cancel = true;
                }
            }
            else
                xDP.dDatDoc = DateTime.MinValue;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }

        // сохранение предыдущего значения типа документа
        private void SaveOldTyp(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
            nTypDOld = xDP.nTypD;
        }

        // изменение типа, вывод нименования
        private void tKT_pTextChanged(object sender, EventArgs e)
        {
            if (bWorkWithDocPars == true)
            {// при просмотре не проверяется
                int nTD = AppC.EMPTY_INT;
                string s = "";

                try
                {
                    nTD = int.Parse(tKT_p.Text);
                }
                catch{}

                s = DocPars.TypName(ref nTD);
                xDP.nTypD = nTD;

                tKT_p.SelectAll();  // если вводим не более 1 символа
                tNT_p.Text = s;
            }
        }



        // проверка типа
        private void tKT_pValidating(object sender, CancelEventArgs e)
        {
            if (xDP.nTypD == AppC.EMPTY_INT)
            {
                //tKT_p.Text = "";
                //tNT_p.Text = "";
                if (tKT_p.Text.Trim().Length > 0)
                    e.Cancel = true;
                //ServClass.TBColor((TextBox)sender, true);
            }
            else
            {
                if (xDP.nTypD != nTypDOld)
                {// сменился тип документа
                    //EnableDocF(nTypDOld, false);
                    //EnableDocF(xDP.nTypD, true);

                    //Control cT = ((Control)(sender));
                    //cT.Parent.SelectNextControl(cT, true, true, false, true);

                    
                }
            }

        }

        // тип документа все-таки сменился
        private void tKT_pValidated(object sender, EventArgs e)
        {
            int i;
            if (xDP.nTypD != nTypDOld)
            {
                if (xDP.nTypD == AppC.EMPTY_INT)
                {
                    tKT_p.Text = "";
                    tNT_p.Text = "";
                    for (i = 0; i < aEdVvod.Count; i++ )
                        aEdVvod[i].Enabled = true;
                }
                else
                {
                    bool bNomEn = true, bPolEn = true;
                    SetTypSensitive(xDP.nTypD, ref bPolEn, ref bNomEn);
                    tKPost_p.Enabled = bPolEn;
                    if (!bPolEn)
                        tKPost_p.Text = "";

                    tNom_p.Enabled = bNomEn;
                    if (!bNomEn)
                        tNom_p.Text = "";
                }
                //EnableDocF(xDP.nTypD, 2);
            }
            //ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }




        // проверка номера документа
        private void tNom_pValidating(object sender, CancelEventArgs e)
        {
            string sT = ((TextBox)sender).Text.Trim();
            xDP.sNomDoc = sT;
            //if (e.Cancel != true)
            //    e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
        }


        // обработка функций и клавиш на панели
        private bool PPars_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool 
                ret = true;

            if (nFunc > 0){}
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ESC:
                        //nCurEditCommand = AppC.CC_CANCEL;
                        EndEditPars(e.KeyValue);
                        break;
                    case W32.VK_UP:
                        //nCurEditCommand = AppC.CC_PREV;
                        //tFiction.Focus();
                        aEdVvod.TryNext(AppC.CC_PREV);
                        break;
                    case W32.VK_DOWN:
                        //nCurEditCommand = AppC.CC_NEXT;
                        //tFiction.Focus();
                        aEdVvod.TryNext(AppC.CC_NEXT);
                        break;
                    case W32.VK_ENTER:
                        bSkipChar = true;
                        //nCurEditCommand = AppC.CC_NEXTOVER;
                        //tFiction.Focus();
                        //if (bQuitEdPars == true)
                        //    EndEditPars(e.KeyValue);

                        if (aEdVvod.TryNext(AppC.CC_NEXTOVER) == AppC.RC_CANCELB)
                            EndEditPars(e.KeyValue);

                        break;
                    case W32.VK_TAB:
                        //nCurEditCommand = (e.Shift == true) ? AppC.CC_PREV : AppC.CC_NEXT;
                        aEdVvod.TryNext((e.Shift == true) ? AppC.CC_PREV : AppC.CC_NEXT);
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);
        }

        // создание массива управления редактированием полей
        private void CreateEdArrPar(CTRL1ST FirstEd, AppC.VerifyEditFields dgV)
        {
            int i;
            bool
                bPost = true,
                bNomEn = true;

            aEdVvod = new AppC.EditListC(dgV);

            aEdVvod.AddC(DocPars.tKTyp);
            switch (nCurFunc)
            {
                case PDA.Service.AppC.F_LOAD_DOC:
                    aEdVvod.AddC(DocPars.tDate, false);
                    break;
                default:
                    aEdVvod.AddC(DocPars.tDate);
                    break;
            }

            SetTypSensitive(xDP.nTypD, ref bPost, ref bNomEn);

            aEdVvod.AddC(DocPars.tKPost, bPost);
            aEdVvod.AddC(DocPars.tNDoc, bNomEn);

            // по умолчанию - с первого доступного
            Control xC = null, 
                xEnbF = null,
                xEnbL = null;

            // установка доступных
            for (i = 0; i < aEdVvod.Count; i++)
            {
                if (aEdVvod[i].Enabled)
                {
                    aEdVvod[i].Enabled = true;
                    if (xEnbF == null)
                        xEnbF = aEdVvod[i];
                    xEnbL = aEdVvod[i];
                }
                else
                    aEdVvod[i].Enabled = false;
            }

            if (FirstEd == CTRL1ST.START_EMPTY)
                xC = Where1Empty(aEdVvod);
            else if (FirstEd == CTRL1ST.START_AVAIL)
                xC = xEnbF;
            else if (FirstEd == CTRL1ST.START_LAST)
                xC = xEnbL;
            else if ((int)FirstEd < 0 )
            {
                i = Math.Abs( (int)FirstEd );
                xC = aEdVvod[i - 1];
            }

            aEdVvod.SetCur(xC);

            //if (xC != null)
            //    xC.Focus();             // хотя бы один должен быть доступен
            //else
            //    xEnbF.Focus();
        }

        private void SetTypSensitive(int nT, ref bool bP, ref bool bN)
        {
            switch (nT)
            {
                case AppC.TYPD_INV:
                    bP = false;
                    bN = true;
                    break;
            }
        }

    }
}
