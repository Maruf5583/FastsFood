namespace FastsFood.ViewModel
{
    public class SubCategoryVm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // For display in Index
    }
}