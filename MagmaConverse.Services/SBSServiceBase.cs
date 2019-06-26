using System;
using MagmaConverse.Interfaces;
using MagmaConverse.Messaging;
using MagmaConverse.Models;
using log4net;

namespace MagmaConverse.Services
{
    public interface ISBSService : IDisposable
    {
        string Name { get; }
    }

    public abstract class SBSServiceBase<TModel, TData> : ISBSService
        where TModel : ISBSModel<TData>
        where TData : class
    {
        protected ILog Logger { get; }

        protected IMessagingService TheMessagingService { get; }


        public string Name  { get; }
        public TModel Model { get; }

        protected SBSServiceBase(string name, TModel model, IServiceCreationSettings settings = null)
        {
            this.Name = name;
            this.Model = model;

            this.Logger = LogManager.GetLogger(typeof(SBSServiceBase<TModel, TData>));

            if (settings == null || settings.NoMessaging == false)
            {
                this.TheMessagingService = MessagingServiceFactory.Create();
            }
        }

        public virtual void Dispose()
        {
            this.Model?.Dispose();
            this.TheMessagingService?.Dispose();
        }
    }
}
