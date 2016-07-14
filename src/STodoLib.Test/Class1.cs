using Xunit;
using STodoLib;
using System.IO;

namespace STodoLib.Test
{
    public class FileParserTests
    {
        [Fact]
        public void GetSectionsTest()
        {
            var obj = TodoObjectFactory.GetFileObject(@"TestData\Todo.txt");


            var sect = obj.GetTodoSections();

            Assert.Equal(2, sect.Count);

            Assert.Equal("whatever", sect[0].Text.Trim());

            Assert.Equal("whatever2", sect[1].Text.Trim());



        }




    }

}