using System;
using System.Collections.Concurrent; // 需要加入這行
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
        private System.Windows.Forms.Timer _uiUpdateTimer; // 新增 Timer
        private static ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>(); // 線程安全的緩衝區

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
            Console.SetOut(new TextBoxWriter());

            // --- 新增：設定定時更新 UI 的 Timer ---
            _uiUpdateTimer = new System.Windows.Forms.Timer();
            _uiUpdateTimer.Interval = 200; // 每 200ms 更新一次 UI，避免頻繁操作導致崩潰
            _uiUpdateTimer.Tick += ProcessLogQueue;
            _uiUpdateTimer.Start();
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

        // 定時從 Queue 取出 Log 並一次性寫入
        private void ProcessLogQueue(object? sender, EventArgs e)
        {
            if (_logQueue.IsEmpty || _logBox.IsDisposed) return;

            StringBuilder sb = new StringBuilder();
            string? logMessage;
            int count = 0;

            // 一次最多處理 500 條，避免卡死 UI
            while (count < 500 && _logQueue.TryDequeue(out logMessage))
            {
                sb.Append(logMessage);
                count++;
            }

            if (sb.Length > 0)
            {
                AppendTextSafe(sb.ToString());
            }
        }

        private void AppendTextSafe(string text)
        {
            if (_logBox.IsDisposed) return;

            try
            {
                // 暫停畫面更新，避免清理時閃爍
                _logBox.SuspendLayout();

                // 檢查是否超過上限 
                if (_logBox.TextLength > 50000)
                {
                    bool originalReadOnly = _logBox.ReadOnly;
                    _logBox.ReadOnly = false;

                    // 選取前一半的內容
                    _logBox.Select(0, _logBox.TextLength / 2);
                    // 刪除選取內容
                    _logBox.SelectedText = "";

                    // 插入提示訊息
                    _logBox.AppendText($"\n[系統] Log 已自動清理 (緩衝區重置)...\n--------------------------------\n");

                    _logBox.ReadOnly = originalReadOnly;
                }

                // 寫入新的 Log
                _logBox.AppendText(text);

                // 捲動到最下方
                _logBox.ScrollToCaret();
            }
            catch (Exception)
            {
                // 忽略錯誤
            }
            finally
            {
                // 恢復畫面更新
                _logBox.ResumeLayout();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                // 如果是程式真正要關閉，記得停止 Timer
                _uiUpdateTimer.Stop();
            }
            base.OnFormClosing(e);
        }

        // 修改後的 Writer：只負責將文字丟入 Queue，不直接操作 UI
        public class TextBoxWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                _logQueue.Enqueue(value.ToString());
            }

            public override void Write(string? value)
            {
                if (value != null)
                {
                    _logQueue.Enqueue(value);
                }
            }

            public override void WriteLine(string? value)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string messageWithTime = $"[{timestamp}] {value ?? ""}{Environment.NewLine}";
                _logQueue.Enqueue(messageWithTime);
            }
        }
    }
}