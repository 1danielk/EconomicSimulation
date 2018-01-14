﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{

    public class InventionsPanelTable : UITableNew
    {
        public override void Refresh()
        {
            ////if (Game.date != 0)
            StartUpdate();
            {
                base.RemoveButtons();
                AddButtons();
            }
            EndUpdate();
        }

        private void AddButtons()
        {
            int counter = 0;
            AddHeader();
            if (Game.Player != null)
                foreach (var next in Game.Player.getAvailable())
                //if (next.Key.isAvailable(Game.Player))
                {
                    // Adding invention name 
                    AddButton(next.Key.ToString(), next.Key);
                    ////Adding possibleStatues
                    if (next.Value)
                        AddButton("Invented", next.Key);
                    else
                        AddButton("Uninvented", next.Key);
                    ////Adding invention price
                    AddButton(next.Key.cost.ToString(), next.Key);
                    counter++;
                }
        }

        protected override void AddHeader()
        {
            // Adding invention name 
            AddButton("Invention");
            ////Adding possibleStatues
            AddButton("Status");
            ////Adding invention price
            AddButton("Science points");
        }
    }
}