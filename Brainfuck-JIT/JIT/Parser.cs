using System.Text;

namespace Brainfuck_JIT.JIT;

public class Parser(BrainfuckProgram program, string programString)
{
    private record Error(OpCode OpCode, int Index, string ErrorMessage);
    
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
        // If there are still some open brackets open without their corresponding closing bracket
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

    private int FindCorrectIndex(int index)
    {
        // Get all the tokens before the index and count them
        int count = 0;
        for (int i = 0; i < index; i++)
        {
            OpCode opcode = program.Instructions[i];
            count += opcode.Repetition;
        }
        return count;
    }
    
    public void Parse()
    {
        Error[] errors = CheckSyntax();
        if (errors.Length == 0)
            return;
        const int range = 5;
        foreach (Error error in errors)
        {
            string rebuiltProgramString = RebuildProgramString(error.Index, range);
            const int indicatorStringErrorIndex = range + 1;
            string indicatorString = new(' ', indicatorStringErrorIndex - 1); 
            indicatorString += "^";
            indicatorString += $" {error.ErrorMessage}";
            // Add one to the index because the index is zero based
            Console.WriteLine($"error: {error.ErrorMessage} at character {error.Index + 1}");
            Console.WriteLine(rebuiltProgramString);
            Console.WriteLine(indicatorString);
            Console.WriteLine("------------");
        }
        Environment.Exit(1);
    }

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