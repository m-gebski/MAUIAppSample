namespace Shared.RestModel.Query
{
    public record SystemCriterionModel
    {
        public string? Type {  get; set; }

        public object? Value { get; set; }

        public string? Operator { get; set; }
    }
}
