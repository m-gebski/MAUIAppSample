namespace Shared.RestModel.Query
{
    public record LinkCriterionModel
    {
        public string? LinkTypeId { get; set; }

        public string? Direction { get; set; }

        public bool? LinkExists { get; set; }

        public List<DataCriterionModel>? DataCriteria { get; set; }
    }
}
