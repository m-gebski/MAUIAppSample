namespace Shared.RestModel.Model
{
    public record FieldTypeModelV2
    {
        public string? Id { get; set; }

        public Dictionary<string,string>? Name { get; set; }

        public string? LocalizedName { get; set; }

        public Dictionary<string, string>? Description { get; set; }

        public string? LocalizedDescription { get; set; }

        public string? DataType { get; set; }

        public bool IsMultiValue { get; set; }

        public bool IsHidden { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsUnique { get; set; }

        public bool TrackChanges { get; set; }

        public string? DefaultValue { get; set; }

        public bool IsExcludedFromDefaultView { get; set; }

        public List<string>? IncludedInFieldSets { get; set; }

        public string? CategoryId { get; set; }

        public int Index { get; set; }

        public string? CvlId { get; set; }

        public string? ParentCvlId { get; set; }

        public Dictionary<string, string>? Settings { get; set; }

        public bool ExpressionSupport { get; set; }
    }
}
