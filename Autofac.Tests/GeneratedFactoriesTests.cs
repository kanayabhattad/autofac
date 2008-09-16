﻿using Autofac;
using NUnit.Framework;
using Autofac.Services;

namespace Autofac.Tests
{
    [TestFixture]
    public class GeneratedFactoriesTests
    {
        public class A<T>
        {
            public T P { get; private set; }

            public delegate A<T> Factory(T p);

            public A(T p)
            {
                P = p;
            }
        }

        [Test]
        public void CreateGenericFromFactoryDelegate()
        {
            var container = new Container();

            container.RegisterType<A<string>>().UnsharedInstances();
            container.RegisterGeneratedFactory<A<string>.Factory>(new TypedService(typeof(A<string>)));

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);
        }

        [Test]
        public void CreateGenericFromFactoryDelegateImpliedServiceType()
        {
            var container = new Container();

            container.RegisterType<A<string>>().UnsharedInstances();
            container.RegisterGeneratedFactory<A<string>.Factory>();

            var factory = container.Resolve<A<string>.Factory>();
            Assert.IsNotNull(factory);

            var s = "Hello!";
            var a = factory(s);
            Assert.IsNotNull(a);
            Assert.AreEqual(s, a.P);
        }

        public class QuoteService
        {
            public decimal GetQuote(string symbol)
            {
                return 2m;
            }
        }

        public class Shareholding
        {
            public delegate Shareholding Factory(string symbol, uint holding);

            public Shareholding(string symbol, uint holding, QuoteService qs)
            {
                Symbol = symbol;
                Holding = holding;
                _qs = qs;
            }

            private QuoteService _qs;

            public string Symbol { get; private set; }

            public uint Holding { get; set; }

            public decimal Quote()
            {
                return _qs.GetQuote(Symbol) * Holding;
            }
        }

        [Test]
        public void ShareholdingExample()
        {
            var container = new Container();

            container.RegisterType<QuoteService>();

            container.RegisterType<Shareholding>()
              .UnsharedInstances();

            container.RegisterGeneratedFactory<Shareholding.Factory>(
                new TypedService(typeof(Shareholding)));

            var shareholdingFactory = container.Resolve<Shareholding.Factory>();

            var shareholding = shareholdingFactory.Invoke("ABC", 1234);

            Assert.AreEqual("ABC", shareholding.Symbol);
            Assert.AreEqual(1234, shareholding.Holding);
            Assert.AreEqual(1234m * 2, shareholding.Quote());
        }

        public class StringHolder
        {
            public delegate StringHolder Factory();
            public string S;
        }

        [Test]
        public void RespectsContexts()
        {
            var container = new Container();
            container.RegisterType<StringHolder>()
                .InstancePerLifetimeScope();
            container.RegisterGeneratedFactory<StringHolder.Factory>(new TypedService(typeof(StringHolder)));

            container.Resolve<StringHolder>().S = "Outer";
            var inner = container.BeginLifetimeScope();
            inner.Resolve<StringHolder>().S = "Inner";

            var outerFac = container.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Outer", outerFac().S);

            var innerFac = inner.Resolve<StringHolder.Factory>();
            Assert.AreEqual("Inner", innerFac().S);
        }
    }
}