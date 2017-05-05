﻿using UnityEngine;
using System.Collections;
using System;
//using System;

public class Procent : Value
{
    private int v;

    //uint value;
    public static bool GetChance(uint procent)
    {
        //TODO fix that GetChance shit
        float realLuck = UnityEngine.Random.value * 100; // (0..100 including)
        if (procent >= realLuck)
            return true;
        else
            return false;
    }

    public Procent(float number) : base(number)
    {

    }
    public static Procent makeProcent(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            Debug.Log("Division by zero in Procent.makeProcent()");
            return new Procent(0f);
        }
        else
            return new Procent(numerator / (float)denominator);
    }
    public Value sendProcentToNew(Value source)
    {

        Value result = new Value(0f);
        source.pay(result, source.multiple(this));
        return result;
    }
    public Storage sendProcentToNew(Storage source)
    {
        Storage result = new Storage(source.getProduct(), 0f);
        source.pay(result, source.multiple(this));
        return result;
    }

    public void add(Procent pro)
    {
        base.add(pro);
        if (base.get() > 1f)
            set(1f);
    }
    public void addPoportionally(int baseValue, int secondValue, Procent secondProcent)
    {
        set((this.get() * baseValue + secondProcent.get() * secondValue) / (float)(baseValue + secondValue));
    }
    public override string ToString()
    {
        if (get() > 0)
            return System.Convert.ToString(get() * 100f) + "%";
        else return "0%";
    }

    //internal uint getProcent(int population)
    //{
    //    return (uint)Mathf.RoundToInt(get() * population);
    //}
    internal int getProcent(int population)
    {
        return Mathf.RoundToInt(get() * population);
    }
    override public void set(float invalue)
    {
        if (invalue < 0f)
            base.set(0f);
        //else
        //    if (invalue > 1f)
        //    base.set(1f);
        else
            base.set(invalue);
    }
}
