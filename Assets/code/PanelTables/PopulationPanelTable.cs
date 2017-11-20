﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class PopulationPanelTable : MyTableNew
{
    public override void refreshContent()
    {
        alreadyInUpdate = true;
        //lock (gameObject)
        {
            RemoveButtons();
            calcSize(Game.popsToShowInPopulationPanel.Count);
            addHeader();
            if (Game.popsToShowInPopulationPanel.Count > 0)
            {
                for (int i = 0; i < howMuchRowsShow; i++)
                //foreach (PopUnit record in Game.popsToShowInPopulationPanel)
                {
                    PopUnit pop = Game.popsToShowInPopulationPanel[i + offset];

                    // Adding number
                    //AddButton(Convert.ToString(i + offset), record);

                    // Adding PopType
                    AddButton(pop.popType.ToString(), pop);
                    ////Adding population
                    AddButton(System.Convert.ToString(pop.getPopulation()), pop);
                    ////Adding culture
                    AddButton(pop.culture.ToString(), pop);
                    ////Adding province
                    AddButton(pop.getProvince().ToString(), pop.getProvince(), "Click to select this province");
                    ////Adding education
                    AddButton(pop.education.ToString(), pop);

                    ////Adding cash
                    AddButton(pop.cash.ToString(), pop);

                    ////Adding needs fulfilling

                    //PopUnit ert = record;
                    AddButton(pop.needsFullfilled.ToString(), pop,
                        //() => ert.consumedTotal.ToStringWithLines()                        
                        () => "Consumed:\n" + pop.getConsumed().getContainer().getString("\n")
                        );

                    ////Adding loyalty
                    string accu;
                    PopUnit.modifiersLoyaltyChange.getModifier(pop, out accu);
                    AddButton(pop.loyalty.ToString(), pop, accu);

                    //Adding Unemployment
                    AddButton(pop.getUnemployedProcent().ToString(), pop);

                    //Adding Movement
                    if (pop.getMovement() == null)
                        AddButton("", pop);
                    else
                        AddButton(pop.getMovement().getShortName(), pop,()=> pop.getMovement().getName());
                }

            }
        }
        alreadyInUpdate = false;
    }
    protected override void addHeader()
    {
        // Adding number
        // AddButton("Number");

        // Adding PopType
        AddButton("Type");

        ////Adding population
        AddButton("Population");

        ////Adding culture
        AddButton("Culture");

        ////Adding province
        AddButton("Province");

        ////Adding education
        AddButton("Education");

        ////Adding storage
        //if (null.storage != null)
        AddButton("Cash");
        //else AddButton("Administration");

        ////Adding needs fulfilling
        AddButton("Needs fulfilled");

        ////Adding loyalty
        AddButton("Loyalty");

        ////Adding Unemployment
        AddButton("Unemployment");

        //Adding Movement
        AddButton("Movement");
    }
}