using System.Globalization;
using System.IO.Enumeration;
using System.Net.Mime;

namespace Brainfuck_JIT;

public enum OpCode
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

class BrainfuckProgram
{
    public byte[] Memory = new byte[12];
    private List<OpCode> Instructions = [];
    private int DataPointer = 0;

    public void DumpMemory()
    {
        Console.Write("|");
        foreach (byte b in Memory)
        {
            Console.Write($"{b.ToString()}|");
        }
    }

    private void Reset()
    {
        Array.Clear(this.Memory, 0, this.Memory.Length);
        this.DataPointer = 0;
    }

    private byte GetCurrentByte()
    {
        return this.Memory[this.DataPointer];
    }

    private void SetCurrentByte(byte value)
    {
        this.Memory[this.DataPointer] = value;
    }

    public void Execute()
    {
        Reset();
        int lastJumpPosition = -1;
        for (int i = 0; i < this.Instructions.Count; i++)
        {
            OpCode opcode = this.Instructions[i];
            byte value = GetCurrentByte();
            int intValue = value;
            switch (opcode)
            { 
                case OpCode.INC:
                    // Get the byte at the current position and increase by one
                    SetCurrentByte((byte)(intValue + 1)); 
                    break;
                case OpCode.DEC:
                    // Get the byte at the current position and dec by one
                    SetCurrentByte((byte)(intValue - 1)); 
                    break;
                case OpCode.INCDP:
                    // Increase the DP by one
                    if (this.DataPointer == this.Memory.Length - 1)
                    {
                        Console.WriteLine($"OOB Memory at {this.DataPointer + 1}");
                        Environment.Exit(1);
                    }
                    this.DataPointer++;
                    break;
                case OpCode.DECDP:
                    // Decrease the DP by one
                    if (this.DataPointer == 0)
                    {
                        Console.WriteLine($"OOB Memory at {this.DataPointer - 1}");
                        Environment.Exit(1);
                    }
                    this.DataPointer--;
                    break;
                case OpCode.OUT:
                    // Print out the current byte
                    Console.Write(Convert.ToChar(value));
                    break;
                case OpCode.INP:
                    // TODO
                    throw new NotImplementedException();
                    break;
                case OpCode.JZ:
                    // if the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
                    if (intValue == 0)
                    {
                        if (lastJumpPosition == -1)
                        {
                            Console.WriteLine($"Invalid jump position {lastJumpPosition}");
                            Environment.Exit(0);
                        }
                        i = lastJumpPosition;
                        break;
                    }
                    lastJumpPosition = i;
                    break;
                case OpCode.JNZ:
                    // if the byte at the data pointer is non-zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
                    if (intValue != 0)
                    {
                        if (lastJumpPosition == -1)
                        {
                            Console.WriteLine($"Invalid jump position {lastJumpPosition}");
                            Environment.Exit(0);
                        }
                        i = lastJumpPosition;
                        break;
                    }
                    lastJumpPosition = i;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        Console.WriteLine("");
    }

    public void AppendOpcode(OpCode opcode)
    {
       this.Instructions.Add(opcode); 
    }
}

class JIT
{
    public static void Run(string programString)
    {
        // Remove all the unnecessary whitespace
        string normalizedProgramString = StringHelper.RemoveWhitespace(programString);
        BrainfuckProgram program = Parse(normalizedProgramString);
        program.Execute();
        program.DumpMemory();
    }

    private static BrainfuckProgram Parse(string programString)
    {
        BrainfuckProgram program = new();
        foreach (char opcode in programString)
        {
            if (!Enum.IsDefined(typeof(OpCode), (int)opcode))
            {
                Console.WriteLine($"Invalid opcode: {opcode}");
                Environment.Exit(1);
            }
            OpCode parsedOpCode = (OpCode)opcode;
            program.AppendOpcode(parsedOpCode);
        }
        return program;
    }
}
