using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STodoLib
{
    public class Class1
    {
        public Class1()
        {
        }

        public void Fire()
        {

        }

    }


    public static class TodoLoader
    {
        public static TodoObject GetObject(string path)
        {
            return new TodoFileObject(System.IO.File.ReadAllText(path));
        }

    }







    public abstract class TodoObject
    {



    }

    public class TodoFileObject : TodoObject
    {
        public TodoFileObject(string content)
        {

        }
    }




    public class TodoSection
    {


    }


    public class TodoItem
    {
    }


}
