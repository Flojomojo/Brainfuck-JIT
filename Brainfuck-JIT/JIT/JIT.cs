namespace Brainfuck_JIT.JIT;

public class JIT
{
    public void Compile(BrainfuckProgram program)
    {
        // Load the boilerplate.asm file
        string[] boilerplate = File.ReadAllLines("boilerplate.asm");
        const string replaceWord = "{{BRAINFUCK_HERE}}";
        // Split the boilerplate file into header and footer
        // Header starts at 0 and ends at the line before replaceWord
        // Footer starts at the line after replaceWorld and ends at EOF
        int replaceWordIndex = FindReplaceWorldIndex(replaceWord, boilerplate);
        if (replaceWordIndex == -1)
        {
            Console.WriteLine($"Could not find replace word {replaceWord} in boilerplate asm code");
            Environment.Exit(1);
        }

        string[] header = boilerplate[0..(replaceWordIndex)];
        string[] footer = boilerplate[(replaceWordIndex + 1)..];
        string[] code = ToAsm(program);
        WriteFile(header,footer, code, "test.asm");
    }

    private static int FindReplaceWorldIndex(string replaceWord, IReadOnlyList<string> fileContent)
    {
        for (int i = 0; i < fileContent.Count; i++)
        {
            if (fileContent[i].Contains(replaceWord))
                return i;
        }

        return -1;
    }

    private static void WriteFile(string[] header, string[] footer, string[] code, string filename)
    {
        List<string> result = [];
        result.AddRange(header);
        result.AddRange(code);
        result.AddRange(footer);
        File.WriteAllLines(filename, result);
        // TODO compile it
    }

    private static string[] ToAsm(BrainfuckProgram program)
    {
        List<string> result = [];
        foreach (OpCode instruction in program.Instructions)
        {
            result.AddRange(instruction.Asm);
        }
        return result.ToArray();
    }
}