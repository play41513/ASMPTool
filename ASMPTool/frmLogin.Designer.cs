namespace ASMPTool
{
    partial class frmLogin
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            PlFrmLoginTop = new Panel();
            btnClose = new Button();
            pictureBox1 = new PictureBox();
            lbWorkOrder = new Label();
            TBoxWorkOrder = new TextBox();
            btnLogin = new Button();
            label7 = new Label();
            TBoxEmployeeID = new TextBox();
            pictureBox2 = new PictureBox();
            ComBoxProductModel = new ComboBox();
            ComboBoxWorkStation = new ComboBox();
            ComboBoxVersion = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            TimerConnect = new System.Windows.Forms.Timer(components);
            pictureBoxConnect = new PictureBox();
            PlFrmLoginTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxConnect).BeginInit();
            SuspendLayout();
            // 
            // PlFrmLoginTop
            // 
            PlFrmLoginTop.BackColor = Color.SteelBlue;
            PlFrmLoginTop.Controls.Add(btnClose);
            PlFrmLoginTop.Dock = DockStyle.Top;
            PlFrmLoginTop.Location = new Point(0, 0);
            PlFrmLoginTop.Margin = new Padding(3, 4, 3, 4);
            PlFrmLoginTop.Name = "PlFrmLoginTop";
            PlFrmLoginTop.Size = new Size(932, 51);
            PlFrmLoginTop.TabIndex = 9;
            PlFrmLoginTop.MouseDown += plFrmLoginTop_MouseDown;
            PlFrmLoginTop.MouseMove += plFrmLoginTop_MouseMove;
            PlFrmLoginTop.MouseUp += plFrmLoginTop_MouseUp;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.SteelBlue;
            btnClose.FlatAppearance.BorderColor = Color.SteelBlue;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("新細明體", 13F);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(891, 4);
            btnClose.Margin = new Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(37, 39);
            btnClose.TabIndex = 0;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(770, 538);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(162, 52);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 10;
            pictureBox1.TabStop = false;
            // 
            // lbWorkOrder
            // 
            lbWorkOrder.AutoSize = true;
            lbWorkOrder.Font = new Font("微软雅黑", 16F);
            lbWorkOrder.ForeColor = Color.DimGray;
            lbWorkOrder.Location = new Point(37, 277);
            lbWorkOrder.Name = "lbWorkOrder";
            lbWorkOrder.Size = new Size(270, 35);
            lbWorkOrder.TabIndex = 11;
            lbWorkOrder.Text = "工單號 Work Order :";
            // 
            // TBoxWorkOrder
            // 
            TBoxWorkOrder.BackColor = Color.White;
            TBoxWorkOrder.BorderStyle = BorderStyle.FixedSingle;
            TBoxWorkOrder.Font = new Font("微软雅黑", 15F);
            TBoxWorkOrder.ForeColor = Color.Black;
            TBoxWorkOrder.ImeMode = ImeMode.Alpha;
            TBoxWorkOrder.Location = new Point(37, 327);
            TBoxWorkOrder.Margin = new Padding(3, 4, 3, 4);
            TBoxWorkOrder.Name = "TBoxWorkOrder";
            TBoxWorkOrder.Size = new Size(397, 40);
            TBoxWorkOrder.TabIndex = 1;
            TBoxWorkOrder.Text = "000-000000000000000";
            TBoxWorkOrder.MouseClick += TBox_MouseClick;
            TBoxWorkOrder.KeyPress += TBox_KeyPress;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.White;
            btnLogin.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("微软雅黑", 12F);
            btnLogin.ForeColor = Color.FromArgb(64, 64, 64);
            btnLogin.Location = new Point(211, 529);
            btnLogin.Margin = new Padding(3, 4, 3, 4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(490, 47);
            btnLogin.TabIndex = 10;
            btnLogin.Text = "登    入   L o g i n";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("微软雅黑", 16F);
            label7.ForeColor = Color.DimGray;
            label7.Location = new Point(37, 399);
            label7.Name = "label7";
            label7.Size = new Size(254, 35);
            label7.TabIndex = 26;
            label7.Text = "工號 Employee ID :";
            // 
            // TBoxEmployeeID
            // 
            TBoxEmployeeID.BackColor = Color.White;
            TBoxEmployeeID.BorderStyle = BorderStyle.FixedSingle;
            TBoxEmployeeID.Font = new Font("微软雅黑", 15F);
            TBoxEmployeeID.ForeColor = Color.Black;
            TBoxEmployeeID.ImeMode = ImeMode.Alpha;
            TBoxEmployeeID.Location = new Point(37, 451);
            TBoxEmployeeID.Margin = new Padding(3, 4, 3, 4);
            TBoxEmployeeID.Name = "TBoxEmployeeID";
            TBoxEmployeeID.Size = new Size(397, 40);
            TBoxEmployeeID.TabIndex = 9;
            TBoxEmployeeID.Text = "T00000";
            TBoxEmployeeID.MouseClick += TBox_MouseClick;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(112, 76);
            pictureBox2.Margin = new Padding(3, 4, 3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(685, 171);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 29;
            pictureBox2.TabStop = false;
            // 
            // ComBoxProductModel
            // 
            ComBoxProductModel.Font = new Font("微软雅黑", 15F);
            ComBoxProductModel.FormattingEnabled = true;
            ComBoxProductModel.Location = new Point(480, 308);
            ComBoxProductModel.Name = "ComBoxProductModel";
            ComBoxProductModel.Size = new Size(409, 40);
            ComBoxProductModel.TabIndex = 30;
            ComBoxProductModel.DropDown += ComboBox_DropDown;
            ComBoxProductModel.TextUpdate += ComBoxProductModel_TextUpdate;
            // 
            // ComboBoxWorkStation
            // 
            ComboBoxWorkStation.Font = new Font("微软雅黑", 15F);
            ComboBoxWorkStation.FormattingEnabled = true;
            ComboBoxWorkStation.Location = new Point(480, 394);
            ComboBoxWorkStation.Name = "ComboBoxWorkStation";
            ComboBoxWorkStation.Size = new Size(409, 40);
            ComboBoxWorkStation.TabIndex = 31;
            ComboBoxWorkStation.DropDown += ComboBox_DropDown;
            // 
            // ComboBoxVersion
            // 
            ComboBoxVersion.Font = new Font("微软雅黑", 15F);
            ComboBoxVersion.FormattingEnabled = true;
            ComboBoxVersion.Location = new Point(480, 479);
            ComboBoxVersion.Name = "ComboBoxVersion";
            ComboBoxVersion.Size = new Size(409, 40);
            ComboBoxVersion.TabIndex = 32;
            ComboBoxVersion.DropDown += ComboBox_DropDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("微软雅黑", 16F);
            label1.ForeColor = Color.DimGray;
            label1.Location = new Point(480, 263);
            label1.Name = "label1";
            label1.Size = new Size(338, 35);
            label1.TabIndex = 33;
            label1.Text = "產品型號 Product Model :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("微软雅黑", 16F);
            label2.ForeColor = Color.DimGray;
            label2.Location = new Point(480, 352);
            label2.Name = "label2";
            label2.Size = new Size(253, 35);
            label2.TabIndex = 34;
            label2.Text = "工站 WorkStation :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("微软雅黑", 16F);
            label3.ForeColor = Color.DimGray;
            label3.Location = new Point(480, 438);
            label3.Name = "label3";
            label3.Size = new Size(174, 35);
            label3.TabIndex = 35;
            label3.Text = "版本 Version";
            // 
            // TimerConnect
            // 
            TimerConnect.Enabled = true;
            TimerConnect.Interval = 2;
            // 
            // pictureBoxConnect
            // 
            pictureBoxConnect.Image = Properties.Resources.disconnect;
            pictureBoxConnect.Location = new Point(858, 76);
            pictureBoxConnect.Name = "pictureBoxConnect";
            pictureBoxConnect.Size = new Size(44, 43);
            pictureBoxConnect.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxConnect.TabIndex = 37;
            pictureBoxConnect.TabStop = false;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(932, 590);
            Controls.Add(pictureBoxConnect);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(ComboBoxVersion);
            Controls.Add(ComboBoxWorkStation);
            Controls.Add(ComBoxProductModel);
            Controls.Add(pictureBox2);
            Controls.Add(TBoxEmployeeID);
            Controls.Add(label7);
            Controls.Add(btnLogin);
            Controls.Add(TBoxWorkOrder);
            Controls.Add(lbWorkOrder);
            Controls.Add(pictureBox1);
            Controls.Add(PlFrmLoginTop);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "frmLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmLogin";
            PlFrmLoginTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxConnect).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel PlFrmLoginTop;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lbWorkOrder;
        private System.Windows.Forms.TextBox TBoxWorkOrder;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TBoxEmployeeID;
        private System.Windows.Forms.PictureBox pictureBox2;
        private ComboBox ComBoxProductModel;
        private ComboBox ComboBoxWorkStation;
        private ComboBox ComboBoxVersion;
        private Label label1;
        private Label label2;
        private Label label3;
        private System.Windows.Forms.Timer TimerConnect;
        private PictureBox pictureBoxConnect;
    }
}