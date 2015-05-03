using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class CardInCollection : INotifyPropertyChanged
    {
        public CardInCollection(Card card, bool isGolden, int amount = 0)
        {
            Card = card;
            Amount = amount;
            IsGolden = isGolden;
        }

        public Card Card { get; private set; }

        private int _amount;

        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        private bool _isGolden;

        public bool IsGolden
        {
            get { return _isGolden; }
            set
            {
                _isGolden = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
