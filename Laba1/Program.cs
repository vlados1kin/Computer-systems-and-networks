using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Laba1;

public class Program
{
    [DllImport("mpr.dll")]
    private static extern int WNetOpenEnum(int dwScope, int dwType, int dwUsage,
        IntPtr lpNetResource, out IntPtr lphEnum);

    [DllImport("mpr.dll")]
    private static extern int WNetEnumResource(IntPtr hEnum, ref int lpcCount,
        IntPtr lpBuffer, ref int lpBufferSize);

    [DllImport("mpr.dll")]
    private static extern int WNetCloseEnum(IntPtr hEnum);

    [StructLayout(LayoutKind.Sequential)]
    public class NETRESOURCE
    {
        public int dwScope;
        public int dwType;
        public int dwDisplayType;
        public int dwUsage;
        public string lpLocalName;
        public string lpRemoteName;
        public string lpComment;
        public string lpProvider;
    }
    
    public static void Main()
    {
        Console.WriteLine("TASK 1");
        Task1();
        Console.WriteLine("TASK 2");
        Task2(IntPtr.Zero);
        
        
        Console.ReadKey();
    }

    private static void Task1()
    {
        NetworkInterface[] networkInterface = NetworkInterface.GetAllNetworkInterfaces();
        if (networkInterface.Length < 1)
            throw new Exception("there is no network interface");
        foreach (NetworkInterface ni in networkInterface)
        {
            Console.WriteLine($"ID: {ni.Id}");
            Console.WriteLine($"Name: {ni.Name}");
            Console.WriteLine($"Description: {ni.Description}");
            Console.WriteLine($"Type: {ni.NetworkInterfaceType}");
            Console.WriteLine($"MAC-Address: {ni.GetPhysicalAddress().ToString()}");
            Console.WriteLine("==============================");
        }
    }

    private static void Task2(IntPtr lpnr)
    {
        IntPtr hEnum;
        int bufferSize = 16384;
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
        int count = -1;
        int result = WNetOpenEnum(2, 0, 0, lpnr, out hEnum);
        if (result == 0)
        {
            result = WNetEnumResource(hEnum, ref count, buffer, ref bufferSize);
            if (result == 0)
            {
                IntPtr currentPtr = buffer;
                for (int i = 0; i < count; i++)
                {
                    NETRESOURCE nr = (NETRESOURCE) Marshal.PtrToStructure(currentPtr, typeof(NETRESOURCE));
                    DisplayStruct(nr);
                    if ((nr.dwUsage & 2) == 2)
                        Task2(currentPtr);
                    currentPtr += Marshal.SizeOf(typeof(NETRESOURCE));
                }
            }
            WNetCloseEnum(hEnum);
        }
        Marshal.FreeHGlobal(buffer);
    }

    private static void DisplayStruct(NETRESOURCE nr)
    {
        switch (nr.dwScope)
        {
            case 1:
                Console.WriteLine("Scope: connected");
                break;
            case 2:
                Console.WriteLine("Scope: all resources");
                break;
            case 3:
                Console.WriteLine("Scope: remembered");
                break;
            default:
                Console.WriteLine("Scope: unknown scope");
                break;
        }
        switch (nr.dwType)
        {
            case 1:
                Console.WriteLine("Type: any");
                break;
            case 2:
                Console.WriteLine("Type: disk");
                break;
            case 3:
                Console.WriteLine("Type: print");
                break;
            default:
                Console.WriteLine("Type: unknown type");
                break;
        }
        switch (nr.dwDisplayType)
        {
            case 0:
                Console.WriteLine("DisplayType: generic");
                break;
            case 1:
                Console.WriteLine("DisplayType: domain");
                break;
            case 2:
                Console.WriteLine("DisplayType: server");
                break;
            case 3:
                Console.WriteLine("DisplayType: share");
                break;
            case 4:
                Console.WriteLine("DisplayType: file");
                break;
            case 5:
                Console.WriteLine("DisplayType: group");
                break;
            case 6:
                Console.WriteLine("DisplayType: network provider");
                break;
            default:
                Console.WriteLine("DisplayType: unknown display type");
                break;
        }
        if ((nr.dwUsage & 1) == 1)
            Console.WriteLine($"Usage: connectable");
        if ((nr.dwUsage & 2) == 2)
            Console.WriteLine($"Usage: container");
        Console.WriteLine($"Resource LocalName: {nr.lpLocalName}");
        Console.WriteLine($"Resource RemoteName: {nr.lpRemoteName}");
        Console.WriteLine($"Resource Comment: {nr.lpComment}");
        Console.WriteLine($"Resource Provider: {nr.lpProvider}");
        Console.WriteLine("==============================");
    }
}

