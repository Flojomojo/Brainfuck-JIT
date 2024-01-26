using System.Runtime.InteropServices;
using Brainfuck_JIT.JIT;

namespace Brainfuck_JIT;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Not enough command line args");
            Console.WriteLine("Usage:");
            Console.WriteLine("\t dotnet run {brainfuck_program.bf} {filename_of_output_asm}");
            Environment.Exit(1);
        }
        string bfFilename = args[0];
        string outputFilename = args[1];
        string programString = LoadProgram(bfFilename);
        Console.WriteLine($"Loaded program {bfFilename}");
        BrainfuckProgram program = IR.Parse(programString);
        //Interpreter interpreter = new();
        //interpreter.Run(program); 
        JIT.JIT jit = new();
        jit.Compile(program, outputFilename);
    }

    /// <summary>
    /// Loads a brainfuck program from a file
    /// </summary>
    /// <param name="filename">The file to load from</param>
    /// <returns>The loaded program as a string</returns>
    private static string LoadProgram(string filename)
    {
        // Check if file exists
        if (!File.Exists(filename))
        {
            Console.WriteLine($"{filename} does not exist!");
            return "";
        }
        return File.ReadAllText(filename);
    }
}