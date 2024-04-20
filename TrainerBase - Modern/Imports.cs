using System.Runtime.InteropServices;

namespace TrainerBase;

public static partial class Imports
{
    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial void CloseHandle(IntPtr handle);
}