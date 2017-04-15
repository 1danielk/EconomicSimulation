﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Province
{

    public Color colorID;

    public Mesh mesh;
    MeshFilter meshFilter;
    GameObject gameObject;
    public MeshRenderer meshRenderer;
    //public static uint maxTribeMenCapacity = 2000;
    private string name;
    public int ID;
    public Country owner;
    public List<PopUnit> allPopUnits = new List<PopUnit>();
    public Vector3 centre;

    public static List<Province> allProvinces = new List<Province>();
    private static uint defaultPopulationSpawn = 10;
    public List<Factory> allFactories = new List<Factory>();
    Product resource;
    internal uint fertileSoil;
    public Province(string iname, int iID, Color icolorID, Mesh imesh, MeshFilter imeshFilter, GameObject igameObject, MeshRenderer imeshRenderer, Product inresource)
    {
        resource = inresource;
        colorID = icolorID; mesh = imesh; name = iname; meshFilter = imeshFilter;
        ID = iID;
        gameObject = igameObject;
        meshRenderer = imeshRenderer;
        fertileSoil = 10000;

    }
    public void SecedeTo(Country taker)
    {
        //this.owner - current owner
        if (this.owner != null)
            if (this.owner.ownedProvinces != null)
                this.owner.ownedProvinces.Remove(this);
        this.owner = taker;
        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);
    }
    public System.Collections.IEnumerator GetEnumerator()
    {
        foreach (Factory f in allFactories)
            yield return f;
        foreach (PopUnit f in allPopUnits)
            //if (f.type == PopType.farmers || f.type == PopType.aristocrats)
            yield return f;
    }
    public uint getMenPopulation()
    {
        uint result = 0;
        foreach (PopUnit pop in allPopUnits)
            result += pop.population;
        return result;
    }
    public uint getFamilyPopulation()
    {
        return getMenPopulation() * Game.familySize;
    }
    public uint getMiddleLoyalty()
    {
        //Procent result = 0;
        foreach (PopUnit pop in allPopUnits)
            ;//  result += pop.amount;
        return 0;
    }
    public static bool isProvinceCreated(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return true;
        return false;
    }
    public List<PopUnit> FindAllPopUnits(PopType ipopType)
    {
        List<PopUnit> result = new List<PopUnit>();
        foreach (PopUnit pop in allPopUnits)
            if (pop.type == ipopType)
                result.Add(pop);
        return result;
    }
    public uint FindPopulationAmountByType(PopType ipopType)
    {
        List<PopUnit> list = FindAllPopUnits(ipopType);
        uint result = 0;
        foreach (PopUnit pop in list)
            if (pop.type == ipopType)
                result += pop.population;
        return result;
    }
    //not called with capitalism
    internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
    {
        List<PopUnit> allAristocratsInProvince = FindAllPopUnits(PopType.aristocrats);
        uint aristoctratAmount = 0;
        foreach (PopUnit pop in allAristocratsInProvince)
            aristoctratAmount += pop.population;
        foreach (Aristocrats aristocrat in allAristocratsInProvince)
        {
            Value howMuch = new Value(taxTotalToPay.get() * (float)aristocrat.population / (float)aristoctratAmount);
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
        //PopUnit result = null;
        foreach (PopUnit pop in allPopUnits)
            if (pop.type == target.type && pop.culture == target.culture)
                return pop;
        return null;
    }
    public Value getMiddleNeedFullfilling(PopType type)
    {
        Value result = new Value(0);
        uint allPopulation = 0;
        List<PopUnit> localPops = FindAllPopUnits(type);
        if (localPops.Count > 0)
        {
            foreach (PopUnit pop in localPops)
            // get middle needs fullfiling according to pop weight            
            {
                allPopulation += pop.population;
                result.add(pop.NeedsFullfilled.multiple(pop.population));
            }
            return result.divide(allPopulation); ;
        }
        else/// add defualt population
        {
            //PopUnit.tempPopList.Add(new PopUnit(Province.defaultPopulationSpawn, type, this.owner.culture, this));
            PopUnit.tempPopList.Add(PopUnit.Instantiate(Province.defaultPopulationSpawn, type, this.owner.culture, this));
            //return new Value(float.MaxValue);// meaning always convert in type if does not exist yet
            return new Value(0);

        }
    }

    public void BalanceEmployableWorkForce()
    {
        List<PopUnit> workforcList = this.FindAllPopUnits(PopType.workers);
        uint totalWorkForce = 0; // = this.FindPopulationAmountByType(PopType.workers);
        uint factoryWants = 0;
        uint factoryWantsTotal = 0;

        foreach (PopUnit pop in workforcList)
            totalWorkForce += pop.population;
        uint popsLeft = totalWorkForce;
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
        if (owner.isInvented(resource))
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
        if ((ft.isResourceGathering() && ft.basicProduction.getProduct() != this.resource) || !owner.isInvented(ft.basicProduction.getProduct()))
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
    internal uint getUnemployed()
    {
        //uint result = 0;
        //List<PopUnit> list = this.FindAllPopUnits(PopType.workers);
        //foreach (PopUnit pop in list)
        //    result += pop.getUnemployed();
        uint totalWorkforce = this.FindPopulationAmountByType(PopType.workers);
        if (totalWorkforce == 0) return 0;
        uint employed = 0;

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

        //TextMesh newText = TextMesh();
        // gameObject.rigidbody.centerOfMass
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
    internal float getOverPopulation()
    {
        float usedLand = 0f;
        foreach (PopUnit pop in allPopUnits)
            switch (pop.type.type)
            {
                case PopType.PopTypes.Tribemen:
                    usedLand += pop.population * Game.minLandForTribemen;
                    break;
                case PopType.PopTypes.Farmers:
                    usedLand += pop.population * Game.minLandForFarmers;
                    break;
            }
        return usedLand / fertileSoil;
    }
    /// <summary>Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
    /// \nCould auto-drop salary on minSalary of there is problems with inputs</summary>
    internal float getLocalMinSalary()
    {
        if (allFactories.Count <= 1)
            return owner.getMinSalary();
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
    /// <summary>Returns salary of a factory with maximum salary in province. If no factory in province, then returns Country.minsalary
    ///</summary>
    internal float getLocalMaxSalary()
    {
        if (allFactories.Count <= 1)
            return owner.getMinSalary();
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
        uint workForce = 0;
        uint capacity = 0;
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
