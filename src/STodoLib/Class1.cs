using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace STodoLib
{
    public static class TodoObjectFactory
    {
        public static TodoObject<TodoFileSection, TodoItem> GetFileObject(string path)
        {
            return new TodoFileObject(path);
        }

    }


    public abstract class TodoObject<TSect, TItem>
        where TSect : TodoSection
        where TItem : TodoItem
    {
        public abstract ReadOnlyCollection<TSect> GetTodoSections();

        public abstract ReadOnlyCollection<TItem> GetTodoItems();


    }

    public class TodoFileObject : TodoObject<TodoFileSection, TodoItem>, IDisposable
    {
        private readonly MemoryStream stream;


        public TodoFileObject(string path)
        {
            stream = new MemoryStream();
            using (FileStream fs = File.OpenRead(path))
            {
                fs.CopyTo(stream);
            }
        }

        public void Dispose()
        {
            if (stream != null)
                try
                {
                    stream.Dispose();
                }
                catch { }
        }


        internal string GetText(TodoToken todoToken)
        {
            stream.Seek(todoToken.StartIndex, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 128, true))
            {
                if (todoToken.ItemType == ItemTypes.Section)
                {
                    long count = todoToken.EndIndex - todoToken.StartIndex;

                    char[] buffer = new char[(int)count];
                    reader.Read(buffer, (int)todoToken.StartIndex, (int)count);
                    return new string(buffer);
                }
            }

            return null;
        }



        internal enum ItemTypes : byte
        {
            None = 0,
            WhiteLine = 1,
            Section = 2,
            TodoItem = 3,
        }

        private List<TodoToken> _lineCache = new List<TodoToken>();


        private bool IsTodo(string line)
        {
            return line.TrimStart().StartsWith("[");
        }

        private bool IsTextLine(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return !IsTodo(line);

            return false;
        }

        internal class TodoToken
        {
            public TodoToken(ItemTypes itemTypeFlag, long startIndex, long endIndex)
            {
                ItemType = itemTypeFlag;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public ItemTypes ItemType { get; }

            public long StartIndex { get; }

            public long EndIndex { get; set; }
        }

        //public string ReadLine(StreamReader reader, out character)
        //{
        //    string str;
        //    if (this.charPos == this.charLen && this.ReadBuffer() == 0)
        //    {
        //        return null;
        //    }
        //    StringBuilder stringBuilder = null;
        //    do
        //    {
        //        int num = this.charPos;
        //        do
        //        {
        //            char chr = this.charBuffer[num];
        //            if (chr == '\r' || chr == '\n')
        //            {
        //                if (stringBuilder == null)
        //                {
        //                    str = new string(this.charBuffer, this.charPos, num - this.charPos);
        //                }
        //                else
        //                {
        //                    stringBuilder.Append(this.charBuffer, this.charPos, num - this.charPos);
        //                    str = stringBuilder.ToString();
        //                }
        //                this.charPos = num + 1;
        //                if (chr == '\r' && (this.charPos < this.charLen || this.ReadBuffer() > 0) && this.charBuffer[this.charPos] == '\n')
        //                {
        //                    this.charPos = this.charPos + 1;
        //                }
        //                return str;
        //            }
        //            num++;
        //        }
        //        while (num < this.charLen);
        //        num = this.charLen - this.charPos;
        //        if (stringBuilder == null)
        //        {
        //            stringBuilder = new StringBuilder(num + 80);
        //        }
        //        stringBuilder.Append(this.charBuffer, this.charPos, num);
        //    }
        //    while (this.ReadBuffer() > 0);
        //    return stringBuilder.ToString();
        //}




        internal IEnumerable<TodoToken> GetObject(int startIndex, ItemTypes itemType)
        {
            stream.Seek(startIndex, SeekOrigin.Begin);

            TodoToken current = new TodoToken(ItemTypes.None, 0, 0);

            long streamStart = startIndex;

            using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 0, true))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            _lineCache.Add(current);
                            if (itemType == ItemTypes.TodoItem)
                                yield return current;

                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            _lineCache.Add(current);
                            if (itemType == ItemTypes.Section)
                                yield return current;

                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current);
                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                    }
                    else if (IsTextLine(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            current.EndIndex = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            current.EndIndex = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current);
                            current = new TodoToken(ItemTypes.Section, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.Section, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }

                    }
                    else if (IsTodo(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            _lineCache.Add(current);
                            if (itemType == ItemTypes.TodoItem)
                                yield return current;

                            current = new TodoToken(ItemTypes.TodoItem, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            _lineCache.Add(current);
                            if (itemType == ItemTypes.Section)
                                yield return current;

                            current = new TodoToken(ItemTypes.TodoItem, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current);
                            current = new TodoToken(ItemTypes.TodoItem, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.TodoItem, streamStart, reader.BaseStream.Position);
                            streamStart = reader.BaseStream.Position;
                        }
                    }
                }

                if (current.ItemType == ItemTypes.TodoItem)
                {
                    _lineCache.Add(current);
                    if (itemType == ItemTypes.TodoItem)
                        yield return current;
                }
                else if (current.ItemType == ItemTypes.Section)
                {
                    _lineCache.Add(current);
                    if (itemType == ItemTypes.Section)
                        yield return current;
                }
                else if (current.ItemType == ItemTypes.WhiteLine)
                {
                    _lineCache.Add(current);
                }
            }
        }





        public override ReadOnlyCollection<TodoFileSection> GetTodoSections()
        {
            return (from i in GetObject(0, ItemTypes.Section)
                    select new TodoFileSection(i, this)).ToList().AsReadOnly();



        }

        public override ReadOnlyCollection<TodoItem> GetTodoItems()
        {
            throw new Exception();


        }
    }

    public abstract class TodoSection
    {
        public abstract string Text { get; set; }


    }

    public class TodoItem
    {


    }


    public class TodoFileSection : TodoSection
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


    }



}
