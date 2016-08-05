using System;

namespace STodoLib.Models.FileObject
{
    public class TodoFileItem : TodoItem
    {
        private readonly TodoFileObject.TodoToken _TodoToken;
        private readonly TodoFileObject _TodoFileObject;

        internal TodoFileItem(TodoFileObject.TodoToken todoToken, TodoFileObject todoFileObject)
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

        public override string ToString()
        {
            return _TodoFileObject.GetDebugString((int)_TodoToken.StartIndex, (int)(_TodoToken.EndIndex - _TodoToken.StartIndex));
        }


    }
}