using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

using ScannerAll;
using PDA.OS;
using ExprDll;
using SavuSocket;
//using KBWait;

namespace Proh
{
    public partial class MainF : Form
    {

        public MainF(BarcodeScanner xSc, Rectangle rtScr)
        {
            InitializeComponent();

            // WM InitializeDop(xSc, new Size(48, 22), new Point(190, 1));
            InitializeDop(xSc, new Size(52, 22), new Point(179, 1));
        }

    }
}