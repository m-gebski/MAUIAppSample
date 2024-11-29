namespace Shared.RestModel.Query
{
    public record DataCriterionModel
    {
        public string? FieldTypeId { get; set; }

        public object? Value { get; set; }

        public string? Language { get; set; }

        public string? Operator { get; set; }
    }
}
