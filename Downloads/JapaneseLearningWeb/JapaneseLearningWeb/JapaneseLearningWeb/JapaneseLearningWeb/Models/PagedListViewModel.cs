//namespace JapaneseLearningWeb.Models
//{
//    public class PagedListViewModel<T>
//    {
//        public IEnumerable<T> Items { get; set; } = new List<T>();
//        public int PageNumber { get; set; }
//        public int PageSize { get; set; }
//        public int TotalItems { get; set; }
//        public string Keyword { get; set; } = string.Empty;

//        public PagedListViewModel(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize, string keyword = "")
//        {
//            Items = items;
//            TotalItems = totalItems;
//            PageNumber = pageNumber;
//            PageSize = pageSize;
//            Keyword = keyword;
//        }
//    }
//}

namespace JapaneseLearningWeb.Models
{
    public class PagedListViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; }

        public PagedListViewModel()
        {
            Items = new List<T>();
        }

        public PagedListViewModel(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize, string keyword = "")
        {
            Items = items ?? new List<T>();
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Keyword = keyword;
        }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}