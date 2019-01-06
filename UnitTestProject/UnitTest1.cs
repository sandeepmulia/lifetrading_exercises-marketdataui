using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarketDataUI;
using MarketDataService;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        public readonly MarketDataUI.PricesUi p;
        public readonly PrivateObject privateObject;

        public UnitTest1()
        {
            p = new MarketDataUI.PricesUi();
            privateObject = new PrivateObject(p);
        }
        

        [TestMethod]
        public void TestCoreDataProcessorWithMockClient()
        {
            var mock = new Mock<IPriceClient>();
            mock.Setup(x => x.Start(0));           
            mock.Object.OnPriceChanged += Client_OnPriceChanged;

            mock.Object.Start(0);
            var priceChangedEvent = new PriceChangedEventArgs("A", 10, 20.3m, 21.3m, 11);
            mock.Raise(item => item.OnPriceChanged += null, "sender", priceChangedEvent);

            var list = (BindingList<Stock>)privateObject.GetField("_stocks");
            //1. Check if the processed object in the bindinglist is the same as the event
            Assert.AreEqual(list[0], ConvertEventObjectToStock(priceChangedEvent));

            //2. Raise another event and check if the list contains the new object at index 1
            var priceChangedEvent1 = new PriceChangedEventArgs("B", 11, 20.4m, 21.3m, 11);
            mock.Raise(item => item.OnPriceChanged += null, "sender", priceChangedEvent1);

            list = (BindingList<Stock>)privateObject.GetField("_stocks");
            Assert.AreEqual(list[1], ConvertEventObjectToStock(priceChangedEvent1));

            //3. Raise an event which updates the exisiting entry in the list @ index 0
            var priceChangedEvent2 = new PriceChangedEventArgs("A", 211, 26.4m, 21.3m, 110);
            mock.Raise(item => item.OnPriceChanged += null, "sender", priceChangedEvent2);

            list = (BindingList<Stock>)privateObject.GetField("_stocks");
            Assert.AreEqual(list[0], ConvertEventObjectToStock(priceChangedEvent2));

        }

        public void Client_OnPriceChanged(object sender, PriceChangedEventArgs e)
        {
            //Check if Price client is raising events with valid data .
            //i.e. simple test check if symbol is always present
            Assert.IsTrue(e.Symbol != "");
            privateObject.Invoke("ProcessData", ConvertEventObjectToStock(e));
        }

        [TestMethod]
        public void TestModel()
        {
            var s = new MarketDataUI.Stock() { Symbol = "A", AskPrice = 10, BidPrice = 9, AskQty = 11, BidQty = 11 };
            var s1 = new MarketDataUI.Stock() { Symbol = "A", AskPrice = 10, BidPrice = 9, AskQty = 11, BidQty = 11 };

            Assert.AreEqual(s, s1);
            Assert.AreEqual(s.GetHashCode(), s1.GetHashCode());


            var s2 = new MarketDataUI.Stock() { Symbol = "B", AskPrice = 10, BidPrice = 9, AskQty = 11, BidQty = 11 };
            Assert.AreNotEqual(s, s2);
            Assert.AreNotEqual(s.GetHashCode(), s2.GetHashCode());
            Assert.AreEqual(s.GetHashCode(), s1.GetHashCode());
        }

        [TestMethod]
        public void TestQueueWithConsumerTaskRunning()
        {
            var queue = (BlockingCollection<Stock>)privateObject.GetField("_queue");
            
            queue.Add(new MarketDataUI.Stock()
            {
                Symbol = "A",
                AskPrice = 10,
                BidPrice = 9,
                BidQty = 11,
                AskQty = 12
            });

            queue.Add(new MarketDataUI.Stock()
            {
                Symbol = "B",
                AskPrice = 10,
                BidPrice = 9,
                BidQty = 11,
                AskQty = 12
            });

            queue.CompleteAdding();
            var cancelTokenSrc = new CancellationTokenSource();
            var token = cancelTokenSrc.Token;
            privateObject.Invoke("StartConsumerTask", token);

            while (!queue.IsCompleted)
            {
                //loop till queue is empty i.e. consumer has pulled out all elements
            }

            //Verify consumer has processed all elements in the queue
            Assert.AreEqual(queue.Count, 0);
        }

        public static Stock ConvertEventObjectToStock(PriceChangedEventArgs e)
        {
            return new Stock()
            {
                Symbol = e.Symbol,
                AskPrice = e.AskPrice,
                BidPrice = e.BidPrice,
                BidQty = e.BidQty,
                AskQty = e.AskQty
            };
        }
    }


}
