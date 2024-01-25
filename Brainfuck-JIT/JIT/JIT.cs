namespace Brainfuck_JIT.JIT;

public record OpCode(OpCodeType Type, int Repetition);

public static class JIT
{
    /// <summary>
    /// Runs a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program</param>
    public static void Run(string programString)
    {
        // Parse and execute the program
        Lexer lexer = new Lexer(programString);
        BrainfuckProgram program = lexer.Tokenize();
        Parser parser = new(program, programString);
        parser.Parse();
        program.Execute();
    }
}