using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DealCapture.Client.Annotations;

namespace DealCapture.Client.Dashboards
{
    public sealed class DealRowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly Guid _dealId;
        private readonly int _categoryVersion;
        private readonly int _dealVersion;
        private string _trader;
        private string _counterparty;
        private string _underlying;
        private decimal _notional;
        private DateTime _deliveryWindowStartDate;
        private DateTime _deliveryWindowEndDate;
        private string _status;
        private Direction _direction;

        #endregion
        
        public DealRowViewModel(Guid dealId, int categoryVersion, int dealVersion)
        {
            _dealId = dealId;
            _categoryVersion = categoryVersion;
            _dealVersion = dealVersion;
        }

        public Guid DealId { get { return _dealId; } }
        
        public string Trader
        {
            get { return _trader; }
            set
            {
                if (value == _trader) return;
                _trader = value;
                OnPropertyChanged();
            }
        }

        public string Counterparty
        {
            get { return _counterparty; }
            set
            {
                if (value == _counterparty) return;
                _counterparty = value;
                OnPropertyChanged();
            }
        }

        public string Underlying
        {
            get { return _underlying; }
            set
            {
                if (value == _underlying) return;
                _underlying = value;
                OnPropertyChanged();
            }
        }

        public decimal Notional
        {
            get { return _notional; }
            set
            {
                if (value == _notional) return;
                _notional = value;
                OnPropertyChanged();
            }
        }

        public Direction Direction
        {
            get { return _direction; }
            set
            {
                if (value == _direction) return;
                _direction = value;
                OnPropertyChanged();
            }
        }

        public DateTime DeliveryStart //DeliveryWindowStartDate
        {
            get { return _deliveryWindowStartDate; }
            set
            {
                if (value.Equals(_deliveryWindowStartDate)) return;
                _deliveryWindowStartDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime DeliveryEnd//DeliveryWindowEndDate
        {
            get { return _deliveryWindowEndDate; }
            set
            {
                if (value.Equals(_deliveryWindowEndDate)) return;
                _deliveryWindowEndDate = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public double PercentageFixed { get; set; }
        public int CategoryVersion { get { return _categoryVersion; } }
        public int DealVersion { get { return _dealVersion; } }


        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}