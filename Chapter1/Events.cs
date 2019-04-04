using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    /// <summary>
    /// Before async/await a program would be forced to use events to manage asyncronous operations
    /// Events are used to notify an object that something has happened.await An object can be made to publish event to which other objects can subscribe
    /// Components of a solution that comunicates through messages such as events are loosely coupled as the only thing one component needs to know about the other
    /// is the design of the publish/subscribe mechanism
    /// </summary>
    public class Events
    {
        class ActionAlarm
        {
            // This is dangerous as it is a public property, can be set externally and also client code can call the OnAlarmRaised 
            public Action OnAlarmRaised{get; set;}

            //Called to raise the alarm if someone has subscribed
            public void RaiseAlarm()
            {                
                OnAlarmRaised?.Invoke();
            }
        }

        class AlarmEventArgs : EventArgs
        {
            public string Location {get; set;}

            public AlarmEventArgs(string location)
            {
                Location = location;
            }
        }
        class EventHandlerAlarm
        {
            public event EventHandler<AlarmEventArgs> OnAlarmRaised = delegate {};
            public void RaiseAlarm(string location)
            {                
                OnAlarmRaised(this, new AlarmEventArgs(location));
            }

            public void RaiseAlarmWithAggregateException(string location){

                var exceptionList = new List<Exception>();

                foreach(Delegate handler in OnAlarmRaised.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke(this, new AlarmEventArgs(location));
                    }
                    catch(TargetInvocationException e)
                    {
                        exceptionList.Add(e.InnerException);
                    }
                }

                if(exceptionList.Any())
                {
                    throw new AggregateException(exceptionList);
                }
            }
        }

        private bool _alarm1Raised;
        private bool _alarm2Raised;

        public Events()
        {
            _alarm1Raised = false;
            _alarm2Raised = false;
        }

        [Fact]
        /// <summary>
        /// Action delegate represents a method that does not return a result and does not accept any parameters
        /// Can be used as binding point for subscribers
        /// </summary>
        /// <returns></returns>
        public void ActionPublishSubscribe()
        {
            ActionAlarm alarm = new ActionAlarm();

            alarm.OnAlarmRaised += AlarmListener1;
            alarm.OnAlarmRaised += AlarmListener2;

            Assert.Equal(false, _alarm1Raised);
            Assert.Equal(false, _alarm2Raised);

            alarm.RaiseAlarm();

            Assert.Equal(true, _alarm1Raised);
            Assert.Equal(true, _alarm2Raised);

            _alarm1Raised = false;
            _alarm2Raised = false; 

            alarm.OnAlarmRaised -= AlarmListener1;            

            Assert.Equal(false, _alarm1Raised);
            Assert.Equal(false, _alarm2Raised);

            alarm.RaiseAlarm();

            Assert.Equal(false, _alarm1Raised);
            Assert.Equal(true, _alarm2Raised);
        }

        [Fact]
        public void EventHandlerPublishSubscribe(){
            var alarm = new EventHandlerAlarm();

            alarm.OnAlarmRaised += EventArgsAlarmListener1;
            alarm.OnAlarmRaised += EventArgsAlarmListener2;

            Assert.Equal(false, _alarm1Raised);
            Assert.Equal(false, _alarm2Raised);

            alarm.RaiseAlarm("location1");

            Assert.Equal(true, _alarm1Raised);
            Assert.Equal(true, _alarm2Raised);
        }

        [Fact]
        public void ExceptionsAtSubscriberLevel(){
            var alarm = new EventHandlerAlarm();

            alarm.OnAlarmRaised += EventArgsAlarmListener1Exception;
            alarm.OnAlarmRaised += EventArgsAlarmListener2;

            Assert.Equal(false, _alarm1Raised);
            Assert.Equal(false, _alarm2Raised);

            Exception ex = Assert.ThrowsAny<Exception>(() => alarm.RaiseAlarm("location1"));
            Assert.Equal("Listener 1 Failed", ex.Message);

            Assert.Equal(true, _alarm1Raised);
            Assert.Equal(false, _alarm2Raised);

            _alarm1Raised = false;
            _alarm2Raised = false;

            AggregateException aggEx = Assert.ThrowsAny<AggregateException>(() => alarm.RaiseAlarmWithAggregateException("location1"));
            Assert.Equal(aggEx.InnerExceptions[0].Message, "Listener 1 Failed");

            Assert.Equal(true, _alarm1Raised);
            Assert.Equal(true, _alarm2Raised);
        }

        private void EventArgsAlarmListener1Exception(object sender, EventArgs e)
        {
            _alarm1Raised = true;
            throw new Exception("Listener 1 Failed");
        }

        private void EventArgsAlarmListener1(object sender, EventArgs e)
        {
            _alarm1Raised = true;
        }

        private void EventArgsAlarmListener2(object sender, EventArgs e)
        {
            _alarm2Raised = true;
        }

        private void AlarmListener1(){
            _alarm1Raised = true;
        }

        private void AlarmListener2(){
            _alarm2Raised = true;
        }

    }
}
