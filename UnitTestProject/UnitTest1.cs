using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarketDataUI;
using MarketDataService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestProducerClient()
        {
            MarketDataUI.PricesUi p = new MarketDataUI.PricesUi();
            var cancelTokenSrc = new CancellationTokenSource();
            var token = cancelTokenSrc.Token;
            var privateObject = new PrivateObject(p);
            privateObject.Invoke("StartProducerTask", token);
            privateObject.Invoke("StartConsumerTask", token);
            Thread.Sleep(5000);
            cancelTokenSrc.Cancel();

            privateObject.Invoke("StopTasks");

            IPriceClient client = new PriceClient();
            var res = Task.Factory.StartNew(() => {
                client.Start();
            });
            
            client.OnPriceChanged += Client_OnPriceChanged;
            Thread.Sleep(1000);
            client.Stop();
            res.Wait();
        }

        private void Client_OnPriceChanged(object sender, PriceChangedEventArgs e)
        {
            //Check if Price client is raising events with valid data .
            //i.e. simple test check if symbol is always present
            Assert.IsTrue(e.Symbol != "");           
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
        }

        [TestMethod]
        public void TestQueueWithConsumer()
        {
            MarketDataUI.PricesUi p = new MarketDataUI.PricesUi();
            p.Queue.Add(new MarketDataUI.Stock()
            {
                Symbol = "A",
                AskPrice = 10,
                BidPrice = 9,
                BidQty = 11,
                AskQty = 12
            });

            p.Queue.Add(new MarketDataUI.Stock()
            {
                Symbol = "B",
                AskPrice = 10,
                BidPrice = 9,
                BidQty = 11,
                AskQty = 12
            });

            var privateObject = new PrivateObject(p);
            var cancelTokenSrc = new CancellationTokenSource();
            var token = cancelTokenSrc.Token;
            privateObject.Invoke("StartConsumerTask", token);

            //Introduce sleep and give consumer task a chance to clear the queue
            //whilst it consumes data
            Thread.Sleep(500);
            Assert.AreEqual(p.Queue.Count, 0);           
        }
    }
}
