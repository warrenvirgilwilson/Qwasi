using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QwasiMath.Expressions.Vectors;

namespace QwasiMath.Expressions.HilbertSpace
{
    public interface IHSExplicitVectorValue<TBasis>
        : IHSExplicitVector<TBasis>, IExplicitVectorValue<IHSExplicitVector<TBasis>, TBasis>
    {
    }

    public class HSExplicitVectorValue<TBasis>
        : ExplicitVectorValue<IHSExplicitVector<TBasis>, TBasis>, IHSExplicitVectorValue<TBasis>
    {
        protected override IHSExplicitVector<TBasis> NewExplicitVectorValue(IDictionary<TBasis, IAlgebraicExpression> terms)
        {
            return new HSExplicitVectorValue<TBasis>(terms);
        }

        public override IAlgebraicExpression Length()
        {
            var squares = this.Select(term => term.Coefficient.ComplexConjugate().Multiply(term.Coefficient));

            return IAlgebraicExpression.Sum(squares).SquareRoot();
        }

        public override IAlgebraicExpression InnerProduct(IHSExplicitVector<TBasis> other)
        {
            if (other is HSExplicitVectorValue<TBasis> v)
            {
                IList<IAlgebraicExpression> squares = new List<IAlgebraicExpression>();
                foreach (var term in this)
                    if (v.ContainsBasisTerm(term.BasisVector))
                        squares.Add(term.Coefficient.ComplexConjugate().Multiply(v[term.BasisVector]));

                return IAlgebraicExpression.Sum(squares);
            }

            return IHSExplicitVector<TBasis>.InnerProductOperation(this.AsExpressionType(), other);
        }

        public HSExplicitVectorValue() : base() { }
        public HSExplicitVectorValue(IDictionary<TBasis, IAlgebraicExpression> terms) : base(terms) { }
        public HSExplicitVectorValue(IEnumerable<VectorTerm<TBasis>> terms) : base(terms) { }
    }
}
