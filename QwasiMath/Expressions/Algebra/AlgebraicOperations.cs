using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace QwasiMath.Expressions
{
    public class AlgebraicUnaryOperation<TOperand> : AbstractUnaryOperation<IAlgebraicExpression, TOperand>, IAlgebraicExpression
        where TOperand : class, IMathExpression<TOperand>
    {
        public AlgebraicUnaryOperation(
            TOperand operand,
            UnaryOpFunction<IAlgebraicExpression, TOperand> opFunction,
            string opName,
            string opSymbol)
            : base(operand, opFunction, opName, opSymbol)
        {
        }
    }

    public class AlgebraicBinaryOperation<TArgA, TArgB> : AbstractBinaryOperation<IAlgebraicExpression, TArgA, TArgB>, IAlgebraicExpression
        where TArgA : class, IMathExpression<TArgA>
        where TArgB : class, IMathExpression<TArgB>
    {
        public AlgebraicBinaryOperation(
            TArgA a,
            TArgB b,
            BinaryOpFunction<IAlgebraicExpression, TArgA, TArgB> opFunction,
            string opName,
            string opSymbol)
            : base(a, b, opFunction, opName, opSymbol)
        {
        }
    }

    public class AlgebraicMultitermOperation<TTerms> : AbstractMultitermOperation<IAlgebraicExpression, TTerms>, IAlgebraicExpression
        where TTerms : class, IMathExpression<TTerms>
    {
        public AlgebraicMultitermOperation(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IAlgebraicExpression, TTerms> opFunction,
            string opName,
            string opSymbol)
            : base(terms, opFunction, opName, opSymbol)
        {
        }
    }

    public partial interface IAlgebraicExpression
    {
        public static IAlgebraicExpression UnaryOperation<TOperand>(
            TOperand operand, UnaryOpFunction<IAlgebraicExpression, TOperand> opFunction, string opName, string opSymbol)
            where TOperand : class, IMathExpression<TOperand>
        {
            return new AlgebraicUnaryOperation<TOperand>(operand, opFunction, opName, opSymbol);
        }

        public static IAlgebraicExpression BinaryOperation<TArgA, TArgB>(
            TArgA a, TArgB b, BinaryOpFunction<IAlgebraicExpression, TArgA, TArgB> opFunction, string opName, string opSymbol)
            where TArgA : class, IMathExpression<TArgA>
            where TArgB : class, IMathExpression<TArgB>
        {
            return new AlgebraicBinaryOperation<TArgA, TArgB>(a, b, opFunction, opName, opSymbol);
        }

        public static IAlgebraicExpression MultitermOperation<TTerms>(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IAlgebraicExpression, TTerms> opFunction,
            string opName,
            string opSymbol)
            where TTerms : class, IMathExpression<TTerms>
        {
            return new AlgebraicMultitermOperation<TTerms>(terms, opFunction, opName, opSymbol);
        }

        IAlgebraicExpression IMathExpression<IAlgebraicExpression>.NewUnaryOperation<TOperand>(
            TOperand operand, UnaryOpFunction<IAlgebraicExpression, TOperand> opFunction, string opName, string opSymbol)
        {
            return new AlgebraicUnaryOperation<TOperand>(operand, opFunction, opName, opSymbol);
        }

        IAlgebraicExpression IMathExpression<IAlgebraicExpression>.NewBinaryOperation<TArgA, TArgB>(
            TArgA a, TArgB b, BinaryOpFunction<IAlgebraicExpression, TArgA, TArgB> opFunction, string opName, string opSymbol)
        {
            return new AlgebraicBinaryOperation<TArgA, TArgB>(a, b, opFunction, opName, opSymbol);
        }

        IAlgebraicExpression IMathExpression<IAlgebraicExpression>.NewMultitermOperation<TTerms>(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IAlgebraicExpression, TTerms> opFunction,
            string opName,
            string opSymbol)
        {
            return new AlgebraicMultitermOperation<TTerms>(terms, opFunction, opName, opSymbol);
        }
    }
}
