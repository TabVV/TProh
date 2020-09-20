using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;

using PDA.OS;
using PDA.Service;

namespace Proh
{
    public partial class MainF : Form
    {
        // переход на вкладку Сервис
        private void EnterInPars()
        {
            if (xSm.urCur > Smena.USERRIGHTS.USER_KLAD)
            {
                tcPars.Enabled = true;
                tSrvParServer.Focus();
            }
        }

        // обработка функций и клавиш на панели
        private bool AppPars_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;
            int nR;

            if (nFunc > 0)
            {
                if (bEditMode == false)
                {
                }
                if (nFunc == AppC.F_UPLD_DOC)
                {// сохранение параметров
                    nR = AppPars.SavePars(xPars);
                    if (AppC.RC_OK == nR)
                    {
                        ServClass.PlayMelody(W32.MB_2PROBK_QUESTION);
                        MessageBox.Show("Параметры сохранены", "Сохранение");
                    }
                    else
                    {
                        ServClass.PlayMelody(W32.MB_3GONG_EXCLAM);
                        MessageBox.Show("Ошибка сохранения!", "Сохранение");
                    }
                    ret = true;
                }
            }
            else
            {
                if (bEditMode == false)
                {// только в режиме просмотра
                    switch (e.KeyValue)
                    {
                        case W32.VK_ENTER:
                            Control xC = ServClass.GetPageControl(tpParPaths, 1, null);
                            ret = true;
                            break;
                    }
                }
            }
            e.Handled |= ret;
            return (ret);
        }


        private void SetBindAppPars()
        {
            Binding bi;

            bi = new Binding("Text", xPars, "sAppStore");
            tSrvAppPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sNSIPath");
            tNsiPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sDataPath");
            tDataPath.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sHostSrv");
            tSrvParServer.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "nSrvPort");
            tSrvParServPort.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "nSrvPortM");
            tSrvParServPortM.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bWaitSock");
            cbWaitSock.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "sHostSrvSh");
            tServSh.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "nSrvPortSh");
            tPortSh.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "Smennost");
            tSmennost.DataBindings.Add(bi);

            // параметры ввода
            bi = new Binding("Checked", xPars, "bOpenSrv");
            chbOpenSrv.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "bCloseSrv");
            chbCloseSrv.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "b2DFull");
            chb2DFull.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "LoginSh");
            tLogin.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "PassSh");
            tPass.DataBindings.Add(bi);

            bi = new Binding("Text", xPars, "DevID");
            tIDSh.DataBindings.Add(bi);

            bi = new Binding("Checked", xPars, "CheckPermitOnOut");
            chCheckPrmOut.DataBindings.Add(bi);
        }


    }
}
