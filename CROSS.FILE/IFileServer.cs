namespace CROSS.FILE
{
    public interface IFileServer
    {
        public Task<string> SaveFile(string destination, byte[] byteFile, string filename = "");
        public Task<byte[]> OpenFile(string destination);
        public Task<bool> DeleteFile(string destination);
        public Task<string> ChangeNameFile(string fullPath, string newFileName);
    }
}
