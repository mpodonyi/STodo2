using System.Collections.ObjectModel;

namespace STodoLib.Models
{
    public abstract class TodoSection<TItem> where TItem : TodoItem
    {
        public abstract string Text { get; set; }

        public abstract ReadOnlyCollection<TItem> GetTodoItems();
    }
}