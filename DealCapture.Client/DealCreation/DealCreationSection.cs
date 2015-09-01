using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DealCapture.Client.Annotations;
using Microsoft.Practices.Prism.Commands;

namespace DealCapture.Client.DealCreation
{
    public sealed class DealCreationSection : INotifyPropertyChanged
    {
        private string _productType;
        private decimal? _notional;
        private DateTime? _deliverFrom;
        private DateTime? _deliverUntil;

        public DealCreationSection()
        {
            PropertyChanged += (s, e) => Validate();
        }

        public string ProductType
        {
            get { return _productType; }
            set
            {
                if (value == _productType) return;
                _productType = value;
                OnPropertyChanged();
            }
        }

        public decimal? Notional
        {
            get { return _notional; }
            set
            {
                if (value == _notional) return;
                _notional = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DeliverFrom
        {
            get { return _deliverFrom; }
            set
            {
                if (value.Equals(_deliverFrom)) return;
                _deliverFrom = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DeliverUntil
        {
            get { return _deliverUntil; }
            set
            {
                if (value.Equals(_deliverUntil)) return;
                _deliverUntil = value;
                OnPropertyChanged();
            }
        }

        public bool IsValid { get; private set; }

        private void Validate()
        {
            IsValid = !string.IsNullOrEmpty(ProductType)
                && Notional.HasValue
                && DeliverFrom.HasValue
                && DeliverUntil.HasValue;
        }

        public DelegateCommand<DealCreationSection> RemoveSectionCommand { get; set; }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}