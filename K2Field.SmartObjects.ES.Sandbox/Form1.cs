using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace K2Field.SmartObjects.ES.Sandbox
{
    public partial class Form1 : Form
    {

        private IEventStoreConnection _connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));

        public Form1()
        {
            InitializeComponent();
            _connection.ConnectAsync();
            ListenToEventStore();
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            //var _connection = EventStoreConnectionFactory.Default();

            //var @event = new MeasurementRead(DateTime.Now, 10.12m).AsJson();
            //_connection.AppendToStream(deviceName, ExpectedVersion.Any, new[] { @event });

            //create connection 
            //run EventStore as addministrator .\EventStore.SingleNode.exe --db=db_name
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

            //add event to stream
            connection.AppendToStreamAsync("jonnoStream", ExpectedVersion.Any, ToEventData(Guid.NewGuid(), new TestEvent { EventName = "Foo", Message = "Some Message" }));
            //retrive slice of stream
            var slice = connection.ReadStreamEventsForwardAsync("jonnoStream", StreamPosition.Start, 100, false).Result;

            foreach (var ev in slice.Events)
            {
                Console.WriteLine("EVENT: " + ev.Event.Created.ToString() + " # " + System.Text.Encoding.Default.GetString(ev.Event.Data));
            }




            var logger = new ConsoleLogger();
            var projectionsManager = new ProjectionsManager(logger, IPEndPointFactory.DefaultHttp(), new TimeSpan(0, 3, 0));

            var all = projectionsManager.ListAllAsync(EventStoreCredentials.Default).Result;

            Console.WriteLine("PROJECTIONS: " + all);
            
            var json = JsonConvert.DeserializeObject<ProjectionList>(all);            

            foreach (var projection in json.Projections)
            {
                var name = projection.name;
                var status = projection.status;

                //Do something with the projection

                Console.WriteLine("PROJ: " + name + " - " + status);
            }

//            var Pname = "MyNewContinuousProjection";
//            var Pprojection = @"fromAll().when({ $any: function (s, e) { 
//                   s.lastEventName = e.eventType; return s; } } )";

//            projectionsManager.CreateContinuousAsync(Pname, Pprojection, EventStoreCredentials.Default);

        }

        //public static IEnumerable<ResolvedEvent> ReadAllEventsInStream(string streamName, IEventStoreConnection connection, int pageSize = 500)
        //{
        //    var result = new List<ResolvedEvent>();
        //    var coursor = StreamPosition.Start;
        //    StreamEventsSlice events = null;
        //    do
        //    {
        //        events = connection.ReadStreamEventsForwardAsync((streamName, coursor, pageSize, false);
        //        result.AddRange(events.Events);
        //        coursor = events.NextEventNumber;

        //    } while (events != null && !events.IsEndOfStream);

        //    return result;
        //}

        private static EventData ToEventData(Guid eventId, object @event, string eventName = null, IDictionary<string, object> headers = null )
        {
            if (headers == null) headers = new Dictionary<string, object>();
            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, serializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
                    {
                        {
                            "EventClrTypeName", @event.GetType().AssemblyQualifiedName
                        }
                    };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, serializerSettings));
            
            string typeName = string.Empty;
            if (!string.IsNullOrWhiteSpace(eventName))
            {
                typeName = eventName;
            }
            else
            {
                typeName = @event.GetType().Name;
            }
                

            return new EventData(eventId, typeName, true, data, metadata);
        }

        private void btnSendEvent_Click(object sender, EventArgs e)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

            //add event to stream
            connection.AppendToStreamAsync("jonnoStream", ExpectedVersion.Any, ToEventData(Guid.NewGuid(), new TestEvent { EventName = "Foo", Message = "Some Message " +new Random().Next(), Name = txtId.Text }, txtEventName.Text));

            //connection.Close();
        }

        private void ListenToEventStore()
        {
            StartReading();
        }

        public void StartReading()
        {
            //_connection.SubscribeToAllFrom(true, Appeared, Dropped, EventStoreCredentials.Default);
            _connection.SubscribeToStreamAsync("jonnoStream", true, Appeared, Dropped, EventStoreCredentials.Default);
            
            //_connection.SubscribeToAllAsync(true, Appeared, Dropped, EventStoreCredentials.Default);
        }

        private void Appeared(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            Console.WriteLine("WEB: " + resolvedEvent.Event.EventType);
            // do something with the events here
            // var @event = resolvedEvent.ParseJson();
        }

        private void Dropped(EventStoreSubscription subscription, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            // is called when the tcp connection is dropped, we could
            // implement recovery here
            //_connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            //StartReading();
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.ConnectAsync();

            //add event to stream
            connection.AppendToStreamAsync("jonnoStream", ExpectedVersion.Any, ToEventData(Guid.NewGuid(), new CustomerEvent { Name = "Foo"}, txtEventName.Text));

        }
    }

    public class CustomerEvent
    {
        public string Name { get; set; }
    }

    public class TestEvent
    {
        public string CorrelationId { get { return Guid.NewGuid().ToString(); } }
        public string EventName { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
    }



    internal class IPEndPointFactory
    {
        public static IPEndPoint DefaultTcp()
        {
            return CreateIPEndPoint(1113);
        }
        public static IPEndPoint DefaultHttp()
        {
            return CreateIPEndPoint(2113);
        }
        private static IPEndPoint CreateIPEndPoint(int port)
        {
            var address = IPAddress.Parse("127.0.0.1");
            return new IPEndPoint(address, port);
        }
    }

    public static class EventStoreConnectionFactory
    {
        public static IEventStoreConnection Default()
        {
            var connection = EventStoreConnection.Create(IPEndPointFactory.DefaultTcp());
            connection.ConnectAsync();
            return connection;
        }
    }

    public class EventStoreCredentials
    {
        private static readonly UserCredentials _credentials =
            new UserCredentials("admin", "changeit");

        public static UserCredentials Default { get { return _credentials; } }
    }

    public class ProjectionList
    {
        public List<Projection> Projections { get; set; }
    }

    public class Projection
    {
        public int coreProcessingTime { get; set; }
        public int version { get; set; }
        public int epoch { get; set; }
        public string effectiveName { get; set; }
        public int writesInProgress { get; set; }
        public int readsInProgress { get; set; }
        public int partitionsCached { get; set; }
        public string status { get; set; }
        public string stateReason { get; set; }
        public string name { get; set; }
        public string mode { get; set; }
        public string position { get; set; }
        public float progress { get; set; }
        public string lastCheckpoint { get; set; }
        public int eventsProcessedAfterRestart { get; set; }
        public string statusUrl { get; set; }
        public string stateUrl { get; set; }
        public string resultUrl { get; set; }
        public string queryUrl { get; set; }
        public string enableCommandUrl { get; set; }
        public string disableCommandUrl { get; set; }
        public string checkpointStatus { get; set; }
        public int bufferedEvents { get; set; }
        public int writePendingEventsBeforeCheckpoint { get; set; }
        public int writePendingEventsAfterCheckpoint { get; set; }
    }

}
