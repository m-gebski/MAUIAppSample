namespace Shared.RestModel.Query
{
    public record QueryModel
    {
        public List<SystemCriterionModel>? SystemCriteria { get; set; }

        public List<DataCriterionModel>? DataCriteria { get; set; }

        public List<LinkCriterionModel>? LinkCriteria { get; set; }

        public string? DataCriteriaOperator { get; set; }
    }
}
