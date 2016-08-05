using System.Collections.ObjectModel;

namespace STodoLib.Models
{
    public abstract class TodoObject<TSect, TItem>
        where TSect : TodoSection<TItem>
        where TItem : TodoItem
    {
        public abstract ReadOnlyCollection<TSect> GetTodoSections();

        public abstract ReadOnlyCollection<TItem> GetTodoItems();


    }
}