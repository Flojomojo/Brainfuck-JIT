namespace Brainfuck_JIT;

class Program
{
    static void Main(string[] args)
    {
        string program = LoadProgram("hello_world.bf");
        Console.WriteLine("Loaded program");
        JIT.Run(program);
    }

    static string LoadProgram(string filename)
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