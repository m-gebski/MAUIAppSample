namespace Shared.RestModel.Model
{
    public record EntityTypeModelV2
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public List<string>? FieldTypes { get; set; }

        public List<string>? InboundLinkTypes { get; set; }

        public List<string>? OutboundLinkTypes { get; set; }

        public bool IsLinkEntityType { get; set; }

        public List<string>? FieldSetIds { get; set; }

        public string? DisplayNameFieldTypeId { get; set; }

        public string? DisplayDescriptionFieldTypeId { get; set; }
    }
}
