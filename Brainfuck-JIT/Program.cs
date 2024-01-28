using System.Runtime.InteropServices;
using Brainfuck_JIT.JIT;
using CommandLine;

namespace Brainfuck_JIT;

class Program
{

    /// <summary>
    /// Command Line Arguments
    /// </summary>
    public class CommandLineArgs
    {
        [Value(0, MetaName = "bf_filename", Required = true, HelpText = "Filename of the bf program.")]
        public string? BfFilename { get; set; } 
        [Option('i', "interpret", Default=false, HelpText = "Instead of jitting the file, interpret it.")]
        public bool Interpret { get; set; }
        [Option("comment", Default = true, HelpText = "Add comments to the *.asm file (slower to generate the code, same execution time)")]
        public bool Comment { get; set; }
    }
    public static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<CommandLineArgs>(args)
            .WithParsed(Run);
    }

    /// <summary>
    /// Actually run the program
    /// </summary>
    /// <param name="args">The args from the cmdline</param>
    public static void Run(CommandLineArgs args)
    {
        string bfFilename = args.BfFilename!;
        string outputFilename = bfFilename.Split(".")[0];
        string programString = LoadProgram(bfFilename);
        Console.WriteLine($"Loaded program '{bfFilename}'");
        BrainfuckProgram program = IR.Parse(programString);
        if (args.Interpret)
        {
            Interpreter interpreter = new();
            interpreter.Run(program);
        }
        else
        {
            JIT.JIT.Compile(program, outputFilename, args.Comment);
        }
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