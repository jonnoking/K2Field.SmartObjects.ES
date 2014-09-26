using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using SourceCode.SmartObjects.Services.ServiceSDK.Objects;
using Attributes = SourceCode.SmartObjects.Services.ServiceSDK.Attributes;
using SourceCode.SmartObjects.Services.ServiceSDK.Types;
using EventStore.ClientAPI;
using System.Net;



namespace K2Field.SmartObjects.Services.ES
{
    [Attributes.ServiceObject("EventStoreEvent", "Event Store Event", "Event Store Event")]
    public class EventStoreEvent
    {
        private string stream = "";
        [Attributes.Property("Stream", SoType.Text, "Stream", "Stream")]
        public string Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        private string message = "";
        [Attributes.Property("Message", SoType.Memo, "Message", "Message")]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string eventname = "";
        [Attributes.Property("EventName", SoType.Memo, "Event Name", "Event Name")]
        public string EventName
        {
            get { return eventname; }
            set { eventname = value; }
        }

        private string messagename = "";
        [Attributes.Property("MessageName", SoType.Memo, "Message Name", "Message Name")]
        public string MessageName
        {
            get { return messagename; }
            set { messagename = value; }
        }

        private string resultstatus = "";
        [Attributes.Property("ResultStatus", SoType.Text, "Result Status", "Result Status")]
        public string ResultStatus
        {
            get { return resultstatus; }
            set { resultstatus = value; }
        }

        private string resultmessage = "";
        [Attributes.Property("ResultMessage", SoType.Text, "Result Message", "Result Message")]
        public string ResultMessage
        {
            get { return resultmessage; }
            set { resultmessage = value; }
        }


        [Attributes.Method("AppendToStream", SourceCode.SmartObjects.Services.ServiceSDK.Types.MethodType.Execute, "Append To Stream", "Append To Stream",
        new string[] { "Stream", "Message", "EventName" }, //required property array (no required properties for this sample)
        new string[] { "Stream", "Message", "EventName", "MessageName" }, //input property array (no optional input properties for this sample)
        new string[] { "Stream", "Message", "EventName", "MessageName", "ResultStatus", "ResultMessage" })] //return property array (2 properties for this example)
        public EventStoreEvent AppendToEventStoreStream()
        {
            // eventstore address, port, auth should be configurable
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            try
            {                
                connection.ConnectAsync().Wait();

                K2EventStoreMessage msg = new K2EventStoreMessage();
                msg.Id = Guid.NewGuid();
                msg.Message = this.Message;
                if (!string.IsNullOrWhiteSpace(this.MessageName))
                {
                    msg.Name = this.MessageName;
                }

                //add event to stream
                WriteResult result = Task.Run(() => connection.AppendToStreamAsync(this.Stream, ExpectedVersion.Any, EventStoreUtils.ToEventData(Guid.NewGuid(), msg, this.EventName)).Result).Result;
                this.resultstatus = "OK";
                
            }
            catch (Exception ex)
            {
                this.ResultStatus = "Error";
                this.ResultMessage = ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return this;
        }

    }

    public class K2EventStoreMessage
    {
        public Guid Id { get; set; }
        //public Guid? CorrelationId { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
    }
}
