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

namespace Proh
{
    public partial class MainF : Form
    {


        // при активации панели сервиса
        private void EnterInServ()
        {
            btQuit.Focus();
        }
       

        private void btMem_Click(object sender, EventArgs e)
        {
            ServClass.MemInfo();
        }

        // кнопка Выход
        private void btQuit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }


        // обработка функций и клавиш на панели
        private bool Service_KeyDown(int nFunc, KeyEventArgs e)
        {
            bool ret = false;

            if (nFunc > 0)
            {
            }
            else
            {
                switch (e.KeyValue)
                {
                    case W32.VK_ENTER:
                        Control xC = ServClass.GetPageControl(tpService, 1, null);
                        if (xC == btQuit)
                            W32.SimulMouseClick(btQuit.Left + 3, btQuit.Top + 3, this);
                        ret = true;
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);
        }







    }
}
