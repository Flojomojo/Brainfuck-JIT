using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Enumeration;
using System.Net.Mime;
using System.Reflection.Metadata;

namespace Brainfuck_JIT;

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

class BrainfuckProgram
{
    /// <summary>
    /// The program consists of x cells each containing one single byte
    /// </summary>
    private byte[] Memory = new byte[32768];
    /// <summary>
    /// The instructions to execute
    /// </summary>
    private readonly List<OpCode> _instructions = [];
    /// <summary>
    /// The pointer to the cell
    /// </summary>
    private int _dataPointer = 0;

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
        this._dataPointer = 0;
    }

    /// <summary>
    /// Get the value of the current cell
    /// </summary>
    /// <returns>The current byte</returns>
    private byte GetCurrentByte()
    {
        return this.Memory[this._dataPointer];
    }

    /// <summary>
    /// Set the current cell to the specified value
    /// </summary>
    /// <param name="value">The byte value to set</param>
    private void SetCurrentByte(byte value)
    {
        this.Memory[this._dataPointer] = value;
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
        for (int i = 0; i < this._instructions.Count; i++)
        {
            var (opCodeType, repetition) = this._instructions[i];
            byte value = GetCurrentByte();
            int intValue = value;
            switch (opCodeType)
            { 
                case OpCodeType.INC:
                    // Get the byte at the current position and increase by x
                    SetCurrentByte((byte)(intValue + repetition)); 
                    break;
                case OpCodeType.DEC:
                    // Get the byte at the current position and dec by x
                    SetCurrentByte((byte)(intValue - repetition)); 
                    break;
                case OpCodeType.INCDP:
                    // Increase the DP by x
                    int newDataPointer = this._dataPointer + repetition;
                    // Prevents OOB READ/WRITE
                    if (newDataPointer > this.Memory.Length - 1)
                    {
                        Console.WriteLine($"OOB Memory at {newDataPointer}");
                        Environment.Exit(1);
                    }

                    this._dataPointer = newDataPointer;
                    break;
                case OpCodeType.DECDP:
                    // Decrease the DP by one
                    newDataPointer = this._dataPointer - repetition;
                    // Prevents OOB READ/WRITE
                    if (newDataPointer < 0)
                    {
                        Console.WriteLine($"OOB Memory at {newDataPointer}");
                        Environment.Exit(1);
                    }

                    this._dataPointer = newDataPointer;
                    break;
                case OpCodeType.OUT:
                    // Print out the current byte as a ASCII char
                    Console.Write(Convert.ToChar(value));
                    break;
                case OpCodeType.INP:
                    // Takes a single byte as input and stores it at the current cell
                    Console.Write("> ");
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    char inputChar = keyInfo.KeyChar;

                    byte byteValue = Convert.ToByte(inputChar);
                    SetCurrentByte(byteValue); 
                    break;
                case OpCodeType.JZ:
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
                case OpCodeType.JNZ:
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
       this._instructions.Add(opcode); 
    }
}

public class Lexer(string programString)
{
    private int _i = 0;
    private string _programString = programString;

    public char? Peek()
    {
        int next = this.FindNextValidOpcode();
        if (next == -1)
            return null;
        return this._programString[next];
    }

    public char Consume()
    {
        int next = this.FindNextValidOpcode();
        char c = this._programString[next];
        this._i = next;
        return c;
    }
    private int FindNextValidOpcode()
    {
        int index = this._i + 1;
        char c = this._programString[index];
        while (!JIT.IsValidOpCode(c))
        {
            index++;
            if (index > this._programString.Length - 1)
                return -1;
            c = this._programString[index];
        }
        return index;
    }
}

public class JIT
{
    /// <summary>
    /// Runs a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program</param>
    public static void Run(string programString)
    {
        // Parse and execute the program
        BrainfuckProgram program = Parse(programString);
        program.Execute();
        // Dump the memory at the end just to 
        //program.DumpMemory();
    }

    public static bool IsValidOpCode(char c)
    {
        return Enum.IsDefined(typeof(OpCodeType), (int)c);
    }
    
    /// <summary>
    /// Takes a program string and then parses it into a program
    /// </summary>
    /// <param name="programString">The brainfuck program to parse</param>
    /// <returns>The parsed Brainfuck Program</returns>
    private static BrainfuckProgram Parse(string programString)
    {
        BrainfuckProgram program = new();
        Lexer lexer = new(programString);
        while(lexer.Peek() is not null)
        {
            char opcode = lexer.Consume();
            // Invalid opcodes are just ignored
            if (!IsValidOpCode(opcode))
                continue;
            // Check if the char is actually a valid opcode
            OpCodeType parsedOpCodeType = (OpCodeType)opcode;
            // If there are repeating characters count them
            int repeatCount = 1;
            while (lexer.Peek() == opcode)
            {
               repeatCount++;
               lexer.Consume();
            }
            OpCode parsedOpCode = new(parsedOpCodeType, repeatCount);
            program.AppendOpcode(parsedOpCode);
        }
        return program;
    }
}
