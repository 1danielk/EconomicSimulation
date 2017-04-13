﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopUnitPanel : DragPanel
{


    public GameObject popUnitPanel;
    public Text generaltext;
    private PopUnit pop;
    // Use this for initialization
    void Start()
    {
        MainCamera.popUnitPanel = this;
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        if (pop != null)
        {
            string demotionText;
            //temp here
            if (pop.WantsDemotion())
                demotionText = pop.getRichestDemotionTarget() + " " + pop.getDemotionSize();
            else
                demotionText = "none";
            string lifeNeeds = ""; string everyDayNeeds = ""; string luxuryNeeds = "";

            var temp = pop.getRealLifeNeeds();
            foreach (Storage next in temp)
                lifeNeeds += next.ToString() + "; ";
            lifeNeeds += pop.getLifeNeedsFullfilling().ToString() + " fullfilled";

            temp = pop.getRealEveryDayNeeds();
            foreach (Storage next in temp)
                everyDayNeeds += next.ToString() + "; ";
            everyDayNeeds += pop.getEveryDayNeedsFullfilling().ToString() + " fullfilled";

            temp = pop.getRealLuxuryNeeds();
            foreach (Storage next in temp)
                luxuryNeeds += next.ToString() + "; ";
            luxuryNeeds += pop.getLuxuryNeedsFullfilling().ToString() + " fullfilled";

            //foreach (Storage next in pop.consumedTotal)
            //    consumedTotal+= next.ToString() + "; ";
            //luxuryNeeds += pop.getLuxuryNeedsFullfilling().ToString() + " fullfilled";

            string loans = "";
            if (pop.loans.get() > 0f)
                loans = "\nLoan: " + pop.loans.ToString();
            generaltext.text = pop + "\n" + "Population: " + pop.population + "\nStorage: " + pop.storageNow.ToString()
                + "\nGain goods: " + pop.gainGoodsThisTurn.ToString()
                + "\nSent to market: " + pop.sentToMarket
                + "\nCash: " + pop.wallet.ToString()
                + "\nMoney income: " + pop.wallet.moneyIncomethisTurn
                + "\nLife needs: " + lifeNeeds + "\nEveryday needs: " + everyDayNeeds + "\nLuxury needs: " + luxuryNeeds
                + "\nConsumed: " + pop.consumedTotal +" costed: "+ Game.market.getCost(pop.consumedTotal)
                + "\nConsumedLT: " + pop.consumedLastTurn + " costed: " + Game.market.getCost(pop.consumedLastTurn)
                + "\nConsumedIM: " + pop.consumedInMarket+ " costed: " + Game.market.getCost(pop.consumedInMarket)
                + "\nDemotion: " + demotionText + "\nGrowth: " + pop.getGrowthSize()
                + "\nUnemployment: " + pop.getUnemployedProcent() + loans;


            //+ "\nExpenses:"
        }
    }
    public void show(PopUnit ipopUnit)
    {
        popUnitPanel.SetActive(true);
        pop = ipopUnit;
        panelRectTransform.SetAsLastSibling();
    }
    public void Hide()
    {
        popUnitPanel.SetActive(false);
    }
    public void onCloseClick()
    {
        Hide();
    }


}
