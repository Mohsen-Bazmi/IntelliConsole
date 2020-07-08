using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;

namespace IntelliConsole.Tests
{
    public static class ObservableTestExtensions
    {
        // public static IDisposable Listen<T>(this IObservable<T> observable)
        // => observable.Subscribe(new TestScheduler().CreateObserver<T>());
        // public static ITestableObserver<T> Listen<T>(this IObservable<T> observable)
        // {
        //     var observer = new TestScheduler().CreateObserver<T>();
        //     observable.Subscribe(observer);
        //     return observer;
        // }
        public static IEnumerable<T> Subscribe<T>(this IObservable<T> observable)
        {
            var observer = new TestScheduler().CreateObserver<T>();
            // observable.Catch(new Func<Exception, IObservable<T>>(e => throw e));
            observable.Subscribe(observer);
            return observer.Messages.Select(m => m.Value.Value);
        }
    }
}