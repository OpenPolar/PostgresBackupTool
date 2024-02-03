using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PostgresBackupTool.Core
{
    public class PostgresFtpBackup
    {
        private string ftpHost { get; set; }
        private int ftpPort { get; set; }
        private string ftpUsername { get; set; }
        private string ftpPassword { get; set; }

        private string postHost { get; set; }
        private int postPort { get; set; }
        private string postUsername { get; set; }
        private string postPassword { get; set; }

        public PostgresFtpBackup(string ftpHost, int ftpPort, string ftpUsername, string ftpPassword, string postHost, int postPort, string postUsername, string postPassword) 
        { 
            this.ftpHost = ftpHost;
            this.ftpPort = ftpPort;
            this.ftpUsername = ftpUsername;
            this.ftpPassword = ftpPassword;

            this.postHost = postHost;
            this.postPort = postPort;
            this.postUsername = postUsername;
            this.postPassword = postPassword;
        }

        public List<string> ListAllDumps(string database)
        {
            using (var ftp = new FtpClient(ftpHost, ftpUsername, ftpPassword, ftpPort))
            {
                var listing = ftp.GetListing(database);

                return listing.Select(x => x.Name).OrderByDescending(x => x).ToList();
            }
        }

        public async Task RestoreAsync(string database, string dump)
        {
            string downloadFileLocation = @$"{Path.GetTempPath()}/{dump}";
            using (var ftp = new FtpClient(ftpHost, ftpUsername, ftpPassword, ftpPort))
            {
                var hasWorked = ftp.DownloadFile(downloadFileLocation, @$"{Path.GetTempPath()}/{dump}");

                if (!hasWorked)
                    throw new Exception("File download error!");
            }
            await new PostgresBackup().PostgreSqlRestore(dump, postHost, postPort.ToString(), database, postUsername, postPassword);
            File.Delete(downloadFileLocation);
        }

        public async Task BackupAsync(List<string> databases)
        {
            foreach (var item in databases)
            {
                string currentTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

                await new PostgresBackup().PostgreSqlDump($"{Path.GetTempPath()}/{currentTime}.dump", postHost, postPort.ToString(), item, postUsername, postPassword);

                CheckIfDirExsists(item);
                using (var ftp = new FtpClient(ftpHost, ftpUsername, ftpPassword, ftpPort))
                {
                    string fileDump = @$"{Path.GetTempPath()}/{currentTime}.dump";
                    var hasWorked = ftp.UploadFile(fileDump, $"/{item}/{currentTime}.dump");
                    File.Delete(fileDump);

                    if (!hasWorked)
                        throw new Exception("File upload error!");
                }
            }
        }

        public void CheckIfDirExsists(string directory)
        {
            using (var ftp = new FtpClient(ftpHost, ftpUsername, ftpPassword, ftpPort))
            {
                bool exists = ftp.DirectoryExists(directory);
                if (!exists)
                    ftp.CreateDirectory(directory);
            }
        }
    }
}
