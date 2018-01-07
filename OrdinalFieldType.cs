using EllieMae.Encompass.Query;

namespace EncompassLibrary.AdvancedSearch
{
    public enum OrdinalFieldMatchOtherType
    {
        BETWEEN = 1,
        NOTBETWEEN = 2
    }

    public class OrdinalFieldType
    {
        public OrdinalFieldMatchType OrdinalFieldMatchType { get; set; }
        public OrdinalFieldMatchOtherType OrdinalFieldMatchOtherType { get; set; }
    }
}
