﻿using UnityEngine;
using UnityEditor;
using Nashet.EconomicSimulation;
using Nashet.Utils;
namespace Nashet.ValueSpace
{
    public class Money : Storage, ICopyable<Money>
    {
        public Money(float value) : base(Product.Gold, value)
        { }
        protected Money(Money value) : base(value)
        { }

        public Money Copy()
        {
            return new Money(this);
        }
        internal Money Divide(int divider, bool showMessageAboutNegativeValue = true)
        {
            if (divider == 0)
            {
                Debug.Log("Can't divide by zero");
                set(99999);
                return this;
            }
            else
                return this.Multiply(1f / divider, showMessageAboutNegativeValue);
        }
        public Money Multiply(Procent multiplier, bool showMessageAboutNegativeValue = true)
        {
            Multiply(multiplier.get());
            return this;
        }
        public Money Multiply(int multiplier, bool showMessageAboutNegativeValue = true)
        {
            return Multiply((float)multiplier, showMessageAboutNegativeValue);
        }
        public Money Multiply(float multiplier, bool showMessageAboutNegativeValue = true)
        {
            if (multiplier < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                set(0);
            }
            else
                set(multiplier * this.get());
            return this;
        }
        ///////////////////Add section
        public Money Add(float adding, bool showMessageAboutNegativeValue = true)
        {
            if (adding + get() < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Money can't be negative");
                set(0);
            }
            else
                set(get() + adding);
            return this;
        }
    }
}