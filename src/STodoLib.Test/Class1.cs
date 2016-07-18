using Xunit;
using STodoLib;
using System.IO;

namespace STodoLib.Test
{
    public class FileParserTests
    {
        [Fact]
        public void GetAllTodoSectionsTest()
        {
            var obj = TodoObjectFactory.GetFileObject(@"TestData\Todo.txt");


            var sect = obj.GetTodoSections();

            Assert.Equal(2, sect.Count);

            Assert.Equal("whatever", sect[0].Text.Trim());

            Assert.Equal("whatever2", sect[1].Text.Trim());



        }

        [Fact]
        public void GetAllTodoItemsTest()
        {
            var obj = TodoObjectFactory.GetFileObject(@"TestData\Todo.txt");

            var sect = obj.GetTodoItems();

            Assert.Equal(3, sect.Count);

            Assert.Equal("mike\r\ntest", sect[0].Text.Trim());

            Assert.Equal("was here", sect[1].Text.Trim());

            Assert.Equal("no content", sect[2].Text.Trim());
        }



        //[Fact]
        //public void GetTodoItemsOf1stSectionTest()
        //{
        //    var obj = TodoObjectFactory.GetFileObject(@"TestData\Todo.txt");
        //    var sect = obj.GetTodoSections()[0];
        //    var items = sect.GetTodoItems();

        //    Assert.Equal(2, items.Count);

        //    Assert.Equal("mike\r\ntest", items[0].Text.Trim());

        //    Assert.Equal("was here", items[1].Text.Trim());
        //}





    }

}