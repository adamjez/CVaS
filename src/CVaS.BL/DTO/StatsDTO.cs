using System;

namespace CVaS.BL.DTO
{
    public class StatsDTO
    {
        public string BrokerStatus { get; set; }
        public int? BrokerClients { get; set; }
        public int RunCountLastHour { get; set; }
        public int RunCountLastDay { get; set; }
        public int ActiveUserCountLastDay { get; set; }
        public int RegisterUserCountThisWeek { get; set; }
        public int UploadedFilesCountThisWeek { get; set; }
        public long UploadedFilesSizeThisWeek { get; set; }
        public int UploadedFilesSizeInMBThisWeek => (int)(UploadedFilesSizeThisWeek / Math.Pow(10, 6));
    }
}