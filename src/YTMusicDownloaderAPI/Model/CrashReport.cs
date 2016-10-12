namespace YTMusicDownloaderAPINet.Model
{
    public class CrashReport
    {
        public string ProcessName { get; set; }
        public string Time { get; set; }
        public string AssemblyVersion { get; set; }
        public int MemoryAllocated { get; set; }
        public string Guid { get; set; }
        public string MachineName { get; set; }
        public string Exception { get; set; }
    }
}