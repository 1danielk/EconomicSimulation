﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tribemen : PopUnit
{
    public Tribemen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.TribeMen, where, culture)
    {
    }
    public Tribemen(int iamount, Culture iculture, Province where) : base(iamount, PopType.TribeMen, iculture, where)
    {
    }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Workers
            //|| targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming) // commented this to get more workers &  more ec. growth           
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy))
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats
            //|| targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            //|| targetType == PopType.Soldiers && !getCountry().isInvented(Invention.ProfessionalArmy))
            )
            return true;
        else
            return false;
    }
    public override void produce()
    {
        Storage producedAmount;
        float overpopulation = province.getOverpopulation();
        if (overpopulation <= 1) // all is OK
            producedAmount = new Storage(Product.Food, getPopulation() * popType.getBasicProduction().get() / 1000f);
        else
            producedAmount = new Storage(Product.Food, getPopulation() * popType.getBasicProduction().get() / 1000f / overpopulation);
        storageNow.add(producedAmount);
        gainGoodsThisTurn.set(producedAmount);
    }
    internal override bool canBuyProducts()
    {
        return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform == Government.Tribal)
    //    {
    //        var baseOpinion = new Procent(1f);
    //        baseOpinion.add(this.loyalty);
    //        //return baseOpinion.getProcent(this.population);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Aristocracy)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Democracy)
    //    {
    //        var baseOpinion = new Procent(0.8f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Despotism)
    //    {
    //        var baseOpinion = new Procent(0.1f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.ProletarianDictatorship)
    //    {
    //        var baseOpinion = new Procent(0.2f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;

    //}

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Tribal || reform == Government.Democracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
public class Farmers : PopUnit
{
    public Farmers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Farmers, where, culture)
    { }
    public Farmers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Farmers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
         || targetType == PopType.TribeMen
         || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats
          || targetType == PopType.Capitalists && getCountry().isInvented(Invention.Manufactories)
            )
            return true;
        else
            return false;
    }
    public override void produce()
    {


        Storage producedAmount = new Storage(Product.Food, getPopulation() * popType.getBasicProduction().get() / 1000f);

        producedAmount.multiple(modEfficiency.getModifier(this));
        gainGoodsThisTurn.set(producedAmount);


        if (Economy.isMarket.checkIftrue(getCountry()))
        {
            sentToMarket.set(gainGoodsThisTurn);
            Game.market.sentToMarket.add(gainGoodsThisTurn);
        }
        else
            storageNow.add(gainGoodsThisTurn);
    }
    override internal bool canSellProducts()
    {
        if (Economy.isMarket.checkIftrue(getCountry()))
            return true;
        else
            return false;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform is Government.ReformValue)
    //    {
    //        if (reform == Government.Tribal)
    //        {
    //            var baseOpinion = new Procent(0f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Aristocracy)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Democracy)
    //        {
    //            var baseOpinion = new Procent(1f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Despotism)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.ProletarianDictatorship)
    //        {
    //            var baseOpinion = new Procent(0.3f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else
    //            return false;
    //    }
    //    else if (reform is TaxationForPoor.ReformValue)
    //    {
    //        TaxationForPoor.ReformValue taxReform = reform as TaxationForPoor.ReformValue;
    //        var baseOpinion = new Procent(1f);
    //        baseOpinion.set(baseOpinion.get() - taxReform.tax.get() * 2);
    //        baseOpinion.set(baseOpinion.get() + loyalty.get() - 0.5f);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;
    //}
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
public class Aristocrats : PopUnit
{
    public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Aristocrats, where, culture)
    { }
    public Aristocrats(int iamount, Culture iculture, Province where) : base(iamount, PopType.Aristocrats, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.TribeMen)
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    internal void dealWithMarket()
    {
        if (storageNow.get() > Options.aristocratsFoodReserv)
        {
            Storage howMuchSend = new Storage(storageNow.getProduct(), storageNow.get() - Options.aristocratsFoodReserv);
            storageNow.send(sentToMarket, howMuchSend);
            //sentToMarket.set(howMuchSend);
            Game.market.sentToMarket.add(howMuchSend);
        }
    }
    public override void produce()
    {
        //Aristocrats don't produce anything
    }
    internal override bool canBuyProducts()
    {
        return true;
    }
    override internal bool canSellProducts()
    {
        return true;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.Aristocracy || reform == Government.Tribal)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopRichStrataVotePower;
            else
                return 1;
        else
            return 0;
    }

}
public class Soldiers : PopUnit
{


    public Soldiers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Soldiers, where, culture)
    { }
    public Soldiers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Soldiers, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (//targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            //||
            targetType == PopType.TribeMen
            || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats // should be officers
         || targetType == PopType.Artisans
         || targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
         )
            return true;
        else
            return false;
    }

    public override bool ShouldPayAristocratTax()
    {
        return false;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Junta)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }

    public override void produce()
    {

    }

    internal void takePayCheck()
    {
        Value payCheck = new Value(getCountry().getSoldierWage());
        payCheck.multiple(getPopulation() / 1000f);
        if (getCountry().canPay(payCheck))
        {
            getCountry().pay(this, payCheck);
            getCountry().soldiersWageExpenseAdd(payCheck);
        }
        else
        {
            this.didntGetPromisedSalary = true;
            getCountry().failedToPaySoldiers = true;
        }
    }
}
public class Capitalists : PopUnit
{
    public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture)
    { }
    public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Artisans
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    public override void produce()
    {
        // Caps don't produce products directly
    }
    internal override bool canBuyProducts()
    {
        return true;
    }
    public override bool ShouldPayAristocratTax()
    {
        return false;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform == Government.Tribal)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Aristocracy)
    //    {
    //        var baseOpinion = new Procent(0f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Democracy)
    //    {
    //        var baseOpinion = new Procent(0.8f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.Despotism)
    //    {
    //        var baseOpinion = new Procent(0.3f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else if (reform == Government.ProletarianDictatorship)
    //    {
    //        var baseOpinion = new Procent(0.1f);
    //        baseOpinion.add(this.loyalty);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;
    //}
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.BourgeoisDictatorship)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopRichStrataVotePower;
            else
                return 1;
        else
            return 0;
    }
}
public class Artisans : PopUnit
{
    private ArtisanProduction artisansProduction;
    public Artisans(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Artisans, where, culture)
    {
        selectProductionType();
    }
    public Artisans(int amount, Culture culture, Province where) : base(amount, PopType.Artisans, culture, where)
    {
        selectProductionType();
    }
    override protected void deleteData()
    {
        base.deleteData();
        artisansProduction = null;
    }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (//|| targetType == PopType.Farmers && !getCountry().isInvented(Invention.Farming)
            targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Capitalists && getCountry().isInvented(Invention.Manufactories))
            return true;
        else
            return false;
    }
    public override void produce()
    {
        if (Game.Random.Next(Options.ArtisansChangeProductionRate) == 1
            && (artisansProduction==null 
            || (artisansProduction !=null && needsFullfilled.isSmallerThan(Options.ArtisansChangeProductionLevel))))
            selectProductionType();
                
        if (artisansProduction != null)
            if (artisansProduction.isAllInputProductsCollected())
            {
                artisansProduction.produce();
                sentToMarket.set(gainGoodsThisTurn);
                storageNow.setZero();
                Game.market.sentToMarket.add(gainGoodsThisTurn);
            }
            else
                selectProductionType();
    }
    public override void buyNeeds()
    {
        base.buyNeeds();
        if (artisansProduction != null)
        {
            payWithoutRecord(artisansProduction, cash);

            // take loan if don't have enough money to buy inputs            
            if (getCountry().isInvented(Invention.Banking) && !artisansProduction.isAllInputProductsCollected())
            {
                var needs = artisansProduction.getRealNeeds();
                if (!artisansProduction.canAfford(needs))
                {
                    var loanSize = Game.market.getCost(needs); // takes little more than really need, could be fixed
                    if (getCountry().bank.canGiveMoney(this, loanSize))
                        getCountry().bank.giveMoney(this, loanSize);
                    payWithoutRecord(artisansProduction, cash);
                }
            }
            //if (artisansProduction.CanAfford(artisansProduction.))

            artisansProduction.buyNeeds();
            artisansProduction.payWithoutRecord(this, artisansProduction.cash);
            this.consumedInMarket.add(artisansProduction.consumedInMarket);
            this.consumedTotal.add(artisansProduction.consumedTotal);
            this.consumedLastTurn.add(artisansProduction.consumedLastTurn);
        }
    }
    internal override bool canBuyProducts()
    {
        return true;
    }
    override internal bool canSellProducts()
    {
        return true;
    }
    public override bool ShouldPayAristocratTax()
    {
        return true;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.BourgeoisDictatorship)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopMiddleStrataVotePower;
            else
                return 1;
        else
            return 0;
    }
    private void selectProductionType()
    {
        KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
        foreach (FactoryType factoryType in FactoryType.getNonResourceTypes(getCountry()))
        {
            float possibleProfit = factoryType.getPossibleProfit(province).get();
            if (possibleProfit > result.Value)
                result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
        }
        if (result.Key != null)
            artisansProduction = new ArtisanProduction(result.Key, province, this);
    }
    public PrimitiveStorageSet getInputProducts()
    {
        if (artisansProduction == null)
            return null;
        else
            return artisansProduction.getInputProductsReserve();
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        if (artisansProduction != null)
            artisansProduction.setStatisticToZero();
    }
}
public class Workers : PopUnit
{
    public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Workers, where, culture)
    { }
    public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.TribeMen
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy))
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
         || targetType == PopType.Artisans
         )
            return true;
        else
            return false;
    }
    public override void produce()
    { }

    public override bool ShouldPayAristocratTax()
    {
        return true;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy)
            && (isStateCulture() || getCountry().minorityPolicy.status == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
//public class PopLinkageValue
//{
//    public PopType type;
//    public Value amount;
//    internal PopLinkageValue(PopType p, Value a)
//    {
//        type = p;
//        amount = a;
//    }
//}