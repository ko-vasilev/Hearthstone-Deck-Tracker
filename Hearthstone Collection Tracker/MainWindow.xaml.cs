using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<Card> _cards;

        protected List<Card> Cards
        {
            get
            {
                if (_cards == null)
                {
                    _cards = Game.GetActualCards();
                }
                return _cards;
            }
        }

        public Thickness TitleBarMargin
        {
            get { return new Thickness(0, TitlebarHeight, 0, 0); }
        }

        public MainWindow()
        {
            InitializeComponent();

            SetStats.ItemsSource = SetCardsManager.Instance.SetCards.Select(gr => new SetDetailInfoViewModel
            {
                SetName = gr.Key,
                SetCards = new TrulyObservableCollection<CardInCollection>(gr.Value)
            });

            Filter = new FilterSettings();
            Filter.PropertyChanged += (sender, args) =>
            {
                HandleFilterChange(sender, args);
            };
        }

        private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
            }
        }

        private void SetDecrease(SetDetailInfoViewModel setInfo)
        {
            CardCollectionEditor.ItemsSource = setInfo.SetCards;

            OpenCollectionFlyout();
        }

        #region Collection management

        public FilterSettings Filter { get; set; }

        private void OpenCollectionFlyout()
        {
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource);
            view.Filter = CardsFilter;
            view.GroupDescriptions.Add(new PropertyGroupDescription("CardClass"));
            view.CustomSort = new CardInCollectionComparer();

            TextBoxCollectionFilter.Focus();
            FlyoutCollection.IsOpen = true;
        }

        private bool CardsFilter(object card)
        {
            CardInCollection c = card as CardInCollection;
            if (Filter.OnlyMissing)
            {
                if ((Filter.GoldenCards && c.AmountGolden >= 2) || (!Filter.GoldenCards && c.AmountNonGolden >= 2))
                {
                    return false;
                }
            }
            if (Filter.FormattedText == string.Empty)
                return true;
            var cardName = Helper.RemoveDiacritics(c.Card.LocalizedName.ToLowerInvariant(), true);
            return cardName.Contains(Filter.FormattedText);
        }

        private CancellationTokenSource filterCancel = new CancellationTokenSource();

        private async Task HandleFilterChange(object sender, PropertyChangedEventArgs args)
        {
            if (filterCancel != null && !filterCancel.IsCancellationRequested)
            {
                filterCancel.Cancel();
            }

            if (args.PropertyName == "Text")
            {
                if (Filter.Text.Length < 4)
                {
                    // wait 300 ms before filtering
                    filterCancel = new CancellationTokenSource();
                    Task t = Task.Delay(TimeSpan.FromMilliseconds(300), filterCancel.Token);
                    await t;
                }
                FilterCollection();
            }
            else
            {
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            if (CardCollectionEditor.ItemsSource != null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource).Refresh();
                }));
            }
        }

        private void TextBoxCollectionFilter_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        #endregion

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }
    }

    internal class CardInCollectionComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is CardInCollection && y is CardInCollection)
            {
                CardInCollection cardX = (CardInCollection)x;
                CardInCollection cardY = (CardInCollection)y;
                int manaCostCompare = cardX.Card.Cost.CompareTo(cardY.Card.Cost);
                if (manaCostCompare != 0)
                    return manaCostCompare;
                return cardX.Card.LocalizedName.CompareTo(cardY.Card.LocalizedName);
            }
            else
            {
                return 1;
            }
        }
    }
}
