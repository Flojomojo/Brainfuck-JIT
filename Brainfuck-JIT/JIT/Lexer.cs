using System.Net.Mail;

namespace Brainfuck_JIT.JIT;

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
        Stack<int> closingBracketPositions = [];
        Stack<int> openingBracketPositions = [];
        while(Peek(0) is not null)
        {
            char opcode = Consume();
            if (!IsValidOpCode(opcode))
                continue;
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
            
            // If there is something like ++++-- this should translate INC 2
            if (parsedOpCodeType is OpCodeType.INC or OpCodeType.DEC)
            {
                while (Peek(0) is '-' or '+')
                {
                    if (Consume() == '-')
                        repeatCount--;
                    else
                        repeatCount++;
                }
            }
            
            // For JZ and JNZ the "repetitions" are the jump positions
            // This is done via back patching for JZ
            if (parsedOpCodeType is OpCodeType.JZ)
            {
                openingBracketPositions.Push(program.Instructions.Count);
            }
            else if (parsedOpCodeType is OpCodeType.JNZ)
            {
                closingBracketPositions.Push(program.Instructions.Count);
                // If there is no opening bracket for the opening bracket this is here just set to 0, because the Parser will handle it 
                openingBracketPositions.TryPop(out var correctOpeningBracketPosition);
                repeatCount = correctOpeningBracketPosition;
            }
            
            OpCode parsedOpCode = new(parsedOpCodeType, repeatCount, program.Instructions.Count);
            //Console.WriteLine(parsedOpCode);
            program.AppendOpcode(parsedOpCode);
        }
         
        // Back patch the jump positions for JZ
        foreach (var opcode in program.Instructions)
        {
            if (opcode.Type is not OpCodeType.JZ)
            {
                continue;
            }
            // If there is no closing bracket for the opening bracket this is here just set to 0, because the Parser will handle it 
            closingBracketPositions.TryPop(out var correctClosingBracketPosition);
            opcode.Repetition = correctClosingBracketPosition;
        }
        
        // Print all opcodes
        foreach (OpCode code in program.Instructions)
        {
            Console.WriteLine(code);
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
            OpCode parsedOpCode = new(parsedOpCodeType, 1, -1);
            program.AppendOpcode(parsedOpCode);
        }
        return program;
    }
}