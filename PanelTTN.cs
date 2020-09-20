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
        // при активации панели ТТН
        private void EnterInTTN()
        {
            dgTTN.Focus();
            if (dgTTN.CurrentRowIndex >= 0)
                dgTTN.CurrentCell = new DataGridCell(dgTTN.CurrentRowIndex, 3);
        }

        // обработка функций и клавиш на панели
        private bool TTN_KeyDown(int nFunc, KeyEventArgs e)
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
                        tcMain.SelectedIndex = PG_DIR;
                        ret = true;
                        break;
                }
            }
            e.Handled |= ret;
            return (ret);
        }







    }
}
