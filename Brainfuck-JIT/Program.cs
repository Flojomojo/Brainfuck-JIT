using System.Runtime.InteropServices;
using Brainfuck_JIT.JIT;

namespace Brainfuck_JIT;

class Program
{
    public static void Main(string[] args)
    {
        string programString = LoadProgram("hello_world.bf");
        Console.WriteLine("Loaded program");
        BrainfuckProgram program = IR.Parse(programString);
        //Interpreter interpreter = new();
        //interpreter.Run(program); 
        JIT.JIT jit = new();
        jit.Compile(program);
    }

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