﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
//using System;
using Nashet.EconomicSimulation;
namespace Nashet.ValueSpace
{
    public class Procent : Value
    {
        internal static readonly Procent HundredProcent = new Procent(1f);
        internal static readonly Procent _50Procent = new Procent(0.5f);
        internal static readonly Procent ZeroProcent = new Procent(0f);
        internal static readonly Procent Max999 = new Procent(999.99f);
        internal static readonly Procent Max = new Procent(int.MaxValue / 1000f);

        public Procent(float number, bool showMessageAboutNegativeValue = true) : base(number, showMessageAboutNegativeValue)
        {
        }
        public Procent(Procent number) : base(number.get())
        {

        }
        public static Procent makeProcent(List<Storage> numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true)
        {
            //result should be  amount of numerator / amount of denominator
            Value numeratorSum = new Value(0f);
            foreach (var item in numerator)
                numeratorSum.Add(item);

            Value denominatorSum = new Value(0f);
            foreach (var item in denominator)
                denominatorSum.Add(item);


            return Procent.makeProcent(numeratorSum, denominatorSum, showMessageAboutOperationFails);
        }
        public static Procent makeProcent(StorageSet numerator, List<Storage> denominator, bool showMessageAboutOperationFails = true)
        {
            //result should be  amount of numerator / amount of denominator
            Value numeratorSum = numerator.GetTotalQuantity();

            Value denominatorSum = new Value(0f);
            foreach (var item in denominator)
            {
                denominatorSum.Add(item);
            }

            return makeProcent(numeratorSum, denominatorSum, showMessageAboutOperationFails);
        }
        public static Procent makeProcent(Value numerator, Value denominator, bool showMessageAboutOperationFails = true)
        {
            return makeProcent(numerator.get(), denominator.get(), showMessageAboutOperationFails);
        }
        public static Procent makeProcent(float numerator, float denominator, bool showMessageAboutOperationFails = true)
        {
            if (denominator == 0f)
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in Procent.makeProcent(float)");
                return Procent.Max999;
            }
            else
                return new Procent(numerator / denominator, showMessageAboutOperationFails);
        }
        public static Procent makeProcent(int numerator, int denominator, bool showMessageAboutOperationFails = true)
        {
            if (denominator == 0)
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in Percent.makeProcent(int)");
                return Procent.Max999;
            }
            else
                return new Procent(numerator / (float)denominator, showMessageAboutOperationFails);
        }
        //TODO check it
        //public static Procent makeProcent(PrimitiveStorageSet numerator, PrimitiveStorageSet denominator)
        //{
        //    float allGoodsAmount = numerator.sum();
        //    if (allGoodsAmount == 0f)
        //        return new Procent(1f);
        //    Dictionary<Product, float> dic = new Dictionary<Product, float>();
        //    //numerator / denominator
        //    float relation;
        //    foreach (var item in numerator)
        //    {
        //        Storage denominatorStorage = denominator.findStorage(item.getProduct());
        //        if (denominatorStorage == null) // no such product
        //            relation = 0f;
        //        else
        //        {
        //            if (denominatorStorage.get() == 0f) // division by zero
        //                relation = 0f;
        //            else
        //            {
        //                relation = item.get() / denominatorStorage.get();
        //                if (relation > 1f) relation = 1f;
        //            }
        //        }
        //        dic.Add(item.getProduct(), relation);
        //    }
        //    float result = 0f;

        //    foreach (var item in dic)
        //    {
        //        result += item.Value * numerator.findStorage(item.Key).get() / allGoodsAmount;
        //    }
        //    return new Procent(result);
        //}

        internal float get50Centre()
        {
            return get() - 0.5f;
        }



        public Procent add(Procent pro, bool showMessageAboutNegativeValue = true)
        {
            base.Add(pro, showMessageAboutNegativeValue);
            return this;
        }
        public void addPoportionally(int baseValue, int secondValue, Procent secondProcent)
        {
            if ((baseValue + secondValue) != 0)
                set((this.get() * baseValue + secondProcent.get() * secondValue) / (float)(baseValue + secondValue));
        }
        public override string ToString()
        {
            //if (get() > 0)
                return (get() * 100f).ToString("0.0") + "%";
            //else
            //    return "0%";
        }

        internal int getProcentOf(int value)
        {
            return Mathf.RoundToInt(get() * value);
        }
        public Value getProcentOf(Value source)
        {
            Value result = new Value(0f);
            source.send(result, source.Copy().multiply(this));
            return result;
        }
        public Storage getProcentOf(Storage source)
        {
            Storage result = new Storage(source.getProduct(), 0f);
            source.send(result, source.multiplyOutside(this));
            return result;
        }
        //override public void set(float invalue)
        //{
        //    if (invalue < 0f)
        //        base.set(0f);
        //    //else
        //    //    if (invalue > 1f)
        //    //    base.set(1f);
        //    else
        //        base.set(invalue);
        //}

        internal void clamp100()
        {
            if (this.isBiggerThan(Procent.HundredProcent))
                this.set(1f);
        }
        public void set(Storage numerator, Storage denominator, bool showMessageAboutOperationFails = true)
        {
            if (denominator.isZero())
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Division by zero in Procent.makeProcent(Value)");
            }
            else
                set(numerator.get() / denominator.get());
        }
    }
}