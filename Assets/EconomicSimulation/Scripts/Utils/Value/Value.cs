﻿using System;
using UnityEngine;
using Nashet.Utils;
namespace Nashet.ValueSpace
{
    
    public class Value : ICopyable<Value>
    {
        ///<summary> storing as value as number * precision </summary>
        private uint value;
        internal readonly static uint precision = 1000; // 0.01
        internal static readonly Value Zero = new Value(0);
        internal static readonly Value Max999 = new Value(999.99f);
        internal static readonly Value Max = new Value(int.MaxValue / 1000f);

        public Value(float number, bool showMessageAboutNegativeValue = true)
        {
            if (number >= 0f)
                set(number); // set already have multiplier
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Can't create negative Value");
                set(0);
            }

        }
        protected Value(Value number)
        {
            set(number); // set already have multiplier
        }
        public static float Convert(float invalue)
        {
            uint intermediate = (uint)Mathf.RoundToInt(invalue * precision);
            return (float)intermediate / (float)precision;
        }
        public bool isBiggerThan(Value invalue)
        {
            return this.value > invalue.value;
        }
        public bool IsEqual(Value invalue)
        {
            return this.value == invalue.value;
        }
        /// <summary>
        /// Returns true if bigger than argument + barrier
        /// </summary>

        public bool isBiggerThan(Value invalue, Value barrier)
        {
            return this.value > invalue.value + barrier.value;
        }
        public bool isBiggerOrEqual(Value invalue)
        {
            return this.value >= invalue.value;
        }
        public bool isSmallerThan(Value invalue)
        {
            return this.value < invalue.value;
        }
        public bool isSmallerOrEqual(Value invalue)
        {
            return this.value <= invalue.value;
        }
        //TODO overflow checks?
        virtual public Value Add(Value invalue, bool showMessageAboutNegativeValue = true)
        {
            if (value + invalue.value < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-Value failed");
                set(0);
            }
            else
                value += invalue.value;
            return this;
        }

        virtual public Value add(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue + get() < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value Add-float failed");
                set(0);
            }
            else
                value += (uint)Mathf.RoundToInt(invalue * precision);
            return this;
        }
        //internal Value addOutside(Value deposits)
        //{
        //    var result = new Value(this);
        //    result.add(deposits);
        //    return result;
        //}
        public Value subtract(Value invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.value > value)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract gave negative result");
                set(0);
                return this;
            }
            else
            {
                value -= invalue.value;
                return this;
            }
        }
        public void subtract(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue  > get())
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value subtract failed");
                value = 0;
            }
            else
                value -= (uint)Mathf.RoundToInt(invalue * precision);
        }
        //public Value subtractOutside(Value invalue, bool showMessageAboutNegativeValue = true)
        //{
        //    if (invalue.value > value)
        //    {
        //        if (showMessageAboutNegativeValue)
        //            Debug.Log("Value subtractOutside failed");
        //        return new Value(0);
        //    }
        //    else
        //        return new Value((this.value - invalue.value) / (float)precision);
        //}

        /// <summary>Keeps result inside</summary>    
        public Value multiply(Value invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.get() < 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                value = 0;
            }
            else
                set(invalue.get() * this.get());
            return this;
        }
        
        public Value multiply(float invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue < 0f)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value multiply failed");
                value = 0;
            }
            else
                set(invalue * this.get());
            return this;
        }
        /// <summary>
        /// returns new value
        /// </summary>
        //internal Value multiplyOutside(int invalue, bool showMessageAboutOperationFails = true)
        //{
        //    if (invalue < 0)
        //    {
        //        if (showMessageAboutOperationFails)
        //            Debug.Log("Value multiply failed");
        //        return new Value(0f);
        //    }
        //    else
        //        return new Value(get() * invalue);
        //}
        //virtual public Value multiplyOutside(float invalue, bool showMessageAboutOperationFails = true)
        //{
        //    if (invalue < 0f)
        //    {
        //        if (showMessageAboutOperationFails)
        //            Debug.Log("Value multiply failed");
        //        return new Value(0f);
        //    }
        //    else
        //        return new Value(get() * invalue);
        //}
        /// <summary>
        /// returns new value
        /// </summary>    
        //virtual public Value multiplyOutside(Value invalue, bool showMessageAboutNegativeValue = true)
        //{
        //    if (invalue.get() < 0)
        //    {
        //        if (showMessageAboutNegativeValue)
        //            Debug.Log("Value multiply failed");
        //        return new Value(0);
        //    }
        //    else
        //        return new Value(get() * invalue.get());
        //}
        /// <summary>Keeps result inside</summary>    
        public Value divide(Value invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue.get() <= 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide failed");
                value = 99999;
            }
            else
                set(this.value / (float)invalue.value);
            return this;
        }
        
        internal Value divide(int v, bool showMessageAboutNegativeValue = true)
        {
            if (v <= 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide failed");
                value = 99999;
            }
            else
                set(this.get() / (float)v);
            return this;
        }


        /// <summary>returns new value </summary>
        internal Value divideOutside(int invalue, bool showMessageAboutNegativeValue = true)
        {
            if (invalue == 0)
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Value divide by zero");
                return Max999;
            }
            else
                return new Value(get() / invalue);
        }
        /// <summary>returns new value </summary>
        //internal Value divideOutside(Value invalue, bool showMessageAboutNegativeValue = true)
        //{
        //    if (invalue.get() == 0)
        //    {
        //        if (showMessageAboutNegativeValue)
        //            Debug.Log("Value divide by zero");
        //        return Max999;
        //    }
        //    else
        //        return new Value(get() / invalue.get());
        //}
        /// <summary>
        /// Bigger than 0
        /// </summary>
        /// <returns></returns>
        public bool isNotZero()
        {
            return value > 0;
        }
        public bool isZero()
        {
            return value == 0;
        }
        internal Procent HowMuchHaveOf(Value need)
        {
            if (need.value == 0)
                return new Procent(1f);
            else
                return Procent.makeProcent((int)this.value, (int)need.value);
        }

        public void send(Value another, float amount, bool showMessageAboutOperationFails = true)
        {
            if (this.get() >= amount)
            {
                this.subtract(amount);
                another.add(amount);
            }
            else
            {
                if (showMessageAboutOperationFails) Debug.Log("No enough value to send");
                sendAll(another);
            }

        }
        public bool send(Value another, Value amount, bool showMessageAboutOperationFails = true)
        {
            if (this.get() >= amount.get())
            {
                subtract(amount);
                another.Add(amount);
                return true;
            }
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("No enough value to send");
                sendAll(another);
                return false;
            }
        }
        //public bool has(Value HowMuch)
        //{
        //    if (HowMuch.value >= this.value)
        //        return false;
        //    else return true;
        //}
        public void sendAll(Value another)
        {
            another.Add(this);
            this.setZero();
        }
        // toDO test that file properly
        public float get()
        {
            return (float)value / (float)precision; //TODO roundation fakup
        }

        internal void setZero()
        {
            value = 0;
        }

        virtual public void set(float invalue, bool showMessageAboutOperationFails = true)
        {
            if (invalue >= 0)
                value = (uint)Mathf.RoundToInt(invalue * precision);
            else
            {
                if (showMessageAboutOperationFails)
                    Debug.Log("Can't set negative value");
                value = 0;
            }

        }
        public void set(Value invalue)
        {
            value = invalue.value;
        }
        override public string ToString()
        {
            if (value > 0)
                return System.Convert.ToString(get());
            else return "0";
        }

        public Value Copy()
        {
            return new Value(this);
        }
        //public int Compare(Value x, Value y)
        //{
        //    if (x.value == y.value)
        //        return 0;
        //    else
        //        if (x.value > y.value)
        //        return -1;
        //    else
        //        return 1;
        //}
    }
}