using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PostgresBackupTool.Core;

namespace PostgresBackupTool.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BackupRestoreManager : ControllerBase
    {
        private readonly ILogger<BackupRestoreManager> _logger;
        private PostgresFtpBackup postgresFtpBackup;

        public BackupRestoreManager(ILogger<BackupRestoreManager> logger)
        {
#pragma warning disable CS8604 // M�gliches Nullverweisargument.
            postgresFtpBackup = new PostgresFtpBackup(Environment.GetEnvironmentVariable("FTP_HOST"),
                int.Parse(Environment.GetEnvironmentVariable("FTP_PORT")), 
                Environment.GetEnvironmentVariable("FTP_USER"), 
                Environment.GetEnvironmentVariable("FTP_PASS"), 
                Environment.GetEnvironmentVariable("POSTGRES_HOST"), 
                int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")), 
                Environment.GetEnvironmentVariable("POSTGRES_USER"),
                Environment.GetEnvironmentVariable("POSTGRES_PASS"));
#pragma warning restore CS8604 // M�gliches Nullverweisargument.
            _logger = logger;
        }

        [HttpPost("/backup")]
        public async Task<ActionResult> Backup(string database)
        {
            await postgresFtpBackup.BackupAsync(new List<string>() { database });
            return StatusCode(200);
        }

        [HttpPost("/backupMultiple")]
        public async Task<ActionResult> BackupMultiple(List<string> databases)
        {
            await postgresFtpBackup.BackupAsync(databases);
            return StatusCode(200);
        }

        [HttpGet("/getDumps")]
        public async Task<ActionResult> GetAllDumps(string database)
        {
            return StatusCode(200, JsonConvert.SerializeObject(postgresFtpBackup.ListAllDumps(database)));
        }

        [HttpPost("/restore")]
        public async Task<ActionResult> Restore(string database, string dump)
        {
            await postgresFtpBackup.RestoreAsync(database, dump);
            return StatusCode(200);
        }
    }
}