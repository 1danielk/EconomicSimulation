﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class ProductionType : IClickable, ISortableName
    {
        static private readonly List<ProductionType> allTypes = new List<ProductionType>();
        internal static ProductionType GoldMine, Furniture, MetalDigging, MetalSmelter, Barnyard, University;

        internal readonly string name;

        ///<summary> per 1000 workers </summary>
        public Storage basicProduction;

        /// <summary>resource input list 
        /// per 1000 workers & per 1 unit outcome</summary>
        internal StorageSet resourceInput;

        private readonly List<Storage> buildingNeeds = new List<Storage> { new Storage(Product.Grain, 40f) };

        /// <summary>Per 1 level upgrade</summary>        
        private readonly List<Storage> upgradeResourceLowTier = new List<Storage> { new Storage(Product.Stone, 2f), new Storage(Product.Wood, 10f) };
        private readonly List<Storage> upgradeResourceMediumTier = new List<Storage> { new Storage(Product.Stone, 10f), new Storage(Product.Lumber, 3f), new Storage(Product.Cement, 2f), new Storage(Product.Metal, 1f) };
        private readonly List<Storage> upgradeResourceHighTier = new List<Storage> { new Storage(Product.Cement, 10f), new Storage(Product.Metal, 4f), new Storage(Product.Machinery, 2f) };



        internal Condition enoughMoneyOrResourcesToBuild;

        ///duplicated in Factory        
        static internal DoubleCondition allowsForeignInvestments = new DoubleCondition((agent, province) =>
        (province as Province).Country == (agent as Agent).Country
        || ((province as Province).Country.economy.getTypedValue().AllowForeignInvestments
        && (agent as Agent).Country.economy.getTypedValue() != Economy.PlannedEconomy),
            (agent) => "Local government allows foreign investments or it isn't foreign investment", true);
        // empty trade
        internal DoubleConditionsList conditionsBuildThis;
        private readonly bool shaft;
        private readonly float nameWeight;
        static ProductionType()
        {
            new ProductionType("Forestry", new Storage(Product.Wood, 2f), false);
            new ProductionType("Gold pit", new Storage(Product.Gold, 2f * Options.goldToCoinsConvert), true);
            new ProductionType("Metal pit", new Storage(Product.MetalOre, 2f), true);
            new ProductionType("Coal pit", new Storage(Product.Coal, 3f), true);
            new ProductionType("Cotton farm", new Storage(Product.Cotton, 2f), false);
            new ProductionType("Quarry", new Storage(Product.Stone, 2f), true);
            new ProductionType("Orchard", new Storage(Product.Fruit, 2f), false);
            new ProductionType("Fishery", new Storage(Product.Fish, 2f), false);
            new ProductionType("Tobacco farm", new Storage(Product.Tobacco, 2f), false);

            new ProductionType("Oil rig", new Storage(Product.Oil, 2f), true);
            new ProductionType("Rubber plantation", new Storage(Product.Rubber, 1f), false);

            StorageSet resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Grain, 1f));
            new ProductionType("Barnyard", new Storage(Product.Cattle, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            new ProductionType("Furniture factory", new Storage(Product.Furniture, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Wood, 1f));
            new ProductionType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Fuel, 0.5f));
            resourceInput.Set(new Storage(Product.MetalOre, 2f));
            new ProductionType("Metal smelter", new Storage(Product.Metal, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Fibers, 1f));
            new ProductionType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Fuel, 0.5f));
            resourceInput.Set(new Storage(Product.Stone, 2f));
            new ProductionType("Cement factory", new Storage(Product.Cement, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Sugar, 1f));
            new ProductionType("Distillery", new Storage(Product.Liquor, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Smithery", new Storage(Product.ColdArms, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Stone, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Ammunition factory", new Storage(Product.Ammunition, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Firearms factory", new Storage(Product.Firearms, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Artillery factory", new Storage(Product.Artillery, 4f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Oil, 1f));
            new ProductionType("Oil refinery", new Storage(Product.MotorFuel, 2f), resourceInput);


            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            new ProductionType("Machinery factory", new Storage(Product.Machinery, 2f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Rubber, 1f));
            new ProductionType("Car factory", new Storage(Product.Cars, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Artillery, 1f));
            new ProductionType("Tank factory", new Storage(Product.Tanks, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Lumber, 1f));
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Machinery, 1f));
            new ProductionType("Airplane factory", new Storage(Product.Airplanes, 6f), resourceInput);

            resourceInput = new StorageSet();
            resourceInput.Set(new Storage(Product.Metal, 1f));
            resourceInput.Set(new Storage(Product.Oil, 1f));
            resourceInput.Set(new Storage(Product.Rubber, 1f));
            new ProductionType("Electronics factory", new Storage(Product.Electronics, 6f), resourceInput);

            University = new ProductionType("University", new Storage(Product.Education, 2f), new StorageSet());
        }
        /// <summary>
        /// Basic constructor for resource getting FactoryType
        /// </summary>    
        internal ProductionType(string name, Storage basicProduction, bool shaft)
        {
            this.name = name;
            nameWeight = name.GetWeight();
            if (name == "Gold pit") GoldMine = this;
            if (name == "Furniture factory") Furniture = this;
            if (name == "Metal pit") MetalDigging = this;
            if (name == "Metal smelter") MetalSmelter = this;
            if (name == "Barnyard") Barnyard = this;
            allTypes.Add(this);
            this.basicProduction = basicProduction;




            enoughMoneyOrResourcesToBuild = new Condition(
                delegate (object forWhom)
                {
                    var agent = forWhom as Agent;
                    if (agent.Country.economy.getValue() == Economy.PlannedEconomy)
                    {
                        return agent.Country.countryStorageSet.has(this.GetBuildNeeds());
                    }
                    else
                    {
                        Value cost = GetBuildCost();
                        return agent.CanPay(cost);
                    }
                },
                delegate
                {
                    var sb = new StringBuilder();
                    Value cost = GetBuildCost();
                    sb.Append("Have ").Append(cost).Append(" coins");
                    sb.Append(" or (with ").Append(Economy.PlannedEconomy).Append(") have ").Append(this.GetBuildNeeds().getString(", "));
                    return sb.ToString();
                }, true);

            // Using: Country () , province, this <FactoryType>
            // used in BuildPanel only, only for Game.Player
            // Should be: de-facto Country, Investor, this <FactoryType> (change Economy.isNot..)
            // or put it in FactoryProject
            conditionsBuildThis = new DoubleConditionsList(new List<Condition> {
                Economy.isNotLF, Economy.isNotInterventionism, enoughMoneyOrResourcesToBuild,
                allowsForeignInvestments}); // 
            this.shaft = shaft;
        }
        /// <summary>
        /// Constructor for resource processing FactoryType
        /// </summary>    
        internal ProductionType(string name, Storage basicProduction, StorageSet resourceInput) : this(name, basicProduction, false)
        {
            this.resourceInput = resourceInput;
        }
        public static IEnumerable<ProductionType> getAllInventedFactories(Country country)
        {
            foreach (var next in allTypes)
                if (country.InventedFactory(next))
                    yield return next;
        }
        public static IEnumerable<ProductionType> getAllInventedArtisanships(Country country)
        {
            foreach (var next in allTypes)
                if (country.InventedArtisanship(next))
                    yield return next;
        }
        //public static IEnumerable<FactoryType> getAllResourceTypes(Country country)
        //{
        //    foreach (var next in getAllInventedTypes(country))
        //        if (next.isResourceGathering())
        //            yield return next;
        //}
        //public static IEnumerable<FactoryType> getAllNonResourceTypes(Country country)
        //{
        //    foreach (var next in getAllInventedTypes(country))
        //        if (!next.isResourceGathering())
        //            yield return next;
        //}
        /// <summary>
        ///Returns new value
        /// </summary>        
        public Money GetBuildCost()
        {
            Money result = Game.market.getCost(GetBuildNeeds());
            result.Add(Options.factoryMoneyReservePerLevel);
            return result;
        }
        /// <summary>
        /// Gives a copy of needs
        /// </summary>        
        internal List<Storage> GetBuildNeeds()
        //internal StorageSet GetBuildNeeds()
        {
            return buildingNeeds.Copy();//.Copy();
            //TODO!has connection in pop.invest!!
            ////if (whoCanProduce(Product.Gold) == this)
            ////        result.Set(new Storage(Product.Wood, 40f));
            //return result;
        }
        /// <summary>
        /// Returns first correct value
        /// Assuming there is only one  FactoryType for each Product
        /// </summary>   
        internal static ProductionType whoCanProduce(Product product)
        {
            foreach (ProductionType ft in allTypes)
                if (ft.basicProduction.isSameProductType(product))
                    return ft;
            return null;
        }

        /// <summary>
        /// Returns copy
        /// </summary>        
        internal List<Storage> GetUpgradeNeeds(int tier)
        {
            switch (tier)
            {
                case 1: return upgradeResourceLowTier.Copy();
                case 2: return upgradeResourceMediumTier.Copy();
                case 3: return upgradeResourceHighTier.Copy();
                default:
                    Debug.Log("Unknown tier");
                    return null;
            }
        }

        override public string ToString() { return name; }
        internal bool isResourceGathering()
        {
            if (hasInput())
                return false;
            else
                return true;
            //resourceInput.Count() == 0
        }
        internal bool IsResourceProcessing()
        {
            return !isResourceGathering() && this != Barnyard && this != University;
        }
        internal bool isShaft()
        {
            return shaft;
        }

        //internal static FactoryType getMostTheoreticalProfitable(Province province)
        //{
        //    KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
        //    foreach (FactoryType factoryType in province.getAllBuildableFactories())
        //    {
        //        float possibleProfit = factoryType.getPossibleProfit().get();
        //        if (possibleProfit > result.Value)
        //            result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
        //    }
        //    return result.Key;
        //}

        //internal static Factory getMostPracticallyProfitable(Province province)
        //{
        //    KeyValuePair<Factory, float> result = new KeyValuePair<Factory, float>(null, 0f);
        //    foreach (Factory factory in province.allFactories)
        //    {
        //        if (province.canUpgradeFactory(factory.Type))
        //        {
        //            float profit = factory.getProfit();
        //            if (profit > result.Value)
        //                result = new KeyValuePair<Factory, float>(factory, profit);
        //        }
        //    }
        //    return result.Key;
        //}

        internal bool hasInput()
        {
            return resourceInput != null;
        }

        /// <summary>
        /// For 1 level / 1000 workers. Not includes tax. Includes modifiers. New value
        /// </summary>        
        internal Value getPossibleProfit(Province province)
        {

            if (Game.market.getDemandSupplyBalance(basicProduction.Product) == Options.MarketZeroDSB)
                return new Value(0); // no demand for result product
            ReadOnlyValue income = Game.market.getCost(basicProduction);
            income.Copy().Multiply(Factory.modifierEfficiency.getModifier(new Factory(province, null, this, null)), false);
            var outCome = new Value(0f);// = province.getLocalMinSalary();//salary
            if (hasInput())
            {
                foreach (Storage inputProduct in resourceInput)
                    if (!Game.market.isAvailable(inputProduct.Product))
                        return new Value(0);// inputs are unavailable                                            

                outCome.Add(Game.market.getCost(resourceInput));
            }
            return income.Copy().Subtract(outCome, false);
        }
        /// <summary>
        /// For artisans. Not including salary
        /// </summary>        
        internal ReadOnlyValue getPossibleProfit()
        {
            if (Game.market.getDemandSupplyBalance(basicProduction.Product) == Options.MarketZeroDSB)
                return new Value(0); // no demand for result product
            ReadOnlyValue income = Game.market.getCost(basicProduction);

            if (hasInput())
            {
                // change to minimal hire limits
                foreach (Storage inputProduct in resourceInput)
                    if (!Game.market.isAvailable(inputProduct.Product))
                        return new Value(0);// inputs are unavailable                                            

                return income.Copy().Subtract(Game.market.getCost(resourceInput), false);
            }
            return income;
        }
        /// <summary>
        /// That is possible margin in that case. Includes tax. New value
        /// </summary>        
        public Procent GetPossibleMargin(Province province)
        {
            var profit = getPossibleProfit(province);
            var taxes = profit.Copy().Multiply(province.Country.taxationForRich.getTypedValue().tax);
            profit.Subtract(taxes);
            return new Procent(profit, GetBuildCost());
        }
        /// <summary>
        /// Doesn't care about builder reforms
        /// </summary>        
        internal bool canBuildNewFactory(Province where, Agent investor)
        {
            if (where.hasFactory(this))
                return false;
            if (isResourceGathering() && basicProduction.Product != where.getResource()
                //|| !where.Country.isInvented(basicProduction.Product)
                || !investor.Country.InventedFactory(this)
                //|| isManufacture() && !investor.Country.Invented(Invention.Manufactures)
                //|| (basicProduction.Product == Product.Cattle && !investor.Country.Invented(Invention.Domestication))
                || !allowsForeignInvestments.checkIftrue(investor, where)
                )
                return false;
            return true;
        }

        public void OnClicked()
        {
            MainCamera.buildPanel.selectFactoryType(this);
            MainCamera.buildPanel.Refresh();
        }
        public bool CanProduce(Product product)
        {
            return basicProduction.Product == product;
        }

        public float GetNameWeight()
        {
            return nameWeight;
        }


        //public Procent GetWorkForceFulFilling()
        //{
        //    return Procent.HundredProcent;
        //}
    }
}