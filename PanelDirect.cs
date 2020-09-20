using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Threading;

using PDA.OS;
using PDA.Service;
using FRACT = System.Decimal;

namespace Proh
{
    public partial class MainF : Form
    {
        //private class LockControl
        //{
        //    private Control xOpen = null;
        //    private Control xClose = null;

        //    public void MayOpen()
        //    {
        //    }
        //}

        private class GRuzType
        {
            private int m_Kod;
            private string m_Name;

            public GRuzType(int nK, string sN)
            {
                m_Kod = nK;
                m_Name = sN;
            }

            // Тип груза
            public int GruzID
            {
                get { return m_Kod; }
                set { m_Kod = value; }
            }

            // Наименование типа
            public string GruzName
            {
                get { return m_Name; }
                set { m_Name = value; }
            }

            public override string ToString()
            {
                return GruzName;
            }
        }

        private AvtoCtrl xAvt;
        private int nDirMove = AppC.DIR_OUT;
        private Control xCurEdit = null;

        // переход на вкладку Сервис
        private void EnterInDirect()
        {
            if (xAvt == null)
            {
                xAvt = new AvtoCtrl(AppC.DIR_OUT);
                SetBindAvtPars();
            }
            if (bEditMode == true)
            {// возвращаемся на вкладку
                if (xCurEdit != null)
                    xCurEdit.Focus();

            }
        }

        private void SetBindAvtPars()
        {
            Binding bi;

            BindingList<GRuzType> blGT = new BindingList<GRuzType>();
            blGT.Add(new GRuzType(AppC.GRUZ_EMPTY, "  ПУСТОЙ"));
            blGT.Add(new GRuzType(AppC.GRUZ_MAIN, "    ГРУЗ "));
            blGT.AllowEdit = false;
            blGT.AllowNew = false;
            blGT.AllowRemove = false;

            cbGruzType.BeginUpdate();
            cbGruzType.Items.Clear();
            cbGruzType.DataSource = new BindingSource(blGT, string.Empty);
            cbGruzType.DisplayMember = "GruzName";
            cbGruzType.ValueMember = "GruzID";

            cbGruzType.EndUpdate();



            if (tPutList.DataBindings.Count == 0)
            {
                bi = new Binding("SelectedValue", xAvt, "GruzType");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                cbGruzType.DataBindings.Add(bi);

                bi = new Binding("SelectedValue", xAvt, "Purp");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                cbPurp.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Putev");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tPutList.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Avto");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tTyag.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Massa");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tMassa.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Mest");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tMestProd.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Summa");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tSumma.DataBindings.Add(bi);

                bi = new Binding("Text", xAvt, "Propusk");
                bi.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                tPropusk.DataBindings.Add(bi);
            }

        }

            
        //BindingSource bsCurSHLG;
        private void SetBindDevPars()
        {
            // вывод имени шлагбаума
            BindingSource bsCurSHLG = new BindingSource(xNSI.dsNSI, NSI.NS_SHLG);
            if ((xSm.nSklad != AppC.EMPTY_INT) && (xSm.nSklad > 0))
                bsCurSHLG.Filter = String.Format("KPT={0}", xSm.nSklad);
            tDevID.DataBindings.Add("Text", bsCurSHLG, "SNM");

            //return;

            // текущий тип груза
            cbPurp.BeginUpdate();
            cbPurp.Items.Clear();
            bsCurSHLG = new BindingSource(xNSI.dsNSI, NSI.NS_PURP);
            bsCurSHLG.AllowNew = false;
            //cbPurp.DataBindings.Add("SelectedValue", bsCurSHLG, "KCL");
            cbPurp.DataSource = bsCurSHLG;
            cbPurp.DisplayMember = "NAME";
            cbPurp.ValueMember = "KCL";
            //cbPurp.SelectedIndex = 0;
            cbPurp.EndUpdate();
            //cbPurp.DataSource = xNSI.DT[NSI.NS_PURP];

            // таблица автомобилей
            //bsCurSHLG = new BindingSource(xNSI.dsM, NSI.TBD_AVT);
            //tTyag.DataBindings.Add("Text", bsCurSHLG, "AVT");

        }


        // обработка функций и клавиш на панели
        private bool Direct_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = true;
            bool bMove;
            int i;
            string sD, sU, sP;
            BindingSource bs;

            if (nFunc > 0)
            {
                bs = (BindingSource)tDevID.DataBindings["Text"].DataSource;
                if (bEditMode == false)
                {// только в режиме просмотра
                    switch (nFunc)
                    {
                        case AppC.F_CHGZMK:
                            //bs = (BindingSource)tDevID.DataBindings["Text"].DataSource;
                            if (bs.Position + 1 < bs.Count)
                                bs.MoveNext();
                            else
                                bs.MoveFirst();
                            break;
                        default:
                            ret = false;
                            break;
                    }
                    if (ret)
                        return (ret);
                    else
                        ret = true;
                }
                else
                {
                    switch (nFunc)
                    {// только в режиме корректировки
                        case AppC.F_DEL_REC:
                            if (lbTTN.Focused == true)
                                DelFaktTTN(lbTTN.SelectedIndex);
                            break;
                        case AppC.F_UPLD_DOC:
                            i = GetAvtInf(AppC.COM_ZOPEN);
                            if ((i == AppC.RC_OK) ||
                                (xAvt.DirMove == AppC.DIR_OUT && !xPars.CheckPermitOnOut))
                            {
                                //lWrite2Base.Text = SrvTime2Str(xAvt.SrvCTM);
                                lbDirect.Text = SrvTime2Str(xAvt.SrvCTM);
                                bQuitEdPars = true;
                                EndEditAvt();
                            }
                            break;
                        default:
                            ret = false;
                            break;
                    }
                    if (ret)
                        return (ret);
                    else
                        ret = true;
                }
                // в любом режиме
                DataRowView drv = (DataRowView)bs.Current;
                try
                {
                    sD = ((string)drv.Row["DEVID"]).Trim();
                }
                catch{ sD = ""; }
                try
                {
                    sU = ((string)drv.Row["USER"]).Trim();
                }
                catch{ sU = ""; }
                try
                {
                    sP = ((string)drv.Row["PASS"]).Trim();
                }
                catch{ sP = ""; }
                switch (nFunc)
                {
                    case AppC.F_OPENSH:
                        ChangeFunc(btOpenSh, false);
                        ShlgbMove(AppC.F_OPENSH, sD, sU, sP);
                        ChangeFunc(btOpenSh, true);
                        break;
                    case AppC.F_CLOSESH:
                        ChangeFunc(btCloseSh, false);
                        ShlgbMove(AppC.F_CLOSESH, sD, sU, sP);
                        ChangeFunc(btCloseSh, true);
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            else
            {// просто клавиша, функций нет
                if (bEditMode == false)
                {// пока просмотр
                    switch (e.KeyValue)
                    {
                        case W32.VK_ENTER:
                            BeginEditAvt(null, "");
                            break;
                        case W32.VK_LEFT:
                        case W32.VK_RIGHT:
                            nDirMove = (nDirMove == AppC.DIR_IN) ? AppC.DIR_OUT : AppC.DIR_IN;
                            ShowDirection(nDirMove);
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else
                {// редактируются поля
                    switch (e.KeyValue)
                    {
                        case W32.VK_ESC:
                            //nCurEditCommand = AppC.CC_CANCEL;
                            xAvt.ClearAvt(nDirMove, null);
                            EndEditAvt();
                            break;
                        case W32.VK_ENTER:
                            bSkipChar = true;
                            if (lbTTN.Focused == true)
                                tcMain.SelectedIndex = PG_TTN;
                            else
                                //ServClass.TryEditNextFiled(null, AppC.CC_NEXT, aEdVvod);
                                aEdVvod.TryNext(AppC.CC_NEXT);
                            break;
                        case W32.VK_UP:
                            bMove = true;
                            if (lbTTN.Focused == true)
                            {
                                if (lbTTN.SelectedIndex > 0)
                                {
                                    bMove = false;
                                    lbTTN.SelectedIndex -= 1;
                                }
                            }
                            if (bMove == true)
                                //ServClass.TryEditNextFiled(null, AppC.CC_PREV, aEdVvod);
                                aEdVvod.TryNext(AppC.CC_PREV);
                            break;
                        case W32.VK_DOWN:
                            bMove = true;
                            if (lbTTN.Focused == true)
                            {
                                if (lbTTN.SelectedIndex < lbTTN.Items.Count - 1)
                                {
                                    bMove = false;
                                    lbTTN.SelectedIndex++;
                                }
                            }
                            if (bMove == true)
                                //ServClass.TryEditNextFiled(null, AppC.CC_NEXT, aEdVvod);
                                aEdVvod.TryNext(AppC.CC_NEXT);
                            break;
                        case W32.VK_LEFT:
                        case W32.VK_RIGHT:
                            if (cbGruzType.Focused == true)
                            {
                                i = cbGruzType.SelectedIndex + 1;
                                cbGruzType.SelectedIndex = (i < cbGruzType.Items.Count) ? i : 0;
                            }
                            else if (cbPurp.Focused == true)
                            {
                                //bs = (BindingSource)cbPurp.DataBindings[0].DataSource;
                                bs = (BindingSource)cbPurp.DataSource;
                                if (bs.Position + 1 < bs.Count)
                                    bs.MoveNext();
                                else
                                    bs.MoveFirst();
                            }
                            else
                            {
                                nDirMove = (nDirMove == AppC.DIR_IN) ? AppC.DIR_OUT : AppC.DIR_IN;
                                xAvt.DirMove = nDirMove;
                                ShowDirection(nDirMove);
                            }

                            break;
                        case W32.VK_DEL:
                            if (lbTTN.Focused == true)
                                DelFaktTTN(lbTTN.SelectedIndex);
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
            }
            e.Handled |= ret;
            return (ret);
        }

        // смена направления въезд/выезд
        private void ShowDirection(int nD)
        {
            lbDirect.SuspendLayout();
            if (nD == AppC.DIR_OUT)
            {
                lbDirect.BackColor = System.Drawing.Color.LightSkyBlue;
                //lbDirect.Text = "<= ВЫЕЗД =>";
                lbDirect.Text = "<= УБЫЛ =>";
            }
            else
            {
                lbDirect.BackColor = System.Drawing.Color.LightGray;
                //lbDirect.Text = "-> ВЪЕЗД <-";
                lbDirect.Text = "-> ПРИБЫЛ <-";
            }
            lbDirect.ResumeLayout();
        }

        // фильтр по цели
        public void FiltForPurpt(int Direct, int GType)
        {
            ((BindingSource)cbPurp.DataSource).Filter = String.Format("KPT={0}", xSm.nSklad);
        }

        public void BeginEditAvt(Control xFoc, string sInit)
        {
            bool bByScan = (xFoc == null) ? false : true;

            xAvt.ClearAvt(nDirMove, xFoc);
            lbTTN.Items.Clear();
            lTTNF.Text = "ТТН    ФАКТ 0";
            lEBD.Text = "           ЭБД    0";
            xNSI.DT[NSI.TBD_TTN].dt.Rows.Clear();
            xNSI.DT[NSI.TBD_DOC].dt.Rows.Clear();
            //xAvt.bCanOpen = true;
            //lWrite2Base.Text = "Запись (F2)";
            ShowDirection(xAvt.DirMove);

            if (bByScan == false)
            {// вход по клавише
                // тип груза по умолчанию
                if (xAvt.DirMove == AppC.DIR_IN)
                    xAvt.GruzType = AppC.GRUZ_EMPTY;
                else
                    xAvt.GruzType = AppC.GRUZ_MAIN;
            }
            else
            {// вход по скану
                // тип груза пока неизвестен
                //cbGruzType.SelectedIndex = -1;
                cbPurp.SelectedIndex = -1;
                xAvt.GruzType = 0;
            }

            TestOpenAvail(xAvt.GruzType, null, "");
/*
            tTyag.Text = "";
            tPutList.Text = "";
            tMassa.Text = "";
            tMestProd.Text = "";
            tSumma.Text = "";
            tTimeIO.Text = "";
            tTTN.Text = "";
*/
            lbTTN.Items.Clear();

            bQuitEdPars = false;

            // выход из редактирования - после выполнения команды "Открыть"
            //ServClass.dgVerEd = new VerifyEditFields(VerifyAvt);
            AvtForEdit(xAvt.GruzType, tPutList);

            ServClass.RefreshAvtBind(aEdVvod);
            tPropusk.DataBindings[0].ReadValue();
            SetEditMode(true);

            // какой активный?
            if (xFoc == null)
                xFoc = tPutList;
/*
            if (sInit != "")
                xFoc.Text = sInit;
 */ 
            xFoc.Focus();
            xFoc.DataBindings[0].ReadValue();
            if (bByScan == true)
                //ServClass.TryEditNextFiled(xFoc, AppC.CC_NEXT, aEdVvod);
                aEdVvod.TryNext(AppC.CC_NEXT);
        }

        public void AvtForEdit(int GType)
        {
            AvtForEdit(GType, null);
        }


        // установка доступных полей
        public void AvtForEdit(int GType, Control xC)
        {
            bool 
                bEnMassa,
                bOldMode = bEditMode;
            Control xOld = null;

            if (bEditMode == true)
            {// переустановка доступных
                //xOld = ServClass.FindCurInEdit(aEdVvod);
                xOld = aEdVvod.Current;
            }

            bEditMode = false;

            bEnMassa = (GType == AppC.GRUZ_MAIN) ? true : false;

            if (xC != null)
            {// начало редактирования
                aEdVvod = new AppC.EditListC(VerifyAvt);
                aEdVvod.AddC(tPutList);

                aEdVvod.AddC(cbGruzType);
                aEdVvod.AddC(cbPurp);

                aEdVvod.AddC(tTyag);
                aEdVvod.AddC(tMassa, bEnMassa);
                aEdVvod.AddC(tSumma, bEnMassa);
                aEdVvod.AddC(tMestProd, bEnMassa);
                aEdVvod.AddC(lbTTN, bEnMassa);
                aEdVvod.SetCur(xC);
            }
            else
            {
                tPutList.Enabled = true;
                cbGruzType.Enabled = true;
                cbPurp.Enabled = true;

                tTyag.Enabled = true;
                tMassa.Enabled = bEnMassa;
                tSumma.Enabled = bEnMassa;
                tMestProd.Enabled = bEnMassa;
                lbTTN.Enabled = bEnMassa;
            }
            bEditMode = bOldMode;

            //if ((bEditMode == true) && (xOld != null))
            //{// переустановка доступных
            //    if (xOld.Enabled == true)
            //    {
            //        if (xOld.Focused == false)
            //            xOld.Focus();
            //    }
            //    //else
            //        //ServClass.TryEditNextFiled(xOld, AppC.CC_NEXT, aEdVvod);
            //}
        }

        // установка признаков окончания редактирования
        private AppC.VerRet VerifyAvt()
        {
            AppC.VerRet 
                v;
            v.nRet = AppC.RC_CANCEL;
            v.cWhereFocus = null;
            return (v);
        }

        public void EndEditAvt()
        {
            SetEditMode(false);
            foreach (Control EE in aEdVvod)
                EE.Enabled = false;

            tpInOut.Focus();
        }

        // установка доступности пунктов Открыть/Закрыть
        private bool TestOpenAvail(int GType, object xSender, string sV)
        {
            bool bCanOpen = true;
            if (GType == AppC.GRUZ_EMPTY)
            {// для пустых
            }
            else
            {// для груженых
            }
            xAvt.bCanOpen = bCanOpen;
            //ChangeFunc(btOpenSh, bCanOpen);
            return (btOpenSh.Enabled);
        }


        // установка доступности пунктов Открыть/Закрыть
        private void ChangeFunc(Control xButt, bool bMayOpen)
        {
            xButt.SuspendLayout();
/*
            if (bMayOpen == true)
            {
                xButt.ForeColor = Color.White;
                xButt.BackColor = (xButt == btOpenSh) ? Color.ForestGreen : Color.Maroon;
            }
            else
            {
                xButt.ForeColor = Color.LightBlue;
                xButt.BackColor = Color.SteelBlue;
            }
 */
            xButt.Enabled = bMayOpen;
            xButt.ResumeLayout();
        }


        // проверка типа груза
        private void cbGruzType_Validated(object sender, EventArgs e)
        {
            if (bEditMode == true)
            {
                AvtForEdit(xAvt.GruzType);
                //TestOpenAvail(xAvt.GruzType, sender, "");
                //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
            }
            // перед уходом
            xCurEdit = (Control)sender;
        }

        // проверка цели въезда/выезда
        private void cbPurp_Validated(object sender, EventArgs e)
        {
            if (bEditMode == true)
            {
                //AvtForEdit(xAvt.GruzType);
                //TestOpenAvail(xAvt.GruzType, sender, "");
                //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
            }
            // перед уходом
            xCurEdit = (Control)sender;
        }


        // проверка поля
        private void AvtFieldValidating(object sender, CancelEventArgs e)
        {
            if (bEditMode == true)
            {
                TextBox xF = ((TextBox)sender);
                string sT = xF.Text.Trim().ToUpper();
                if (sT.Length > 0)
                {
                    if (xF == tPutList)
                    {
                    }
                    if (xF == tTyag)
                    {
                        if (sT.Length < 4)
                        {
                            ServClass.ErrorMsg("Не меньше 4 цифр!");
                            e.Cancel = true;
                        }
                    }

                }

                if (e.Cancel != true)
                {
                    //((TextBox)sender).Text = sT;
                    //TestOpenAvail(cbGruzType.SelectedIndex, sender, sT);
                    //e.Cancel = !ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
                }
                // перед уходом
                if (e.Cancel == false)
                    xCurEdit = (Control)sender;
            }
        }
        private void AvtValidated(object sender, EventArgs e)
        {
            if (bEditMode == true)
            {
                TextBox xF = ((TextBox)sender);

                string sT = xF.Text.Trim().ToUpper();
                if (sT.Length > 0)
                {
                    if (xF == tPutList)
                    {
                        if (xAvt.PLWasLoad != sT)
                        {
                            xAvt.PLWasLoad = sT;
                            if (GetAvtInf(AppC.COM_ZPL) == AppC.RC_OK)
                            {// могли измениться атрибуты => доступные поля
                                SetAvtFields();
                                AvtForEdit(xAvt.GruzType);
                                if ( xAvt.GruzType == AppC.GRUZ_MAIN)
                                    //ServClass.TryEditNextFiled(tPutList, AppC.CC_PREV, aEdVvod);
                                    aEdVvod.TryNext(AppC.CC_PREV);
                            }
                            else
                                xF.Focus();
                        }
                        //ServClass.TryEditNextFiled((Control)sender, nCurEditCommand, aEdVvod);
                    }
                }
            }

        }

        // разбор и установка параметров с сервера
        public void SetAvtFields()
        {
            int i;
            FRACT f;
            string s;

            Dictionary<string, string> aP = SrvCommParse(xAvt.SrvAns, new char[] { ',' });

            try{ i = int.Parse(aP["IO"]); }
            catch { i = 0; }
            if (i > 0)
            {
                //nDirMove = i;
                //xAvt.DirMove = i;
                //ShowDirection(i);
            }

            s = aP["PTVL"].Trim();
            if (s.Length > 0)
                xAvt.Putev = s;

            try { i = int.Parse(aP["TG"]); }
            catch { i = 0; }
            if (i > 0)
                xAvt.GruzType = i;

            try { i = int.Parse(aP["CEL"]); }
            catch { i = 0; }
            if (i > 0)
                xAvt.Purp = i;

            s = aP["AVT"].Trim();
            if (s.Length > 0)
                xAvt.Avto = s;

            try { f = FRACT.Parse(aP["MASSA"]); }
            catch { f = 0.0M; }
            if (f > 0)
                xAvt.Massa = f;

            try { f = FRACT.Parse(aP["SUMMA"]); }
            catch { f = 0.0M; }
            if (f > 0)
                xAvt.Summa = f;

            try { i = int.Parse(aP["MEST"]); }
            catch { i = 0; }
            if (i > 0)
                xAvt.Mest = i;

            if (xAvt.DirMove == AppC.DIR_OUT)
            {
                try { s = aP["PRPSK"]; }
                catch { s = ""; }
                
                //if (s.Length > 0)
                //    xAvt.Propusk = s;
            }

            //SrvTime2Str(xAvt.SrvCTM);

            // количество ТТН по базе
            lEBD.Text = "           ЭБД  " + xAvt.KolTTN_EBD.ToString();
            ServClass.RefreshAvtBind(aEdVvod);
            tPropusk.DataBindings[0].ReadValue();
        }

        private string SrvTime2Str(string sT)
        {
            string ret = "";
            DateTime d;
            
            try { d = DateTime.ParseExact(sT, "HH:mm:ss", null); }
            catch { d = DateTime.MinValue; }
            if (d > DateTime.MinValue)
            {
                xAvt.TimeOut = d;
                ret = d.ToString("HH:mm");
                //lWrite2Base.Text = ret;
            }

            return (ret);
        }


        private void NewTTNScanned(string sNom, int nTypDoc)
        {
            DataView dv;
            Color newCol, old;

            dv = new DataView(xNSI.DT[NSI.TBD_TTN].dt,
                String.Format("NTTN={0}", sNom), "", DataViewRowState.CurrentRows);
            if (dv.Count > 0)
            {
                dv[0].Row["CTRL"] = 1;
                newCol = System.Drawing.Color.LightGreen;
            }
            else
                newCol = System.Drawing.Color.Red;

            if (tcMain.SelectedIndex == PG_DIR)
            {
                old = lbTTN.BackColor;
                lbTTN.SuspendLayout();
                lbTTN.BackColor = newCol;
                lbTTN.ResumeLayout();
                lbTTN.Refresh();
                Thread.Sleep(300);
                lbTTN.SuspendLayout();
                lbTTN.BackColor = old;
                lbTTN.ResumeLayout();
                lbTTN.Refresh();
            }

            if (!lbTTN.Items.Contains(sNom))
            {
                lbTTN.Items.Add(sNom);
                xAvt.KolTTN += 1;
                lTTNF.Text = "ТТН    ФАКТ " + lbTTN.Items.Count.ToString();
            }
        }


        private void DelFaktTTN(int iD)
        {
            if (iD >= 0)
            {
                DataView dv;
                string sNom = (string)lbTTN.SelectedItem;

                dv = new DataView(xNSI.DT[NSI.TBD_TTN].dt,
                    String.Format("NTTN={0}", sNom), "", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                    dv[0].Row["CTRL"] = 0;

                lbTTN.Items.RemoveAt(iD);
                xAvt.KolTTN--;
                lTTNF.Text = "ТТН    ФАКТ " + lbTTN.Items.Count.ToString();
            }
        }

        private void tPropusk_TextChanged(object sender, EventArgs e)
        {
            int nL = ((TextBox)sender).Text.Length;
            if (nL > 0)
            {
                lPropusk.BackColor = Color.LightSkyBlue;
                lPropusk.ForeColor = Color.Black;

                tPropusk.BackColor = Color.White;
                tPropusk.ForeColor = Color.Black;
            }
            else
            {// пропуск не виден
                lPropusk.BackColor = Color.SteelBlue;
                lPropusk.ForeColor = Color.RoyalBlue;

                tPropusk.BackColor = Color.SteelBlue;
                tPropusk.ForeColor = Color.RoyalBlue;
            }
            tPropusk.DataBindings[0].WriteValue();
        }


    }
}
