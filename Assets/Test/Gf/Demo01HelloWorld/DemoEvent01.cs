using System;
using Spring.Core;
using Spring.Event;
using Spring.Logger;

namespace Test.Gf.Demo01HelloWorld
{
    public class HelloEvent : IEvent
    {
        public string message;
    }

    public interface IStudent
    {
        void OnHelloEvent(HelloEvent eve);
    }
    
    [Bean]
    public class Student : IStudent
    {
        private int a = 1;
        private string b = "aaa";

        public override string ToString()
        {
            return "a=" + 1 + ";b=" + b;
        }

        [EventReceiver]
        public void OnHelloEvent(HelloEvent eve)
        {
            Log.Info(eve.message);
        }
    }

    [Bean]
    public class Teacher
    {
        [Autowired]
        private IStudent student;

        public void teachStudent()
        {
            Log.Info(student);
        }
    }
}