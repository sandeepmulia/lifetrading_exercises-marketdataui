using System.ComponentModel;

namespace MarketDataUI
{
    internal class Stock : INotifyPropertyChanged
    {
        private string _symbol;
        public decimal _bidprice;
        public decimal _askprice;
        public decimal _askQty;
        public decimal _bidQty;


        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Symbol)));
            }
        }

        public decimal BidPrice
        {
            get
            {
                return _bidprice;
            }
            set
            {
                _bidprice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BidPrice)));
            }
        }

        public decimal AskPrice
        {
            get
            {
                return _askprice;
            }
            set
            {
                _askprice = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AskPrice)));
            }
        }

        public decimal BidQty
        {
            get
            {
                return _bidQty;
            }
            set
            {
                _bidQty = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BidQty)));
            }
        }

        public decimal AskQty
        {
            get
            {
                return _askQty;
            }
            set
            {
                _askQty = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AskQty)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return string.Format("Symbol {0} : BidPrice {1} : AskPrice {2} : BidQty {3} : AskQty {4}", Symbol, BidPrice, AskPrice, BidQty, BidPrice);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

    }
}