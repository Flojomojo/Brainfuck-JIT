using Brainfuck_JIT.JIT;

namespace Brainfuck_JIT;

class Program
{
    public static void Main(string[] args)
    {
        string program = LoadProgram("hello_world.bf");
        Console.WriteLine("Loaded program");
        Interpreter interpreter = new();
        interpreter.Run(program); 
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