using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace STodoLib.Models.FileObject
{
    public class TodoFileObject : TodoObject<TodoFileSection, TodoFileItem>, IDisposable
    {
        // private readonly MemoryStream stream;
        private readonly StringBuilder  s_stream;


        public TodoFileObject(string path)
        {
            s_stream = new StringBuilder(File.ReadAllText(path));

            //stream = new MemoryStream();
            //using (FileStream fs = File.OpenRead(path))
            //{
            //    fs.CopyTo(stream);
            //}
        }

        public void Dispose()
        {
            //if (stream != null)
            //    try
            //    {
            //        stream.Dispose();
            //    }
            //    catch { }
        }

        internal string GetDebugString(int start, int length)
        {
            return s_stream.ToString(start, length);
        }


        internal string GetText(TodoToken todoToken)
        {
            if (todoToken.ItemType == ItemTypes.Section)
            {
                return s_stream.ToString((int)todoToken.StartIndex, (int)(todoToken.EndIndex - todoToken.StartIndex));
            }
            if (todoToken.ItemType == ItemTypes.TodoItem)
            {
                string retval = s_stream.ToString((int)todoToken.StartIndex, (int)(todoToken.EndIndex - todoToken.StartIndex));

                return TrimStart(TrimStart(retval, "[]"), "[x]");
            }

            return null;
        }

        private static string TrimStart(string word, string trimWord)
        {
            var retVal = word.TrimStart();

            return retVal.StartsWith(trimWord)
                ? retVal.Substring(trimWord.Length)
                : word;
        }



        internal enum ItemTypes : byte
        {
            None = 0,
            WhiteLine = 1,
            Section = 2,
            TodoItem = 3,
        }

        private readonly SortedList<int, TodoToken> _lineCache = new SortedList<int, TodoToken>();
        private bool _ParsingComplete = false;

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
            public TodoToken(ItemTypes itemTypeFlag, int startIndex, int endIndex)
            {
                ItemType = itemTypeFlag;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public ItemTypes ItemType { get; }

            public int StartIndex { get; }

            public int EndIndex { get; set; }
        }




        private string ReadLine(StringReader reader, ref int _pos)
        {
            int startpos = _pos;
            string retval = null;

            int r;
            char chr;
            while ((r = reader.Read()) != -1)
            {
                chr = (char)r;
                _pos++;
                if (chr == '\r' || chr == '\n')
                {
                    char peeked = (char)reader.Peek();
                    if (chr == '\r' && peeked != -1 && peeked == '\n')
                    {
                        reader.Read();
                        _pos++;
                    }
                    break;
                }
                else
                {
                    retval += chr;
                }
            }
            return retval;
        }

        internal IEnumerable<TodoToken> GetObject(int startIndex, Func<TodoToken, bool> func, bool greedy)
        {
            TodoToken current = new TodoToken(ItemTypes.None, 0, 0);

            if (_lineCache.Count > 0)
            {
                foreach (int key in _lineCache.Keys.Where(k => k >= startIndex))
                {
                    current = _lineCache[key];
                    if (func(current))
                    {
                        yield return current;
                    }
                    else
                    {
                        if (!greedy)
                            yield break;
                    }
                }

                if (_ParsingComplete == true)
                    yield break;
            }

            int streamStart = current.EndIndex;
            int streamcurr = streamStart;

            using (StringReader reader = new StringReader(s_stream.ToString(startIndex, s_stream.Length - startIndex)))
            {
                while (reader.Peek() >= 0)
                {
                    string line = ReadLine(reader, ref streamcurr);

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            if (func(current))
                            {
                                yield return current;
                            }
                            else
                            {
                                if (!greedy)
                                    yield break;
                            }

                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            if (func(current))
                            {
                                yield return current;
                            }
                            else
                            {
                                if (!greedy)
                                    yield break;
                            }

                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                    }
                    else if (IsTextLine(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            current.EndIndex = streamcurr;
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            current.EndIndex = streamcurr;
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            current = new TodoToken(ItemTypes.Section, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.Section, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }

                    }
                    else if (IsTodo(line))
                    {
                        if (current.ItemType == ItemTypes.TodoItem)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            if (func(current))
                            {
                                yield return current;
                            }
                            else
                            {
                                if (!greedy)
                                    yield break;
                            }

                            current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.Section)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            if (func(current))
                            {
                                yield return current;
                            }
                            else
                            {
                                if (!greedy)
                                    yield break;
                            }

                            current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else if (current.ItemType == ItemTypes.WhiteLine)
                        {
                            _lineCache.Add(current.StartIndex, current);
                            current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                        else
                        {
                            current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
                            streamStart = streamcurr;
                        }
                    }
                }

                if (current.ItemType == ItemTypes.TodoItem)
                {
                    _lineCache.Add(current.StartIndex, current);
                    _ParsingComplete = true;
                    if (func(current))
                    {
                        yield return current;
                    }
                    else
                    {
                        if (!greedy)
                            yield break;
                    }
                }
                else if (current.ItemType == ItemTypes.Section)
                {
                    _lineCache.Add(current.StartIndex, current);
                    _ParsingComplete = true;
                    if (func(current))
                    {
                        yield return current;
                    }
                    else
                    {
                        if (!greedy)
                            yield break;
                    }
                }
                else if (current.ItemType == ItemTypes.WhiteLine)
                {
                    _lineCache.Add(current.StartIndex, current);
                    _ParsingComplete = true;
                }
            }
        }



        //internal IEnumerable<TodoToken> GetObject(int startIndex, ItemTypes itemType)
        //{
        //    TodoToken current = new TodoToken(ItemTypes.None, 0, 0);

        //    if (_lineCache.Count > 0)
        //    {
        //        foreach (int key in _lineCache.Keys.Where(k => k >= startIndex))
        //        {
        //            current = _lineCache[key];
        //            if (current.ItemType == itemType)
        //                yield return current;
        //        }

        //        if (_ParsingComplete == true)
        //            yield break;
        //    }

        //    int streamStart = current.EndIndex;
        //    int streamcurr = streamStart;

        //    using (StringReader reader = new StringReader(s_stream.Substring(startIndex)))
        //    {
        //        while (reader.Peek() >= 0)
        //        {
        //            string line = ReadLine(reader, ref streamcurr);

        //            if (string.IsNullOrWhiteSpace(line))
        //            {
        //                if (current.ItemType == ItemTypes.TodoItem)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    if (itemType == ItemTypes.TodoItem)
        //                        yield return current;

        //                    current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.Section)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    if (itemType == ItemTypes.Section)
        //                        yield return current;

        //                    current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.WhiteLine)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else
        //                {
        //                    current = new TodoToken(ItemTypes.WhiteLine, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //            }
        //            else if (IsTextLine(line))
        //            {
        //                if (current.ItemType == ItemTypes.TodoItem)
        //                {
        //                    current.EndIndex = streamcurr;
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.Section)
        //                {
        //                    current.EndIndex = streamcurr;
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.WhiteLine)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    current = new TodoToken(ItemTypes.Section, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else
        //                {
        //                    current = new TodoToken(ItemTypes.Section, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }

        //            }
        //            else if (IsTodo(line))
        //            {
        //                if (current.ItemType == ItemTypes.TodoItem)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    if (itemType == ItemTypes.TodoItem)
        //                        yield return current;

        //                    current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.Section)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    if (itemType == ItemTypes.Section)
        //                        yield return current;

        //                    current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else if (current.ItemType == ItemTypes.WhiteLine)
        //                {
        //                    _lineCache.Add(current.StartIndex, current);
        //                    current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //                else
        //                {
        //                    current = new TodoToken(ItemTypes.TodoItem, streamStart, streamcurr);
        //                    streamStart = streamcurr;
        //                }
        //            }
        //        }

        //        if (current.ItemType == ItemTypes.TodoItem)
        //        {
        //            _lineCache.Add(current.StartIndex, current);
        //            _ParsingComplete = true;
        //            if (itemType == ItemTypes.TodoItem)
        //                yield return current;
        //        }
        //        else if (current.ItemType == ItemTypes.Section)
        //        {
        //            _lineCache.Add(current.StartIndex, current);
        //            _ParsingComplete = true;
        //            if (itemType == ItemTypes.Section)
        //                yield return current;
        //        }
        //        else if (current.ItemType == ItemTypes.WhiteLine)
        //        {
        //            _lineCache.Add(current.StartIndex, current);
        //            _ParsingComplete = true;
        //        }
        //    }
        //}




        public override ReadOnlyCollection<TodoFileSection> GetTodoSections()
        {
            return GetObject(0, t => t.ItemType == ItemTypes.Section, true).Select(i => new TodoFileSection(i, this)).ToList().AsReadOnly();



        }

        public override ReadOnlyCollection<TodoFileItem> GetTodoItems()
        {
            return GetObject(0, t => t.ItemType == ItemTypes.TodoItem, true).Select(i => new TodoFileItem(i, this)).ToList().AsReadOnly();
        }
    }
}