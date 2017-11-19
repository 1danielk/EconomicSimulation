﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

public class Product : Name
{
    private enum type
    {
        military, industrial, consumerProduct
    }
    //private static HashSet<Product> allProducts = new HashSet<Product>();
    private static readonly List<Product> allProducts = new List<Product>();
    private static int resourceCounter;

    private readonly Value defaultPrice;
    private readonly bool _isResource;
    private readonly bool _isAbstract;
    private readonly bool _isMilitary;
    private readonly bool _isIndustrial;
    private readonly bool _isConsumerProduct;
    private readonly List<Product> substitutes;
    private readonly Color color;




    internal static readonly Product
        //Fish, Grain, Cattle, Wood, Lumber, Furniture, Gold, Metal, MetalOre,
        //Cotton, Clothes, Stone, Cement, Fruit, Liquor, ColdArms, Ammunition, Firearms, Artillery,
        //Oil, MotorFuel, Cars, Tanks, Airplanes, Rubber, Machinery,
        Gold = new Product("Gold", 4f, Color.yellow, type.industrial),
        Fish = new Product("Fish", 0.04f, Color.cyan, type.consumerProduct),
        Grain = new Product("Grain", 0.04f, new Color(0.57f, 0.75f, 0.2f), type.industrial),//greenish
        Cattle = new Product("Cattle", 0.04f, type.military),

        Fruit = new Product("Fruit", 1f, new Color(1f, 0.33f, 0.33f), type.consumerProduct),//pinkish
        Liquor = new Product("Liquor", 3f, type.consumerProduct),

        Wood = new Product("Wood", 2.7f, new Color(0.5f, 0.25f, 0f), type.industrial), // brown
        Lumber = new Product("Lumber", 8f, type.industrial),
        Furniture = new Product("Furniture", 7f, type.consumerProduct),

        Cotton = new Product("Cotton", 1f, Color.white, type.consumerProduct),
        Clothes = new Product("Clothes", 6f, type.consumerProduct),

        Stone = new Product("Stone", 1f, new Color(0.82f, 0.62f, 0.82f), type.industrial),//light grey
        Cement = new Product("Cement", 2f, type.industrial),

        MetalOre = new Product("Metal ore", 3f, Color.blue, type.industrial),
        Metal = new Product("Metal", 6f, type.industrial),

        ColdArms = new Product("Cold arms", 13f, type.military),
        Ammunition = new Product("Ammunition", 13f, type.military),
        Firearms = new Product("Firearms", 13f, type.military),
        Artillery = new Product("Artillery", 13f, type.military),

        Oil = new Product("Oil", 10f, new Color(0.25f, 0.25f, 0.25f), type.military),
        MotorFuel = new Product("Motor Fuel", 15f, type.military),
        Machinery = new Product("Machinery", 8f, type.industrial),
        Rubber = new Product("Rubber", 10f, new Color(0.67f, 0.67f, 0.47f), type.industrial), //light grey
        Cars = new Product("Cars", 15f, type.military),
        Tanks = new Product("Tanks", 20f, type.military),
        Airplanes = new Product("Airplanes", 20f, type.military),
        Coal = new Product("Coal", 1f, Color.black, type.industrial),
        Tobacco = new Product("Tobacco", 1f, Color.green, type.consumerProduct),
        Electronics = new Product("Electronics", 1f, type.consumerProduct);

    internal static readonly Product //Food, Sugar, Fibers, Fuel;
        Food = new Product("Food", 0.04f, new List<Product> { Fish, Grain, Cattle, Fruit }, type.consumerProduct),
        Sugar = new Product("Sugar", 0.04f, new List<Product> { Grain, Fruit }, type.consumerProduct),
        Fibers = new Product("Fibers", 0.04f, new List<Product> { Cattle, Cotton }, type.consumerProduct),
        Fuel = new Product("Fuel", 0.04f, new List<Product> { Wood, Coal, Oil }, type.industrial);
    //static initialization
    static Product()
    {

        // abstract products
        foreach (var item in getAllNonAbstract())
            if (item != Product.Gold)
            {
                Game.market.SetDefaultPrice(item, item.defaultPrice.get());
            }
    }
    /// <summary>
    /// General constructor
    /// </summary>    
    /// , bool _isMilitary, bool _isIndustrial, bool _isConsumerProduct
    private Product(string name, float defaultPrice, type productType) : base(name)
    {
        this.defaultPrice = new Value(defaultPrice);
        //if (isAbstract())
        //    allProducts.Insert(0, this);        
        //else
        allProducts.Add(this);
        switch (productType)
        {
            case type.military:
                this._isMilitary = true;
                break;
            case type.industrial:
                this._isIndustrial = true;
                break;
            case type.consumerProduct:
                this._isConsumerProduct = true;
                break;
            default:
                break;
        }

        //TODO checks for duplicates&
    }
    /// <summary>
    /// Constructor for resource product
    /// </summary>                     
    private Product(string name, float defaultPrice, Color color, type productType) : this(name, defaultPrice, productType)
    {
        this.color = color;
        _isResource = true;
        resourceCounter++;
    }
    /// <summary>
    /// Constructor for abstract products
    /// </summary>    
    private Product(string name, float defaultPrice, List<Product> substitutes, type productType) : this(name, defaultPrice, productType)
    {
        _isAbstract = true;
        this.substitutes = substitutes;
    }
    public static IEnumerable<Product> getAllAbstract()
    {
        foreach (var item in allProducts)
            if (item.isAbstract())
                yield return item;
    }
    public static IEnumerable<Product> getAllNonAbstract()
    {
        foreach (var item in allProducts)
            if (!item.isAbstract())
                yield return item;
    }
    /// <summary>
    /// Products go in industrial-military-consumer order
    /// </summary>    
    public static IEnumerable<Product> getAllNonAbstractTradableInPEOrder(Country country)
    {
        foreach (var item in getAllSpecificProductsTradable(x => x.isIndustrial()))
            yield return item;
        foreach (var item in getAllSpecificProductsTradable(x => x.isMilitary()))
            yield return item;
        foreach (var item in getAllSpecificProductsTradable(x => x.isConsumerProduct()))
            yield return item;
    }
    public static IEnumerable<Product> getAll()
    {
        foreach (var item in allProducts)
            yield return item;
    }
    public static IEnumerable<Product> getAllSpecificProductsInvented(Func<Product, bool> selector, Country country)
    {
        foreach (var item in getAllNonAbstract())
            if (selector(item) && item.isInventedBy(country))
                yield return item;
    }
    public static IEnumerable<Product> getAllSpecificProductsTradable(Func<Product, bool> selector)
    {
        foreach (var item in getAllNonAbstract())
            if (selector(item) && item.isTradable())
                yield return item;
    }
    //public static IEnumerable<Product> getAllMilitaryProductsInvented(Country country)
    //{
    //    foreach (var item in getAllNonAbstract())
    //        if (item.isMilitary() && item != Product.Gold && item.isInventedBy(country))
    //            yield return item;
    //}
    //public static IEnumerable<Product> getAllMilitaryProductsTradable()
    //{
    //    foreach (var item in getAllNonAbstract())
    //        if (item.isMilitary() && item.isTradable())
    //            yield return item;
    //}
    //public static IEnumerable<Product> getAllIndustrialProducts(Country country)
    //{
    //    foreach (var item in getAllNonAbstract())
    //        if (item.isIndustrial() && item.isInventedBy(country))
    //            yield return item;
    //}
    //public static IEnumerable<Product> getAllConsumerProducts(Country country)
    //{
    //    foreach (var item in getAllNonAbstract())
    //        if (item.isConsumerProduct() && item.isInventedBy(country))
    //            yield return item;
    //}
    public IEnumerable<Product> getSubstitutes()
    {
        foreach (var item in substitutes)
        {
            yield return item;
        }
    }
    internal static Product getRandomResource(bool ignoreGold)
    {
        if (ignoreGold)
            return Product.Wood;
        return Product.allProducts.PickRandom(x => x.isResource());
    }
    public static void sortSubstitutes()
    {
        foreach (var item in getAllAbstract())
        //if (item.isTradable()) 
        // Abstract are always invented and not gold
        {
            item.substitutes.Sort(CostOrder);
        }
    }
    static public int CostOrder(Product x, Product y)
    {
        //eats less memory
        float sumX = Game.market.getPrice(x).get();
        float sumY = Game.market.getPrice(y).get();
        return sumX.CompareTo(sumY);
        //return Game.market.getCost(x).get().CompareTo(Game.market.getCost(y).get());
    }


    public bool isTradable()
    {
        return this != Product.Gold && isInventedByAnyOne();
    }

    public bool isAbstract()
    {
        return _isAbstract;
    }
    public bool isMilitary()
    {
        return _isMilitary;
    }
    public bool isIndustrial()
    {
        return _isIndustrial;
    }
    public bool isConsumerProduct()
    {
        return _isConsumerProduct;
    }
    /// <summary> Returns true if products exactly same or this is substitute for anotherProduct</summary>
    public bool isSameProduct(Product anotherProduct)
    {
        if (this == anotherProduct)
            return true;
        if (anotherProduct.isAbstract() && anotherProduct.substitutes.Contains(this))
            return true;
        else
            return false;
    }
    /// <summary> Assuming product is abstract product</summary>       
    public bool isSubstituteFor(Product product)
    {
        if (product.substitutes.Contains(this))
            return true;
        else
            return false;
    }

    internal bool isResource()
    {
        return _isResource;
    }


    public bool isInventedByAnyOne()
    {
        // including dead countries. Because dead country could organize production
        //of some freshly invented product
        if (isAbstract())
            return true;
        foreach (var country in Country.allCountries)
            if (this.isInventedBy(country))
                return true;
        return false;
    }
    public bool isInventedBy(Country country)
    {
        if (isAbstract())
            return true;
        if (
            ((this == Metal || this == MetalOre || this == ColdArms) && !country.isInvented(Invention.Metal))
            || (!country.isInvented(Invention.SteamPower) && (this == Machinery || this == Cement))
            || ((this == Artillery || this == Ammunition) && !country.isInvented(Invention.Gunpowder))
            || (this == Firearms && !country.isInvented(Invention.Firearms))
            || (this == Coal && !country.isInvented(Invention.Coal))
            //|| (this == Cattle && !country.isInvented(Invention.Domestication))
            || (!country.isInvented(Invention.CombustionEngine) && (this == Oil || this == MotorFuel || this == Rubber || this == Cars))
            || (!country.isInvented(Invention.Tanks) && this == Tanks)
            || (!country.isInvented(Invention.Airplanes) && this == Airplanes)
            || (this == Tobacco && !country.isInvented(Invention.Tobacco))
            || (this == Electronics && !country.isInvented(Invention.Electronics))
            //|| (!isResource() && !country.isInvented(Invention.Manufactories))
            )
            return false;
        else
            return true;
    }
    //bool isStorable()
    //{
    //    return storable;
    //}
    //void setStorable(bool isStorable)
    //{
    //    storable = isStorable;
    //}   

    internal Value getDefaultPrice()
    {
        if (isResource())
        {
            return defaultPrice.multiplyOutside(Options.defaultPriceLimitMultiplier);
        }
        else
        {
            var type = FactoryType.whoCanProduce(this);
            if (type == null)
                return defaultPrice.multiplyOutside(Options.defaultPriceLimitMultiplier);
            else
            {
                Value res = Game.market.getCost(type.resourceInput);
                res.multiply(Options.defaultPriceLimitMultiplier);
                res.divide(type.basicProduction);
                return res;
            }
        }
    }
    public string ToStringWithoutSubstitutes()
    {        
        return base.ToString();
    }
    public override string ToString()
    {
        if (isAbstract())
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append(" (");
            bool firstLine = true;
            foreach (var item in getSubstitutes())
                if (item.isInventedByAnyOne())
                {
                    if (!firstLine)
                        sb.Append(" or ");
                    sb.Append(item);
                    firstLine = false;
                }
            sb.Append(")");
            return sb.ToString();
            //getSubstitutes().ToList().getString(" or ");
        }
        else
            return base.ToString();
    }

    internal Color getColor()
    {
        return color;
    }
}
