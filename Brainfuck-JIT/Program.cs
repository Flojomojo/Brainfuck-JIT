namespace Brainfuck_JIT;

class Program
{
    static void Main(string[] args)
    {
        string program = File.ReadAllText(@"F:\Desktop HDD\repos\Brainfuck-JIT\Brainfuck-JIT\hello_world.bf");
        Console.WriteLine("Loaded program");
        JIT.Run(program);
    }
}