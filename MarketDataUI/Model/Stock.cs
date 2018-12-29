using System.ComponentModel;

namespace MarketDataUI
{
    /// <summary>
    /// The core Model class which implements INotifyPropertyChanged to 
    /// indicate properties which have changed by firing an event
    /// </summary>
    public class Stock : INotifyPropertyChanged
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
                if (_symbol != value)
                {
                    _symbol = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Symbol)));
                }
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
                if (_bidQty != value)
                {
                    _bidQty = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(BidQty)));
                }
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
                if (_bidprice != value)
                {
                    _bidprice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(BidPrice)));
                }
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
                if (_askprice != value)
                {
                    _askprice = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AskPrice)));
                }
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
                if (_askQty != value)
                {
                    _askQty = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AskQty)));
                }
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

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Stock stk = (Stock)obj;
                return (Symbol == stk.Symbol) && (AskPrice == stk.AskPrice)
                     && (BidPrice == stk.BidPrice) && (AskQty == stk.AskQty)
                     && (BidQty == stk.BidQty);
            }
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode() ^ AskPrice.GetHashCode() ^ BidPrice.GetHashCode() ^ AskQty.GetHashCode() ^ BidQty.GetHashCode();
        }
    }
}