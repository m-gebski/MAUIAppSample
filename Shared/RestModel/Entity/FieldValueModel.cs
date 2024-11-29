namespace Shared.RestModel.Entity
{
    public record FieldValueModel
    {
        public string? FieldTypeId { get; set; }

        public object? Value { get; set; }
    }
}
