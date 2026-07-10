using CROSS.SECRYPT;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CROSS.FILE
{
    public class FileServer : IFileServer
    {
        private readonly IManagerSecrypt secrypt;
        private readonly FileServerConfig fileServerConfig;

        public FileServer(IConfiguration configuration, IManagerSecrypt secrypt)
        {
            this.secrypt = secrypt;

            FileServerConfig fileServerConfig = new();
            configuration.GetSection("FileServerConfig").Bind(fileServerConfig);
            this.fileServerConfig = fileServerConfig;
        }


        public async Task<string> SaveFile(string destination, byte[] byteFile, string filename = "")
        {
            if (byteFile == null || byteFile.Length == 0)
                throw new Exception("Archivo vacío");

            if (string.IsNullOrWhiteSpace(filename))
                filename = Guid.NewGuid().ToString("N");

            string fullPath = BuildPath(destination, filename);

            return await RunAsUser(async () =>
            {
                string? dir = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);

                await File.WriteAllBytesAsync(fullPath, byteFile);

                return fullPath;
            });
        }

        public async Task<byte[]> OpenFile(string destination)
        {
            string fullPath = BuildPath(destination);

            return await RunAsUser(async () =>
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Archivo no encontrado");

                return await File.ReadAllBytesAsync(fullPath);
            });
        }
        public async Task<bool> DeleteFile(string destination)
        {
            string fullPath = BuildPath(destination);
            return await RunAsUser(async () =>
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Archivo no encontrado");
                File.Delete(fullPath);
                return true;
            });
        }
        public async Task<string> ChangeNameFile(string fullPath, string newFileName)
        {
            return await RunAsUser(async () =>
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Archivo no encontrado", fullPath);

                string directory = Path.GetDirectoryName(fullPath)!;
                string extension = Path.GetExtension(fullPath);

                // Aseguramos que mantenga extensión si no viene incluida
                if (!newFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    newFileName += extension;

                string newFullPath = Path.Combine(directory, newFileName);

                if (File.Exists(newFullPath))
                    throw new IOException("Ya existe un archivo con ese nombre");

                File.Move(fullPath, newFullPath);

                return newFullPath;
            });
        }

        // =========================
        // Helpers
        // =========================

        private string BuildPath(string destination, string filename = "")
        {
            string root;

            if (fileServerConfig.CarpetaPrincipal.StartsWith(@"\\"))
                root = fileServerConfig.CarpetaPrincipal;
            else
                root = $@"\\{fileServerConfig.Servidor}\{fileServerConfig.CarpetaPrincipal}";

            if (!string.IsNullOrWhiteSpace(filename))
                return Path.Combine(root, destination, filename);

            return Path.Combine(root, destination);
        }

        private Task<T> RunAsUser<T>(Func<Task<T>> action)
        {
            var credential = new NetworkCredential(fileServerConfig.User, secrypt.Desencriptar(fileServerConfig.Password), fileServerConfig.Domain);
            return ImpersonationHelper.RunAsync(credential, action);
        }

    }
}
