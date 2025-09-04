namespace ASMPTool
{
    partial class frmMttSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMttSelection));
            btnConfirm = new Button();
            checkedListBoxItems = new CheckedListBox();
            btnCancel = new Button();
            chkLoopTest = new CheckBox();
            numLoopCount = new NumericUpDown();
            label1 = new Label();
            checkedListBoxSubItems = new CheckedListBox();
            label2 = new Label();
            picSave = new PictureBox();
            saveFileDialog = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)numLoopCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSave).BeginInit();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.Location = new Point(429, 460);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(208, 43);
            btnConfirm.TabIndex = 0;
            btnConfirm.Text = "確定 Confirm";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // checkedListBoxItems
            // 
            checkedListBoxItems.Font = new Font("Microsoft JhengHei UI", 11F);
            checkedListBoxItems.FormattingEnabled = true;
            checkedListBoxItems.Location = new Point(12, 49);
            checkedListBoxItems.Name = "checkedListBoxItems";
            checkedListBoxItems.Size = new Size(405, 394);
            checkedListBoxItems.TabIndex = 1;
            checkedListBoxItems.ItemCheck += checkedListBoxItems_ItemCheck;
            checkedListBoxItems.SelectedIndexChanged += checkedListBoxItems_SelectedIndexChanged;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(643, 460);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(191, 43);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "取消 Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // chkLoopTest
            // 
            chkLoopTest.AutoSize = true;
            chkLoopTest.Font = new Font("Microsoft JhengHei UI", 16F);
            chkLoopTest.Location = new Point(3, 462);
            chkLoopTest.Name = "chkLoopTest";
            chkLoopTest.Size = new Size(269, 39);
            chkLoopTest.TabIndex = 3;
            chkLoopTest.Text = "循環測試 LoopTest";
            chkLoopTest.UseVisualStyleBackColor = true;
            chkLoopTest.CheckedChanged += chkLoopTest_CheckedChanged;
            // 
            // numLoopCount
            // 
            numLoopCount.Font = new Font("Microsoft JhengHei UI", 16F);
            numLoopCount.Location = new Point(273, 461);
            numLoopCount.Name = "numLoopCount";
            numLoopCount.Size = new Size(148, 41);
            numLoopCount.TabIndex = 4;
            numLoopCount.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft JhengHei UI", 15F);
            label1.Location = new Point(15, 8);
            label1.Name = "label1";
            label1.Size = new Size(122, 32);
            label1.TabIndex = 5;
            label1.Text = "任務 Task";
            // 
            // checkedListBoxSubItems
            // 
            checkedListBoxSubItems.Font = new Font("Microsoft JhengHei UI", 11F);
            checkedListBoxSubItems.FormattingEnabled = true;
            checkedListBoxSubItems.Location = new Point(428, 49);
            checkedListBoxSubItems.Name = "checkedListBoxSubItems";
            checkedListBoxSubItems.Size = new Size(405, 394);
            checkedListBoxSubItems.TabIndex = 6;
            checkedListBoxSubItems.ItemCheck += checkedListBoxSubItems_ItemCheck;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft JhengHei UI", 15F);
            label2.Location = new Point(429, 8);
            label2.Name = "label2";
            label2.Size = new Size(123, 32);
            label2.TabIndex = 7;
            label2.Text = "項目 Item";
            // 
            // picSave
            // 
            picSave.Image = (Image)resources.GetObject("picSave.Image");
            picSave.Location = new Point(788, 2);
            picSave.Name = "picSave";
            picSave.Size = new Size(43, 42);
            picSave.SizeMode = PictureBoxSizeMode.StretchImage;
            picSave.TabIndex = 8;
            picSave.TabStop = false;
            picSave.Click += picSave_Click;
            // 
            // frmMttSelection
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.HighlightText;
            ClientSize = new Size(842, 515);
            Controls.Add(picSave);
            Controls.Add(label2);
            Controls.Add(checkedListBoxSubItems);
            Controls.Add(label1);
            Controls.Add(numLoopCount);
            Controls.Add(chkLoopTest);
            Controls.Add(btnCancel);
            Controls.Add(checkedListBoxItems);
            Controls.Add(btnConfirm);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmMttSelection";
            StartPosition = FormStartPosition.CenterParent;
            Text = "frmMttSelection";
            ((System.ComponentModel.ISupportInitialize)numLoopCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSave).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConfirm;
        private CheckedListBox checkedListBoxItems;
        private Button btnCancel;
        private CheckBox chkLoopTest;
        private NumericUpDown numLoopCount;
        private Label label1;
        private CheckedListBox checkedListBoxSubItems;
        private Label label2;
        private PictureBox picSave;
        private SaveFileDialog saveFileDialog;
    }
}