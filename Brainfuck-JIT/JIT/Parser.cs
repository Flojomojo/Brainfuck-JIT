using System.Text;

namespace Brainfuck_JIT.JIT;

public class Parser(BrainfuckProgram program, string programString)
{
    /// <summary>
    /// The error class
    /// </summary>
    /// <param name="OpCode">The opcode that causes the error</param>
    /// <param name="Index">The index of the error</param>
    /// <param name="ErrorMessage">The error message</param>
    private record Error(OpCode OpCode, int Index, string ErrorMessage);
   
    /// <summary>
    /// Checks the syntax of a brainfuck program
    /// </summary>
    /// <returns>A array of errors contained in the program</returns>
    private Error[] CheckSyntax()
    {
        List<Error> errors = [];
        List<int> openingBracketsFound = [];
        for (int i = 0; i < program.Instructions.Count; i++)
        {
            OpCode opcode = program.Instructions[i];
            switch (opcode.Type)
            {
                case OpCodeType.JZ:
                    openingBracketsFound.Add(i);
                    break;
                // If there are ] without a corresponding [
                case OpCodeType.JNZ:
                    if (openingBracketsFound.Count <= 0)
                    {
                        Error mismatchedOpeningBracket = new(opcode, FindCorrectIndex(i), "Mismatched closing delimiter");
                        errors.Add(mismatchedOpeningBracket);
                        break;
                    }
                    openingBracketsFound.RemoveAt(openingBracketsFound.Count - 1);
                    break;
            }
        }
        // If there are still some [ without their corresponding ]
        if (openingBracketsFound.Count > 0)
        {
            foreach (int openingBracketIndex in openingBracketsFound)
            {
                OpCode openingBracket = program.Instructions[openingBracketIndex];
                Error mismatchedClosingBracket = new(openingBracket, FindCorrectIndex(openingBracketIndex), "Unclosed delimiter found");
                errors.Add(mismatchedClosingBracket);
            }
        }
        return errors.ToArray();
    }

    /// <summary>
    /// Finds the correct index of a opcode
    /// So if we have the nth opcode what was the original index of the opcode in the source code
    /// </summary>
    /// <param name="index">The "fake" index</param>
    /// <returns>The correct string index</returns>
    private int FindCorrectIndex(int index)
    {
        int count = 0;
        for (int i = 0; i < index; i++)
        {
            OpCode opcode = program.Instructions[i];
            count += opcode.Repetition;
        }
        return count;
    }
   
    /// <summary>
    /// Checks the syntax and handles the errors
    /// </summary>
    public void Parse()
    {
        Error[] errors = CheckSyntax();
        if (errors.Length == 0)
            return;
        const int range = 8;
        foreach (Error error in errors)
        {
            string rebuiltProgramString = RebuildProgramString(error.Index, range);
            const int indicatorStringErrorIndex = range + 1;
            string indicatorString = new(' ', indicatorStringErrorIndex - 1); 
            indicatorString += "^";
            indicatorString += $" {error.ErrorMessage}";
            // Add one to the index because the index is zero based
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("error");
            Console.ResetColor();
            Console.WriteLine($": {error.ErrorMessage} at character {error.Index + 1}");
            Console.ResetColor();
            Console.WriteLine(rebuiltProgramString);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(indicatorString);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("------------");
            Console.ResetColor();
        }
        Environment.Exit(1);
    }

    /// <summary>
    /// Gets the range of characters around the error
    /// </summary>
    /// <param name="index">The index of the error character</param>
    /// <param name="range">The range around the characters</param>
    /// <returns>The correct program string snippet</returns>
    private string RebuildProgramString(int index, int range)
    {
        // TODO do this with only the tokens
        StringBuilder builder = new();
        int startIndex = index - range;
        int endIndex = index + range;
        if (startIndex < 0)
            startIndex = 0;
        if (endIndex > programString.Length - 1)
            endIndex = programString.Length - 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            builder.Append(programString[i]);
        }
        return builder.ToString(); 
    }
}