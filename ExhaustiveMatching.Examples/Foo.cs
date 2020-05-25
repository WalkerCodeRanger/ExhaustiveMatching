using System;

namespace Examples
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TestingAttribute : Attribute { }


    [Testing]
    [Testing]
    public class Foo
    {
    }
}
