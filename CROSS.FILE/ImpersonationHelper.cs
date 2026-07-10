using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

public static class ImpersonationHelper
{
    public static async Task<T> RunAsync<T>(NetworkCredential credential, Func<Task<T>> action)
    {
        IntPtr tokenHandle = IntPtr.Zero;

        bool success = LogonUser(
            credential.UserName,
            credential.Domain,
            credential.Password,
            2, // Interactive
            0,
            out tokenHandle);

        if (!success)
            throw new Exception("Error en LogonUser");

        using (var safeToken = new SafeAccessTokenHandle(tokenHandle))
        {
            return await WindowsIdentity.RunImpersonated(safeToken, action);
        }
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LogonUser(
        string username,
        string domain,
        string password,
        int logonType,
        int logonProvider,
        out IntPtr token);
}