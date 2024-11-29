namespace Shared.RestModel.Entity
{
    public record LinkModel
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public string LinkTypeId { get; set; }

        public int SourceEntityId { get; set; }

        public int TargetEntityId { get; set; }

        public int? LinkEntityId { get; set; }

        public int Index { get; set; }
    }
}
