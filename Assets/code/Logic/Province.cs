﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

public class Province
{

    Color colorID;
    Color color;
    public Mesh mesh;
    MeshFilter meshFilter;
    internal GameObject gameObject;
    public MeshRenderer meshRenderer;
    //public static int maxTribeMenCapacity = 2000;
    private string name;
    private int ID;
    Country owner;
    public List<PopUnit> allPopUnits = new List<PopUnit>();
    public Vector3 centre;

    public static List<Province> allProvinces = new List<Province>();
    private static int defaultPopulationSpawn = 10;
    public List<Factory> allFactories = new List<Factory>();
    private Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
    private List<Province> neighbors = new List<Province>();
    Product resource;
    internal int fertileSoil;
    public Province(string iname, int iID, Color icolorID, Mesh imesh, MeshFilter imeshFilter, GameObject igameObject, MeshRenderer imeshRenderer, Product inresource)
    {
        allProducers = getProducers();
        resource = inresource;
        colorID = icolorID; mesh = imesh; name = iname; meshFilter = imeshFilter;
        ID = iID;
        gameObject = igameObject;
        meshRenderer = imeshRenderer;
        fertileSoil = 10000;

    }
    internal Country getOwner()
    {
        //if (owner == null)
        //    return Country.NullCountry;
        //else
        return owner;
    }
    internal int getID()
    { return ID; }
    public void InitialOwner(Country taker)
    {
        if (this.getOwner() != null)
            if (this.getOwner().ownedProvinces != null)
                this.getOwner().ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);
        color = taker.getColor().getAlmostSameColor();
        meshRenderer.material.color = color;
    }
    public void secedeTo(Country taker)
    {
        //refuse loans to old country bank
        foreach (var producer in allProducers)
            if (producer.loans.get() != 0f)
                getOwner().bank.defaultLoaner(producer);

        if (getOwner().isOneProvince())
            getOwner().killCountry(taker);
        else
            if (isCapital())
            getOwner().moveCapitalTo(getOwner().getRandomOwnedProvince(x => x != this));

        this.demobilize();




        if (this.getOwner() != null)
            if (this.getOwner().ownedProvinces != null)
                this.getOwner().ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);

        color = taker.getColor().getAlmostSameColor();
        meshRenderer.material.color = color;

    }

    internal bool isCapital()
    {
        return getOwner().getCapital() == this;
    }

    internal void demobilize()
    {
        allPopUnits.ForEach(x => getOwner().allArmies.demobilize(x));
    }



    internal static Province getRandomProvinceInWorld(Predicate<Province> predicate)
    {
        return allProvinces.PickRandom(predicate);
    }
    internal List<Province> getNeigbors(Predicate<Province> predicate)
    {
        return neighbors.FindAll(predicate);

    }
    internal IEnumerable<Producer> allProducers;
    IEnumerable<Producer> getProducers()
    //public System.Collections.IEnumerator GetEnumerator()
    {
        foreach (Factory f in allFactories)
            yield return f;
        foreach (PopUnit f in allPopUnits)
            //if (f.type == PopType.farmers || f.type == PopType.aristocrats)
            yield return f;
    }
    public int getMenPopulation()
    {
        int result = 0;
        foreach (PopUnit pop in allPopUnits)
            result += pop.getPopulation();
        return result;
    }
    public int getFamilyPopulation()
    {
        return getMenPopulation() * Game.familySize;
    }
    public int getMiddleLoyalty()
    {
        //Procent result = 0;
        foreach (PopUnit pop in allPopUnits)
            ;//  result += pop.amount;
        return 0;
    }

    internal void mobilize()
    {
        var army = this.getOwner().homeArmy;
        foreach (var pop in allPopUnits)
            if (pop.type.canMobilize())
                army.add(pop.mobilize());
    }

    public static bool isProvinceCreated(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return true;
        return false;
    }
    public static Province findProvince(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return anyProvince;
        return null;
    }
    public List<PopUnit> FindAllPopUnits(PopType ipopType)
    {
        List<PopUnit> result = new List<PopUnit>();
        foreach (PopUnit pop in allPopUnits)
            if (pop.type == ipopType)
                result.Add(pop);
        return result;
    }

    internal static Province findByID(int number)
    {
        foreach (var pro in allProvinces)
            if (pro.ID == number)
                return pro;
        return null;
    }

    public int FindPopulationAmountByType(PopType ipopType)
    {
        List<PopUnit> list = FindAllPopUnits(ipopType);
        int result = 0;
        foreach (PopUnit pop in list)
            if (pop.type == ipopType)
                result += pop.getPopulation();
        return result;
    }
    //not called with capitalism
    internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
    {
        List<PopUnit> allAristocratsInProvince = FindAllPopUnits(PopType.aristocrats);
        int aristoctratAmount = 0;
        foreach (PopUnit pop in allAristocratsInProvince)
            aristoctratAmount += pop.getPopulation();
        foreach (Aristocrats aristocrat in allAristocratsInProvince)
        {
            Value howMuch = new Value(taxTotalToPay.get() * (float)aristocrat.getPopulation() / (float)aristoctratAmount);
            fromWho.pay(aristocrat.storageNow, howMuch);
            aristocrat.gainGoodsThisTurn.add(howMuch);
            aristocrat.dealWithMarket();
            //aristocrat.sentToMarket.set(aristocrat.gainGoodsThisTurn);
            //Game.market.tmpMarketStorage.add(aristocrat.gainGoodsThisTurn);
        }
    }
    ///<summary> Simular by popType & culture</summary>    
    public PopUnit FindSimularPopUnit(PopUnit target)
    {
        foreach (PopUnit pop in allPopUnits)
            if (pop.type == target.type && pop.culture == target.culture)
                return pop;
        return null;
    }

    internal Color getColorID()
    {
        return colorID;
    }
    internal Color getColor()
    {
        return color;
    }
    public Value getMiddleNeedFullfilling(PopType type)
    {
        Value result = new Value(0);
        int allPopulation = 0;
        List<PopUnit> localPops = FindAllPopUnits(type);
        if (localPops.Count > 0)
        {
            foreach (PopUnit pop in localPops)
            // get middle needs fullfiling according to pop weight            
            {
                allPopulation += pop.getPopulation();
                result.add(pop.needsFullfilled.multiple(pop.getPopulation()));
            }
            return result.divide(allPopulation); ;
        }
        else/// add defualt population
        {
            //PopUnit.tempPopList.Add(new PopUnit(Province.defaultPopulationSpawn, type, this.getOwner().culture, this));
            PopUnit.PopListToAddToGeneralList.Add(PopUnit.Instantiate(Province.defaultPopulationSpawn, type, this.getOwner().culture, this));
            //return new Value(float.MaxValue);// meaning always convert in type if does not exist yet
            return new Value(0);
        }
    }
    public void BalanceEmployableWorkForce()
    {
        List<PopUnit> workforcList = this.FindAllPopUnits(PopType.workers);
        int totalWorkForce = 0; // = this.FindPopulationAmountByType(PopType.workers);
        int factoryWants = 0;
        int factoryWantsTotal = 0;

        foreach (PopUnit pop in workforcList)
            totalWorkForce += pop.getPopulation();
        int popsLeft = totalWorkForce;
        if (totalWorkForce > 0)
        {
            // workforcList = workforcList.OrderByDescending(o => o.population).ToList();
            allFactories = allFactories.OrderByDescending(o => o.getSalary()).ToList();
            //foreach (Factory shownFactory in allFactories)
            //    factoryWantsTotal += shownFactory.HowMuchWorkForceWants();
            //if (factoryWantsTotal > 0)
            foreach (Factory factory in allFactories)
            {
                //if (shownFactory.getLevel() > 0)
                if (factory.isWorking())
                {
                    factoryWants = factory.HowMuchWorkForceWants();
                    if (factoryWants > popsLeft)
                        factoryWants = popsLeft;

                    //if (factoryWants > 0)
                    //shownFactory.HireWorkforce(totalWorkForce * factoryWants / factoryWantsTotal, workforcList);
                    if (factoryWants > 0 && factory.getWorkForce() == 0)
                        factory.justHiredPeople = true;
                    else
                        factory.justHiredPeople = false;
                    factory.HireWorkforce(factoryWants, workforcList);

                    popsLeft -= factoryWants;
                    //if (popsLeft <= 0) break;
                }
                else
                {
                    factory.HireWorkforce(0, null);
                }
            }
        }
    }
    internal void setResource(Product inres)
    {
        resource = inres;
    }
    internal Product getResource()
    {
        //if (getOwner().isInvented(resource))
        if (resource.isInventedByAnyOne())
            return resource;
        else
            return null;
    }
    internal Factory getResourceFactory()
    {
        foreach (Factory f in allFactories)
            if (f.type.basicProduction.getProduct() == resource)
                return f;
        return null;
    }

    internal List<FactoryType> WhatFactoriesCouldBeBuild()
    {
        List<FactoryType> result = new List<FactoryType>();
        foreach (FactoryType ft in FactoryType.allTypes)
            if (CanBuildNewFactory(ft))
                result.Add(ft);
        return result;
    }


    internal bool CanBuildNewFactory(FactoryType ft)
    {
        if (HaveFactory(ft))
            return false;
        if ((ft.isResourceGathering() && ft.basicProduction.getProduct() != this.resource) || !getOwner().isInvented(ft.basicProduction.getProduct()))
            return false;

        return true;
    }
    internal bool CanUpgradeFactory(FactoryType ft)
    {
        if (!HaveFactory(ft))
            return false;
        // if (ft.isResourceGathering() && ft.basicProduction.getProduct() != this.resource)
        //     return false;

        return true;
    }
    internal bool HaveFactory(FactoryType ft)
    {
        foreach (Factory f in allFactories)
            if (f.type == ft)
                return true;
        return false;
    }
    override public string ToString()
    {
        return name;
    }
    internal int getUnemployed()
    {
        //int result = 0;
        //List<PopUnit> list = this.FindAllPopUnits(PopType.workers);
        //foreach (PopUnit pop in list)
        //    result += pop.getUnemployed();
        int totalWorkforce = this.FindPopulationAmountByType(PopType.workers);
        if (totalWorkforce == 0) return 0;
        int employed = 0;

        foreach (Factory factory in allFactories)
            employed += factory.getWorkForce();
        return totalWorkforce - employed;
    }
    internal bool isThereMoreThanFactoriesInUpgrade(int limit)
    {
        int counter = 0;
        foreach (Factory factory in allFactories)
            if (factory.isUpgrading())
            {
                counter++;
                if (counter == limit)
                    return true;
            }
        return false;
    }

    internal void SetLabel()
    {

        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(this.gameObject.transform, false);

        //newProvince.centre = (meshRenderer.bounds.max + meshRenderer.bounds.center) / 2f;
        txtMeshTransform.position = this.centre;

        TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();
        txtMesh.text = this.ToString();
        txtMesh.color = Color.red; // Set the text's color to red

    }

    internal Factory findFactory(FactoryType proposition)
    {
        foreach (Factory f in allFactories)
            if (f.type == proposition)
                return f;
        return null;
    }
    internal bool isProducingOnFactories(PrimitiveStorageSet resourceInput)
    {
        foreach (Storage stor in resourceInput)
            foreach (Factory factory in allFactories)
                if (factory.isWorking() && factory.type.basicProduction.getProduct() == stor.getProduct())
                    return true;
        return false;
    }
    internal float getOverpopulation()
    {
        float usedLand = 0f;
        foreach (PopUnit pop in allPopUnits)
            switch (pop.type.type)
            {
                case PopType.PopTypes.Tribemen:
                    usedLand += pop.getPopulation() * Game.minLandForTribemen;
                    break;
                case PopType.PopTypes.Farmers:
                    usedLand += pop.getPopulation() * Game.minLandForFarmers;
                    break;
            }
        return usedLand / fertileSoil;
    }
    /// <summary>Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
    /// \nCould auto-drop salary on minSalary of there is problems with inputs</summary>
    internal float getLocalMinSalary()
    {
        if (allFactories.Count <= 1)
            return getOwner().getMinSalary();
        else
        {
            float minSalary;
            minSalary = getLocalMaxSalary();

            foreach (Factory fact in allFactories)
                if (fact.isWorking() && !fact.justHiredPeople)
                {
                    if (minSalary > fact.getSalary())
                        minSalary = fact.getSalary();
                }
            return minSalary;
        }
    }

    internal void addNeigbor(Province found)
    {
        if (found != this && !distances.ContainsKey(found))
            distances.Add(found, 1);
        if (!neighbors.Contains(found))
            neighbors.Add(found);

    }
    /// <summary>
    /// for debug reasons
    /// </summary>
    /// <returns></returns>
    internal string getNeigborsList()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var t in distances)
            sb.Append("\n").Append(t.Key.ToString());
        return sb.ToString();
    }
    /// <summary>Returns salary of a factory with maximum salary in province. If no factory in province, then returns Country.minsalary
    ///</summary>
    internal float getLocalMaxSalary()
    {
        if (allFactories.Count <= 1)
            return getOwner().getMinSalary();
        else
        {
            float maxSalary;
            maxSalary = allFactories.First().getSalary();

            foreach (Factory fact in allFactories)
                if (fact.isWorking())
                {
                    if (fact.getSalary() > maxSalary)
                        maxSalary = fact.getSalary();
                }
            return maxSalary;
        }
    }
    internal float getMiddleFactoryWorkforceFullfilling()
    {
        int workForce = 0;
        int capacity = 0;
        foreach (Factory fact in allFactories)
            if (fact.isWorking())
            {
                workForce += fact.getWorkForce();
                capacity += fact.getMaxWorkforceCapacity();
            }
        if (capacity == 0) return 0f;
        else
            return workForce / (float)capacity;
    }
}
