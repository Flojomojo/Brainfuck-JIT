namespace Brainfuck_JIT.JIT;

public static class IR
{
    public static BrainfuckProgram Parse(string programString)
    {
        // Parse and execute the program
        Lexer lexer = new Lexer(programString);
        BrainfuckProgram program = lexer.Tokenize();
        Parser parser = new(program, programString);
        parser.Parse();
        return program;
    }
}