using STodoLib.Models;
using STodoLib.Models.FileObject;

namespace STodoLib
{
    public static class TodoObjectFactory
    {
        public static TodoObject<TodoFileSection, TodoFileItem> GetFileObject(string path)
        {
            return new TodoFileObject(path);
        }

    }
}