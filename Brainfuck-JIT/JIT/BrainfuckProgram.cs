using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
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

public class OpCode(OpCodeType type, int repetition, int index)
{
    public OpCodeType Type { get; set; } = type;

    public int Repetition = repetition;

    /// <summary>
    /// The asm of the opcode
    /// </summary>
    public string[] RawAsm
    {
        get
        {
            this.UpdateTable();
            string[] lines = _opCodeToAsmTable.TryGetValue(this.Type, out string[]? value) ? value : [$"; {this.Type}"];
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
            string[] newlines = new string[lines.Length + 1];
            // Add a tab to each lines for better readability
            // Except the first line which is the label
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Split(" ").Length == 1 && lines[i].Contains("LOOP"))
                    newlines[i] = lines[i];
                else
                    newlines[i] = "\t" + lines[i];
                // Also append a comment with the opcode
                newlines[i] += $"\t";
            }
            return newlines;
        }
    }
    /// <summary>
    /// The asm of the opcode with comments
    /// </summary>
    public string[] Asm
    {
        get
        {
            this.UpdateTable();
            string[] lines = _opCodeToAsmTable.TryGetValue(this.Type, out string[]? value) ? value : [$"; {this.Type}"];
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
            string[] newlines = new string[lines.Length + 1];
            // Add a tab to each lines for better readability
            // Except the first line which is the label
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Split(" ").Length == 1 && lines[i].Contains("LOOP"))
                    newlines[i] = lines[i];
                else
                    newlines[i] = "\t" + lines[i];
                // Also append a comment with the opcode
                newlines[i] += $"\t";
            }
            // Add a comment with the opcode
            // First shift all the lines by one
            Array.Copy(newlines, 0, newlines, 1, newlines.Length - 1);
            // Then add the comment
            newlines[0] = $"\t; {this.Type}({this.Repetition})";
            return newlines;
        }
    }

    private void UpdateTable()
    {
        this._opCodeToAsmTable = new Dictionary<OpCodeType, string[]>
        {
            { OpCodeType.INC, [$"add byte [r12], {this.Repetition}"]},
            { OpCodeType.DEC, [$"sub byte [r12], {this.Repetition}"]},
            { OpCodeType.INCDP, [$"add r12, {this.Repetition}"]},
            { OpCodeType.DECDP, [$"sub r12, {this.Repetition}"]},
            { OpCodeType.OUT, [
                    "mov rax, SYS_WRITE",
                    "mov rdi, STDOUT",
                    "mov rsi, r12",
                    "mov rdx, 1",
                    "syscall"
                ]},
            { OpCodeType.JZ, [
                "cmp byte [r12], 0",
                $"je END_LOOP_{this.Repetition}",
                $"START_LOOP_{index}:"
            ]},
            { OpCodeType.JNZ, [
                "cmp byte [r12], 0",
                $"jne START_LOOP_{this.Repetition}",
                $"END_LOOP_{index}:"
            ]},
            { OpCodeType.INP, [
                "mov rax, SYS_READ",
                "mov rdi, STDIN",
                "mov rsi, r12",
                "mov rdx, 1",
                "syscall"
            ]}
        };
    }


    /// <summary>
    /// The table of opcodes to assembly
    /// </summary>
    private Dictionary<OpCodeType, string[]> _opCodeToAsmTable = new();

    public override string ToString()
    {
        return $"{{ Type = {Type}, Repetition = {Repetition}, Index = {index} }}";
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