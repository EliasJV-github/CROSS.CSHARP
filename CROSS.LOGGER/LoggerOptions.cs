namespace CROSS.LOGGER
{
    public class LoggerOptions
    {
        public string Path { get; set; } = string.Empty;

        public string File { get; set; } = string.Empty;

        public string Template { get; set; } = string.Empty;

        public int FileSizeLimitMegaBytes { get; set; }

        public int Frame { get; set; }

        public string LogEventLevel { get; set; } = string.Empty;

        public int RetainedFileCountLimit { get; set; }
    }
}