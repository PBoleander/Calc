using System.IO;

namespace Calc.Models;

public class Calculation
{
    private readonly double _firstValue;
    private readonly double _secondValue;
    private readonly Operation? _operation;

    public Calculation(double firstValue, double secondValue, Operation? operation)
    {
        _firstValue = firstValue;
        _secondValue = secondValue;
        _operation = operation;
    }

    public double Calculate()
    {
        return _operation switch
        {
            Operation.Add => _firstValue + _secondValue,
            Operation.Substract => _firstValue - _secondValue,
            Operation.Multiply => _firstValue * _secondValue,
            Operation.Divide => _firstValue / _secondValue,
            _ => throw new InvalidDataException("Operation not allowed")
        };
    }
}