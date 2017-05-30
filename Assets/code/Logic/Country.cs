﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;



public class Country : Consumer
{
    public string name;
    public static List<Country> allCountries = new List<Country>();
    public List<Province> ownedProvinces = new List<Province>();

    public CountryStorageSet storageSet = new CountryStorageSet();
    //public Procent countryTax;
    public Procent aristocrstTax;//= new Procent(0.10f);
    public InventionsList inventions = new InventionsList();

    internal Government government;
    internal Economy economy;
    internal Serfdom serfdom;
    internal MinimalWage minimalWage;
    internal UnemploymentSubsidies unemploymentSubsidies;
    internal TaxationForPoor taxationForPoor;
    internal TaxationForRich taxationForRich;
    internal MinorityPolicy minorityPolicy;
    internal List<AbstractReform> reforms = new List<AbstractReform>();
    public Culture culture;
    Color nationalColor;
    Province capital;

    internal Army homeArmy;
    internal Army sendingArmy;
    internal List<Army> allArmies = new List<Army>();

    TextMesh messhCapitalText;

    //public Bank bank;
    

    /// <summary>
    /// per 1000 men
    /// </summary>
    //private Value minSalary = new Value(0.5f);
    public Value sciencePoints = new Value(0f);
    internal static readonly Country NullCountry = new Country("Uncolonized lands", new Culture("Ancient tribes"), Color.yellow, null);
    static Condition condDontHaveDeposits = new Condition(x => (x as Agent).deposits.get() == 0f, "Don't have deposits", false);
    static Condition condDontHaveLoans = new Condition(x => (x as Agent).loans.get() == 0f, "Don't have loans", false);
    public static ConditionsList condCanTakeLoan = new ConditionsList(new List<Condition> { condDontHaveDeposits });
    public static ConditionsList condCanPutOnDeposit = new ConditionsList(new List<Condition> { condDontHaveLoans });


    Value poorTaxIncome = new Value(0f);
    Value richTaxIncome = new Value(0f);
    Value goldMinesIncome = new Value(0f);
    Value ownedFactoriesIncome = new Value(0f);

    Value unemploymentSubsidiesExpense = new Value(0f);
    Value factorySubsidiesExpense = new Value(0f);
    Value storageBuyingExpense = new Value(0f);

    public Country(string iname, Culture iculture, Color color, Province capital) : base(null)
    {
        //wallet = new CountryWallet(0f, bank);
        bank = new Bank();
        
        homeArmy = new Army(this);
        sendingArmy = new Army(this);
        government = new Government(this);

        economy = new Economy(this);
        serfdom = new Serfdom(this);

        minimalWage = new MinimalWage(this);
        unemploymentSubsidies = new UnemploymentSubsidies(this);
        taxationForPoor = new TaxationForPoor(this);
        taxationForRich = new TaxationForRich(this);
        minorityPolicy = new MinorityPolicy(this);
        name = iname;
        allCountries.Add(this);

        culture = iculture;
        nationalColor = color;
        this.capital = capital;
        //if (!Game.devMode)
        {
            government.status = Government.Aristocracy;

            economy.status = Economy.StateCapitalism;

            inventions.MarkInvented(InventionType.farming);
            inventions.MarkInvented(InventionType.manufactories);
            inventions.MarkInvented(InventionType.banking);
            inventions.MarkInvented(InventionType.metal);
            // inventions.MarkInvented(InventionType.individualRights);
            serfdom.status = Serfdom.Abolished;
        }
    }

    internal void demobilize()
    {
        //ownedProvinces.ForEach(x => x.demobilize());
        allArmies.ForEach(x => x.demobilize());
    }

    internal void demobilize(Province province)
    {
        province.demobilize();
    }

    public bool isExist()
    {
        return ownedProvinces.Count > 0;
    }
    internal static IEnumerable<Country> allExisting = getExisting();
    internal int autoPutInBankLimit = 2000;

    static IEnumerable<Country> getExisting()
    {
        foreach (var c in allCountries)
            if (c.isExist() && c != Country.NullCountry)
                yield return c;

    }
    internal void killCountry(Country byWhom)
    {
        if (messhCapitalText != null) //todo WTF!!
            UnityEngine.Object.Destroy(messhCapitalText.gameObject);
        setSatisticToZero();
        //take all money from bank
        byWhom.bank.add(this.bank);

        //byWhom.storageSet.
        storageSet.sendAll(byWhom.storageSet);

        if (this == Game.player)
            new Message("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay");
    }

    internal bool isOneProvince()
    {
        return ownedProvinces.Count == 1;
    }

    internal Province getCapital()
    {
        return capital;
    }

    internal void sendArmy(Army sendingArmy, Province province)
    {
        sendingArmy.moveTo(province);
        //walkingArmies.Add(new Army(sendingArmy));
        //allArmies.Add(sendingArmy);
        //sendingArmy.clear();
    }

    internal void mobilize()
    {
        foreach (var province in ownedProvinces)
            province.mobilize();
    }

    internal List<Province> getNeighborProvinces()
    {
        List<Province> result = new List<Province>();
        foreach (var province in ownedProvinces)
            result.AddRange(
                province.getNeigbors(p => p.getCountry() != this && !result.Contains(p))
                );
        return result;
    }
    internal Province getRandomNeighborProvince()
    {
        if (isOnlyCountry())
            return null;
        else
            return getNeighborProvinces().PickRandom();
    }

    private bool isOnlyCountry()
    {
        foreach (var any in Country.allExisting)
            if (any != this)
                return false;
        return true;
    }

    internal Province getRandomOwnedProvince()
    {
        return ownedProvinces.PickRandom();
    }
    internal Province getRandomOwnedProvince(Predicate<Province> predicate)
    {
        return ownedProvinces.PickRandom(predicate);
    }

    //todo move to Province.cs
    internal void makeCapitalTextMesh()
    {
        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(capital.gameObject.transform, false);


        Vector3 capitalTextPosition = capital.centre;
        capitalTextPosition.y += 2f;
        txtMeshTransform.position = capitalTextPosition;

        messhCapitalText = txtMeshTransform.GetComponent<TextMesh>();
        messhCapitalText.text = this.ToString();
        if (this == Game.player)

        {
            messhCapitalText.color = Color.blue;
            messhCapitalText.fontSize += messhCapitalText.fontSize / 2;

        }
        else
        {
            messhCapitalText.color = Color.cyan; // Set the text's color to red
            messhCapitalText.fontSize += messhCapitalText.fontSize / 3;
        }
    }
    internal void moveCapitalTo(Province newCapital)
    {
        if (messhCapitalText == null)
            makeCapitalTextMesh();
        else
        {
            Vector3 capitalTextPosition = newCapital.centre;
            capitalTextPosition.y += 2f;
            messhCapitalText.transform.position = capitalTextPosition;
        }
        capital = newCapital;
    }
    internal Color getColor()
    {
        return nationalColor;
    }
    internal Procent getYesVotes(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
    {
        // calculate how much of population wants selected reform
        int totalPopulation = this.getMenPopulation();
        int votingPopulation = 0;
        int populationSayedYes = 0;
        int votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0);
        //Procent procentPopulationSayedYes = new Procent(0f);
        foreach (Province pro in ownedProvinces)
            foreach (PopUnit pop in pro.allPopUnits)
            {
                if (pop.canVote())
                {
                    if (pop.getSayingYes(reform))
                    {
                        votersSayedYes += pop.getPopulation();// * pop.getVotingPower();
                        populationSayedYes += pop.getPopulation();// * pop.getVotingPower();
                    }
                    votingPopulation += pop.getPopulation();// * pop.getVotingPower();
                }
                else
                {
                    if (pop.getSayingYes(reform))
                        populationSayedYes += pop.getPopulation();// * pop.getVotingPower();
                }
            }
        if (totalPopulation != 0)
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
        return procentVotersSayedYes;
    }
    /// <summary>
    /// Not finished, dont use it
    /// </summary>
    /// <param name="reform"></param>   
    internal Procent getYesVotes2(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
    {
        int totalPopulation = this.getMenPopulation();
        int votingPopulation = 0;
        int populationSayedYes = 0;
        int votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0f);
        Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
        Dictionary<PopType, int> divisionVotersResult = this.getYesVotesByType(reform, ref divisionPopulationResult);
        foreach (KeyValuePair<PopType, int> next in divisionVotersResult)
            votersSayedYes += next.Value;

        if (totalPopulation != 0)
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
        return procentVotersSayedYes;
    }
    internal Dictionary<PopType, int> getYesVotesByType(AbstractReformValue reform, ref Dictionary<PopType, int> divisionPopulationResult)
    {  // division by pop types
        Dictionary<PopType, int> divisionVotersResult = new Dictionary<PopType, int>();
        foreach (PopType type in PopType.allPopTypes)
        {
            divisionVotersResult.Add(type, 0);
            divisionPopulationResult.Add(type, 0);
            foreach (Province pro in this.ownedProvinces)
            {
                var popList = pro.getAllPopUnits(type);
                foreach (PopUnit pop in popList)
                    if (pop.getSayingYes(reform))
                    {
                        divisionPopulationResult[type] += pop.getPopulation();// * pop.getVotingPower();
                        if (pop.canVote())
                            divisionVotersResult[type] += pop.getPopulation();// * pop.getVotingPower();
                    }
            }
        }
        return divisionVotersResult;
    }
    public bool isInvented(InventionType type)
    {

        return inventions.isInvented(type);
    }
    public bool isInvented(Product product)
    {
        if (
            ((product == Product.Metal || product == Product.MetallOre || product == Product.ColdArms) && !isInvented(InventionType.metal))
            || ((product == Product.Artillery || product == Product.Ammunition) && !isInvented(InventionType.Gunpowder))
            || (product == Product.Firearms && !isInvented(InventionType.Firearms))
            || (!product.isResource() && !isInvented(InventionType.manufactories))
            )
            return false;
        else
            return true;
    }
    internal float getMinSalary()
    {
        return (minimalWage.getValue() as MinimalWage.ReformValue).getWage();
        //return minSalary.get();
    }
    override public string ToString()
    {
        if (this == Game.player)
            return name + " country (you are)";
        else
            return name + " country";
    }
    internal void think()
    {
        if (Game.devMode)
            sciencePoints.add(this.getMenPopulation());
        else
            sciencePoints.add(this.getMenPopulation() * Options.defaultSciencePointMultiplier);
        sciencePoints.add(this.getMenPopulation());

        if (this.autoPutInBankLimit > 0f)
        {
            float extraMoney = haveMoney.get() - (float)this.autoPutInBankLimit;
            if (extraMoney > 0f)
                bank.takeMoney(this, new Value(extraMoney));
        }
        allArmies.ForEach(x => x.consume());
        buyNeeds(); // Should go After all Armies consumption

        if (isAI() && !isOnlyCountry())
            if (Game.random.Next(10) == 1)
            {
                var possibleTarget = getRandomNeighborProvince();
                if (possibleTarget != null)
                    if ((this.getStreght() * 1.5f > possibleTarget.getCountry().getStreght() && possibleTarget.getCountry() == Game.player) || possibleTarget.getCountry() == NullCountry
                        || possibleTarget.getCountry() != Game.player && this.getStreght() < possibleTarget.getCountry().getStreght() * 0.5f)
                    {
                        mobilize();
                        sendArmy(homeArmy, possibleTarget);
                    }
                //mobilize();
                //if (homeArmy.getSize() > 50 + Game.random.Next(100))
                //    sendArmy(homeArmy, getRandomNeighborProvince());
            }
    }

    public override void buyNeeds()
    {
        var needs = getNeeds();
        //if (wallet.canPay(Game.market.getCost(needs)))
        //buy 1 day needs
        foreach (var pro in Product.allProducts)
        {
            // if I want to buy           
            Storage toBuy = new Storage(pro, needs.getStorage(pro).get() - storageSet.getStorage(pro).get());
            buyNeeds(toBuy);
        }
        //buy x day needs
        foreach (var pro in Product.allProducts)
        {
            Storage toBuy = new Storage(pro, needs.getStorage(pro).get() * Options.CountryForHowMuchDaysMakeReservs - storageSet.getStorage(pro).get());
            buyNeeds(toBuy);
        }
    }
    void buyNeeds(Storage toBuy)
    {
        // if I want to buy           
        //Storage toBuy = new Storage(pro, needs.getStorage(pro).get()* days - storageSet.getStorage(pro).get());
        if (toBuy.get() > 0f)
        {
            //if (toBuy.get() < 10f) toBuy.set(10);
            toBuy.multiple(Game.market.buy(this, toBuy, null));
            storageSet.add(toBuy);
            storageBuyingExpenseAdd(new Value(Game.market.getCost(toBuy)));
        }
    }
    public PrimitiveStorageSet getNeeds()
    {
        PrimitiveStorageSet res = new PrimitiveStorageSet();
        foreach (var item in allArmies)
            res.add(item.getNeeds());
        return res;
    }
    private float getStreght()
    {
        return howMuchCanMobilize();
    }

    private float howMuchCanMobilize()
    {
        float result = 0f;
        foreach (var pr in ownedProvinces)
            foreach (var po in pr.allPopUnits)
                if (po.type.canMobilize())
                    result += po.howMuchCanMobilize();
        return result;
    }

    private bool isAI()
    {
        return Game.player != this;
    }
    public Value getGDP()
    {
        Value result = new Value(0);
        foreach (var prov in ownedProvinces)
        {
            foreach (var prod in prov.allFactories)
                if (prod.gainGoodsThisTurn.get() > 0f)
                    result.add(Game.market.getCost(prod.gainGoodsThisTurn) - Game.market.getCost(prod.consumedTotal).get());

            foreach (var pop in prov.allPopUnits)
                if (pop.type.isProducer())
                    if (pop.gainGoodsThisTurn.get() > 0f)
                        result.add(Game.market.getCost(pop.gainGoodsThisTurn));
        }
        return result;
    }
    public Procent getUnemployment()
    {
        Procent result = new Procent(0f);
        int calculatedBase = 0;
        foreach (var item in ownedProvinces)
        {
            int population = item.getMenPopulation();
            result.addPoportionally(calculatedBase, population, item.getUnemployment());
            calculatedBase += population;
        }
        return result;
    }
    internal int getMenPopulation()
    {
        int result = 0;
        foreach (Province pr in ownedProvinces)
            result += pr.getMenPopulation();
        return result;
    }
    internal int getFamilyPopulation()
    {
        return getMenPopulation() * Options.familySize;
    }
    public int FindPopulationAmountByType(PopType ipopType)
    {
        int result = 0;
        foreach (Province pro in ownedProvinces)
            result += pro.getPopulationAmountByType(ipopType);
        return result;
    }

    internal Value getGDPPer1000()
    {
        Value res = getGDP();
        res.multiple(1000);
        res.divide(getMenPopulation());

        return res;
    }
    //****************************
    internal Value getAllExpenses()
    {
        Value result = new Value(0f);
        result.add(unemploymentSubsidiesExpense);
        result.add(factorySubsidiesExpense);
        result.add(storageBuyingExpense);
        return result;
    }
    internal float getBalance()
    {
        return moneyIncomethisTurn.get() - getAllExpenses().get();
    }

    internal void setSatisticToZero()
    {
        poorTaxIncome.set(0f);
        richTaxIncome.set(0f);
        goldMinesIncome.set(0f);
        unemploymentSubsidiesExpense.set(0f);
        ownedFactoriesIncome.set(0f);
        factorySubsidiesExpense.set(0f);
        moneyIncomethisTurn.set(0f);
        storageBuyingExpense.set(0f);
    }

    internal void takeFactorySubsidies(Consumer byWhom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            payWithoutRecord(byWhom, howMuch);
            factorySubsidiesExpense.add(howMuch);
        }
        else
        {
            //sendAll(byWhom.wallet);
            payWithoutRecord(byWhom, byWhom.haveMoney);
            factorySubsidiesExpense.add(byWhom.haveMoney);
        }

    }
    internal float getfactorySubsidiesExpense()
    {
        return factorySubsidiesExpense.get();
    }
    internal float getPoorTaxIncome()
    {
        return poorTaxIncome.get();
    }

    internal float getRichTaxIncome()
    {
        return richTaxIncome.get();
    }

    internal float getGoldMinesIncome()
    {
        return goldMinesIncome.get();
    }


    internal float getOwnedFactoriesIncome()
    {
        return ownedFactoriesIncome.get();
    }

    internal float getUnemploymentSubsidiesExpense()
    {
        return unemploymentSubsidiesExpense.get();
    }
    internal float getStorageBuyingExpense()
    {
        return storageBuyingExpense.get();
    }

    internal void poorTaxIncomeAdd(Value toAdd)
    {
        poorTaxIncome.add(toAdd);
    }
    internal void richTaxIncomeAdd(Value toAdd)
    {
        richTaxIncome.add(toAdd);
    }
    internal void goldMinesIncomeAdd(Value toAdd)
    {
        goldMinesIncome.add(toAdd);
    }
    internal void unemploymentSubsidiesExpenseAdd(Value toAdd)
    {
        unemploymentSubsidiesExpense.add(toAdd);
    }
    internal void storageBuyingExpenseAdd(Value toAdd)
    {
        storageBuyingExpense.add(toAdd);
    }
    internal void ownedFactoriesIncomeAdd(Value toAdd)
    {
        ownedFactoriesIncome.add(toAdd);
    }

}
public class DontUseThatMethod : Exception
{
    /// <summary>
    /// Just create the exception
    /// </summary>
    public DontUseThatMethod()
      : base()
    {
    }

    /// <summary>
    /// Create the exception with description
    /// </summary>
    /// <param name="message">Exception description</param>
    public DontUseThatMethod(String message)
      : base(message)
    {
    }

    /// <summary>
    /// Create the exception with description and inner cause
    /// </summary>
    /// <param name="message">Exception description</param>
    /// <param name="innerException">Exception inner cause</param>
    public DontUseThatMethod(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Create the exception from serialized data.
    /// Usual scenario is when exception is occured somewhere on the remote workstation
    /// and we have to re-create/re-throw the exception on the local machine
    /// </summary>
    /// <param name="info">Serialization info</param>
    /// <param name="context">Serialization context</param>
    protected DontUseThatMethod(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}