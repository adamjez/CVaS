namespace CVaS.Shared.Options
{
    public class FilesCleaningOptions
    {
        public int PeriodInMinutes { get; set; } = 10;
        public int DrivePressureSpaceInMB { get; set; } = 1000;
        public double FileCacheRetentionTimeInMinutes { get; set; } = 5;
        public int DirectoryMaxSpaceInMB { get; set; } = 5000;
    }
}