using System;
using System.Globalization;
using System.Linq;

namespace Calc.Models;

public static class Calculator
{
    public static string Calculate(string calc)
    {
        // ========== Manage parentheses ========== //
        var numberOfOpeningParentheses = CountIn(calc, '(');
        var numberOfClosingParentheses = CountIn(calc, ')');
        
        if (numberOfOpeningParentheses != numberOfClosingParentheses)
            return string.Empty;
        
        int indexOfOpeningParenthesis;
        while ((indexOfOpeningParenthesis = calc.LastIndexOf("(", StringComparison.Ordinal)) != -1)
        {
            var indexOfClosingParenthesis = calc.IndexOf(")", indexOfOpeningParenthesis, StringComparison.Ordinal);
            // Replace parentheses with its result
            calc = calc[..indexOfOpeningParenthesis] +
                   Calculate(calc[(indexOfOpeningParenthesis + 1)..indexOfClosingParenthesis]) +
                   calc[(indexOfClosingParenthesis + 1)..];
        }
        
        // ========== Manage other operations ========== //
        int indexOfOperator;
        
        // Search calculations with precedence first. When there isn't more, continue with the others
        while ((indexOfOperator = calc.IndexOfAny(OperationChar.PrecedentOperators, 1)) > 0 || // precedent operations first
               (indexOfOperator = calc.IndexOfAny(OperationChar.NonPrecedentOperators, 1)) > 0)
        {
            // ==== Find the first operand ==== //
            var indexOfPreviousOperator = SetIndexOfPreviousOperator(calc, indexOfOperator);
            var stringOfFirstValue = calc[(indexOfPreviousOperator + 1)..indexOfOperator];
            var startIndexOfCalculation = indexOfPreviousOperator + 1;

            // First value could be just the sign -, e.g. in --3
            if (stringOfFirstValue.Length == 1 &&
                OperationChar.IsAnOperator(stringOfFirstValue[0]))
            {
                stringOfFirstValue = "0";
                indexOfOperator--; // This way operator would be - and secondValue -3
            }
            
            // ==== Find the second operand ==== //
            // startIndex = indexOfOperator + 2 avoids to detect sign of second value as operator
            var indexOfNextOperator = calc.IndexOfAny(OperationChar.Operators, indexOfOperator + 2);

            if (indexOfNextOperator == -1) // Last calculation
                indexOfNextOperator = calc.Length;
            
            var stringOfSecondValue = calc[(indexOfOperator + 1)..indexOfNextOperator];
            var nextIndexAfterCalculation = indexOfNextOperator;

            // ==== Construct the calculation ==== //
            var firstValue = Convert.ToDouble(stringOfFirstValue);
            var operation = CharToOperation(calc[indexOfOperator]);
            var secondValue = Convert.ToDouble(stringOfSecondValue);

            var calculation = new Calculation(firstValue, secondValue, operation);
            
            // Replace calculation with its result
            calc = calc[..startIndexOfCalculation] +
                   Convert.ToString(calculation.Calculate(), CultureInfo.CurrentCulture) +
                   calc[nextIndexAfterCalculation..];
        }

        return calc;
    }

    private static Operation? CharToOperation(char? character)
    {
        return character switch
        {
            OperationChar.Add => Operation.Add,
            OperationChar.Substract => Operation.Substract,
            OperationChar.Multiply => Operation.Multiply,
            OperationChar.Divide => Operation.Divide,
            _ => null
        };
    }
    
    private static int CountIn(string s, char character)
    {
        var count = 0;
        
        foreach (var c in s)
            if (c.Equals(character))
                count++;

        return count;
    }

    private static int SetIndexOfPreviousOperator(string calc, int indexOfOperator)
    {
        var indexOfPreviousOperator = calc.LastIndexOfAny(OperationChar.Operators, indexOfOperator - 1);
        
        // First calculation. There could not be an operator at the beginning, it must be a sign
        if (indexOfPreviousOperator == 0)
        {
            indexOfPreviousOperator = -1;
        }
        // If the first value is negative and not the first calculation, an operator must be just before the index
        // previously calculated as the indexOfPreviousOperator, e.g. in a+-b/c the - isn't previousOperator, it's +
        else if (indexOfPreviousOperator > 0 &&
                 calc[indexOfPreviousOperator].Equals(OperationChar.Substract) && // minus sign
                 OperationChar.IsAnOperator(calc[indexOfPreviousOperator - 1])) // previous index contains an operator
        {
            indexOfPreviousOperator--;
        }
        
        return indexOfPreviousOperator;
    }
}