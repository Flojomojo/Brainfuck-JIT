using System.Globalization;
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
    public byte[] Memory = new byte[1024];
    private List<OpCode> Instructions = [];
    private int DataPointer = 0;

    public void PrintMemory()
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
        foreach (OpCode opcode in this.Instructions)
        {
            byte value = GetCurrentByte();
            int intValue = (int)value;
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
                    Console.WriteLine(value);
                    break;
                case OpCode.INP:
                    // TODO
                    throw new NotImplementedException();
                    break;
                case OpCode.JZ:
                    // TODO
                    throw new NotImplementedException();
                    break;
                case OpCode.JNZ:
                    // TODO
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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
