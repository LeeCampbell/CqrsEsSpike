using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DealCapture.Client.Annotations;
using DealCapture.Client.Repositories;
using DealCapture.Client.Repositories.Dto;
using Microsoft.Practices.Prism.Commands;

namespace DealCapture.Client.DealCreation
{
    public sealed class DealCreationViewModel : INotifyPropertyChanged
    {
        private readonly IDealRepository _dealRepository;
        private readonly ObservableCollection<DealCreationSection> _sections = new ObservableCollection<DealCreationSection>();
        private string _counterparty;
        private readonly Guid _dealId;
        private ViewModelState _state;

        public DealCreationViewModel(IDealRepository dealRepository, Guid dealId)
        {
            _dealRepository = dealRepository;
            _dealId = dealId;
            AddSectionCommand = new DelegateCommand(AddSection);
            SubmitCommand = new DelegateCommand(Submit, CanSubmit);

            State = ViewModelState.Idle;
            AddSection();
        }

        public Guid DealId { get { return _dealId; } }

        public string Trader { get { return "Lee Campbell"; } }

        public ViewModelState State
        {
            get { return _state; }
            private set
            {
                if (Equals(value, _state)) return;
                _state = value;
                OnPropertyChanged();
                SubmitCommand.RaiseCanExecuteChanged();
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
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<DealCreationSection> Sections { get { return _sections; } }

        public DelegateCommand AddSectionCommand { get; private set; }

        public DelegateCommand SubmitCommand { get; private set; }
        

        private void AddSection()
        {
            var section = new DealCreationSection();
            section.RemoveSectionCommand = new DelegateCommand<DealCreationSection>(RemoveSection);
            section.PropertyChanged += Section_PropertyChanged;
            _sections.Add(section);
        }

        void Section_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SubmitCommand.RaiseCanExecuteChanged();
        }

        private void RemoveSection(DealCreationSection section)
        {
            section.PropertyChanged += Section_PropertyChanged;
            _sections.Remove(section);
            SubmitCommand.RaiseCanExecuteChanged();
        }

        private bool CanSubmit()
        {
            return
                State!=null
                && !State.IsProcessing
                && !string.IsNullOrEmpty(Counterparty)
                && Sections.Any()
                && Sections.All(s => s.IsValid);
        }

        private void Submit()
        {
            State = ViewModelState.Processing;
            
            var cmd = BuildCreateDealCommand();

            _dealRepository.CreateDeal(cmd)
                .SubscribeOn(Scheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(
                    _ => { },
                    ex =>
                    {
                        State = ViewModelState.Error(ex.ToString());
                    },
                    () =>
                    {
                        State = ViewModelState.Terminal;
                    });

        }

        private CreateDealCommand BuildCreateDealCommand()
        {
            return new CreateDealCommand
            {
                DealId = DealId,
                Trader = Trader,
                Counterparty = Counterparty,
                Sections = Sections.Select(BuildDealSection).ToList()
            };
        }

        private static DealSection BuildDealSection(DealCreationSection input)
        {
            return new DealSection
            {
                ProductType = input.ProductType,
                Direction = input.Direction.DtoValue,
                Notional = input.Notional.Value,
                DeliveryFrom = input.DeliveryFrom.Value,
                DeliveryUntil = input.DeliveryUntil.Value
            };
        }

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