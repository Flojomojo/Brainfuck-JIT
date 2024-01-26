namespace Brainfuck_JIT.JIT;


public class Interpreter
{
    /// <summary>
    /// Runs a brainfuck program
    /// </summary>
    /// <param name="programString">The brainfuck program</param>
    public void Run(string programString)
    {
        // Parse and execute the program
        Lexer lexer = new Lexer(programString);
        BrainfuckProgram program = lexer.Tokenize();
        Parser parser = new(program, programString);
        parser.Parse();
        Execute(program);
    }
    
    /// <summary>
    /// The program consists of x cells each containing one single byte
    /// </summary>
    private byte[] _memory = new byte[(int)Math.Pow(2,30)];

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
    public void Reset()
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
    /// Executes a program
    /// </summary>
    /// <param name="program">The program to execute</param>
    public void Execute(BrainfuckProgram program)
    {
        // First make sure everything is reset
        Reset();
        // Iterate over all instructions and execute them line by line
        for (int i = 0; i < program.Instructions.Count; i++)
        {
            OpCode code = program.Instructions[i];
            OpCodeType opCodeType = code.Type;
            int repetition = code.Repetition;
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
                        // Need to subtract one because the for loop will add one
                        i = repetition - 1;
                    }
                    break;
                case OpCodeType.JNZ:
                    // if the byte at the data pointer is non-zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
                    if (intValue != 0)
                    {
                        // Need to subtract one because the for loop will add one
                        i = repetition - 1;
                        break;
                    }
                    break;
                default:
                    // Invalid opcode (should just be ignored)
                    break;
            }
        }

        Console.WriteLine("");
    }
}