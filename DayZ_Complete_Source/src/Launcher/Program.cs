using System;
using System.Windows.Forms;
using DayZ.Core;

namespace DayZ.Launcher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LauncherForm());
        }
    }

    public class LauncherForm : Form
    {
        private GameEngine _engine = null!;
        private Label _statusLabel = null!;
        private Button _startButton = null!;
        private Button _settingsButton = null!;
        private RichTextBox _logsTextBox = null!;

        public LauncherForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "DayZ 1.25 Launcher";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.White;

            // Create layout
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = System.Drawing.Color.FromArgb(40, 40, 40)
            };
            this.Controls.Add(panelTop);

            _statusLabel = new Label
            {
                Text = "Status: Ready",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10),
                Font = new System.Drawing.Font("Arial", 12),
                ForeColor = System.Drawing.Color.LimeGreen
            };
            panelTop.Controls.Add(_statusLabel);

            _startButton = new Button
            {
                Text = "START GAME",
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(120, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 100, 0),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _startButton.Click += StartButton_Click;
            panelTop.Controls.Add(_startButton);

            _settingsButton = new Button
            {
                Text = "SETTINGS",
                Location = new System.Drawing.Point(140, 40),
                Size = new System.Drawing.Size(120, 35),
                BackColor = System.Drawing.Color.FromArgb(100, 100, 0),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _settingsButton.Click += SettingsButton_Click;
            panelTop.Controls.Add(_settingsButton);

            // Logs panel
            _logsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                ForeColor = System.Drawing.Color.LimeGreen,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9)
            };
            this.Controls.Add(_logsTextBox);

            this.Shown += LauncherForm_Shown;
            this.FormClosing += LauncherForm_FormClosing;
        }

        private void LauncherForm_Shown(object? sender, EventArgs e)
        {
            Log("DayZ Launcher initializing...");
            Log("Version: 1.25");
            Log("Build Date: " + DateTime.Now);
            Log("");

            try
            {
                _engine = new GameEngine();
                Log("Game Engine initialized");
                Log("Status: Ready to start");
                _statusLabel.Text = "Status: Ready";
                _statusLabel.ForeColor = System.Drawing.Color.LimeGreen;
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
                _statusLabel.Text = "Status: Error";
                _statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Log("Starting game...");
                _statusLabel.Text = "Status: Starting...";
                _statusLabel.ForeColor = System.Drawing.Color.Yellow;

                if (_engine == null)
                {
                    _engine = new GameEngine();
                }

                _engine.Initialize();
                Log("Game started successfully");
                _statusLabel.Text = "Status: Running";
                _statusLabel.ForeColor = System.Drawing.Color.Cyan;

                // Simulate game loop
                for (int i = 0; i < 5; i++)
                {
                    _engine.Tick();
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(16); // ~60 FPS
                }

                Log("Game running...");
            }
            catch (Exception ex)
            {
                Log("ERROR starting game: " + ex.Message);
                _statusLabel.Text = "Status: Error";
                _statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            Log("Opening settings...");
            MessageBox.Show("Settings window would open here", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LauncherForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_engine != null)
            {
                Log("Shutting down engine...");
                _engine.Shutdown();
                Log("Engine shutdown complete");
            }
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => Log(message));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logsTextBox.AppendText($"[{timestamp}] {message}\n");
            _logsTextBox.ScrollToCaret();
        }
    }
}
