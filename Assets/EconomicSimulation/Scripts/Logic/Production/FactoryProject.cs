﻿using UnityEngine;

using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class NewFactoryProject : IInvestable
    {
        public readonly ProductionType Type;
        private readonly Province province;
        public NewFactoryProject(Province province, ProductionType type)
        {
            this.Type = type;
            this.province = province;
        }
        public Country Country
        {
            get { return province.Country; }
        }
        public Province Province
        {
            get { return province; }
        }
        public bool CanProduce(Product product)
        {
            return Type.CanProduce(product);
        }

        
        public Money GetInvestmentCost()
        {
            return Type.GetBuildCost();
        }

        public Procent GetMargin()
        {
            return Type.GetPossibleMargin(Province);
        }

        
    }
}