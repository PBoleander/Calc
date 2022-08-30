using System.Linq;

namespace Calc.Models;

public static class OperationChar
{
    public const char Add = '+';
    public const char Substract = '-';
    public const char Multiply = '*';
    public const char Divide = '/';
    
    public static readonly char[] Operators = { Add, Substract, Multiply, Divide };
    public static readonly char[] PrecedentOperators = { Multiply, Divide };
    public static readonly char[] NonPrecedentOperators = { Add, Substract };

    public static bool IsAnOperator(char character)
    {
        return Operators.Contains(character);
    }
}