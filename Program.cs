using Blake3;

namespace Blake3Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Please provide a filename as an argument.");
                return;
            }
            if (args[0].Contains("?"))
            {
                // Display usage
                // Get the exe name
                var exeName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                Console.WriteLine($"Calculates the BLAKE3 hash of a file.");
                Console.WriteLine($"  {exeName} <filename>");
                Console.WriteLine($"Install context menu");
                Console.WriteLine($"  {exeName} -i");
                return;
            }
            if (args[0] == "-i")
            {
                // Install context menu for current user
                var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (exeName == null)
                {
                    Console.WriteLine("Unable to determine executable name.");
                    return;
                }
                using (var reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\Blake3"))
                {
                    if (reg == null)
                    {
                        Console.WriteLine("Failed to create registry key.");
                        return;
                    }
                    reg.SetValue("", "Calculate Blake3 hash");
                    using (var commandKey = reg.CreateSubKey("command"))
                    {
                        if (commandKey == null)
                        {
                            Console.WriteLine("Failed to create command subkey.");
                            return;
                        }
                        commandKey.SetValue("", $"{exeName} \"%1\" pause");
                    }
                }
                Console.WriteLine("Context menu installed.");
                return;
            }
            if (!System.IO.File.Exists(args[0]))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            // Create a new hasher for the file
            using var fileHasher = Hasher.New();

            // Read the filename from args, open a stream, read the file and hash it
            var filename = args[0];
            using var fileStream = System.IO.File.OpenRead(filename);
            while (fileStream.Position < fileStream.Length)
            {
                var buffer = new byte[4096];
                var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                fileHasher.Update(buffer.AsSpan(0, bytesRead));
            }

            // Finalize the file hash and output it
            byte[] actualFileHash = fileHasher.Finalize().AsSpan().ToArray();
            Console.WriteLine($"File hash: {BitConverter.ToString(actualFileHash).Replace("-", "")}");

            if (args.Length > 1 && args[1] == "pause")
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
