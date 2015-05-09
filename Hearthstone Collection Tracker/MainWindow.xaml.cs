﻿using Hearthstone_Deck_Tracker.Hearthstone;
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

        private void CardCollectionEditor_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void CardCollectionEditor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                if ((Filter.GoldenCards && c.AmountGolden >= c.MaxAmountInCollection)
                    || (!Filter.GoldenCards && c.AmountNonGolden >= c.MaxAmountInCollection))
                {
                    return false;
                }
            }
            if (Filter.FormattedText == string.Empty)
                return true;
            var cardName = Helper.RemoveDiacritics(c.Card.LocalizedName.ToLowerInvariant(), true);
            return cardName.Contains(Filter.FormattedText);
        }

        private CancellationTokenSource _filterCancel = new CancellationTokenSource();

        private async Task HandleFilterChange(object sender, PropertyChangedEventArgs args)
        {
            if (_filterCancel != null && !_filterCancel.IsCancellationRequested)
            {
                _filterCancel.Cancel();
            }

            if (args.PropertyName == "Text")
            {
                if (Filter.Text.Length < 4)
                {
                    // wait 300 ms before filtering
                    _filterCancel = new CancellationTokenSource();
                    Task t = Task.Delay(TimeSpan.FromMilliseconds(300), _filterCancel.Token);
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
                CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource).Refresh();
            }
        }

        private void TextBoxCollectionFilter_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var index = CardCollectionEditor.SelectedIndex;
            CardInCollection card = null;
            switch (e.Key)
            {
                case Key.Enter:
                    if (CardCollectionEditor.SelectedItem != null)
                        card = (CardInCollection)CardCollectionEditor.SelectedItem;
                    else if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D1:
                    if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D2:
                    if (CardCollectionEditor.Items.Count > 1)
                        card = (CardInCollection)CardCollectionEditor.Items[1];
                    break;
                case Key.D3:
                    if (CardCollectionEditor.Items.Count > 2)
                        card = (CardInCollection)CardCollectionEditor.Items[2];
                    break;
                case Key.D4:
                    if (CardCollectionEditor.Items.Count > 3)
                        card = (CardInCollection)CardCollectionEditor.Items[3];
                    break;
                case Key.D5:
                    if (CardCollectionEditor.Items.Count > 4)
                        card = (CardInCollection)CardCollectionEditor.Items[4];
                    break;
                case Key.Down:
                    if (index < CardCollectionEditor.Items.Count - 1)
                        CardCollectionEditor.SelectedIndex += 1;
                    break;
                case Key.Up:
                    if (index > 0)
                        CardCollectionEditor.SelectedIndex -= 1;
                    break;
            }
            if (card != null)
            {
                UpdateCardsAmount(card, 1);
                e.Handled = true;
            }
        }

        private void UpdateCardsAmount(CardInCollection card, int difference)
        {
            if (Filter.GoldenCards)
            {
                int newValue = card.AmountGolden + difference;
                newValue = Clamp(newValue, 0, card.MaxAmountInCollection);
                card.AmountGolden = newValue;
            }
            else
            {
                int newValue = card.AmountNonGolden + difference;
                newValue = Clamp(newValue, 0, card.MaxAmountInCollection);
                card.AmountNonGolden = newValue;
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        #endregion

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void CardCollectionEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            int? amount = null;
            if (e.Key == Key.Enter)
            {
                amount = 1;
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                amount = -1;
            }
            if (amount.HasValue)
            {
                CardInCollection card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null || string.IsNullOrEmpty(card.Card.Name))
                    return;

                UpdateCardsAmount(card, amount.Value);
            }
        }

        private void CardCollectionEditor_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, 1);
            }
        }

        private void CardCollectionEditor_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, -1);
            }
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
