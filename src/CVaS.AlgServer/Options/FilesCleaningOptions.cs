namespace CVaS.AlgServer.Options
{
    public class FilesCleaningOptions
    {
        public int PeriodInMinutes { get; set; }
        public int DrivePressureSpaceInMB { get; set; }
        public double FileCacheRetentionTimeInMinutes { get; set; }
        public int DirectoryMaxSpaceInMB { get; set; }
    }
}