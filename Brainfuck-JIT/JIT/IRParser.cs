namespace Brainfuck_JIT.JIT;

public static class IRParser
{
    /// <summary>
    /// Parse a program string into a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program as a string</param>
    /// <returns>The parsed brainfuck program</returns>
    public static BrainfuckProgram Parse(string programString)
    {
        // Tokenize 
        Lexer lexer = new Lexer(programString);
        BrainfuckProgram program = lexer.Tokenize();
        // Parse and report syntax errors
        Parser parser = new(program, programString);
        parser.Parse();
        return program;
    }
}