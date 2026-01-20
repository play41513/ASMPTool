using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ASMPTool
{
    public class DebugConsoleForm : Form
    {
        private RichTextBox _logBox;
        private static DebugConsoleForm? _instance;

        // 確保只有一個實體 (Singleton pattern)
        public static DebugConsoleForm Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new DebugConsoleForm();
                }
                return _instance;
            }
        }

        private DebugConsoleForm()
        {
            InitializeComponent();
            // 初始化時，將 Console 輸出重導到這個視窗
            Console.SetOut(new TextBoxWriter(_logBox));
        }

        private void InitializeComponent()
        {
            this._logBox = new RichTextBox();
            this.SuspendLayout();

            // 設定文字框屬性
            this._logBox.Dock = DockStyle.Fill;
            this._logBox.BackColor = Color.Black;
            this._logBox.ForeColor = Color.Lime; 
            this._logBox.Font = new Font("Consolas", 10F);
            this._logBox.ReadOnly = true;
            this._logBox.Name = "logBox";

            // 設定視窗屬性
            this.ClientSize = new Size(800, 500);
            this.Controls.Add(this._logBox);
            this.Name = "DebugConsoleForm";
            this.Text = "Debug Console Output";
            this.ShowIcon = false;
            this.ResumeLayout(false);
        }

        // 攔截視窗關閉事件，改為隱藏而不是銷毀，這樣Log才不會斷掉
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        // 用來將 Console 寫入 Textbox 的幫助類別
        public class TextBoxWriter : TextWriter
        {
            private readonly RichTextBox _output;
            private const int MaxTextLength = 50000;
            public TextBoxWriter(RichTextBox output) { _output = output; }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                // 轉發給 Write(string) 統一處理
                Write(value.ToString());
            }

            public override void Write(string? value)
            {
                if (value == null) return;
                if (_output.IsDisposed) return;

                // 確保在 UI 執行緒執行
                if (_output.InvokeRequired)
                {
                    // 使用 BeginInvoke 避免死鎖
                    _output.BeginInvoke(new Action<string>(Write), value);
                    return;
                }

                // 清理舊 Log 機制
                if (_output.TextLength > MaxTextLength)
                {
                    // 一次刪除舊的一半內容
                    _output.Select(0, _output.TextLength / 2);
                    _output.SelectedText = "";

                    // 補上一行提示，讓使用者知道前面被刪了
                    _output.AppendText($"\n[系統] 為節省記憶體，已自動清除舊的 Log...\n--------------------------------\n");
                }
                // ------------------------------------

                _output.AppendText(value);
                _output.ScrollToCaret();
            }

            public override void WriteLine(string? value)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string messageWithTime = $"[{timestamp}] {value ?? ""}{Environment.NewLine}";
                Write(messageWithTime);
            }
        }
    }
}