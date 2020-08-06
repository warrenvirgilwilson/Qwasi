using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QwasiMath.Expressions.Vectors
{
    public interface IVectorExpression<TVector> : IBasicExpression<TVector>
        where TVector : class, IVectorExpression<TVector>
    {
        public static TVector ScalarMultiplication(IAlgebraicExpression scalar, TVector vector)
        {
            return vector.NewBinaryOperation(scalar, vector, (a, b) => b.ScalarProduct(a), "scalar_multiply", "*");
        }

        public static IAlgebraicExpression LengthOf(TVector vector)
        {
            return IAlgebraicExpression.UnaryOperation(vector, v => v.Length(), "length", null);
        }

        public static TVector Normalization(TVector vector)
        {
            return vector.NewUnaryOperation(vector, v => v.Normalize(), "normalize", null);
        }

        public TVector ScalarProduct(IAlgebraicExpression scalar) => ScalarMultiplication(scalar, this.AsExpressionType());
        public IAlgebraicExpression Length() => LengthOf(this.AsExpressionType());
        public TVector Normalize() => Normalization(this.AsExpressionType());
    }

    public interface IVectorValue<TVector> : IVectorExpression<TVector>, IBasicValue<TVector>
        where TVector : class, IVectorExpression<TVector>
    {
    }
}
