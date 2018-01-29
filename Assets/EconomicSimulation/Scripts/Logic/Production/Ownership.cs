﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    /// <summary>
    /// Represent entity which can be owned by several owners as joint stock company
    /// </summary>
    public interface IShareable { }
    /// <summary>
    /// Represents ability to own enterprise shares
    /// </summary>
    public interface IShareOwner
    {
        //Properties GetOwnership();
    }
    public class Owners : IInvestable
    // IOwnerShip<IShareOwner, Record>,
    {
        private readonly Factory parent;
        private readonly Procent marketPriceModifier = new Procent(Procent.HundredProcent);
        private readonly Dictionary<IShareOwner, Share> ownership = new Dictionary<IShareOwner, Share>();
        private Value totallyInvested = new Value(0f);
        public Owners(IShareable parent)
        {
            this.parent = parent as Factory;
        }

        public void Add(IShareOwner owner, Value value)
        {
            //if (IsCorrectData(value.))
            {
                Share record;
                if (ownership.TryGetValue(owner, out record))
                    record.Increase(value);
                else
                    ownership.Add(owner, new Share(value));
                totallyInvested.add(value);
                //owner.GetOwnership().Add(parent, value);
            }
        }
        private void Remove(IShareOwner fromWho, Value howMuchRemove)
        {
            Share record;
            if (ownership.TryGetValue(fromWho, out record))
            {
                if (record.GetShare().isBiggerThan(howMuchRemove))
                    record.Decrease(howMuchRemove);
                else if (record.GetShare().IsEqual(howMuchRemove))
                    ownership.Remove(fromWho);
                else
                    Debug.Log("Doesn't have that much");
            }
            Debug.Log("No such owner");
        }
        /// <summary>
        /// Test it!!
        /// </summary>        
        public bool Transfer(IShareOwner oldOwner, IShareOwner newOwner, Value amount)
        {
            //if (IsCorrectData(share.get()))
            //{
            Share oldOwnerAsset;
            if (ownership.TryGetValue(oldOwner, out oldOwnerAsset))
            {
                if (oldOwnerAsset.GetShare().isBiggerOrEqual(amount))
                {
                    Share newOwnerAsset;
                    if (ownership.TryGetValue(newOwner, out newOwnerAsset))
                        newOwnerAsset.Increase(amount);
                    else
                        ownership.Add(newOwner, new Share(amount));

                    Remove(oldOwner, amount);
                    return true;
                }
                else
                {
                    TransferAll(oldOwner, newOwner);
                    Debug.Log("Not enough property to transfer");
                    return false;
                }
            }
            else
            {
                Debug.Log("No such owner");
                return false;
            }
            //}
            //else return false;
        }
        internal void TransferAll(IShareOwner oldOwner, IShareOwner newOwner, bool showMessageAboutOperationFails = true)
        {
            Share oldOwnerAsset;
            if (ownership.TryGetValue(oldOwner, out oldOwnerAsset))
            {
                Share newOwnerAsset;
                if (ownership.TryGetValue(newOwner, out newOwnerAsset))
                    newOwnerAsset.Increase(oldOwnerAsset.GetShare());
                else
                    ownership.Add(newOwner, new Share(oldOwnerAsset.GetShare()));

                ownership.Remove(oldOwner);
            }
            else
                if (showMessageAboutOperationFails) Debug.Log("No such owner");
        }
        internal void Nationilize(Country byWhom)
        {
            foreach (var owner in GetAll().ToList())
                if (owner.Key != byWhom)
                {
                    TransferAll(owner.Key, Game.Player);
                    var isPop = owner.Key as PopUnit;
                    if (isPop != null)
                        isPop.loyalty.subtract(Options.PopLoyaltyDropOnNationalization, false);
                    else
                    {
                        //var isCountry = owner.Key as Country;
                        //if (isCountry != null)
                        //todo drop relations
                    }
                }
        }
        public IEnumerable<KeyValuePair<IShareOwner, Share>> GetAll()
        {
            foreach (var item in ownership)
            {
                yield return item;
            }
        }
        public IEnumerable<KeyValuePair<IShareOwner, Procent>> GetAllShares()
        {
            var total = GetAllAssetsValue();
            foreach (var item in ownership)
            {
                yield return new KeyValuePair<IShareOwner, Procent>(item.Key, Procent.makeProcent(item.Value.GetShare(), total));
            }
        }

        internal bool HasOwner(IShareOwner owner)
        {
            return ownership.ContainsKey(owner);
        }
        internal bool Has(IShareOwner owner, Procent share)
        {
            Share found;
            if (ownership.TryGetValue(owner, out found))
            {
                if (found.GetShare().isBiggerOrEqual(GetShareAssetsValue(share)))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        internal bool IsCountryOwnsControlPacket()
        {
            Value ownedByAnyCountry = new Value(0f);
            Value total = new Value(0f);
            foreach (var item in GetAll())
            {
                var value = item.Value.GetShare();
                if (item.Key is Country)
                    ownedByAnyCountry.add(value);
                total.add(value);
            }

            var res = Procent.makeProcent(ownedByAnyCountry, total);
            if (res.isBiggerOrEqual(Procent._50Procent))
                return true;
            else
                return false;
        }

        internal Procent GetTotalOnSale()
        {
            var onSale = new Value(0f);
            foreach (var item in ownership)
                onSale.add(item.Value.GetShareForSale());
            return Procent.makeProcent(onSale, totallyInvested);
        }

        internal bool IsOnlyOwner(IShareOwner owner)
        {
            return ownership.ContainsKey(owner) && ownership.Count == 1;
        }

        internal bool IsOnSale()
        {
            return ownership.Any(x => x.Value.GetShareForSale().isNotZero());
        }
        /// <summary>
        /// Readonly !!
        /// </summary>        
        internal Procent HowMuchSelling(IShareOwner owner)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
                return Procent.makeProcent(record.GetShareForSale(), GetAllAssetsValue());
            else
                return new Procent(Procent.ZeroProcent);
        }
        /// <summary>
        /// Readonly !!
        /// </summary>        
        internal Procent HowMuchOwns(IShareOwner owner)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
                return Procent.makeProcent(record.GetShare(), GetAllAssetsValue());
            else
                return new Procent(Procent.ZeroProcent);
        }

        public void SetToSell(IShareOwner owner, Procent share, bool showMessageAboutOperationFails = true)
        {

            Share record;
            if (ownership.TryGetValue(owner, out record))
            {
                var value = GetShareAssetsValue(share);
                record.SetToSell(value);
            }
            else
               if (showMessageAboutOperationFails)
                Debug.Log("No such owner");
        }
        public void CancelBuyOrder(IShareOwner owner, Procent share)
        {
            Share record;
            if (ownership.TryGetValue(owner, out record))
            {
                var value = GetShareAssetsValue(share);
                record.CancelBuyOrder(value);
            }
            else
                Debug.Log("No such owner");
        }

        //internal KeyValuePair<IShareOwner, Record> GetRandomSaleBiggerThan(Value desireableValue)
        //{
        //    return ownership.Where(x => x.Value.GetAssetForSale().isBiggerOrEqual(desireableValue)).Random();
        //}
        internal Value GetAllAssetsValue()
        {
            return new Value(totallyInvested);
        }
        internal Value GetMarketValue()
        {
            return marketPriceModifier.multiplyOutside(totallyInvested);
        }
        internal Value GetShareMarketValue(Procent share)
        {
            return share.getProcentOf(GetMarketValue());
        }
        internal Value GetShareAssetsValue(Procent share)
        {
            return share.getProcentOf(GetAllAssetsValue());
        }
        internal void CalcMarketPrice()
        {
            var isOnsale = IsOnSale();
            if (isOnsale || parent.IsClosed)
            {
                // reduce price
                marketPriceModifier.subtract(0.01f, false);
                if (marketPriceModifier.isZero())
                    marketPriceModifier.set(0.01f);
            }
            if (!isOnsale && parent.IsOpen) //rise price
                marketPriceModifier.add(0.01f);
        }
        /// <summary>
        /// Buy that share (or less). Assumes that there is something on sale. Assumes that buyer has enough money
        /// </summary>        
        internal void BuyStandardShare(IShareOwner buyer)
        {
            var purchaseValue = GetShareAssetsValue(Options.PopBuyAssetsAtTime);
            var sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().IsEqual(purchaseValue));

            if (sharesToBuy.Count() == 0)
            {
                //if no equal sharesToBuy find smaller one
                sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().isSmallerThan(purchaseValue)
                && x.Value.GetShareForSale().isNotZero());
            }
            if (sharesToBuy.Count() == 0)
            {
                //if no smaller sharesToBuy find bigger one
                sharesToBuy = ownership.Where(x => x.Value.GetShareForSale().isBiggerThan(purchaseValue)
                && x.Value.GetShareForSale().isNotZero());
            }
            if (sharesToBuy.Count() != 0)
            {
                var shareToBuy = sharesToBuy.Random();
                var cost = shareToBuy.Value.GetShareForSale();
                if (cost.isBiggerThan(purchaseValue))
                    cost.set(purchaseValue);
                var agent = buyer as Agent;

                if (agent.pay(shareToBuy.Key as Agent, cost))
                {
                    Transfer(shareToBuy.Key, buyer, cost);
                    //reduce onSale amount on successful deal

                    shareToBuy.Value.ReduceSale(cost);

                    var boughtProcent = Procent.makeProcent(cost, parent.ownership.totallyInvested);
                    Debug.Log(buyer + " bough " + cost + " (" + boughtProcent + ") of " + parent + " from " + shareToBuy.Key);
                }
            }
        }

        /// <summary>
        /// Margin per market value
        /// </summary>        
        public Procent getMargin()
        {
            return Procent.makeProcent(getInvestmentCost(), GetMarketValue(), false);
        }
        /// <summary>
        /// Cost of standard share
        /// </summary>        
        public Value getInvestmentCost()
        {
            return Options.PopBuyAssetsAtTime.getProcentOf(GetMarketValue());
        }

        public bool canProduce(Product product)
        {
            return parent.getType().canProduce(product);
        }

        //public Procent GetWorkForceFulFilling()
        //{
        //    return new Procent(Procent.HundredProcent);
        //    //return parent.GetWorkForceFulFilling();
        //}
    }

}