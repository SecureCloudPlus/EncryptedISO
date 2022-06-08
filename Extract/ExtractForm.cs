using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Extract
{
    public partial class ExtractForm : Form
    {
        private ExtractISO extractISO = new ExtractISO();
        private Thread extractThread;

        public ExtractForm()
        {
            InitializeComponent();
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            updateStatusLabel("Ready...");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox.TextLength == 0)
            {
                updateStatusLabel( "An ERROR occured extracting the files.");
                return;
            }
            FolderBrowserDialog folderDlg = new FolderBrowserDialog
            {
                Description = "Extract files to this location",
                ShowNewFolderButton = true
            };
            DialogResult dlgRes = folderDlg.ShowDialog();
            if (dlgRes == DialogResult.OK)
            {
                DirectoryInfo dinfo = new DirectoryInfo(Directory.GetCurrentDirectory());
                int max = 0;
                foreach (FileInfo fi in dinfo.GetFiles(@"*.enc"))
                {
                    max += (int)(fi.Length / 4096);
                }
                progressBar.Maximum = max;
                updateStatusLabel("Working... please wait");
                string source_path = Directory.GetCurrentDirectory();
                string dest_path = folderDlg.SelectedPath + "\\";
                string pwd = textBox.Text;
                long result = -1;
                extractThread = new Thread(() => result = extractISO.ExtractDirectory(source_path, dest_path, pwd));
                extractThread.Start();
                while (extractThread.IsAlive)
                {
                    if (extractISO.Progress <= progressBar.Maximum)
                    {
                        updateProgressBar(extractISO.Progress);
                    }
                    Application.DoEvents();
                }
                extractThread = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (result < 1)
                {
                    updateStatusLabel("Please ensure your password is correct.");
                    updateProgressBar(0);
                    return;
                }
                updateStatusLabel("Successfully extracted " + result + " files.");
                updateProgressBar(progressBar.Maximum);
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderDlg.SelectedPath + "\\",
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
            }
        }

        private void updateStatusLabel(string message)
        {
            statusLabel.Text = message;
            statusLabel.Invalidate();
            this.Refresh();
        }

        private void updateProgressBar(int value)
        {
            progressBar.Value = value;
            progressBar.Invalidate();
            this.Refresh();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (textBox.Text.Length > 0)
                if (textBox.PasswordChar == '●')
                    textBox.PasswordChar = '\x0000';
                else
                    textBox.PasswordChar = '●';
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            textBox.PasswordChar = '●';
        }

        private void ExtractForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            extractThread = null;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            textBox.Clear();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = false;
            if (textBox.TextLength > 0)
                button1.Enabled = true;
        }
    }
}