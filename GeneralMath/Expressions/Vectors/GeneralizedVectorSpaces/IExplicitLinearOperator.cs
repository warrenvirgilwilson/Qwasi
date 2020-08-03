using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralMath.Expressions.Vectors
{
    public interface IExplicitLinearOperator<TVector, TBasis>
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>
    {
        string OpName { get; }

        TVector ApplyToBasisVector(TBasis basisVector);
    }

    public delegate TVector LinearOperatorFunction<TVector, TBasis>(TBasis basisVector)
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>;

    public class ExplicitLinearOperator<TVector, TBasis>
        : IExplicitLinearOperator<TVector, TBasis>
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>
    {
        public string OpName { get; }

        public LinearOperatorFunction<TVector, TBasis> OperatorFunction { get; }

        public TVector ApplyToBasisVector(TBasis basisVector)
        {
            return this.OperatorFunction(basisVector);
        }

        public ExplicitLinearOperator(LinearOperatorFunction<TVector, TBasis> operatorFunction, string opName)
        {
            this.OperatorFunction = operatorFunction;
            this.OpName = opName;
        }
    }
}
