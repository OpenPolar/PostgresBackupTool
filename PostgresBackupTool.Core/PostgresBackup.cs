using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace PostgresBackupTool.Core
{
    public class PostgresBackup
    {
        string Set = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "set " : "export ";

        public async Task PostgreSqlDump(
            string outFile,
            string host,
            string port,
            string database,
            string user,
            string password)
        {
            string dumpCommand =
                 $"pg_dump" + " -Fc" + " -h " + host + " -p " + port + " -d " + database + " -U " + user + "";

            string batchContent = "" + dumpCommand + "  > " + "\"" + outFile + "\"" + "\n";
            if (File.Exists(outFile)) File.Delete(outFile);

            await Execute(batchContent);
        }

        public async Task PostgreSqlRestore(
                   string inputFile,
                   string host,
                   string port,
                   string database,
                   string user,
                   string password)
        {
            string dumpCommand = $"psql -h {host} -p {port} -U {user} -d {database} -c \"select pg_terminate_backend(pid) from pg_stat_activity where datname = '{database}'\"\n" +
                                 $"dropdb -h " + host + " -p " + port + " -U " + user + $" {database}\n" +
                                 $"createdb -h " + host + " -p " + port + " -U " + user + $" {database}\n" +
                                 "pg_restore -h " + host + " -p " + port + " -d " + database + " -U " + user + "";

            //psql command disconnect database
            //dropdb and createdb  remove database and create.
            //pg_restore restore database with file create with pg_dump command
            dumpCommand = $"{dumpCommand} {inputFile}";

            await Execute(dumpCommand);
        }

        private Task Execute(string dumpCommand)
        {
            return Task.Run(() =>
            {

                string batFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}." + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bat" : "sh"));
                try
                {
                    string batchContent = "";
                    batchContent += $"{dumpCommand}";

                    File.WriteAllText(batFilePath, batchContent, Encoding.ASCII);

                    ProcessStartInfo info = ProcessInfoByOS(batFilePath);

                    using System.Diagnostics.Process? proc = System.Diagnostics.Process.Start(info);

                    if (proc == null)
                        throw new Exception("Process is null!");

                    proc.WaitForExit();
                    proc.Close();
                }
                catch (Exception e)
                {
                    throw new Exception("Error happened: ", e);

                }
                finally
                {
                    if (File.Exists(batFilePath)) File.Delete(batFilePath);
                }
            });
        }

        private static ProcessStartInfo ProcessInfoByOS(string batFilePath)
        {
            ProcessStartInfo info;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                info = new ProcessStartInfo(batFilePath)
                {
                };
            }
            else
            {
                info = new ProcessStartInfo("sh")
                {
                    Arguments = $"{batFilePath}"
                };
            }

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.RedirectStandardError = true;

            return info;
        }
    }
}