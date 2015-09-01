using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DealCapture.Client.Annotations;
using DealCapture.Client.DealCreation;
using DealCapture.Client.DealEnrichment;
using DealCapture.Client.Repositories;
using Microsoft.Practices.Prism.Commands;

namespace DealCapture.Client.Dashboards
{
    public sealed class ActiveDealDashboardViewModel : INotifyPropertyChanged
    {
        private int _headVersion = -1;
        private readonly IDealRepository _dealRepo;
        private readonly ObservableCollection<DealRowViewModel> _activeDeals = new ObservableCollection<DealRowViewModel>();
        private DealRowViewModel _selectedDeal;
        private ViewModelState _state;

        public ActiveDealDashboardViewModel(IDealRepository dealRepo)
        {
            _dealRepo = dealRepo;

            State = ViewModelState.Idle;
            CreateDealCommand = new DelegateCommand(ShowDealCapture);
            EditSelectedDealCommand = new DelegateCommand(ShowDealEnrichment, () => SelectedDeal != null);
        }

        public IDisposable Start()
        {
            State = ViewModelState.Processing;

            return Observable.Create<DealRowViewModel>(
                async obs =>
                {
                    _headVersion = await _dealRepo.GetAllDealUpdatesHead();

                    return _dealRepo.GetAllDealUpdates()
                        .Subscribe(obs);
                })
                .SubscribeOn(Scheduler.Default)
                        .ObserveOnDispatcher()
                        .Subscribe(ApplyRowUpdate); ;

            
        }

        public DelegateCommand CreateDealCommand { get; private set; }
        public DelegateCommand EditSelectedDealCommand { get; private set; }
        public DealRowViewModel SelectedDeal
        {
            get { return _selectedDeal; }
            set
            {
                if (Equals(value, _selectedDeal)) return;
                _selectedDeal = value;
                OnPropertyChanged();
                EditSelectedDealCommand.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<DealRowViewModel> ActiveDeals { get { return _activeDeals; } }

        public ViewModelState State
        {
            get { return _state; }
            set
            {
                if (Equals(value, _state)) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        private void ShowDealCapture()
        {
            var dealId = Guid.NewGuid();

            var dealEntryVm = new DealCreationViewModel(_dealRepo, dealId);
            var view = new DealCaptureView
            {
                DataContext = dealEntryVm
            };

            dealEntryVm.OnPropertyChanges(vm => vm.State)
                .Where(state => state.IsTerminal)
                .TakeUntil(Observable.FromEventPattern(h => view.Closed += h, h => view.Closed -= h))
                .Take(1)
                .Subscribe(_ => view.Close());

            view.Show();
        }

        private void ShowDealEnrichment()
        {
            var dealEntryVm = new DealEnrichmentViewModel(_dealRepo, SelectedDeal.DealId);
            var view = new DealEnrichmentView
            {
                DataContext = dealEntryVm
            };

            view.Show();
        }

        private void ApplyRowUpdate(DealRowViewModel row)
        {
            int indexOf = IndexOf(ActiveDeals, ad => ad.DealId == row.DealId);
            if (indexOf == -1)
            {
                ActiveDeals.Add(row);
            }
            else
            {
                ActiveDeals[indexOf] = row;
            }

            if (row.CategoryVersion >= _headVersion)
            {
                State = ViewModelState.Idle;
            }
        }

        private int IndexOf<T>(ObservableCollection<T> source, Predicate<T> filter)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (filter(source[i]))
                {
                    return i;
                }
            }
            return -1;
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