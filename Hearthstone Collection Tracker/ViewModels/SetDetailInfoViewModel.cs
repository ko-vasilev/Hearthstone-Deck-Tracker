using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Collection_Tracker.Internal;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    public class SetDetailInfoViewModel : DependencyObject, INotifyPropertyChanged
    {
        public SetDetailInfoViewModel()
        {
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SetCards")
                {
                    List<CardStatsByRarity> cardStats = SetCards.GroupBy(c => c.Card.Rarity, c => c)
                        .Select(gr => new CardStatsByRarity(gr.Key, gr.AsEnumerable())).ToList();
                    TotalSetStats = new CardStatsByRarity("Total", SetCards);
                    cardStats.Add(TotalSetStats);
                    StatsByRarity = cardStats;
                }
            };
        }

        public string SetName { get; set; }

        public static readonly DependencyProperty SetCardsProperty = DependencyProperty.Register("SetCards",
            typeof (TrulyObservableCollection<CardInCollection>), typeof (SetDetailInfoViewModel),
            new PropertyMetadata(new TrulyObservableCollection<CardInCollection>()));

        public TrulyObservableCollection<CardInCollection> SetCards
        {
            get { return (TrulyObservableCollection<CardInCollection>)GetValue(SetCardsProperty); }
            set
            {
                if (SetCards != null)
                {
                    SetCards.CollectionChanged -= NotifySetCardsChanged;
                }
                SetValue(SetCardsProperty, value);
                if (value != null)
                {
                    value.CollectionChanged += NotifySetCardsChanged;
                    OnPropertyChanged("SetCards");
                }
            }
        }

        public static readonly DependencyProperty StatsByRarityProperty = DependencyProperty.Register("StatsByRarity",
            typeof (IEnumerable<CardStatsByRarity>), typeof (SetDetailInfoViewModel));

        public IEnumerable<CardStatsByRarity> StatsByRarity
        {
            get { return (IEnumerable<CardStatsByRarity>) GetValue(StatsByRarityProperty); }
            private set { SetValue(StatsByRarityProperty, value); }
        }

        public static readonly DependencyProperty TotalSetStatsProperty = DependencyProperty.Register("TotalSetStats",
    typeof(CardStatsByRarity), typeof(SetDetailInfoViewModel));

        public CardStatsByRarity TotalSetStats
        {
            get { return (CardStatsByRarity)GetValue(TotalSetStatsProperty); }
            private set { SetValue(TotalSetStatsProperty, value); }
        }

        private void NotifySetCardsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged("SetCards");
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class CardStatsByRarity
    {
        private static readonly ReadOnlyDictionary<string, double> CardProbabilities = new ReadOnlyDictionary<string, double>(
            new Dictionary<string, double>
            {
                { "Common", 0.7 },
                { "Rare", 0.214 },
                { "Epic", 0.0428 },
                { "Legendary", 0.0108 }
            });

        private static readonly ReadOnlyDictionary<string, double> GoldenCardProbabilities = new ReadOnlyDictionary<string, double>(
            new Dictionary<string, double>
            {
                { "Common", 0.0147 },
                { "Rare", 0.0137 },
                { "Epic", 0.00308 },
                { "Legendary", 0.00111 }
            });

        public CardStatsByRarity() { }

        public CardStatsByRarity(string rarity, IEnumerable<CardInCollection> cards)
        {
            Rarity = rarity;
            TotalAmount = cards.Select(c => c.Card.Id)
                .Distinct()
                .Count() * 2;
            PlayerHas = cards.Sum(c => c.AmountNonGolden > 2 ? 2 : c.AmountNonGolden);
            PlayerHasGolden = cards.Sum(c => c.AmountGolden > 2 ? 2 : c.AmountGolden);

            OpenGoldenOdds = CalculateOpeningOdds(cards, card => 2 - card.AmountGolden, GoldenCardProbabilities);
            OpenNonGoldenOdds = CalculateOpeningOdds(cards, card => 2 - card.AmountNonGolden, CardProbabilities);
        }

        private const int CARDS_IN_PACK = 5;

        public string Rarity { get; set; }

        public int TotalAmount { get; set; }

        public int PlayerHas { get; set; }

        public int PlayerHasGolden { get; set; }

        public double OpenGoldenOdds { get; set; }

        public double OpenNonGoldenOdds { get; set; }

        private double CalculateOpeningOdds(IEnumerable<CardInCollection> cards, Func<CardInCollection, int> cardsAmount, IDictionary<string, double> probabilities)
        {
            double rarityOdds = 1.0;
            foreach (var group in cards.GroupBy(c => c.Card.Rarity, cardsAmount))
            {
                double currentProbability = probabilities[group.Key];
                int missingCardsAmount = group.Sum();
                int totalCardsAmount = group.Count()*2;
                double missingCardsOdds = (double) missingCardsAmount/totalCardsAmount;
                double openingCardOdds = 1.0;
                for (int i = 0; i < CARDS_IN_PACK; ++i)
                {
                    openingCardOdds *= 1 - currentProbability*missingCardsOdds;
                }
                double oddsInPack = 1 - openingCardOdds;
                rarityOdds *= (1 - oddsInPack);
            }
            return 1 - rarityOdds;
        }
    }
}
