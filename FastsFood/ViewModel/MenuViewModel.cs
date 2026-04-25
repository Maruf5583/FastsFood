using FastsFood.Models;

namespace FastsFood.ViewModel
{
    public class MenuViewModel
    {
        public List<Item> Items { get; set; }
        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }
        public string SearchTerm { get; set; }
    }
}
