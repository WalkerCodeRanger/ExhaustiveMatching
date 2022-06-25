namespace Examples {
    public abstract record ResultRecord<TSuccess, TError>
    {
        private ResultRecord() { }

        public sealed record Success(TSuccess Value) : ResultRecord<TSuccess, TError>;

        public sealed record Error(TError Value) : ResultRecord<TSuccess, TError>;
    }
}
