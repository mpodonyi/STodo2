using Xunit;

namespace MyFirstDotNetCoreTests
{
    public class Class1
    {
        [Fact]
        public void PassingTest()
        {
            var tt = new STodoLib.Class1();

            tt.Fire();

            Assert.Equal(4, Add(2, 2));
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(5, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}