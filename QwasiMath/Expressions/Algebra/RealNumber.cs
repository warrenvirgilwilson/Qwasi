using System;
using System.Collections.Generic;
using System.Text;

namespace QwasiMath.Expressions
{
    public interface IRealNumber : IAlgebraicValue
    {
        public double Value { get; }
    }

    public class RealNumber : IRealNumber
    {
        public double Value { get; private set; }

        public IAlgebraicExpression Add(IAlgebraicExpression other)
            => (other is RealNumber rn) ? new RealNumber(this.Value + rn.Value) : IAlgebraicExpression.Addition(this, other);

        public IAlgebraicExpression SubtractBy(IAlgebraicExpression other)
            => (other is RealNumber rn) ? new RealNumber(this.Value - rn.Value) : IAlgebraicExpression.Subtraction(this, other);

        public IAlgebraicExpression Multiply(IAlgebraicExpression other)
            => (other is RealNumber rn) ? new RealNumber(this.Value * rn.Value) : IAlgebraicExpression.Multiplication(this, other);

        public IAlgebraicExpression DivideBy(IAlgebraicExpression other)
            => (other is RealNumber rn) ? new RealNumber(this.Value / rn.Value) : IAlgebraicExpression.Division(this, other);

        public IAlgebraicExpression ToExponent(IAlgebraicExpression other)
            => (other is RealNumber rn) ? new RealNumber(Math.Pow(this.Value, rn.Value)) : IAlgebraicExpression.Exponentiation(this, other);

        public IAlgebraicExpression SquareRoot() => new RealNumber(Math.Sqrt(this.Value));
        public IAlgebraicExpression Reciprocal() => new RealNumber(1 / this.Value);
        public IAlgebraicExpression ComplexConjugate() => this;
        public IAlgebraicExpression Negate() => new RealNumber(-this.Value);

        public IAlgebraicExpression Evaluate(MathContext context) => this;
        public IAlgebraicExpression Substitute(MathContext context) => this;

        internal RealNumber(double value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }

    public class LazyRealNumber : IRealNumber
    {
        public double Value { get; private set; }

        public IAlgebraicExpression Evaluate(MathContext context) => IAlgebraicExpression.RealNumber(this.Value);
        public IAlgebraicExpression Substitute(MathContext context) => this;

        internal LazyRealNumber(double value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
