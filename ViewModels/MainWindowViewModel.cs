using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using Calc.Models;
using ReactiveUI;

namespace Calc.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _shownString = string.Empty;
        private string _shownResult = string.Empty;
        private int _numberOfOpeningParentheses;
        private int _numberOfClosingParentheses;
        
        // Commands
        public ReactiveCommand<Unit, Unit> AddDecimalSeparatorCommand { get; }
        public ReactiveCommand<int, Unit> AddNumberCommand { get; }
        public ReactiveCommand<Operation, Unit> AddOperationCommand { get; }
        public ReactiveCommand<Unit, Unit> AddParenthesisCommand { get; }
        public ReactiveCommand<Unit, Unit> AlternateNegativePositiveCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearScreenCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteLastCommand { get; }
        public ReactiveCommand<Unit, Unit> PickResultCommand { get; }

        public MainWindowViewModel()
        {
            AddDecimalSeparatorCommand = ReactiveCommand.Create(AddDecimalSeparator);
            AddNumberCommand = ReactiveCommand.Create<int>(AddNumber);
            AddOperationCommand = ReactiveCommand.Create<Operation>(AddOperation);
            AddParenthesisCommand = ReactiveCommand.Create(AddParenthesis);
            AlternateNegativePositiveCommand = ReactiveCommand.Create(AlternateNegativePositive);
            ClearScreenCommand = ReactiveCommand.Create(ClearScreen);
            DeleteLastCommand = ReactiveCommand.Create(DeleteLast);
            PickResultCommand = ReactiveCommand.Create(PickResult);
        }

        public string ShownString
        {
            get => _shownString;
            set => this.RaiseAndSetIfChanged(ref _shownString, value);
        }

        public string ShownResult
        {
            get => _shownResult;
            set => this.RaiseAndSetIfChanged(ref _shownResult, value);
        }

        private void AddDecimalSeparator()
        {
            if (CanDecimalSeparatorBePlaced())
                ShownString += CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        private void AddNumber(int value)
        {
            ShownString += value;
            Calculate();
        }

        private void AddOperation(Operation operation)
        {
            if (ShownString[^1].Equals('('))
                return;
            
            if (IsLastInputAnOperation())
                ShownString = ShownString[..^1];

            ShownString += operation switch
            {
                Operation.Add => OperationChar.Add,
                Operation.Substract => OperationChar.Substract,
                Operation.Multiply => OperationChar.Multiply,
                Operation.Divide => OperationChar.Divide,
                _ => throw new InvalidDataException("Operation not allowed")
            };
        }

        private void AddParenthesis()
        {
            if (ShownString.Length == 0 || IsLastInputAnOperation() || ShownString[^1].Equals('('))
            {
                ShownString += "(";
                _numberOfOpeningParentheses++;
            }
            else if (_numberOfClosingParentheses < _numberOfOpeningParentheses)
            {
                ShownString += ")";
                _numberOfClosingParentheses++;
                Calculate();
            }
        }

        private void AlternateNegativePositive()
        {
            var indexWhereSetOrUnsetSign = SetIndexWhereSetSign();

            if (ShownString.Length == 0)
                ShownString += OperationChar.Substract;
            else
            {
                switch (ShownString[indexWhereSetOrUnsetSign])
                {
                    case OperationChar.Substract:
                        if (indexWhereSetOrUnsetSign == 0 ||
                            OperationChar.Operators.Contains(ShownString[indexWhereSetOrUnsetSign - 1]))
                            
                            ShownString = ShownString.Remove(indexWhereSetOrUnsetSign, 1);
                        else
                            // Add -
                            ShownString = ShownString[..indexWhereSetOrUnsetSign] +
                                          OperationChar.Substract +
                                          ShownString[indexWhereSetOrUnsetSign..];
                        break;
                    default:
                        // Add -
                        ShownString = ShownString[..indexWhereSetOrUnsetSign] +
                                      OperationChar.Substract +
                                      ShownString[indexWhereSetOrUnsetSign..];
                        break;
                }
                
                Calculate();
            }
        }

        private void Calculate()
        {
            if (_numberOfOpeningParentheses == _numberOfClosingParentheses)
                ShownResult = Calculator.Calculate(ShownString);
        }
        
        private bool CanDecimalSeparatorBePlaced()
        {
            var indexLastDecimalSeparator = ShownString.LastIndexOf(
                CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                StringComparison.Ordinal);
            var indexLastOperator = ShownString.LastIndexOfAny(OperationChar.Operators);

            if (indexLastDecimalSeparator == -1 && indexLastOperator == -1)
                return true;
            
            return indexLastOperator > indexLastDecimalSeparator;
        }

        private void ClearScreen()
        {
            ShownString = string.Empty;
            ShownResult = string.Empty;
            _numberOfOpeningParentheses = 0;
            _numberOfClosingParentheses = 0;
        }

        private void DeleteLast()
        {
            // Update number of parentheses
            switch (ShownString[^1])
            {
                case '(':
                    _numberOfOpeningParentheses--;
                    break;
                case ')':
                    _numberOfClosingParentheses--;
                    break;
            }
            
            ShownString = ShownString[..^1];
            ShownResult = IsLastInputAnOperation() ?
                Calculator.Calculate(ShownString[..^1]) :
                Calculator.Calculate(ShownString);
        }

        private bool IsLastInputAnOperation()
        {
            switch (ShownString[^1])
            {
                case OperationChar.Add:
                case OperationChar.Substract:
                case OperationChar.Multiply:
                case OperationChar.Divide:
                    return true;
                default:
                    return false;
            }
        }
        
        private static int MaxOf(int number1, int number2)
        {
            return number1 > number2 ? number1 : number2;
        }
        
        private void PickResult()
        {
            ShownString = ShownResult;
            ShownResult = string.Empty;
        }

        private int SetIndexWhereSetSign()
        {
            char[] nonSubstractOperators = { OperationChar.Add, OperationChar.Multiply, OperationChar.Divide };
            
            var indexAfterLastNonSubstractOperator = ShownString.LastIndexOfAny(nonSubstractOperators) + 1;
            var indexOfLastSubstractOperator = ShownString.LastIndexOf(OperationChar.Substract);
            var indexAfterLastParenthesis = ShownString.LastIndexOf('(') + 1;
            var indexLastOperator = MaxOf(indexAfterLastNonSubstractOperator, indexOfLastSubstractOperator);

            return MaxOf(indexAfterLastParenthesis, indexLastOperator);
        }
    }
}