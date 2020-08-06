using System;
using System.Collections.Generic;
using System.Text;
using QwasiMath.Expressions.Vectors;

namespace QwasiMath.Expressions.HilbertSpace
{
    public partial interface IHSExplicitVector<TBasis> : IExplicitGeneralizedVector<IHSExplicitVector<TBasis>, TBasis>
    {
        public static IHSExplicitVector<TBasis> UnaryOperation<TOperand>(
            TOperand operand, UnaryOpFunction<IHSExplicitVector<TBasis>, TOperand> opFunction, string opName, string opSymbol)
            where TOperand : class, IMathExpression<TOperand>
        {
            return new HSExplicitUnaryOperation<TBasis, TOperand>(operand, opFunction, opName, opSymbol);
        }

        public static IHSExplicitVector<TBasis> BinaryOperation<TArgA, TArgB>(
            TArgA a, TArgB b, BinaryOpFunction<IHSExplicitVector<TBasis>, TArgA, TArgB> opFunction, string opName, string opSymbol)
            where TArgA : class, IMathExpression<TArgA>
            where TArgB : class, IMathExpression<TArgB>
        {
            return new HSExplicitBinaryOperation<TBasis, TArgA, TArgB>(a, b, opFunction, opName, opSymbol);
        }

        public static IHSExplicitVector<TBasis> MultitermOperation<TTerms>(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IHSExplicitVector<TBasis>, TTerms> opFunction,
            string opName,
            string opSymbol)
            where TTerms : class, IMathExpression<TTerms>
        {
            return new HSExplicitMultitermOperation<TBasis, TTerms>(terms, opFunction, opName, opSymbol);
        }

        IHSExplicitVector<TBasis> IMathExpression<IHSExplicitVector<TBasis>>.NewUnaryOperation<TOperand>(
            TOperand operand, UnaryOpFunction<IHSExplicitVector<TBasis>, TOperand> opFunction, string opName, string opSymbol)
            => UnaryOperation(operand, opFunction, opName, opSymbol);

        IHSExplicitVector<TBasis> IMathExpression<IHSExplicitVector<TBasis>>.NewBinaryOperation<TArgA, TArgB>(
            TArgA a, TArgB b, BinaryOpFunction<IHSExplicitVector<TBasis>, TArgA, TArgB> opFunction, string opName, string opSymbol)
            => BinaryOperation(a, b, opFunction, opName, opSymbol);

        IHSExplicitVector<TBasis> IMathExpression<IHSExplicitVector<TBasis>>.NewMultitermOperation<TTerms>(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IHSExplicitVector<TBasis>, TTerms> opFunction,
            string opName,
            string opSymbol)
            => MultitermOperation(terms, opFunction, opName, opSymbol);
    }

    public class HSExplicitUnaryOperation<TBasis, TOperand>
        : AbstractUnaryOperation<IHSExplicitVector<TBasis>, TOperand>, IHSExplicitVector<TBasis>
        where TOperand : class, IMathExpression<TOperand>
    {
        public HSExplicitUnaryOperation(
            TOperand operand,
            UnaryOpFunction<IHSExplicitVector<TBasis>, TOperand> opFunction,
            string opName,
            string opSymbol)
            : base(operand, opFunction, opName, opSymbol)
        {
        }
    }

    public class HSExplicitBinaryOperation<TBasis, TArgA, TArgB>
        : AbstractBinaryOperation<IHSExplicitVector<TBasis>, TArgA, TArgB>, IHSExplicitVector<TBasis>
        where TArgA : class, IMathExpression<TArgA>
        where TArgB : class, IMathExpression<TArgB>
    {
        public HSExplicitBinaryOperation(
            TArgA a,
            TArgB b,
            BinaryOpFunction<IHSExplicitVector<TBasis>, TArgA, TArgB> opFunction,
            string opName,
            string opSymbol)
            : base(a, b, opFunction, opName, opSymbol)
        {
        }
    }

    public class HSExplicitMultitermOperation<TBasis, TTerms>
        : AbstractMultitermOperation<IHSExplicitVector<TBasis>, TTerms>, IHSExplicitVector<TBasis>
        where TTerms : class, IMathExpression<TTerms>
    {
        public HSExplicitMultitermOperation(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<IHSExplicitVector<TBasis>, TTerms> opFunction,
            string opName,
            string opSymbol)
            : base(terms, opFunction, opName, opSymbol)
        {
        }
    }
}
