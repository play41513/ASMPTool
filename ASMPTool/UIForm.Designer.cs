namespace ASMPTool
{
    partial class UIForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UIForm));
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            pictureBox1 = new PictureBox();
            lbTime = new Label();
            tableLayoutPanel3 = new TableLayoutPanel();
            lbWorkOrderTitle = new Label();
            lbVersion = new Label();
            lbVersionTitle = new Label();
            lbWorkStation = new Label();
            lbWorkStationTitle = new Label();
            lbProduct = new Label();
            lbProductTitle = new Label();
            lbEmployeeID = new Label();
            lbEmployeeIDTitle = new Label();
            lbWorkOrder = new Label();
            panel1 = new Panel();
            dataGridView = new DataGridView();
            tableLayoutPanel4 = new TableLayoutPanel();
            lbResult = new Label();
            lbTotalTime = new Label();
            label3 = new Label();
            textBox = new RichTextBox();
            loginInfoModelBindingSource = new BindingSource(components);
            plMessageBox = new Panel();
            lbMessageBoxTitle = new Label();
            panel2 = new Panel();
            tBoxScanBarcode = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)loginInfoModelBindingSource).BeginInit();
            plMessageBox.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 3);
            tableLayoutPanel1.Controls.Add(dataGridView, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(2, 2, 2, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 66.6666641F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tableLayoutPanel1.Size = new Size(1024, 538);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104F));
            tableLayoutPanel2.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel2.Controls.Add(lbTime, 2, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(2, 2);
            tableLayoutPanel2.Margin = new Padding(2, 2, 2, 2);
            tableLayoutPanel2.MinimumSize = new Size(8, 0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tableLayoutPanel2.Size = new Size(1020, 56);
            tableLayoutPanel2.TabIndex = 5;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(2, 2);
            pictureBox1.Margin = new Padding(2, 2, 2, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(196, 52);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // lbTime
            // 
            lbTime.AutoSize = true;
            lbTime.Dock = DockStyle.Fill;
            lbTime.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            lbTime.Location = new Point(918, 0);
            lbTime.Margin = new Padding(2, 0, 2, 0);
            lbTime.Name = "lbTime";
            lbTime.Size = new Size(100, 56);
            lbTime.TabIndex = 1;
            lbTime.Text = "lbTime";
            lbTime.TextAlign = ContentAlignment.BottomCenter;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.BackColor = SystemColors.ControlDarkDark;
            tableLayoutPanel3.ColumnCount = 6;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15.1325512F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18.4617023F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.0714874F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.0759563F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.1330681F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.1252384F));
            tableLayoutPanel3.Controls.Add(lbWorkOrderTitle, 0, 2);
            tableLayoutPanel3.Controls.Add(lbVersion, 5, 0);
            tableLayoutPanel3.Controls.Add(lbVersionTitle, 4, 0);
            tableLayoutPanel3.Controls.Add(lbWorkStation, 3, 0);
            tableLayoutPanel3.Controls.Add(lbWorkStationTitle, 2, 0);
            tableLayoutPanel3.Controls.Add(lbProduct, 1, 0);
            tableLayoutPanel3.Controls.Add(lbProductTitle, 0, 0);
            tableLayoutPanel3.Controls.Add(lbEmployeeID, 5, 2);
            tableLayoutPanel3.Controls.Add(lbEmployeeIDTitle, 4, 2);
            tableLayoutPanel3.Controls.Add(lbWorkOrder, 1, 2);
            tableLayoutPanel3.Controls.Add(panel1, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(2, 474);
            tableLayoutPanel3.Margin = new Padding(2, 2, 2, 2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(1020, 62);
            tableLayoutPanel3.TabIndex = 1;
            // 
            // lbWorkOrderTitle
            // 
            lbWorkOrderTitle.AutoSize = true;
            lbWorkOrderTitle.Dock = DockStyle.Fill;
            lbWorkOrderTitle.Font = new Font("Microsoft JhengHei UI", 12F);
            lbWorkOrderTitle.ForeColor = Color.White;
            lbWorkOrderTitle.Location = new Point(2, 33);
            lbWorkOrderTitle.Margin = new Padding(2, 0, 2, 0);
            lbWorkOrderTitle.Name = "lbWorkOrderTitle";
            lbWorkOrderTitle.Size = new Size(150, 29);
            lbWorkOrderTitle.TabIndex = 23;
            lbWorkOrderTitle.Text = "工單 WorkOrder :";
            lbWorkOrderTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbVersion
            // 
            lbVersion.AutoSize = true;
            lbVersion.Dock = DockStyle.Fill;
            lbVersion.Font = new Font("Microsoft JhengHei UI", 12F);
            lbVersion.ForeColor = Color.White;
            lbVersion.Location = new Point(855, 0);
            lbVersion.Margin = new Padding(2, 0, 2, 0);
            lbVersion.Name = "lbVersion";
            lbVersion.Size = new Size(163, 29);
            lbVersion.TabIndex = 22;
            lbVersion.Text = "Version";
            lbVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbVersionTitle
            // 
            lbVersionTitle.AutoSize = true;
            lbVersionTitle.Dock = DockStyle.Fill;
            lbVersionTitle.Font = new Font("Microsoft JhengHei UI", 12F);
            lbVersionTitle.ForeColor = Color.White;
            lbVersionTitle.Location = new Point(681, 0);
            lbVersionTitle.Margin = new Padding(2, 0, 2, 0);
            lbVersionTitle.Name = "lbVersionTitle";
            lbVersionTitle.Size = new Size(170, 29);
            lbVersionTitle.TabIndex = 21;
            lbVersionTitle.Text = "| 版本 Version :";
            lbVersionTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbWorkStation
            // 
            lbWorkStation.AutoSize = true;
            lbWorkStation.Dock = DockStyle.Fill;
            lbWorkStation.Font = new Font("Microsoft JhengHei UI", 12F);
            lbWorkStation.ForeColor = Color.White;
            lbWorkStation.Location = new Point(507, 0);
            lbWorkStation.Margin = new Padding(2, 0, 2, 0);
            lbWorkStation.Name = "lbWorkStation";
            lbWorkStation.Size = new Size(170, 29);
            lbWorkStation.TabIndex = 20;
            lbWorkStation.Text = "WorkStation";
            lbWorkStation.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbWorkStationTitle
            // 
            lbWorkStationTitle.AutoSize = true;
            lbWorkStationTitle.Dock = DockStyle.Fill;
            lbWorkStationTitle.Font = new Font("Microsoft JhengHei UI", 12F);
            lbWorkStationTitle.ForeColor = Color.White;
            lbWorkStationTitle.Location = new Point(344, 0);
            lbWorkStationTitle.Margin = new Padding(2, 0, 2, 0);
            lbWorkStationTitle.Name = "lbWorkStationTitle";
            lbWorkStationTitle.Size = new Size(159, 29);
            lbWorkStationTitle.TabIndex = 19;
            lbWorkStationTitle.Text = "| 工站 WorkStation :";
            lbWorkStationTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbProduct
            // 
            lbProduct.AutoSize = true;
            lbProduct.Dock = DockStyle.Fill;
            lbProduct.Font = new Font("Microsoft JhengHei UI", 12F);
            lbProduct.ForeColor = Color.White;
            lbProduct.Location = new Point(156, 0);
            lbProduct.Margin = new Padding(2, 0, 2, 0);
            lbProduct.Name = "lbProduct";
            lbProduct.Size = new Size(184, 29);
            lbProduct.TabIndex = 18;
            lbProduct.Text = "Product";
            lbProduct.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbProductTitle
            // 
            lbProductTitle.AutoSize = true;
            lbProductTitle.Dock = DockStyle.Fill;
            lbProductTitle.Font = new Font("Microsoft JhengHei UI", 12F);
            lbProductTitle.ForeColor = Color.White;
            lbProductTitle.Location = new Point(2, 0);
            lbProductTitle.Margin = new Padding(2, 0, 2, 0);
            lbProductTitle.Name = "lbProductTitle";
            lbProductTitle.Size = new Size(150, 29);
            lbProductTitle.TabIndex = 17;
            lbProductTitle.Text = "產品 Product :";
            lbProductTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbEmployeeID
            // 
            lbEmployeeID.AutoSize = true;
            lbEmployeeID.Dock = DockStyle.Fill;
            lbEmployeeID.Font = new Font("Microsoft JhengHei UI", 12F);
            lbEmployeeID.ForeColor = Color.White;
            lbEmployeeID.Location = new Point(855, 33);
            lbEmployeeID.Margin = new Padding(2, 0, 2, 0);
            lbEmployeeID.Name = "lbEmployeeID";
            lbEmployeeID.Size = new Size(163, 29);
            lbEmployeeID.TabIndex = 3;
            lbEmployeeID.Text = "Employee";
            lbEmployeeID.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbEmployeeIDTitle
            // 
            lbEmployeeIDTitle.AutoSize = true;
            lbEmployeeIDTitle.Dock = DockStyle.Fill;
            lbEmployeeIDTitle.Font = new Font("Microsoft JhengHei UI", 12F);
            lbEmployeeIDTitle.ForeColor = Color.White;
            lbEmployeeIDTitle.Location = new Point(681, 33);
            lbEmployeeIDTitle.Margin = new Padding(2, 0, 2, 0);
            lbEmployeeIDTitle.Name = "lbEmployeeIDTitle";
            lbEmployeeIDTitle.Size = new Size(170, 29);
            lbEmployeeIDTitle.TabIndex = 2;
            lbEmployeeIDTitle.Text = "| 工號 Employee ID :";
            lbEmployeeIDTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lbWorkOrder
            // 
            lbWorkOrder.AutoSize = true;
            tableLayoutPanel3.SetColumnSpan(lbWorkOrder, 2);
            lbWorkOrder.Dock = DockStyle.Fill;
            lbWorkOrder.Font = new Font("Microsoft JhengHei UI", 12F);
            lbWorkOrder.ForeColor = Color.White;
            lbWorkOrder.Location = new Point(156, 33);
            lbWorkOrder.Margin = new Padding(2, 0, 2, 0);
            lbWorkOrder.Name = "lbWorkOrder";
            lbWorkOrder.Size = new Size(347, 29);
            lbWorkOrder.TabIndex = 1;
            lbWorkOrder.Text = "WorkOrder";
            lbWorkOrder.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ButtonHighlight;
            tableLayoutPanel3.SetColumnSpan(panel1, 6);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(2, 31);
            panel1.Margin = new Padding(2, 2, 2, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(1016, 1);
            panel1.TabIndex = 24;
            // 
            // dataGridView
            // 
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Location = new Point(2, 62);
            dataGridView.Margin = new Padding(2, 2, 2, 2);
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersWidth = 51;
            dataGridView.Size = new Size(1020, 271);
            dataGridView.TabIndex = 2;
            dataGridView.CellFormatting += DataGridView_CellFormatting;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.BackColor = Color.White;
            tableLayoutPanel4.ColumnCount = 4;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.91011F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.0898933F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9F));
            tableLayoutPanel4.Controls.Add(lbResult, 2, 2);
            tableLayoutPanel4.Controls.Add(lbTotalTime, 2, 1);
            tableLayoutPanel4.Controls.Add(label3, 0, 1);
            tableLayoutPanel4.Controls.Add(textBox, 0, 2);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(2, 337);
            tableLayoutPanel4.Margin = new Padding(2, 2, 2, 2);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 6F));
            tableLayoutPanel4.Size = new Size(1020, 133);
            tableLayoutPanel4.TabIndex = 3;
            // 
            // lbResult
            // 
            lbResult.BackColor = Color.Silver;
            lbResult.Dock = DockStyle.Fill;
            lbResult.Font = new Font("Microsoft JhengHei UI", 20F, FontStyle.Bold);
            lbResult.Location = new Point(731, 24);
            lbResult.Margin = new Padding(2, 0, 2, 0);
            lbResult.Name = "lbResult";
            lbResult.Size = new Size(277, 103);
            lbResult.TabIndex = 7;
            lbResult.Text = "WAIT";
            lbResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lbTotalTime
            // 
            lbTotalTime.Dock = DockStyle.Top;
            lbTotalTime.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold);
            lbTotalTime.Location = new Point(731, 4);
            lbTotalTime.Margin = new Padding(2, 0, 2, 0);
            lbTotalTime.Name = "lbTotalTime";
            lbTotalTime.Size = new Size(277, 20);
            lbTotalTime.TabIndex = 5;
            lbTotalTime.Text = "Total Time : 00.00";
            lbTotalTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Microsoft JhengHei UI", 12F);
            label3.Location = new Point(2, 4);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(717, 20);
            label3.TabIndex = 0;
            label3.Text = "紀錄 Record";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBox
            // 
            textBox.Dock = DockStyle.Fill;
            textBox.Location = new Point(2, 26);
            textBox.Margin = new Padding(2, 2, 2, 2);
            textBox.Name = "textBox";
            textBox.Size = new Size(717, 99);
            textBox.TabIndex = 8;
            textBox.Text = "";
            textBox.TextChanged += textBox_TextChanged;
            // 
            // loginInfoModelBindingSource
            // 
            loginInfoModelBindingSource.DataSource = typeof(Model.LoginInfoModel);
            // 
            // plMessageBox
            // 
            plMessageBox.BackColor = SystemColors.ControlDarkDark;
            plMessageBox.Controls.Add(lbMessageBoxTitle);
            plMessageBox.Controls.Add(panel2);
            plMessageBox.ImeMode = ImeMode.Alpha;
            plMessageBox.Location = new Point(253, 122);
            plMessageBox.Margin = new Padding(2, 2, 2, 2);
            plMessageBox.Name = "plMessageBox";
            plMessageBox.Size = new Size(465, 148);
            plMessageBox.TabIndex = 2;
            plMessageBox.Visible = false;
            // 
            // lbMessageBoxTitle
            // 
            lbMessageBoxTitle.Dock = DockStyle.Top;
            lbMessageBoxTitle.Font = new Font("Microsoft JhengHei UI", 20F);
            lbMessageBoxTitle.ForeColor = Color.White;
            lbMessageBoxTitle.Location = new Point(0, 0);
            lbMessageBoxTitle.Margin = new Padding(2, 0, 2, 0);
            lbMessageBoxTitle.Name = "lbMessageBoxTitle";
            lbMessageBoxTitle.Size = new Size(465, 54);
            lbMessageBoxTitle.TabIndex = 1;
            lbMessageBoxTitle.Text = "掃描Barcode (Scan Barcode)";
            lbMessageBoxTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(tBoxScanBarcode);
            panel2.Location = new Point(7, 47);
            panel2.Margin = new Padding(2, 2, 2, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(450, 92);
            panel2.TabIndex = 0;
            // 
            // tBoxScanBarcode
            // 
            tBoxScanBarcode.BackColor = SystemColors.ButtonShadow;
            tBoxScanBarcode.Font = new Font("Microsoft JhengHei UI", 20F);
            tBoxScanBarcode.ForeColor = Color.FromArgb(64, 64, 64);
            tBoxScanBarcode.Location = new Point(18, 33);
            tBoxScanBarcode.Margin = new Padding(2, 2, 2, 2);
            tBoxScanBarcode.Name = "tBoxScanBarcode";
            tBoxScanBarcode.Size = new Size(421, 41);
            tBoxScanBarcode.TabIndex = 0;
            tBoxScanBarcode.TextAlign = HorizontalAlignment.Center;
            tBoxScanBarcode.KeyPress += tBoxScanBarcode_KeyPress;
            // 
            // UIForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1024, 538);
            Controls.Add(plMessageBox);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(2, 2, 2, 2);
            MinimumSize = new Size(1040, 39);
            Name = "UIForm";
            Text = "ASMPTool (Actionstar.)";
            FormClosing += UIForm_FormClosing;
            Resize += UIForm_Resize;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)loginInfoModelBindingSource).EndInit();
            plMessageBox.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel3;
        private Label lbWorkOrder;
        private Label lbEmployeeID;
        private Label lbEmployeeIDTitle;
        private DataGridView dataGridView;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label3;
        private TableLayoutPanel tableLayoutPanel2;
        private PictureBox pictureBox1;
        private Label lbTime;
        private BindingSource loginInfoModelBindingSource;
        private Panel plMessageBox;
        private Label lbMessageBoxTitle;
        private Panel panel2;
        private TextBox tBoxScanBarcode;
        private Label lbTotalTime;
        private Label lbResult;
        private Label lbWorkOrderTitle;
        private Label lbVersion;
        private Label lbVersionTitle;
        private Label lbWorkStation;
        private Label lbWorkStationTitle;
        private Label lbProduct;
        private Label lbProductTitle;
        private Panel panel1;
        private RichTextBox textBox;
    }
}