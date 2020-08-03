using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneralMath.Expressions
{
    /*public static class AlgebraicExpressions
    {
        public static AlgebraicVariable Variable(object identifier) => new AlgebraicVariable(identifier);
        public static RealNumber RealNumber(double value) => new RealNumber(value);
    }*/

    public partial interface IAlgebraicExpression : IBasicExpression<IAlgebraicExpression>
    {
        public static IAlgebraicVariable Variable(object identifier) => new AlgebraicVariable(identifier);
        public static IAlgebraicVariable Variable(params object[] identifier) => new AlgebraicVariable(identifier);
        public static IRealNumber RealNumber(double value) => new RealNumber(value);

        public static IAlgebraicExpression Multiplication(IAlgebraicExpression a, IAlgebraicExpression b)
        {
            if (a is IVariableExpression<IAlgebraicExpression> && !(b is IVariableExpression<IAlgebraicExpression>))
                return a.NewBinaryOperation(b, a, (v1, v2) => v1.Multiply(v2), "multiply", "*");

            return a.NewBinaryOperation(a, b, (v1, v2) => v1.Multiply(v2), "multiply", "*");
        }

        public static IAlgebraicExpression Division(IAlgebraicExpression a, IAlgebraicExpression b)
            => a.NewBinaryOperation(a, b, (v1, v2) => v1.DivideBy(v2), "divide", "/");

        public static IAlgebraicExpression SquareRootOperation(IAlgebraicExpression value)
            => value.NewUnaryOperation(value, v => v.SquareRoot(), "sqrt", null);

        public static IAlgebraicExpression Exponentiation(IAlgebraicExpression a, IAlgebraicExpression b)
            => a.NewBinaryOperation(a, b, (v1, v2) => v1.ToPower(v2), "power", "^");

        public static IAlgebraicExpression ReciprocalOperation(IAlgebraicExpression value)
            => value.NewUnaryOperation(value, v => v.ComplexConjugate(), "recip", null);

        public static IAlgebraicExpression ComplexConjugation(IAlgebraicExpression value)
            => value.NewUnaryOperation(value, v => v.ComplexConjugate(), "complex_conjugate", null);

        public static IAlgebraicExpression operator *(IAlgebraicExpression a, IAlgebraicExpression b) => a.Multiply(b);
        public static IAlgebraicExpression operator /(IAlgebraicExpression a, IAlgebraicExpression b) => a.DivideBy(b);
        public static IAlgebraicExpression operator ^(IAlgebraicExpression a, IAlgebraicExpression b) => a.ToPower(b);

        public IAlgebraicExpression Multiply(IAlgebraicExpression other) => Multiplication(this, other);
        public IAlgebraicExpression DivideBy(IAlgebraicExpression other) => Division(this, other);
        public IAlgebraicExpression ToPower(IAlgebraicExpression other) => Exponentiation(this, other);
        public IAlgebraicExpression SquareRoot() => SquareRootOperation(this);
        public IAlgebraicExpression Reciprocal() => ReciprocalOperation(this);
        public IAlgebraicExpression ComplexConjugate() => ComplexConjugation(this);
    }

    public interface IAlgebraicValue : IAlgebraicExpression, IBasicValue<IAlgebraicExpression>
    {
    }
}
