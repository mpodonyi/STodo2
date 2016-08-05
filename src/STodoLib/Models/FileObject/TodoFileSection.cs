using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace STodoLib.Models.FileObject
{
    public class TodoFileSection : TodoSection<TodoFileItem>
    {
        private readonly TodoFileObject.TodoToken _TodoToken;
        private readonly TodoFileObject _TodoFileObject;

        internal TodoFileSection(TodoFileObject.TodoToken todoToken, TodoFileObject todoFileObject)
        {
            _TodoToken = todoToken;
            _TodoFileObject = todoFileObject;
        }

        public override string Text
        {
            get
            {
                return _TodoFileObject.GetText(_TodoToken);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override ReadOnlyCollection<TodoFileItem> GetTodoItems()
        {
            return _TodoFileObject.GetObject(_TodoToken.EndIndex, t => t.ItemType == TodoFileObject.ItemTypes.TodoItem , false).Select(i => new TodoFileItem(i, _TodoFileObject)).ToList().AsReadOnly();
        }

        public override string ToString()
        {
            return _TodoFileObject.GetDebugString((int)_TodoToken.StartIndex, (int)(_TodoToken.EndIndex - _TodoToken.StartIndex));
        }
    }
}