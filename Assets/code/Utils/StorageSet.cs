﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSet
{
    //private static Storage tStorage;
    private List<Storage> container = new List<Storage>();
    public StorageSet()
    {
        container = new List<Storage>();
    }
    public StorageSet(List<Storage> incontainer)
    {
        container = incontainer;
    }
    public StorageSet getCopy()
    {
        StorageSet res = new StorageSet();
        foreach (Storage stor in this)
            res.container.Add(new Storage(stor.getProduct(), stor.get()));
        return res;
    }
    public void sort(Comparison<Storage> comparison)
    {
        container.Sort(comparison);
    }
    /// <summary>
    /// If duplicated than overwrites
    /// </summary>    
    public void set(Storage setValue)
    {
        Storage find = this.hasStorage(setValue.getProduct());
        if (find == null)
            container.Add(new Storage(setValue));
        else
            find.set(setValue);
    }
    /// <summary>
    /// If duplicated than overwrites
    /// </summary>    
    public void set(Product product, Value value)
    {
        Storage find = hasStorage(product);
        if (find == null)
            container.Add(new Storage(product, value));
        else
            find.set(value);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(Storage need)
    {
        Storage find = hasStorage(need.getProduct());
        if (find == null)
            container.Add(new Storage(need));
        else
            find.add(need);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(StorageSet need)
    {
        foreach (Storage n in need)
            this.add(n);
    }
    /// <summary>
    /// If duplicated than adds
    /// </summary>
    internal void add(List<Storage> need)
    {
        foreach (Storage n in need)
            this.add(n);
    }
    public IEnumerator<Storage> GetEnumerator()
    {
        for (int i = 0; i < container.Count; i++)
        {
            yield return container[i];
        }
    }
    public List<Storage> getContainer()
    {
        return container;
    }

    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        Storage storage = getBiggestStorage(what.getProduct());
        if (storage.isZero())
            return false;
        else
            return storage.send(whom.storage, what);
    }
    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, StorageSet what)
    {
        bool res = true;
        foreach (var item in what)
        {
            if (!send(whom, item))//!has(item) || 
                res = false;
        }
        return res;
    }
    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, List<Storage> what)
    {
        bool result = true;
        foreach (var item in what)
        {
            if (!send(whom, item))
                result = false;
        }
        return result;
    }
    public bool has(Storage what)
    {
        Storage foundStorage = getBiggestStorage(what.getProduct());
        return (foundStorage.isBiggerOrEqual(what)) ? true : false;
    }
    /// <summary>Returns False when some check not presented in here</summary>    
    internal bool has(StorageSet check)
    {
        foreach (Storage stor in check)
            if (!has(stor))
                return false;
        return true;
    }
    /// <summary>Returns False if any item from are not available in that storage</summary>    
    internal bool has(List<Storage> list)
    {
        foreach (Storage stor in list)
            if (!has(stor))
                return false;
        return true;
    }
    /// <summary>Returns non null if container allready has storage for that product</summary>    
    protected Storage hasStorage(Product product)
    {
        foreach (Storage stor in container)
            if (stor.isExactlySameProduct(product))
                return stor;
        return null;
    }
    //internal Procent HowMuchHaveOf(PrimitiveStorageSet need)
    //{
    //    PrimitiveStorageSet shortage = this.subtractOuside(need);
    //    return Procent.makeProcent(shortage, need);
    //}

    /// <summary>Returns NULL if search is failed</summary>
    //internal Storage findStorage(Product product)
    //{
    //    foreach (Storage stor in container)
    //        if (stor.isSameProductType(product))
    //            return stor;
    //    return null;
    //}
    /// <summary>Returns NULL if search is failed</summary>
    //internal Storage findStorage(Storage storageToFind)
    //{
    //    foreach (Storage stor in container)
    //        if (stor.isSameProduct(storageToFind))
    //            return stor;
    //    return null;
    //}
    /// <summary>Returns NULL if search is failed</summary>
    //internal Storage findSubstitute(Storage need)
    //{
    //    foreach (Storage storage in container)
    //        if (storage.hasSubstitute(need))
    //            return storage;
    //    return null;
    //}
    /// <summary>Returns NULL if search is failed</summary>
    //internal Storage findSubstitute(Product need)
    //{
    //    foreach (Storage storage in container)
    //        if (storage.isSubstituteProduct(need))
    //            return storage;
    //    return null;
    //}
    /// <summary>Returns NULL if search is failed</summary>
    //internal Storage findExistingSubstitute(Storage need)
    //{
    //    foreach (Storage storage in container)
    //        if (storage.hasSubstitute(need))
    //            return storage;
    //    return null;
    //} 

    /// <summary>Return first found storage of what products or it's substitute
    /// Returns NEW empty storage if search is failed</summary>
    internal Storage getFirstStorage(Product what)
    {
        foreach (Storage storage in container)
            if (storage.isSameProductType(what))
                return storage;
        //if not found
        return new Storage(what, 0f);
    }
    /// <summary>Gets storage if there is enough product of that type. 
    /// Returns NEW empty storage if search is failed</summary>    
    internal Storage getExistingStorage(Storage what)
    {
        foreach (Storage storage in container)
            if (storage.has(what))
                return storage;
        //if not found
        return new Storage(what.getProduct(), 0f);
    }
    /// <summary> Finds substitute for abstrat need and returns new storage with product converted to non-abstract product
    /// Returns copy of need if need was not abstract (make check)
    /// If didn't find substitute Returns copy of empty storage of need product</summary>  
    //todo Make same method for chapest substitute?
    internal Storage convertToBiggestStorageProduct(Storage need)
    {
        return new Storage(getBiggestStorage(need.getProduct()).getProduct(), need);
    }
    /// <summary>Gets biggest storage of that product type. Returns NEW empty storage if search is failed</summary>    
    internal Storage getBiggestStorage(Product what)
    {
        return getStorage(what, CollectionExtensions.MaxBy, x => x.get());
    }
    /// <summary>Gets cheapest storage of that product type. Returns NEW empty storage if search is failed</summary>    
    internal Storage getCheapestStorage(Product what)
    {
        return getStorage(what, CollectionExtensions.MinBy, x => Game.market.getPrice(x.getProduct()).get());
    }
    /// <summary>
    ///  Universal search for storages
    /// </summary>       
    private Storage getStorage(Product what,
       Func<IEnumerable<Storage>, Func<Storage, float>, Storage> selectorMethod,
       Func<Storage, float> selector)
    {
        if (what.isAbstract())
        {
            List<Storage> res = new List<Storage>();
            foreach (Storage storage in container)
                if (storage.isSameProductType(what))// && !storage.isAbstractProduct())
                    res.Add(storage);
            var found = selectorMethod(res, selector);
            if (found == null)
                return new Storage(what, 0f);
            return found;
        }
        else
        {
            foreach (Storage storage in container)
                if (storage.isExactlySameProduct(what))
                    return storage;
            return new Storage(what, 0f);
        }
    }

    override public string ToString()
    {
        return container.getString(", ");
    }
    internal void setZero()
    {
        foreach (Storage st in this)
            st.set(0f);
    }

    internal int Count()
    {
        return container.Count;
    }
    /// <summary>
    /// returns new copy
    /// </summary>    
    internal StorageSet Divide(float v)
    {
        StorageSet result = new StorageSet();
        foreach (Storage stor in container)
            result.set(new Storage(stor.getProduct(), stor.get() / v));
        return result;
    }

    //internal bool subtract(Storage stor)
    //{
    //    Storage find = this.findStorage(stor.getProduct());
    //    if (find == null)
    //        return false;//container.Add(value);
    //    else
    //    {
    //        if (find.has(stor))
    //            return find.subtract(stor);
    //        else
    //        {
    //            Debug.Log("Someone tried to subtract from Pr.Storage more than it has");
    //            find.setZero();
    //            return false;
    //        }
    //    }
    //}
    virtual public bool subtract(Storage storage, bool showMessageAboutNegativeValue = true)
    {
        Storage found = hasStorage(storage.getProduct());
        if (found == null)
        {
            if (showMessageAboutNegativeValue)
                Debug.Log("Someone tried to subtract from StorageSet more than it has - " + storage);
            return false;//container.Add(value);
        }
        else
            return found.subtract(storage, showMessageAboutNegativeValue);
    }
    internal Storage subtractOutside(Storage stor)
    {
        Storage found = hasStorage(stor.getProduct());
        if (found == null)
        {
            Debug.Log("Someone tried to subtract from StorageSet more than it has");
            return new Storage(stor.getProduct(), 0f);
        }
        else
            return new Storage(stor.getProduct(), found.subtractOutside(stor).get());
    }
    public void subtract(StorageSet set, bool showMessageAboutNegativeValue = true)
    {
        foreach (Storage stor in set)
            this.subtract(stor, showMessageAboutNegativeValue);
    }
    internal void subtract(List<Storage> set, bool showMessageAboutNegativeValue = true)
    {
        foreach (Storage stor in set)
            this.subtract(stor, showMessageAboutNegativeValue);
    }
    internal StorageSet subtractOuside(StorageSet substracting)
    {
        StorageSet result = new StorageSet();
        foreach (Storage stor in substracting)
            result.add(this.subtractOutside(stor));
        return result;
    }

    internal void copyDataFrom(StorageSet consumed)
    {
        foreach (Storage stor in consumed)
            //if (stor.get() > 0f)
            this.set(stor);
        // SetZero();
    }


    internal void sendAll(StorageSet toWhom)
    {
        toWhom.add(this);
        this.setZero();
    }

    internal float sum()
    {
        float result = 0f;
        foreach (var item in container)
            result += item.get();
        return result;

    }
    //internal bool hasSubstitute(Storage need)
    //{
    //    foreach (var item in container)
    //    {
    //        if (item.hasSubstitute(need))
    //            return true;
    //    }
    //    return false;
    //}



    //internal PrimitiveStorageSet Copy()
    //{
    //    oldList.ForEach((item) =>
    //    {
    //        newList.Add(new YourType(item));
    //    });
    //}
}
