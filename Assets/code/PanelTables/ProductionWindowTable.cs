﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public class ProductionWindowTable : MyTableNew
{
    public override void refreshContent()
    {
        startUpdate();
        base.RemoveButtons();
        var howMuchRowsShow=calcSize(Game.factoriesToShowInProductionPanel.Count);
        //int counter = 0;
        addHeader();
        for (int i = 0; i < howMuchRowsShow; i++)
        //foreach (Factory next in Game.factoriesToShowInProductionPanel)
        {
            Factory next = Game.factoriesToShowInProductionPanel[i + getRowOffset()];
            // Adding shownFactory name 
            AddButton(next.getType().name + " L" + next.getLevel(), next);

            // Adding province 
            AddButton(next.getProvince().ToString(), next.getProvince());

            ////Adding production
            AddButton(next.getGainGoodsThisTurn().ToString(), next);

            ////Adding effective resource income
            AddButton(next.getInputFactor().ToString(), next);

            ////Adding workforce
            AddButton(next.getWorkForce().ToString(), next);

            ////Adding profit
            if (next.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddButton("none", next);
            else
                AddButton(next.getProfit().ToString(), next);

            ////Adding margin
            if (next.isUpgrading())
                AddButton("Upgrading", next);
            else
            {
                if (next.isBuilding())
                    AddButton("Building", next);
                else
                {
                    if (!next.isWorking())
                        AddButton("Closed", next);
                    else
                    {
                        if (next.getCountry().economy.getValue() == Economy.PlannedEconomy)
                            AddButton("none", next);
                        else
                            AddButton(next.getMargin().ToString(), next);
                    }
                }
            }

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (next.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddButton("centralized", next);
            else
            {
                if (next.getCountry().economy.getValue() == Economy.NaturalEconomy)
                    AddButton(next.getSalary().ToString() + " food", next);
                else
                    AddButton(next.getSalary().ToString() + " coins", next);
            }
            //counter++;
            //contentPanel.r
        }
        endUpdate();
    }

    protected override void addHeader()
    {
        // Adding product name 
        AddButton("Type");

        // Adding province 
        AddButton("Province");

        ////Adding production
        AddButton("Production");

        ////Adding effective resource income
        AddButton("Resources");

        ////Adding workforce
        AddButton("Workforce");

        ////Adding money income
        AddButton("Profit");

        ////Adding profit
        AddButton("Profitability");

        ////Adding salary
        AddButton("Salary");
    }
}
