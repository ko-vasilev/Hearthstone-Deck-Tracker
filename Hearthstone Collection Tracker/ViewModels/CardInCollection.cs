using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class CardInCollection : INotifyPropertyChanged
    {
        public CardInCollection(Card card, int amountNonGolden = 0, int amountGolden = 0)
        {
            Card = card;
            AmountNonGolden = amountNonGolden;
            AmountGolden = amountGolden;
        }

        [XmlIgnore]
        public Card Card { get; private set; }

        private int _amountNonGolden;

        public int AmountNonGolden
        {
            get { return _amountNonGolden; }
            set
            {
                _amountNonGolden = value;
                OnPropertyChanged();
            }
        }

        private int _amountGolden;

        public int AmountGolden
        {
            get { return _amountGolden; }
            set
            {
                _amountGolden = value;
                OnPropertyChanged();
            }
        }

        public int MaxAmountInCollection
        {
            get
            {
                if (Card == null)
                    throw new ArgumentNullException();
                return Card.Rarity == "Legendary" ? 1 : 2;
            }
        }

        [XmlIgnore]
        public string CardClass
        {
            get { return Card == null ? string.Empty : Card.GetPlayerClass; }
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
