using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Brainfuck_JIT.JIT;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum OpCodeType
{
    INC = '+',
    DEC = '-',
    INCDP = '>',
    DECDP = '<',
    OUT = '.',
    INP = ',',
    JZ = '[',
    JNZ = ']'
}

public class OpCode(OpCodeType type, int repetition)
{
    public OpCodeType Type { get; set; } = type;
    public int Repetition { get; set; } = repetition;

    /// <summary>
    /// The asm of the opcode
    /// </summary>
    public string[] Asm
    {
        get
        {
            string[] lines = _opCodeToAsmTable.TryGetValue(this.Type, out string[]? value) ? value : [""];
            if (this.Type == OpCodeType.OUT)
            {
               // There is no way of easily outputting twice in assembly so we just have to duplicate the code as many times as we have repetitions
               string[] duplicatedArray = new string[lines.Length * this.Repetition];

               for (int i = 0; i < this.Repetition; i++)
               {
                   Array.Copy(lines, 0, duplicatedArray, i * lines.Length, lines.Length);
               }

               lines = duplicatedArray;
            }
            string[] newlines = new string[lines.Length];
            // Add a tab to each lines for better readability
            // Except the first line which is the label
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Split(" ").Length == 1 && lines[i].Contains("LOOP"))
                    newlines[i] = lines[i];
                else
                    newlines[i] = "\t" + lines[i];
            }
            return newlines;
        }
    }


    /// <summary>
    /// The table of opcodes to assembly
    /// </summary>
    private readonly Dictionary<OpCodeType, string[]> _opCodeToAsmTable = new()
    {
        { OpCodeType.INC, [$"add byte [r12], {repetition}"]},
        { OpCodeType.DEC, [$"sub byte [r12], {repetition}"]},
        { OpCodeType.INCDP, [$"add r12, {repetition}"]},
        { OpCodeType.DECDP, [$"sub r12, {repetition}"]},
        { OpCodeType.OUT, [
                "mov rax, SYS_WRITE",
                "mov rdi, STDOUT",
                "mov rsi, r12",
                "mov rdx, 1",
                "syscall"
            ]},
        { OpCodeType.JZ, [
            "cmp byte [r12], 0",
            $"je END_LOOP_{repetition-1}",
            $"START_LOOP_{repetition}:"
        ]},
        { OpCodeType.JNZ, [
            "cmp byte [r12], 0",
            $"jne START_LOOP_{repetition}",
            $"END_LOOP_{repetition-1}:"
        ]},
        { OpCodeType.INP, [
            "mov rax, SYS_READ",
            "mov rdi, STDIN",
            "mov rsi, r12",
            "mov rdx, 1",
            "syscall"
        ]}
    };

    public override string ToString()
    {
        return $"{{ Type = {Type}, Repetition = {Repetition} }}";
    }
}
public class BrainfuckProgram
{

    /// <summary>
    /// The instructions to execute
    /// </summary>
    public readonly List<OpCode> Instructions = [];
    
    /// <summary>
    /// Append a opcode to the list of opcodes
    /// </summary>
    /// <param name="opcode">The opcode to append</param>
    public void AppendOpcode(OpCode opcode)
    {
        this.Instructions.Add(opcode);
    }
}