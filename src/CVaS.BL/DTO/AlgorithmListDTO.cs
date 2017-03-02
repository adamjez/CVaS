namespace CVaS.BL.DTO
{
    public class AlgorithmListDTO
    {
        public string CodeName { get; set; }
        public string Description { get; set; }
    }

    public class AlgorithmStatsListDTO
    {
        public string CodeName { get; set; }
        public string Description { get; set; }
        public int LaunchCount { get; set; }
        public int LaunchCountLastHour { get; set; }
        public int LaunchCountLastDay { get; set; }

    }

    public class AlgorithmDTO
    {
        public string CodeName { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
}