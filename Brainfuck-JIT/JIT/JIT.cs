using System.Diagnostics;

namespace Brainfuck_JIT.JIT;

public class JIT
{
    public void Compile(BrainfuckProgram program, string filename)
    {
        // Load the boilerplate.asm file
        string[] boilerplate = File.ReadAllLines("boilerplate.asm");
        const string replaceWord = "{{BRAINFUCK_HERE}}";
        // Split the boilerplate file into header and footer
        // Header starts at 0 and ends at the line before replaceWord
        // Footer starts at the line after replaceWorld and ends at EOF
        int replaceWordIndex = FindReplaceWorldIndex(replaceWord, boilerplate);
        if (replaceWordIndex == -1)
        {
            Console.WriteLine($"Could not find replace word {replaceWord} in boilerplate asm code");
            Environment.Exit(1);
        }

        string[] header = boilerplate[0..(replaceWordIndex)];
        string[] footer = boilerplate[(replaceWordIndex + 1)..];
        string[] code = ToAsm(program);
        WriteFile(header,footer, code, $"{filename}.asm");
        RunCompileCommand(filename);
    }
    
    static bool IsCommandAvailable(string commandName, string arguments)
    {
        try
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = commandName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            // An exception is thrown if the command is not found
            return false;
        }
    } 
    
    private static void ExecuteCommand(string command)
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.StandardInput.WriteLine(command);
        process.StandardInput.Close();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Command failed with exit code {process.ExitCode}");
            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"Error: {error}");
        }
        else
        {
            Console.WriteLine($"Command executed successfully");
        }
    }

    private static void RunCompileCommand(string filename)
    {
        if (!IsCommandAvailable("nasm", "--version"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Nasm is not installed or could not be found!");
            Console.ResetColor();
            Environment.Exit(1);
        }
        // Compile
        ExecuteCommand($"nasm -felf64 {filename}.asm");
        // Link
        ExecuteCommand($"ld -o {filename} {filename}.o");
        // Remove the intermediate object file
        ExecuteCommand($"rm {filename}.o");
    }

    private static int FindReplaceWorldIndex(string replaceWord, IReadOnlyList<string> fileContent)
    {
        for (int i = 0; i < fileContent.Count; i++)
        {
            if (fileContent[i].Contains(replaceWord))
                return i;
        }

        return -1;
    }

    private static void WriteFile(string[] header, string[] footer, string[] code, string filename)
    {
        List<string> result = [];
        result.AddRange(header);
        result.AddRange(code);
        result.AddRange(footer);
        File.WriteAllLines(filename, result);
        // TODO compile it
    }

    private static string[] ToAsm(BrainfuckProgram program)
    {
        List<string> result = [];
        foreach (OpCode instruction in program.Instructions)
        {
            result.AddRange(instruction.Asm);
        }
        return result.ToArray();
    }
}