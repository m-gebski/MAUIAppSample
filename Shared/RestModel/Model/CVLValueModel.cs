namespace Shared.RestModel.Model
{
    public class CVLValueModel
    {
        public string? Key { get; set; }

        public object? Value { get; set; }

        public int Index { get; set; }

        public string? ParentKey { get; set; }

        public bool Deactivated { get; set; }
    }
}
