﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Sanguosha.Core.Cards;
using System.Windows.Media;
using System.Windows.Input;
using Sanguosha.UI.Animations;
using Sanguosha.Core.Games;

namespace Sanguosha.UI.Controls
{
    public class PlayerViewBase : UserControl, IDeckContainer
    {
        #region Constructors

        public PlayerViewBase()
        {
            handCardArea = new CardStack();
        }

        #endregion

        CardStack handCardArea;

        #region Fields
        public PlayerViewModel PlayerModel
        {
            get
            {
                return DataContext as PlayerViewModel;
            }
        }

        public CardStack HandCardArea
        {
            get { return handCardArea; }
            set { handCardArea = value; }
        }

        private GameView parentGameView;
        
        public GameView ParentGameView 
        {
            get
            {
                return parentGameView;
            }
            set
            {
                parentGameView = value;
                handCardArea.ParentGameView = value;
            }
        }
        #endregion

        #region Abstract Functions

        // The following functions are in its essence abstract function. They are not declared
        // abstract only to make designer happy to render their subclasses. (VS and blend will
        // not be able to create designer view for abstract class bases.

        protected virtual void AddEquipment(CardView card)
        {
        }

        protected virtual CardView RemoveEquipment(Card card)
        {
            return null;
        }

        protected virtual void AddDelayedTool(CardView card)
        {
        }

        protected virtual CardView RemoveDelayedTool(Card card)
        {
            return null;
        }

        protected virtual void AddRoleCard(CardView card)
        {
        }

        protected virtual CardView RemoveRoleCard(Card card)
        {
            return null;
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Compute card position on global canvas such that the card center is aligned to the center of <paramref name="element"/>.
        /// </summary>
        /// <param name="card">Card to be aligned.</param>
        /// <param name="element">FrameworkElement to be aligned to.</param>
        /// <returns>Position of card relative to global canvas.</returns>
        protected Point ComputeCardCenterPos(CardView card, FrameworkElement element)
        {
            double width = element.ActualWidth;
            double height = element.ActualHeight;
            if (width == 0) width = element.Width;
            if (height == 0) height = element.Height;
            Point dest = element.TranslatePoint(new Point(element.Width / 2, element.Height / 2),
                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            return dest;
        }
        #endregion

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            foreach (CardView card in cards)
            {
                card.CardModel.IsEnabled = false;                
            }
            if (deck == DeckType.Hand)
            {
                handCardArea.AddCards(cards, 0.5d);
                foreach (var card in cards)
                {
                    PlayerModel.HandCards.Add(card.CardModel);
                }
                PlayerModel.HandCardCount += cards.Count;
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Card.Type as Equipment;
                    
                    if (equip != null)
                    {
                        EquipCommand command = new EquipCommand(){ Card = card.Card };
                        switch(equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = command;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = command;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = command;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = command;
                                break;
                        }
                    }
                    AddEquipment(card);
                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                foreach (var card in cards)
                {
                    AddDelayedTool(card);
                }
            }
            else if (deck == RoleGame.RoleDeckType)
            {
                foreach (var card in cards)
                {
                    AddRoleCard(card);
                }
            }
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards)
        {
            List<CardView> cardsToRemove = new List<CardView>();
            if (deck == DeckType.Hand)
            {
                foreach (var card in cards)
                {
                    bool found = false;
                    foreach (var cardView in handCardArea.Cards)
                    {
                        CardViewModel viewModel = cardView.DataContext as CardViewModel;
                        Trace.Assert(viewModel != null);
                        if (viewModel.Card == card)
                        {
                            cardsToRemove.Add(cardView);
                            PlayerModel.HandCards.Remove(viewModel);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        cardsToRemove.Add(CardView.CreateCard(card));
                    }
                }
                Trace.Assert(cardsToRemove.Count == cards.Count);
                handCardArea.RemoveCards(cardsToRemove);                
                PlayerModel.HandCardCount -= cardsToRemove.Count;
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Type as Equipment;

                    if (equip != null)
                    {
                        switch (equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = null;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = null;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = null;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = null;
                                break;
                        }
                    }
                    CardView cardView = RemoveEquipment(card);
                    cardsToRemove.Add(cardView);
                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                foreach (var card in cards)
                {
                    CardView cardView = RemoveDelayedTool(card);
                    cardsToRemove.Add(cardView);
                }
            }
            else if (deck == RoleGame.RoleDeckType)
            {
                foreach (var card in cards)
                {
                    CardView cardView = RemoveRoleCard(card);
                    cardsToRemove.Add(cardView);
                }
            }

            foreach (var card in cardsToRemove)
            {
                card.CardModel.IsSelectionMode = false;
            }

            return cardsToRemove;
        }

        public void UpdateCardAreas()
        {
            handCardArea.RearrangeCards(0d);
        }

        public virtual void PlayAnimation(AnimationBase animation, int playCenter, Point offset)
        {
        }

        public virtual void Tremble()
        {
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
        }

    }
}
