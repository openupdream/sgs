﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class WuXieKeJi : CardHandler
    {
        public static readonly CardAttribute CannotBeCountered = CardAttribute.Register("CannotBeCountered");

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            return VerifierResult.Fail;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }

    public class WuXieKeJiTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ICard card = eventArgs.Card;
            if ((card is Card) || (card is CompositeCard))
            {
                SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is WuXieKeJi; }, new WuXieKeJi());
                Card nCard = card as Card;
                CompositeCard cCard = card as CompositeCard;
                List<Card> cards;
                List<Player> players;
                ISkill skill;
                Player responder;
                bool WuXieSuccess = false;
                Trace.Assert(eventArgs.Targets.Count == 1);
                Player promptPlayer = eventArgs.Targets[0];
                ICard promptCard = eventArgs.Card;
                if ((nCard != null && CardCategoryManager.IsCardCategory(nCard.Type.Category, CardCategory.Tool) && nCard[WuXieKeJi.CannotBeCountered] == 0) ||
                    (cCard != null && CardCategoryManager.IsCardCategory(cCard.Type.Category, CardCategory.Tool) && cCard[WuXieKeJi.CannotBeCountered] == 0))
                {
                    while (true)
                    {
                        Prompt prompt = new CardUsagePrompt("WuXieKeJi", promptPlayer, promptCard);
                        if (Game.CurrentGame.GlobalProxy.AskForCardUsage(
                            prompt, v1, out skill, out cards, out players, out responder))
                        {
                            if (!Game.CurrentGame.HandleCardPlay(responder, skill, cards, players))
                            {
                                continue;
                            }
                            promptPlayer = responder;
                            promptCard = new CompositeCard();
                            promptCard.Type = new WuXieKeJi();
                            (promptCard as CompositeCard).Subcards = null;
                            WuXieSuccess = !WuXieSuccess;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (WuXieSuccess)
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }
        }
    }
}
