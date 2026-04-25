using FastsFood.Models;

namespace FastsFood.ViewModel
{
    public class HomeViewModel
    {
        public List<Item> HeroItems { get; set; }
        public List<Item> PopularItems { get; set; }
        public List<Category> Categories { get; set; }
        public List<Item> TodaySpecial { get; set; }
    }
}
