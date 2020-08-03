using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;

namespace GeneralMath.Expressions.Vectors
{
    public interface IExplicitVectorValue<TVector, TBasis> :
        IExplicitGeneralizedVector<TVector, TBasis>,
        IVectorValue<TVector>,
        IEnumerable<VectorTerm<TBasis>>
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>
    {
        IAlgebraicExpression this[TBasis basisVector] { get; }
        bool ContainsBasisTerm(TBasis basisVector);
    }

    public struct VectorTerm<TBasis>
    {
        public static implicit operator VectorTerm<TBasis>((TBasis basisVector, IAlgebraicExpression coefficient) tuple)
        {
            return new VectorTerm<TBasis>(tuple.basisVector, tuple.coefficient);
        }

        public TBasis BasisVector { get; }
        public IAlgebraicExpression Coefficient { get; }

        public VectorTerm(TBasis basisVector, IAlgebraicExpression coefficient)
        {
            this.BasisVector = basisVector;
            this.Coefficient = coefficient;
        }
    }

    public abstract class ExplicitVectorValue<TVector, TBasis> : IExplicitVectorValue<TVector, TBasis>
        where TVector : class, IExplicitGeneralizedVector<TVector, TBasis>
    {
        protected IDictionary<TBasis, IAlgebraicExpression> Terms { get; }

        public IAlgebraicExpression this[TBasis basisVector]
            => this.Terms.ContainsKey(basisVector) ? this.Terms[basisVector] : IAlgebraicExpression.RealNumber(0);

        public bool ContainsBasisTerm(TBasis basisVector) => this.Terms.ContainsKey(basisVector);

        protected abstract TVector NewExplicitVectorValue(IDictionary<TBasis, IAlgebraicExpression> terms);

        public virtual TVector Add(TVector other)
        {
            if (other is IExplicitVectorValue<TVector, TBasis> v)
            {
                var retTerms = new Dictionary<TBasis, IAlgebraicExpression>(this.Terms);

                foreach (var term in v)
                {
                    if (retTerms.ContainsKey(term.BasisVector))
                        retTerms[term.BasisVector] = retTerms[term.BasisVector].Add(term.Coefficient);
                    else
                        retTerms[term.BasisVector] = term.Coefficient;
                }

                return this.NewExplicitVectorValue(retTerms);
            }

            return IExplicitGeneralizedVector<TVector, TBasis>.Addition(this.AsExpressionType(), other);
        }

        public virtual TVector SubtractBy(TVector other)
        {
            if (other is IExplicitVectorValue<TVector, TBasis> v)
            {
                var retTerms = new Dictionary<TBasis, IAlgebraicExpression>(this.Terms);

                foreach (var term in v)
                {
                    if (retTerms.ContainsKey(term.BasisVector))
                        retTerms[term.BasisVector] = retTerms[term.BasisVector].SubtractBy(term.Coefficient);
                    else
                        retTerms[term.BasisVector] = term.Coefficient.Negate();
                }

                return this.NewExplicitVectorValue(retTerms);
            }

            return IExplicitGeneralizedVector<TVector, TBasis>.Subtraction(this.AsExpressionType(), other);
        }

        public virtual TVector Negate()
        {
            var retTerms = new Dictionary<TBasis, IAlgebraicExpression>(this.Terms);

            foreach (var kvp in this.Terms)
                retTerms[kvp.Key] = kvp.Value.Negate();

            return this.NewExplicitVectorValue(retTerms);
        }

        public virtual TVector ScalarProduct(IAlgebraicExpression scalar)
        {
            var retTerms = new Dictionary<TBasis, IAlgebraicExpression>(this.Terms);

            foreach (var kvp in this.Terms)
                retTerms[kvp.Key] = scalar.Multiply(kvp.Value);

            return this.NewExplicitVectorValue(retTerms);
        }

        public virtual IAlgebraicExpression Length()
        {
            var squares = this.Select(term => term.Coefficient.Multiply(term.Coefficient));

            return IAlgebraicExpression.Sum(squares).SquareRoot();
        }

        public virtual TVector Normalize() => this.ScalarProduct(this.Length().Reciprocal());

        public virtual IAlgebraicExpression InnerProduct(TVector other)
        {
            if (other is IExplicitVectorValue<TVector, TBasis> v)
            {
                IList<IAlgebraicExpression> squares = new List<IAlgebraicExpression>();
                foreach (var term in this)
                    if (v.ContainsBasisTerm(term.BasisVector))
                        squares.Add(term.Coefficient.Multiply(v[term.BasisVector]));

                return IAlgebraicExpression.Sum(squares);
            }

            return IExplicitGeneralizedVector<TVector, TBasis>.InnerProductOperation(this.AsExpressionType(), other);
        }

        public virtual TVector ApplyOperator(IExplicitLinearOperator<TVector, TBasis> linearOperator)
            => this.ValueSum(this.Select(t => linearOperator.ApplyToBasisVector(t.BasisVector).ScalarProduct(t.Coefficient)));

        public TVector Evaluate(MathContext context)
        {
            var evalTerms = new Dictionary<TBasis, IAlgebraicExpression>();

            foreach (var term in this.Terms)
                evalTerms.Add(term.Key, term.Value.Evaluate(context));

            return this.NewExplicitVectorValue(evalTerms);
        }

        public TVector Substitute(MathContext context)
        {
            var evalTerms = new Dictionary<TBasis, IAlgebraicExpression>();

            foreach (var term in this.Terms)
                evalTerms.Add(term.Key, term.Value.Substitute(context));

            return this.NewExplicitVectorValue(evalTerms);
        }

        public IEnumerator<VectorTerm<TBasis>> GetEnumerator()
        {
            foreach (var term in this.Terms)
                yield return new VectorTerm<TBasis>(term.Key, term.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public TVector ValueSum(IEnumerable<TVector> terms)
        {
            IDictionary<TBasis, IAlgebraicExpression> sumTerms = new Dictionary<TBasis, IAlgebraicExpression>();

            foreach (var vector in terms.Cast<IExplicitVectorValue<TVector, TBasis>>())
            {
                foreach (var term in vector)
                {
                    if (sumTerms.ContainsKey(term.BasisVector))
                        sumTerms[term.BasisVector] = sumTerms[term.BasisVector].Add(term.Coefficient);
                    else
                        sumTerms[term.BasisVector] = term.Coefficient;
                }
            }

            return this.NewExplicitVectorValue(sumTerms);
        }

        public override string ToString()
        {
            return this.Aggregate("", (acc, term) => (acc == "" ? "" : acc + " ") + term.Coefficient + "|" + term.BasisVector + ">");
        }

        protected ExplicitVectorValue() => this.Terms = new Dictionary<TBasis, IAlgebraicExpression>();
        protected ExplicitVectorValue(IDictionary<TBasis, IAlgebraicExpression> terms) => this.Terms = terms;
        protected ExplicitVectorValue(IEnumerable<VectorTerm<TBasis>> terms)
        {
            this.Terms = new Dictionary<TBasis, IAlgebraicExpression>(
                terms.Select(v => new KeyValuePair<TBasis, IAlgebraicExpression>(v.BasisVector, v.Coefficient)));
        }
    }
}
