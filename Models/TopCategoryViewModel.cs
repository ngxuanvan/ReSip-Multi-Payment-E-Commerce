namespace ResipWeb.Models
{
    /// <summary>
    /// ViewModel cho Top Categories để hiển thị trong Dashboard
    /// </summary>
    public class TopCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}
