using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Extract
{
    public partial class ExtractForm : Form
    {
        private ExtractISO extractISO = new ExtractISO();
        public ExtractForm()
        {
            InitializeComponent();
            progressBar.Value = 0;
            progressBar.Minimum = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox.TextLength == 0)
            {
                statusLabel.Text = "An ERROR occured extracting the files.";
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
                progressBar.Maximum = dinfo.GetFiles(@"*.enc").Length + 1;
                statusLabel.Text = "Working... please wait";
                this.Refresh();
                string source_path = Directory.GetCurrentDirectory();
                string dest_path = folderDlg.SelectedPath + "\\";
                string pwd = textBox.Text;

                int result = -1;
                var thread = new Thread(() => result = extractISO.ExtractDirectory(source_path, dest_path, pwd));
                thread.Start();
                int cnt = -1;
                while (thread.IsAlive) 
                {
                    if (extractISO.Progress > cnt)
                    {
                        cnt = extractISO.Progress;
                        updateProgressBar(cnt);
                    }
                    Application.DoEvents();
                }
                thread = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                result--;
                if (result == -1)
                {
                    statusLabel.Text = "An ERROR occured extracting the files.";
                    updateProgressBar(0);
                    return;
                }
                else if (result == 0)
                {
                    statusLabel.Text = "Please ensure your password is correct.";
                    updateProgressBar(0);
                    return;
                }
                statusLabel.Text = "Successfully extracted " + result + " files.";
                updateProgressBar(cnt);
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

        private void updateProgressBar(int value)
        {
            progressBar.Value = value;
            progressBar.Invalidate();
            this.Refresh();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            if (textBox.PasswordChar == '●')
                textBox.PasswordChar = '\x0000';
            else
                textBox.PasswordChar = '●';
        }
    }
}