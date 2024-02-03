using System;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace PostgresBackupTool.Core
{
	public class FtpClient : IDisposable
	{
        private SftpClient sftpClient;
        public FtpClient(string ftpHost, string ftpUsername, string ftpPassword, int ftpPort)
		{
            sftpClient = new SftpClient(ftpHost, ftpPort, ftpUsername, ftpPassword);
            sftpClient.Connect();
		}

        public bool DownloadFile(string remotePath, string localPath)
        {
            try
            {
                using (var fs = new FileStream(Path.GetFileName(localPath), FileMode.OpenOrCreate))
                {
                    sftpClient.DownloadFile(remotePath, fs);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public IEnumerable<SftpFile> GetListing(string remotePath)
        {
            try
            {
                return sftpClient.ListDirectory(remotePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<SftpFile>();
            }
        }

        public void CreateDirectory(string remotePath)
        {
            try
            {
                sftpClient.CreateDirectory(remotePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool DirectoryExists(string remotePath)
        {
            try
            {
                return sftpClient.Exists(remotePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool UploadFile(string localPath, string remotePath)
        {
            try
            {
                using (var fs = new FileStream(localPath, FileMode.Open))
                {
                    sftpClient.UploadFile(fs, remotePath);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void Dispose()
        {
            sftpClient.Disconnect();
            sftpClient.Dispose();
        }
    }
}

