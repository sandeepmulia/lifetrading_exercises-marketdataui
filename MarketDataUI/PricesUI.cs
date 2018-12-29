using MarketDataService;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MarketDataUI.Helper;

namespace MarketDataUI
{
    public partial class PricesUi : Form
    {
        private Task _consumer;
        private Task _producer;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private BindingList<Stock> _stocks = new BindingList<Stock>();
        private readonly IPriceClient _priceClient = new PriceClient();
        private BlockingCollection<Stock> _queue = new BlockingCollection<Stock>();
        private bool _formClosing = false;
        private bool _useMsgQueue = false;

        public PricesUi()
        {
            InitializeComponent();

            _useMsgQueue = SettingsHelper.GetSetting<bool>("UseMessageQueue", false);

            PricesDataGridView.AutoGenerateColumns = true;
            PricesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            PricesDataGridView.RowHeadersVisible = false;
            PricesDataGridView.DataSource = _stocks;

        }

        private void PriceClient_OnPriceChanged(object sender, PriceChangedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("Price changed.. {0} {1} {2} {3} {4}", e.Symbol, e.BidQty, e.BidPrice, e.AskQty, e.AskPrice);
#endif
            var stock = new Stock()
            {
                Symbol = e.Symbol,
                AskPrice = e.AskPrice,
                BidPrice = e.BidPrice,
                AskQty = e.AskQty,
                BidQty = e.BidQty
            };

            if (_useMsgQueue)
            {
                /*
                 * If Producer outpaces the consumer, then one can see a slight delay but the queue is
                 * acting as a buffer for the messages and it's assumed that this is acceptable. If not,
                 * then, the UseMessageQueue flag should be set to false in App.Config and the program should be rerun.
                 * 
                 * With Producer-Consumer queue, the task cancels will take a while i.e. consumer queue should
                 * become empty after which the program stops updating the grid
                 * */
                Queue.Add(stock);
            }
            else
            {

                /* This snippet of code will bypass the usage of concurrent queue which speeds up
                 * the consumption of data and response to stop task is instantaneous*/
                ProcessData(stock);                
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopTasks();
            Application.Exit();
        }


        /// <summary>
        /// Start the flow of price data to the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartConsumerTask();
            StartProducerTask();
        }

        /// <summary>
        /// Stop the flow of price data to the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopTasks();
        }

        /// <summary>
        /// This method will Stop the price client if it's running and issue cancel
        /// on the producer and consumer tasks
        /// </summary>
        internal protected void StopTasks()
        {
            if (_priceClient.IsRunning)
            {
                _priceClient.Stop();
            }

            _priceClient.OnPriceChanged -= PriceClient_OnPriceChanged;

            if (_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _cancellationToken.Cancel();
                }
                catch
                {
                    //Catch any exception when Thread is being cancelled. Do not rethrow
                }
            }
        }

        /// <summary>
        /// Starts the price client in a separate Task to ensure the UI is not blocked
        /// </summary>
        internal protected void StartProducerTask()
        {
            _producer = Task.Factory.StartNew(() =>
            {
                _priceClient.Start();
            }, _cancellationToken.Token);

            _priceClient.OnPriceChanged += PriceClient_OnPriceChanged;
        }

        /// <summary>
        /// Starts the consumer and UI updater task which consumes data put into the
        /// queue and then updates the BindingList bound to data grid triggering
        /// an update
        /// </summary>
        internal protected void StartConsumerTask()
        {
            _consumer = Task.Factory.StartNew(() =>
            {
                while (!Queue.IsCompleted)
                {
                    Queue.TryTake(out Stock item);
                    if (item != null)
                    {
                        ProcessData(item);
                    }
                }
            }, _cancellationToken.Token);
        }

        private void ProcessData(Stock item)
        {
            MethodInvoker invoker = delegate ()
            {
                var resObj = _stocks.ToList().Find(x => x.Symbol == item.Symbol);
                if (resObj == null)
                {
                    _stocks.Add(item);
                }
                else
                {
                    resObj.Symbol = item.Symbol;
                    resObj.AskPrice = item.AskPrice;
                    resObj.AskQty = item.AskQty;
                    resObj.BidPrice = item.BidPrice;
                    resObj.BidQty = item.BidQty;
                }
            };

            try
            {
                if (InvokeRequired)
                {
                    if (invoker != null && !this.IsDisposed)
                        Invoke(invoker);
                    Debug.WriteLine(item);
                }
                else
                {
                    invoker();
                }
            }
            catch (ObjectDisposedException ex)
            {
#if DEBUG
                Debug.WriteLine("Object disposed exception caught !", ex.Message);
#endif
            }

        }


        private void PricesUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_formClosing)
                return;
            e.Cancel = true;
            _formClosing = true;
        }

        private void PricesUi_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopTasks();
        }

        public BlockingCollection<Stock> Queue
        {
            get
            {
                return _queue;
            }

            set
            {
                _queue = value;
            }
        }
    }
}
