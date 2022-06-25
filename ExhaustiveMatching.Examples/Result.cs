using System;
using System.Collections.Generic;

namespace Examples {
    public abstract class Result<TSuccess, TError> : IEquatable<Result<TSuccess, TError>>
    {
        private Result() { }

        public sealed class Success : Result<TSuccess, TError>
        {
            public TSuccess Value { get; }

            public Success(TSuccess value) { Value = value; }

            public override bool Equals(Result<TSuccess, TError> other)
            {
                return other is Success success && EqualityComparer<TSuccess>.Default.Equals(Value, success.Value);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<TSuccess>.Default.GetHashCode(Value);
            }
        }

        public sealed class Error : Result<TSuccess, TError>
        {
            public TError Value { get; }

            public Error(TError value) { Value = value; }

            public override bool Equals(Result<TSuccess, TError> other)
            {
                return other is Error error && EqualityComparer<TError>.Default.Equals(Value, error.Value);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<TError>.Default.GetHashCode(Value);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Result<TSuccess, TError> result && Equals(result);
        }

        public abstract override int GetHashCode();

        public abstract bool Equals(Result<TSuccess, TError> other);

        public static bool operator ==(Result<TSuccess, TError> left, Result<TSuccess, TError> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Result<TSuccess, TError> left, Result<TSuccess, TError> right)
        {
            return !Equals(left, right);
        }
    }
}
