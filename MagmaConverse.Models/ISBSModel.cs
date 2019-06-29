using System;
using Magmasystems.Framework;

namespace MagmaConverse.Models
{
    public interface ISBSModel<TData> : IDisposable 
        where TData : class
    {
        string Name { get; }
        DictionaryRepository<TData> Repository { get; }
    }

    public interface ISBSModel : ISBSModel<object>
    {
    }

    public interface ISingletonSBSModel<TData> : ISBSModel<TData> where TData : class
    {        
    }

    public interface IPersistableModel : IDisposable
    {
        void InitialDataLoad();
    }
}