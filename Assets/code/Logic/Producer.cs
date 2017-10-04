﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represent anyone who can consume (but can't produce by itself)
/// Stores data about last consumption
/// </summary>
public abstract class Consumer : Agent
{
    /// <summary>How much product actually left for now. Stores food, except for Artisans</summary>
    // may move it back to Producer
    public Storage storage;
    private readonly PrimitiveStorageSet consumedTotal = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet consumedLastTurn = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet consumedInMarket = new PrimitiveStorageSet();
    public abstract void consumeNeeds();
    public abstract List<Storage> getRealNeeds();

    protected Consumer(Bank bank, Province province) : base(0f, bank, province)
    {

    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public PrimitiveStorageSet getConsumedTotal()
    {
        return consumedTotal;
    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public PrimitiveStorageSet getConsumedLastTurn()
    {
        return consumedLastTurn;
    }
    /// <summary>
    /// Use for only reads!
    /// </summary>    
    public PrimitiveStorageSet getConsumedInMarket()
    {
        return consumedInMarket;
    }
    public void consumeFromMarket(Storage what)
    {
        //pay(Game.market, what.multiplyOutside(price));
        //if (fromMarket)
        ///{
        consumedTotal.add(what);
        consumedInMarket.add(what);
        Game.market.sentToMarket.subtract(what);        
        //}        

        // from Market
        //if (this is SimpleProduction)
        //    (this as SimpleProduction).getInputProductsReserve().add(what);
    }
    public void consumeFromItself(Storage what)
    {
        consumedTotal.add(what);
        storage.subtract(what);        
    }
    public void consumeFromCountryStorage(List<Storage> what, Country country)
    {
        consumedTotal.add(what);
        country.storageSet.subtract(what);        
    }
    public virtual void setStatisticToZero()
    {
        moneyIncomeLastTurn.set(moneyIncomethisTurn);
        moneyIncomethisTurn.set(0f);
        consumedLastTurn.copyDataFrom(consumedTotal); // temp   
        consumedTotal.setZero();
        consumedInMarket.setZero();
    }

}
/// <summary>
/// Represents anyone who can produce, store and sell product (1 product)
/// also linked to Province
/// </summary>
public abstract class Producer : Consumer
{
    /// <summary>How much was gained (before any payments). Not money!! Generally, gets value in PopUnit.produce and Factore.Produce </summary>
    public Storage gainGoodsThisTurn;

    /// <summary>How much sent to market, Some other amount could be consumedTotal or stored for future </summary>
    public Storage sentToMarket;

    /// <summary> /// Return in pieces  /// </summary>    
    //public abstract float getLocalEffectiveDemand(Product product);
    public abstract void simulate(); ///!!!
    public abstract void produce();
    public abstract void payTaxes();

    protected Producer(Province province) : base(province.getCountry().getBank(), province)
    {
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        gainGoodsThisTurn.setZero();
        sentToMarket.setZero();
    }
    public Value getProducing()
    {
        return gainGoodsThisTurn;
    }
    public void getMoneyForSoldProduct()
    {
        if (sentToMarket.get() > 0f)
        {
            Value DSB = new Value(Game.market.getDemandSupplyBalance(sentToMarket.getProduct()));
            if (DSB.get() > 1f) DSB.set(1f);
            Storage realSold = new Storage(sentToMarket);
            realSold.multiply(DSB);
            Value cost = new Value(Game.market.getCost(realSold));

            // assuming gainGoodsThisTurn & realSold have same product
            if (storage.isSameProduct(gainGoodsThisTurn))
                storage.add(gainGoodsThisTurn);
            else
                storage = new Storage(gainGoodsThisTurn);
            storage.subtract(realSold.get());

            if (Game.market.canPay(cost)) //&& Game.market.tmpMarketStorage.has(realSold)) 
            {
                Game.market.pay(this, cost);
                //Game.market.sentToMarket.subtract(realSold);
            }
            else if (Game.market.howMuchMoneyCanNotPay(cost).get() > 10f)
                Debug.Log("Failed market - producer payment: " + Game.market.howMuchMoneyCanNotPay(cost)); // money in market ended... Only first lucky get money
        }
    }

}


