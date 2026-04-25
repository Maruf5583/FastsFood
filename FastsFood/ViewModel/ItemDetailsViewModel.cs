using FastsFood.Models;

namespace FastsFood.ViewModel
{
    public class ItemDetailsViewModel
    {
        public Item Item { get; set; }
        public List<Item> RelatedItems { get; set; }
        public bool IsInCart { get; set; }
    }
}
