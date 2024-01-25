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
    /// <summary>
    /// The program consists of x cells each containing one single byte
    /// </summary>
    public byte[] Memory = new byte[32768];
    /// <summary>
    /// The instructions to execute
    /// </summary>
    private List<OpCode> Instructions = [];
    /// <summary>
    /// The pointer to the cell
    /// </summary>
    private int DataPointer = 0;

    /// <summary>
    /// Dumps the memory of the program and prints it to the console
    /// </summary>
    public void DumpMemory()
    {
        Console.Write("|");
        foreach (byte b in Memory)
        {
            Console.Write($"{b.ToString()}|");
        }
    }

    /// <summary>
    /// Resets the program to its base state
    /// </summary>
    private void Reset()
    {
        Array.Clear(this.Memory, 0, this.Memory.Length);
        this.DataPointer = 0;
    }

    /// <summary>
    /// Get the value of the current cell
    /// </summary>
    /// <returns>The current byte</returns>
    private byte GetCurrentByte()
    {
        return this.Memory[this.DataPointer];
    }

    /// <summary>
    /// Set the current cell to the specified value
    /// </summary>
    /// <param name="value">The byte value to set</param>
    private void SetCurrentByte(byte value)
    {
        this.Memory[this.DataPointer] = value;
    }

    /// <summary>
    /// Actually executes the program
    /// </summary>
    public void Execute()
    {
        // First make sure everything is reset
        Reset();
        int lastJumpPosition = -1;
        // Iterate over all instructions and execute them line by line
        for (int i = 0; i < this.Instructions.Count; i++)
        {
            OpCode opcode = this.Instructions[i];
            Console.WriteLine(opcode);
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
                    // Prevents OOB READ/WRITE
                    if (this.DataPointer == this.Memory.Length - 1)
                    {
                        Console.WriteLine($"OOB Memory at {this.DataPointer + 1}");
                        Environment.Exit(1);
                    }
                    this.DataPointer++;
                    break;
                case OpCode.DECDP:
                    // Decrease the DP by one
                    // Prevents OOB READ/WRITE
                    if (this.DataPointer == 0)
                    {
                        Console.WriteLine($"OOB Memory at {this.DataPointer - 1}");
                        Environment.Exit(1);
                    }
                    this.DataPointer--;
                    break;
                case OpCode.OUT:
                    // Print out the current byte as a ASCII char
                    Console.Write(Convert.ToChar(value));
                    break;
                case OpCode.INP:
                    // Takes a single byte as input and stores it at the current cell
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    char inputChar = keyInfo.KeyChar;

                    byte byteValue = Convert.ToByte(inputChar);
                    SetCurrentByte(byteValue); 
                    break;
                case OpCode.JZ:
                    // if the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
                    if (intValue == 0)
                    {
                        if (lastJumpPosition == -1)
                        {
                            // The jump position has not been set before (should not happen)
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
                        // The jump position has not been set before (should not happen)
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
                    // Invalid opcode (should just be ignored)
                    break;
            }
        }
        Console.WriteLine("");
    }

    /// <summary>
    /// Append a opcode to the list of opcodes
    /// </summary>
    /// <param name="opcode">The opcode to append</param>
    public void AppendOpcode(OpCode opcode)
    {
       this.Instructions.Add(opcode); 
    }
}

class JIT
{
    /// <summary>
    /// Runs a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program</param>
    public static void Run(string programString)
    {
        // Remove all the unnecessary whitespace
        string normalizedProgramString = StringHelper.RemoveWhitespace(programString);
        // Parse and execute the program
        BrainfuckProgram program = Parse(normalizedProgramString);
        program.Execute();
        // Dump the memory at the end just to 
        //program.DumpMemory();
    }

    /// <summary>
    /// Takes a program string and then parses it into a program
    /// </summary>
    /// <param name="programString">The brainfuck program to parse</param>
    /// <returns>The parsed Brainfuck Program</returns>
    private static BrainfuckProgram Parse(string programString)
    {
        BrainfuckProgram program = new();
        foreach (char opcode in programString)
        {
            // Check if the char is actually a valid opcode
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
