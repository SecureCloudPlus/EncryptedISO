using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Extract
{
    public partial class ExtractForm : Form
    {

        public ExtractForm()
        {
            InitializeComponent();
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
                statusLabel.Text = "Working... please wait";
                this.Refresh();
                string source_path = Directory.GetCurrentDirectory();
                string dest_path = folderDlg.SelectedPath + "\\";
                string pwd = textBox.Text;

                int result = ExtractISO.ExtractDirectory(source_path, dest_path, pwd);
                if (result == -1)
                {
                    statusLabel.Text = "An ERROR occured extracting the files.";
                    return;
                }
                else if (result == 0)
                {
                    statusLabel.Text = "Failed to extract files. Please ensure your password is correct.";
                    return;
                }
                statusLabel.Text = "Successfully extracted " + result + " files.";
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
    }
}