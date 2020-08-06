using System;
using System.Collections.Generic;
using System.Text;

namespace QwasiMath.Expressions.Vectors
{
    public interface IExplicitGeneralizedVector<TVector, TBasis> : IVectorExpression<TVector>
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>
    {
        public static ExplicitLinearOperator<TVector, TBasis> CreateLinearOperator(
            LinearOperatorFunction<TVector, TBasis> operatorFunction, string opName)
        {
            return new ExplicitLinearOperator<TVector, TBasis>(operatorFunction, opName);
        }

        public static IAlgebraicExpression InnerProductOperation(TVector a, TVector b)
        {
            return IAlgebraicExpression.BinaryOperation(a, b, (a, b) => a.InnerProduct(b), "inner_product", "");
        }

        public static TVector LinearOperation(
            TVector vector, IExplicitLinearOperator<TVector, TBasis> linearOperator)
        {
            return vector.NewUnaryOperation(
                vector, v => v.ApplyOperator(linearOperator), "linOp_" + linearOperator.OpName, null);
        }

        public IAlgebraicExpression InnerProduct(TVector other) => InnerProductOperation(this.AsExpressionType(), other);
        public TVector ApplyOperator(IExplicitLinearOperator<TVector, TBasis> linearOperator)
            => LinearOperation(this.AsExpressionType(), linearOperator);
    }
}
