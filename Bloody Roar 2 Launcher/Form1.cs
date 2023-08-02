using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileDownloaderApp
{
    public partial class MainForm : Form
    {
        private bool isPaused = false;
        private string originalWorkingDirectory;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void DownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string fileUrl = "https://dl.dropboxusercontent.com/s/psp1ehj322ntxpb/BR2%20Online%20by%20NK.zip?dl=0";
                string savePath = "BR2 Online by NK.zip";

                if (!File.Exists(savePath))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        isPaused = false;
                        await DownloadFileAsync(webClient, new Uri(fileUrl), savePath);
                        ExtractZipFile(savePath);
                        MessageBox.Show("The installation process is complete.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string extractPath = Path.GetDirectoryName(savePath);
                    string zipFileName = Path.GetFileNameWithoutExtension(savePath);
                    string destinationPath = Path.Combine(extractPath, zipFileName);

                    if (!Directory.Exists(destinationPath))
                    {
                        ExtractZipFile(savePath);
                        MessageBox.Show("The installation process is complete.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("The game folder already exists.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the download: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadFileAsync(WebClient webClient, Uri fileUrl, string savePath)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();

            void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
            {
                webClient.DownloadFileCompleted -= DownloadFileCompleted;
                taskCompletionSource.SetResult(null);
            }

            void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                progressBar1.Value = e.ProgressPercentage;
            }

            webClient.DownloadFileCompleted += DownloadFileCompleted;
            webClient.DownloadProgressChanged += DownloadProgressChanged;
            await webClient.DownloadFileTaskAsync(fileUrl, savePath);

            await taskCompletionSource.Task;
        }

        private void ExtractZipFile(string zipFilePath)
        {
            string extractPath = Path.GetDirectoryName(zipFilePath);
            string zipFileName = Path.GetFileNameWithoutExtension(zipFilePath);
            string destinationPath = Path.Combine(extractPath, zipFileName);

            if (!Directory.Exists(destinationPath))
            {
                ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
            }
        }
        private void proxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string gameFolder = "BR2 Online by NK"; // Change this to the actual folder name if it's different
            string proxyExePath = Path.Combine(gameFolder, "proxy.exe");

            if (File.Exists(proxyExePath))
            {
                try
                {
                    Process.Start(proxyExePath);
                }
                catch (Exception ex)
                {
                    // Show an error message if there was an issue starting the process
                    MessageBox.Show($"Failed to run proxy.exe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("proxy.exe not found in the game folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string gameFolder = "BR2 Online by NK"; // Change this to the actual folder name if it's different
            string duckstationExeName = "duckstation-qt-x64-ReleaseLTCG.exe";
            string duckstationExePath = Path.Combine(gameFolder, duckstationExeName);

            if (File.Exists(duckstationExePath))
            {
                try
                {
                    // Store the current working directory
                    string currentDirectory = Environment.CurrentDirectory;

                    // Set the working directory to the game folder
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, gameFolder);

                    string romFilePath = Path.Combine("roms", "SCUS-94424.cue");
                    string stateFilePath = Path.Combine("savestates", "netplay", "SCUS-94424.sav");
                    string arguments = $"-fastboot \"{romFilePath}\" -statefile \"{stateFilePath}\"";

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = duckstationExeName,
                        Arguments = arguments
                    };

                    // Start Duckstation and wait for it to exit
                    Process duckstationProcess = Process.Start(psi);
                    duckstationProcess.WaitForExit();

                    // Restore the original working directory
                    Environment.CurrentDirectory = currentDirectory;
                }
                catch (Exception ex)
                {
                    // Show an error message if there was an issue starting or waiting for Duckstation
                    MessageBox.Show($"Failed to start Duckstation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"Duckstation not found in the game folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
