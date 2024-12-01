using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2 || args[0] != "-e")
        {
            Console.WriteLine("Usage: -e <exeFilePath>");
            return;
        }

        string exeFilePath = args[1];

        if (!File.Exists(exeFilePath))
        {
            Console.WriteLine($"Error: The file '{exeFilePath}' does not exist.");
            return;
        }

        string exeDirectory = Path.GetDirectoryName(exeFilePath);

        // get PATH
        string[] systemPaths = Environment.GetEnvironmentVariable("PATH")
                                          .Split(';')
                                          .Where(p => !string.IsNullOrWhiteSpace(p))
                                          .ToArray();

        // get dependency
        List<string> dependentDlls = GetDependentDlls(exeFilePath);

        if (dependentDlls.Count == 0)
        {
            Console.WriteLine("No dependencies found for the exe file.");
            return;
        }

        // seek for dll
        foreach (string dll in dependentDlls)
        {
            string dllPath = FindDllInPaths(dll, systemPaths);

            if (dllPath != null)
            {
                // copy dll
                string destinationPath = Path.Combine(exeDirectory, Path.GetFileName(dllPath));
                if (File.Exists(destinationPath))
                {
                    Console.WriteLine($"DLL '{dll}' already exists at '{destinationPath}'. Skipping.");
                }
                else
                {
                    File.Copy(dllPath, destinationPath);
                    Console.WriteLine($"Copied '{dll}' from '{dllPath}' to '{destinationPath}'.");
                }
            }
            else
            {
                Console.WriteLine($"DLL '{dll}' not found in system PATH.");
            }
        }

        Console.WriteLine("Dependency analysis and DLL copy complete.");
    }

    // get dll list
    private static List<string> GetDependentDlls(string exeFilePath)
    {
        List<string> dlls = new List<string>();

        try
        {
            // use dumpbin
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dumpbin",
                Arguments = $"/DEPENDENTS \"{exeFilePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            using (var reader = process.StandardOutput)
            {
                string output = reader.ReadToEnd();
                string[] lines = output.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains(".dll"))
                    {
                        string dllName = line.Trim().Split(' ')[0];
                        if (!dlls.Contains(dllName))
                        {
                            dlls.Add(dllName);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing dependencies: {ex.Message}");
        }

        return dlls;
    }

    // search dll
    private static string FindDllInPaths(string dllName, string[] systemPaths)
    {
        foreach (string path in systemPaths)
        {
            string dllPath = Path.Combine(path, dllName);
            if (File.Exists(dllPath))
            {
                return dllPath;
            }
        }
        return null;
    }
}
