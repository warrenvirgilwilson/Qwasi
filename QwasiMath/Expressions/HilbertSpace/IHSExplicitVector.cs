using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QwasiMath.Expressions.Vectors;

namespace QwasiMath.Expressions.HilbertSpace
{
    public partial interface IHSExplicitVector<TBasis> : IExplicitGeneralizedVector<IHSExplicitVector<TBasis>, TBasis>
    {
        public static IHSExplicitVectorValue<TBasis> VectorValue(IEnumerable<VectorTerm<TBasis>> terms)
            => new HSExplicitVectorValue<TBasis>(terms);

        public static IHSExplicitVectorValue<TBasis> ZeroVector(IEnumerable<TBasis> basisVectors)
            => VectorValue(basisVectors.Select(bv => new VectorTerm<TBasis>(bv, IAlgebraicExpression.RealNumber(0))));

        public static IHSExplicitVectorValue<TBasis> ArbitraryVector(IEnumerable<TBasis> basisVectors, object vectorIdentifier)
            => VectorValue(basisVectors.Select(bv => new VectorTerm<TBasis>(bv, IAlgebraicExpression.Variable(vectorIdentifier, bv))));

        public static IHSExplicitVectorVariable<TBasis> Variable(object identifier)
            => new HSExplicitVectorVariable<TBasis>(identifier);

        public static IHSExplicitVectorVariable<TBasis> Variable(params object[] identifier)
            => new HSExplicitVectorVariable<TBasis>(identifier);
    }
}
