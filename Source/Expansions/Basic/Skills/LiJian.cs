﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 离间―出牌阶段，你可以弃置一张牌并选择两名男性角色，然后视为其中一名男性角色对另一名男性角色使用一张【决斗】。此【决斗】不能被【无懈可击】响应。每阶段限一次。 
    /// </summary>
    public class LiJian : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[LiJianUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if ((cards == null || cards.Count == 0) && (arg.Targets == null || arg.Targets.Count == 0))
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            foreach (Card card in cards)
            {
                if (card.Owner != Owner)
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, card))
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets != null && arg.Targets.Count > 2)
            {
                return VerifierResult.Fail;
            }
            foreach (Player p in arg.Targets)
            {
                if (!p.IsMale)
                {
                    return VerifierResult.Fail;
                }
                if (p == Owner)
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets != null && arg.Targets.Count == 2)
            {
                CompositeCard c = new CompositeCard();
                c.Type = new JueDou();
                c.Subcards = null;
                c[WuXieKeJi.CannotBeCountered] = 1;
                List<Player> dests = new List<Player>();
                dests.Add(arg.Targets[0]);
                if (!Game.CurrentGame.PlayerCanBeTargeted(arg.Targets[1], dests, c))
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanUseCard(arg.Targets[1], c))
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets == null || arg.Targets.Count < 2)
            {
                return VerifierResult.Partial;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[LiJianUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count == 1 && arg.Targets.Count == 2);
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            GameEventArgs args = new GameEventArgs();
            args.Source = arg.Targets[1];
            args.Targets = new List<Player>();
            args.Targets.Add(arg.Targets[0]);
            args.Skill = new LiJianCardTransform();
            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            return true;
        }

        private class LiJianCardTransform : CardTransformSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
            {
                Trace.Assert(cards == null || cards.Count == 0);
                card = new CompositeCard();
                card.Type = new JueDou();
                card[WuXieKeJi.CannotBeCountered] = 1;
                return VerifierResult.Success;
            }

            public override CardHandler PossibleResult
            {
                get { throw new NotImplementedException(); }
            }
        }

        public override Core.Players.Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                base.Owner = value;
                Owner.AutoResetAttributes.Add(LiJianUsed);
            }
        }

        public static readonly string LiJianUsed = "LiJianUsed";

        public override void CardRevealPolicy(Core.Players.Player p, List<Card> cards, List<Core.Players.Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }
    }
}
