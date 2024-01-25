using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Enumeration;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Text;

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

public class BrainfuckProgram
{
    /// <summary>
    /// The program consists of x cells each containing one single byte
    /// </summary>
    private byte[] _memory = new byte[32768];
    /// <summary>
    /// The instructions to execute
    /// </summary>
    public readonly List<OpCode> Instructions = [];
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
        foreach (byte b in _memory)
        {
            Console.Write($"{b.ToString()}|");
        }
    }

    /// <summary>
    /// Resets the program to its base state
    /// </summary>
    private void Reset()
    {
        Array.Clear(this._memory, 0, this._memory.Length);
        this._dataPointer = 0;
    }

    /// <summary>
    /// Get the value of the current cell
    /// </summary>
    /// <returns>The current byte</returns>
    private byte GetCurrentByte()
    {
        return this._memory[this._dataPointer];
    }

    /// <summary>
    /// Set the current cell to the specified value
    /// </summary>
    /// <param name="value">The byte value to set</param>
    private void SetCurrentByte(byte value)
    {
        this._memory[this._dataPointer] = value;
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
            var (opCodeType, repetition) = this.Instructions[i];
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
                    if (newDataPointer > this._memory.Length - 1)
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
                    for (int j = 0; j < repetition; j++)
                    {
                        Console.Write(Convert.ToChar(value));
                    }
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
       this.Instructions.Add(opcode); 
    }
}

public class Lexer(string programString)
{
    private int _i = 0;

    /// <summary>
    /// Peek the next nth character
    /// </summary>
    /// <param name="offset">The offset of the character</param>
    /// <returns>The character or null if there are no more characters</returns>
    private char? Peek(int offset = 1)
    {
        int next = this._i + offset;
        if (next > programString.Length - 1)
            return null;
        return programString[next];
    }

    /// <summary>
    /// Consumes the next character
    /// </summary>
    /// <returns>The consumed character</returns>
    private char Consume()
    {
        char c = programString[this._i];
        this._i++;
        return c;
    }
    
    private static bool IsValidOpCode(char c)
    {
        return Enum.IsDefined(typeof(OpCodeType), (int)c);
    }
    
    /// <summary>
    /// Takes a program string and then tokenize it into a program
    /// </summary>
    /// <returns>The parsed Brainfuck Program</returns>
    public BrainfuckProgram Tokenize()
    {
        BrainfuckProgram program = new();
        while(Peek(0) is not null)
        {
            char opcode = Consume();
            // Check if the char is actually a valid opcode
            OpCodeType parsedOpCodeType = (OpCodeType)opcode;
            // If there are repeating characters count them
            int repeatCount = 1;
            // Exclude [, ], ,
            if (parsedOpCodeType is not OpCodeType.JZ and not OpCodeType.JNZ and not OpCodeType.INP)
            {
                while (Peek(0) == opcode)
                {
                   repeatCount++;
                   Consume();
                }
            }
            OpCode parsedOpCode = new(parsedOpCodeType, repeatCount);
            //Console.WriteLine(parsedOpCode);
            program.AppendOpcode(parsedOpCode);
        }
        return program;
    }

    /// <summary>
    /// Takes a program string and then tokenizes it without of repeating character folding into a program
    /// </summary>
    /// <returns>The lexed Brainfuck Program</returns>
    public BrainfuckProgram TokenizeWithoutFolding()
    {
        BrainfuckProgram program = new();
        foreach (var opcode in programString)
        {
            // Invalid opcodes are just ignored
            if (!IsValidOpCode(opcode))
                continue;
            // Check if the char is actually a valid opcode
            OpCodeType parsedOpCodeType = (OpCodeType)opcode;
            // If there are repeating characters count them
            OpCode parsedOpCode = new(parsedOpCodeType, 1);
            program.AppendOpcode(parsedOpCode);
        }
        return program;
    }
}

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

public class JIT
{
    /// <summary>
    /// Runs a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program</param>
    public static void Run(string programString)
    {
        // Parse and execute the program
        Lexer lexer = new Lexer(programString);
        BrainfuckProgram program = lexer.Tokenize();
        Parser parser = new(program, programString);
        parser.Parse();
        program.Execute();
    }
}