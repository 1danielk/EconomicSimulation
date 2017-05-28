﻿using System;
using System.Collections.Generic;



public class Product
{
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    internal static List<Product> allProducts = new List<Product>();
    // bool storable = true;
    private string name;
    internal bool resource = false;
    static int resourceCounter = 0;
    Value defaultPrice;

    internal static Product Food, Wood, Lumber, Furniture, Gold, Metal, MetallOre,
     Wool, Clothes, Stone, Cement, Fruit, Wine, ColdArms, Ammunition, Firearms;

    internal bool isResource()
    {
        return resource;
    }
    internal static Product getRandomResource(bool ignoreGold)
    {
        int random = Game.random.Next(resourceCounter);
        int counter = 0;
        foreach (Product pro in Product.allProducts)
        {
            if (pro.isResource())
            {
                if (counter == random)
                    return pro;
                counter++;
            }
        }
        if (ignoreGold)
            return Product.Wood;
        return null;
    }
    public Product(string inName, bool inlanded, float defaultPrice)
    {
        this.defaultPrice = new Value(defaultPrice);
        resource = inlanded;
        if (resource) resourceCounter++;
        name = inName;
        allProducts.Add(this);
        Game.market.SetDefaultPrice(this, defaultPrice);
        if (inName == "Food") Food = this;
        if (inName == "Wood") Wood = this;
        if (inName == "Lumber") Lumber = this;
        if (inName == "Furniture") Furniture = this;
        if (inName == "Gold") Gold = this;
        if (inName == "Metal") Metal = this;
        if (inName == "Metal ore") MetallOre = this;
        if (inName == "Wool") Wool = this;
        if (inName == "Clothes") Clothes = this;
        if (inName == "Stone") Stone = this;
        if (inName == "Cement") Cement = this;
        if (inName == "Fruit") Fruit = this;
        if (inName == "Wine") Wine = this;
        if (inName == "Cold arms") ColdArms = this;
        if (inName == "Ammunition") Ammunition = this;
        if (inName == "Firearms") Firearms = this;

        //TODO checks for duplicates&
    }
    public static Product findByName(string name)
    {  //HashSet set = new HashSet();
        foreach (Product next in allProducts)
        {
            if (next.getName().Equals(name))
                return next;
        }
        return null;

    }
    public string getName()
    {
        return name;
    }
    public bool isInventedByAnyOne()
    {
        foreach (var any in Country.allCountries)
            if (any.isInvented(this))
                return true;
        return false;
    }
    //bool isStorable()
    //{
    //    return storable;
    //}
    //void setStorable(bool isStorable)
    //{
    //    this.storable = isStorable;
    //}
    override public string ToString()
    {
        return getName();
    }

    internal Value getDefaultPrice()
    {
        if (isResource())
        {
            return defaultPrice.multipleOuside(Options.defaultPriceLimitMultiplier);
        }
        else
        {
            var type = FactoryType.whoCanProduce(this);
            if (type == null)
                return defaultPrice.multipleOuside(Options.defaultPriceLimitMultiplier);
            else
            {
                Value res = Game.market.getCost(type.resourceInput);
                res.multiple(Options.defaultPriceLimitMultiplier);
                res.divide(type.basicProduction);
                return res;
            }
        }
    }

}
