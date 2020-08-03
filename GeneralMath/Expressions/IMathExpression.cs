using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralMath.Expressions
{
    public partial interface IMathExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
    {
        TExpression Evaluate(MathContext context);
        TExpression Substitute(MathContext context);

        protected TExpression NewUnaryOperation<TOperand>(
            TOperand operand,
            UnaryOpFunction<TExpression, TOperand> opFunction,
            string opName,
            string opSymbol)
            where TOperand : class, IMathExpression<TOperand>
        {
            throw new NotImplementedException();
        }

        protected TExpression NewBinaryOperation<TArgA, TArgB>(
            TArgA a,
            TArgB b,
            BinaryOpFunction<TExpression, TArgA, TArgB> opFunction,
            string opName,
            string opSymbol)
            where TArgA : class, IMathExpression<TArgA>
            where TArgB : class, IMathExpression<TArgB>
        {
            throw new NotImplementedException();
        }

        protected TExpression NewMultitermOperation<TTerms>(
            IEnumerable<TTerms> terms,
            MultitermOpFunction<TExpression, TTerms> opFunction,
            string opName,
            string opSymbol)
            where TTerms : class, IMathExpression<TTerms>
        {
            throw new NotImplementedException();
        }
    }

    public interface IValueExpression<TExpression> : IMathExpression<TExpression>
        where TExpression : class, IMathExpression<TExpression>
    {
    }

    public static class IMathExpressionExtensions
    {
        public static TExpression AsExpressionType<TExpression>(this IMathExpression<TExpression> e)
            where TExpression : class, IMathExpression<TExpression>
        {
            if (e is TExpression ret)
                return ret;

            throw new Exception("Expression does not inherit from its generic type parameter TExpression.");
        }
    }
}
