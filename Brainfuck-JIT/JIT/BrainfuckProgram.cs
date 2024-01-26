using System.Diagnostics.CodeAnalysis;

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

public record OpCode(OpCodeType Type, int Repetition);
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