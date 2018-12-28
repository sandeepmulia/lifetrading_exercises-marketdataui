using MarketDataService;
using MarketDataUI.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarketDataUI
{
    public partial class PricesUi : Form
    {
        private Task _consumer;
        private Task _producer;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private BindingList<Stock> _stocks = new BindingList<Stock>();
        private readonly IPriceClient _priceClient = new PriceClient();
        private readonly BlockingCollection<Stock> _queue = new BlockingCollection<Stock>();
        private bool _formClosing = false;

        public PricesUi()
        {
            InitializeComponent();

            PricesDataGridView.AutoGenerateColumns = true;
            PricesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            PricesDataGridView.RowHeadersVisible = false;
            PricesDataGridView.DataSource = _stocks;

            _priceClient.OnPriceChanged += PriceClient_OnPriceChanged;
        }

        private void PriceClient_OnPriceChanged(object sender, PriceChangedEventArgs e)
        {
            Debug.WriteLine("Price changed.. {0} {1} {2} {3} {4}", e.Symbol, e.BidQty, e.BidPrice, e.AskQty, e.AskPrice);
            _queue.Add(new Stock()
            {
                Symbol = e.Symbol,
                AskPrice = e.AskPrice,
                BidPrice = e.BidPrice,
                AskQty = e.AskQty,
                BidQty = e.BidQty
            });
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _priceClient.OnPriceChanged -= PriceClient_OnPriceChanged;
            Application.Exit();
        }


        /// <summary>
        /// Start the flow of price data to the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _producer = Task.Factory.StartNew(() =>
            {
                _priceClient.Start();
            }, _cancellationToken.Token);

            _consumer = Task.Factory.StartNew(() =>
            {
                while (!_queue.IsCompleted)
                {
                    _queue.TryTake(out Stock item);
                    if (item != null)
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
                        }catch (ObjectDisposedException ex)
                        {
                            Debug.WriteLine("Object disposed exception caught !", ex.Message);
                        }
                    }
                }
            }, _cancellationToken.Token);
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

        private void StopTasks()
        {
            if (_priceClient.IsRunning)
            {
                _priceClient.Stop();
            }

            if (_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _cancellationToken.Cancel();
                }
                catch
                {

                }
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
    }
}
