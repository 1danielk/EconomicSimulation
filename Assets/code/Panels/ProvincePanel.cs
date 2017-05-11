﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProvincePanel : MonoBehaviour
{
    public Text generaltext;
    public GameObject provincePanel;
    public Button btnOwner;
    // Use this for initialization
    // thisPanel;
    void Start()
    {
        //generaltext = transform.FindChild("GeneralText").gameObject.GetComponent<Text>();
        MainCamera.provincePanel = this;
        hide();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void hide()
    {
        provincePanel.SetActive(false);
    }
    public void show()
    {
        provincePanel.SetActive(true);
    }
    public void onCloseClick()
    {
        hide();
    }
    public void onBuildClick()
    {
        //MainCamera.buildPanel.show(true);
        if (MainCamera.buildPanel.isActiveAndEnabled)
            MainCamera.buildPanel.hide();
        else
            MainCamera.buildPanel.show(true);
    }
    public void onPopulationDetailsClick()
    {
        //Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
        //MainCamera.populationPanel.show(true);
        if (MainCamera.populationPanel.isActiveAndEnabled)
            if (MainCamera.populationPanel.showAll)
            {
                MainCamera.populationPanel.hide();
                Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
                MainCamera.populationPanel.showingProvince = Game.selectedProvince;
                MainCamera.populationPanel.showAll = false;
                MainCamera.populationPanel.show(true);
            }
            else
                if (MainCamera.populationPanel.showingProvince == Game.selectedProvince)
                MainCamera.populationPanel.hide();
            else
            {
                Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
                MainCamera.populationPanel.showingProvince = Game.selectedProvince;
                MainCamera.populationPanel.showAll = false;
                MainCamera.populationPanel.show(true);
            }
        else
        {
            Game.popsToShowInPopulationPanel = Game.selectedProvince.allPopUnits;
            MainCamera.populationPanel.showingProvince = Game.selectedProvince;
            MainCamera.populationPanel.showAll = false;
            MainCamera.populationPanel.show(true);
        }

    }
    public void onEnterprisesClick()
    {
        //Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
        //MainCamera.productionWindow.show(Game.selectedProvince, true);

        if (MainCamera.productionWindow.isActiveAndEnabled)
            if (MainCamera.productionWindow.showingProvince == null)
            {
                MainCamera.productionWindow.hide();
                Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
                MainCamera.productionWindow.showingProvince = Game.selectedProvince;
                MainCamera.productionWindow.show(Game.selectedProvince, true);
            }
            else
                if (MainCamera.productionWindow.showingProvince == Game.selectedProvince)
                MainCamera.productionWindow.hide();
            else
            {
                MainCamera.productionWindow.hide();
                Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories; ;
                MainCamera.productionWindow.showingProvince = Game.selectedProvince;
                MainCamera.productionWindow.show(Game.selectedProvince, true);
            }
        else
        {
            Game.factoriesToShowInProductionPanel = Game.selectedProvince.allFactories;
            MainCamera.productionWindow.showingProvince = Game.selectedProvince;
            MainCamera.productionWindow.show(Game.selectedProvince, true);
        }
    }
    public void UpdateProvinceWindow(Province province)
    {
        generaltext.text = "name: " + province
            + "\nID: " + province.getID()
            + "\nPopulation (with families): " + province.getFamilyPopulation()
            + "\nMiddle loyalty: " + province.getMiddleLoyalty()
            + "\nTax income: " + province.getIncomeTax()
            + "\nResource: " + province.getResource()
            + "\nRural overpopulation: " + province.getOverpopulation()
            //+ "\nNeighbors " + province.getNeigborsList()
            ;
        Text text = btnOwner.GetComponentInChildren<Text>();
        text.text = "Owner: " + province.getOwner();

        if (Game.devMode) generaltext.text += "\nColor: " + province.getColorID();
    }
}
