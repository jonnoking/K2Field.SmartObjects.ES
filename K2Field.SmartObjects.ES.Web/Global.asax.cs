using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace K2Field.SmartObjects.ES.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _connection.ConnectAsync();
            ListenToEventStore();
        }

        private IEventStoreConnection _connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));

        private void ListenToEventStore()
        {
            StartReading();
        }

        public void StartReading()
        {
            //_connection.SubscribeToAllFrom(true, Appeared, Dropped, EventStoreCredentials.Default);
            _connection.SubscribeToStreamAsync("jonnoStream", true, Appeared, Dropped, EventStoreCredentials.Default);
        }

        private void Appeared(EventStoreSubscription subscription, ResolvedEvent resolvedEvent)
        {
            
            // do something with the events here
            var @event = System.Text.Encoding.Default.GetString(resolvedEvent.Event.Data);
        }

        private void Dropped(EventStoreSubscription subscription, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            // is called when the tcp connection is dropped, we could
            // implement recovery here
            _connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            _connection.ConnectAsync();
            StartReading();
        }
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

}
