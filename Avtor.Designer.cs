namespace Proh
{
    partial class Avtor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lUser = new System.Windows.Forms.Label();
            this.lPass = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tDefSmena = new System.Windows.Forms.TextBox();
            this.lSmena = new System.Windows.Forms.Label();
            this.tDefKSKL = new System.Windows.Forms.TextBox();
            this.tDefDateDoc = new System.Windows.Forms.TextBox();
            this.tDefNameSkl = new System.Windows.Forms.TextBox();
            this.lDefSklad = new System.Windows.Forms.Label();
            this.lDefDatDoc = new System.Windows.Forms.Label();
            this.tUser = new System.Windows.Forms.TextBox();
            this.tPass = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lUser
            // 
            this.lUser.Location = new System.Drawing.Point(7, 7);
            this.lUser.Name = "lUser";
            this.lUser.Size = new System.Drawing.Size(85, 20);
            this.lUser.Text = "Пользователь";
            // 
            // lPass
            // 
            this.lPass.Location = new System.Drawing.Point(7, 31);
            this.lPass.Name = "lPass";
            this.lPass.Size = new System.Drawing.Size(85, 20);
            this.lPass.Text = "Пароль";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.tDefSmena);
            this.panel1.Controls.Add(this.lSmena);
            this.panel1.Controls.Add(this.tDefKSKL);
            this.panel1.Controls.Add(this.tDefDateDoc);
            this.panel1.Controls.Add(this.tDefNameSkl);
            this.panel1.Controls.Add(this.lDefSklad);
            this.panel1.Controls.Add(this.lDefDatDoc);
            this.panel1.Location = new System.Drawing.Point(4, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(210, 82);
            // 
            // tDefSmena
            // 
            this.tDefSmena.Enabled = false;
            this.tDefSmena.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.tDefSmena.Location = new System.Drawing.Point(72, 53);
            this.tDefSmena.Multiline = true;
            this.tDefSmena.Name = "tDefSmena";
            this.tDefSmena.Size = new System.Drawing.Size(32, 18);
            this.tDefSmena.TabIndex = 14;
            this.tDefSmena.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tDefSmena.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tDefSmena.Validating += new System.ComponentModel.CancelEventHandler(this.tDefSmena_Validating);
            // 
            // lSmena
            // 
            this.lSmena.BackColor = System.Drawing.Color.Lavender;
            this.lSmena.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lSmena.Location = new System.Drawing.Point(5, 53);
            this.lSmena.Name = "lSmena";
            this.lSmena.Size = new System.Drawing.Size(60, 18);
            this.lSmena.Text = "Смена";
            // 
            // tDefKSKL
            // 
            this.tDefKSKL.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.tDefKSKL.Location = new System.Drawing.Point(72, 8);
            this.tDefKSKL.Multiline = true;
            this.tDefKSKL.Name = "tDefKSKL";
            this.tDefKSKL.Size = new System.Drawing.Size(32, 20);
            this.tDefKSKL.TabIndex = 0;
            this.tDefKSKL.Text = "658";
            this.tDefKSKL.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tDefKSKL.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tDefKSKL.Validating += new System.ComponentModel.CancelEventHandler(this.tDefKSKL_Validating);
            // 
            // tDefDateDoc
            // 
            this.tDefDateDoc.Location = new System.Drawing.Point(72, 31);
            this.tDefDateDoc.Multiline = true;
            this.tDefDateDoc.Name = "tDefDateDoc";
            this.tDefDateDoc.Size = new System.Drawing.Size(77, 18);
            this.tDefDateDoc.TabIndex = 2;
            this.tDefDateDoc.Text = "22.04.07";
            this.tDefDateDoc.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tDefDateDoc.Validating += new System.ComponentModel.CancelEventHandler(this.tDefDateDoc_Validating);
            // 
            // tDefNameSkl
            // 
            this.tDefNameSkl.Location = new System.Drawing.Point(106, 8);
            this.tDefNameSkl.Multiline = true;
            this.tDefNameSkl.Name = "tDefNameSkl";
            this.tDefNameSkl.ReadOnly = true;
            this.tDefNameSkl.Size = new System.Drawing.Size(100, 20);
            this.tDefNameSkl.TabIndex = 5;
            this.tDefNameSkl.Text = "Масло";
            // 
            // lDefSklad
            // 
            this.lDefSklad.BackColor = System.Drawing.Color.Lavender;
            this.lDefSklad.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lDefSklad.Location = new System.Drawing.Point(5, 9);
            this.lDefSklad.Name = "lDefSklad";
            this.lDefSklad.Size = new System.Drawing.Size(60, 18);
            this.lDefSklad.Text = "Пост";
            // 
            // lDefDatDoc
            // 
            this.lDefDatDoc.BackColor = System.Drawing.Color.Lavender;
            this.lDefDatDoc.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lDefDatDoc.Location = new System.Drawing.Point(5, 31);
            this.lDefDatDoc.Name = "lDefDatDoc";
            this.lDefDatDoc.Size = new System.Drawing.Size(60, 18);
            this.lDefDatDoc.Text = "Дата";
            // 
            // tUser
            // 
            this.tUser.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.tUser.Location = new System.Drawing.Point(99, 7);
            this.tUser.MaxLength = 64;
            this.tUser.Name = "tUser";
            this.tUser.Size = new System.Drawing.Size(115, 21);
            this.tUser.TabIndex = 2;
            this.tUser.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tUser.Validated += new System.EventHandler(this.tUser_Validated);
            this.tUser.Validating += new System.ComponentModel.CancelEventHandler(this.tUser_Validating);
            this.tUser.TextChanged += new System.EventHandler(this.tUser_TextChanged);
            // 
            // tPass
            // 
            this.tPass.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.tPass.Location = new System.Drawing.Point(99, 31);
            this.tPass.MaxLength = 64;
            this.tPass.Name = "tPass";
            this.tPass.PasswordChar = '*';
            this.tPass.Size = new System.Drawing.Size(115, 21);
            this.tPass.TabIndex = 3;
            this.tPass.GotFocus += new System.EventHandler(this.SelAllTextF);
            this.tPass.Validating += new System.ComponentModel.CancelEventHandler(this.tPass_Validating);
            // 
            // Avtor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(219, 154);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tPass);
            this.Controls.Add(this.tUser);
            this.Controls.Add(this.lPass);
            this.Controls.Add(this.lUser);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Avtor";
            this.Text = "Параметры сеанса";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Avtor_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Avtor_KeyDown);
            this.Load += new System.EventHandler(this.Avtor_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lUser;
        private System.Windows.Forms.Label lPass;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tDefKSKL;
        private System.Windows.Forms.TextBox tDefNameSkl;
        private System.Windows.Forms.Label lDefSklad;
        private System.Windows.Forms.TextBox tDefDateDoc;
        private System.Windows.Forms.Label lDefDatDoc;
        private System.Windows.Forms.TextBox tUser;
        private System.Windows.Forms.TextBox tPass;
        private System.Windows.Forms.TextBox tDefSmena;
        private System.Windows.Forms.Label lSmena;
    }
}