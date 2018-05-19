﻿using EasyNetQ;
using Matrix.Agent.Configurator.Bus.Handlers;
using Matrix.Agent.Configurator.Business;
using Matrix.Agent.Configurator.Messages.Commands.Requests;
using Matrix.Agent.Messages;
using Matrix.Agent.Middlewares;
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Matrix.Agent.Configurator.Bus
{
    public class RabbitMiddleware : Middleware
    {
        public override bool Connected { get { return Bus != null && Bus.IsConnected; } }

        private IBus Bus { get; set; }

        private IApplicationService Application { get; }

        private IConfigurationService Server { get; }

        public RabbitMiddleware(IMiddlewareContext context, ILogger logger, IApplicationService application, IConfigurationService server)
            : base(context, logger)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public override async Task Connect()
        {
            await Task.Run(() =>
            {
                Bus = RabbitHutch.CreateBus(Context.Connection);

                Bus.SubscribeAsync<HeartBeat>(Process.GetCurrentProcess().ProcessName, new HeartBeatHandler(Logger, Application).Handle);
                Bus.SubscribeAsync<GetConfigurationRequest>(Process.GetCurrentProcess().ProcessName, new ConfigurationHandler(Logger, Server).Handle);
            });
        }

        public override async Task Disconnect()
        {
            await Task.Run(() => { });
        }

        public override async Task Send<T>(T o, int timeout = 0)
        {
            if (timeout.Equals(0))
            {
                await Bus.PublishAsync(o);
            }
            else
            {
                await Bus.PublishAsync(o, i =>
                {
                    i.WithExpires(timeout);
                });
            }
        }

        public override void Dispose()
        {
            Bus.Dispose();
        }
    }
}