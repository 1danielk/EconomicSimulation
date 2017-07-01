﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class BuildPanelTable : MyTable
{
    override protected void refresh()
    {
        ////if (Game.date != 0)
        {
            base.RemoveButtons();
            AddButtons();
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gameObject.transform.childCount / this.columnsAmount * rowHeight + 50);
        }
    }
    protected void AddButton(string text, FactoryType type)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(gameObject.transform, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        //if (inventionType == null)
        //    sampleButton.Setup(text, this, null);
        //else
        sampleButton.Setup(text, this, type);
    }
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding shownFactory type
        AddButton("Name");

        ////Adding cost
        AddButton("Cost");

        ////Adding resource needed
        //AddButton("Input", null);

        ////Adding potential output
        AddButton("Output");

        ////Adding potential profit
        AddButton("Potential margin");
        if (Game.selectedProvince != null)
        {
            var factoryList = Game.selectedProvince.whatFactoriesCouldBeBuild();

            foreach (var next in factoryList)
            {
                // Adding shownFactory type
                AddButton(next.ToString(), next);

                ////Adding cost
                //if (Game.player.isInvented(InventionType.capitalism))
                if (Economy.isMarket.checkIftrue(Game.Player))
                    AddButton(next.getBuildCost().ToString(), next);
                else
                    AddButton(next.getBuildNeeds().ToString(), next);


                ////Adding resource needed
                //AddButton(next.resourceInput.ToString(), next);

                ////Adding potential output
                AddButton(next.basicProduction.ToString(), next);

                ////Adding potential profit
                AddButton(next.getPossibleMargin(Game.selectedProvince).ToString(), next);
                
                counter++;
            }
        }
    }
}