﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public class ProvincePanel : Window
    {
        [SerializeField]
        private Text generaltext;

        [SerializeField]
        private Button btnOwner, btnBuild, btAttackThat, btMobilize, btGrandIndependence;

        //private Province selectedProvince;
        // Use this for initialization    
        void Start()
        {
            MainCamera.provincePanel = this;
            Hide();
        }
       
        public void onBuildClick()
        {
            //MainCamera.buildPanel.show(true);
            if (MainCamera.buildPanel.isActiveAndEnabled)
                MainCamera.buildPanel.Hide();
            else
                MainCamera.buildPanel.Show();
        }
        public void onGrantIndependenceClick()
        {
            Country whomGrant = Game.selectedProvince.getRandomCore(x => x != Game.Player && !x.isAlive());
            if (whomGrant == null)
                whomGrant = Game.selectedProvince.getRandomCore(x => x != Game.Player);

            whomGrant.onGrantedProvince(Game.selectedProvince);
            MainCamera.refreshAllActive();
        }
        public void onCountryDiplomacyClick()
        {
            if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            {
                if (MainCamera.diplomacyPanel.getSelectedCountry() == Game.selectedProvince.getCountry())

                    MainCamera.diplomacyPanel.Hide();
                else
                    MainCamera.diplomacyPanel.show(Game.selectedProvince.getCountry());
            }
            else
                MainCamera.diplomacyPanel.show(Game.selectedProvince.getCountry());
        }
        public void onMobilizeClick()
        {
            Game.selectedProvince.mobilize();
            MainCamera.militaryPanel.show(null);
        }
        public void onEnterprisesClick()
        {
            if (MainCamera.productionWindow.isActiveAndEnabled)
                if (MainCamera.productionWindow.IsSelectedAnyProvince())
                {
                    if (MainCamera.productionWindow.IsSelectedProvince(Game.selectedProvince))
                        MainCamera.productionWindow.Hide();
                    else
                    {
                        MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                        MainCamera.productionWindow.Refresh();
                    }
                }
                else
                {
                    MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                    MainCamera.productionWindow.Refresh();
                }
            else
            {
                MainCamera.productionWindow.SelectProvince(Game.selectedProvince);
                MainCamera.productionWindow.Show();
            }
        }
        public void onPopulationDetailsClick()
        {
            if (MainCamera.populationPanel.isActiveAndEnabled)
                if (MainCamera.populationPanel.IsSelectedAnyProvince())
                {                    
                    //if (MainCamera.populationPanel.IsAppliedThatFilter(PopulationPanel.filterSelectedProvince))
                    if (MainCamera.populationPanel.IsSelectedProvince(Game.selectedProvince))
                        MainCamera.populationPanel.Hide();
                    else
                    {
                        //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                        MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                        MainCamera.populationPanel.Refresh();                        
                    }
                }
                else
                {
                    //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                    MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                    MainCamera.populationPanel.Refresh();
                }
            else
            {
                //MainCamera.populationPanel.AddFilter(PopulationPanel.filterSelectedProvince);
                MainCamera.populationPanel.SelectProvince(Game.selectedProvince);
                MainCamera.populationPanel.Show();
            }

        }
        public void onAttackThatClick()
        {
            MainCamera.militaryPanel.show(Game.selectedProvince);
        }
        
        
        public override void Refresh()
        {
            var sb = new StringBuilder("Province name: ").Append(Game.selectedProvince);
           // sb.Append("\nID: ").Append(Game.selectedProvince.getID());
            sb.Append("\nPopulation (with families): ").Append(Game.selectedProvince.getFamilyPopulation());
            sb.Append("\nAverage loyalty: ").Append(Game.selectedProvince.getAverageLoyalty());
            sb.Append("\nMajor culture: ").Append(Game.selectedProvince.getMajorCulture());
            sb.Append("\nGDP: ").Append(Game.selectedProvince.getGDP());
            sb.Append("\nResource: ");
            if (Game.selectedProvince.getResource() == null)
                sb.Append("none ");
            else
                sb.Append(Game.selectedProvince.getResource());
            sb.Append("\nTerrain: ").Append(Game.selectedProvince.getTerrain());
            sb.Append("\nRural overpopulation: ").Append(Game.selectedProvince.getOverpopulation() * 100).Append("%");
            sb.Append("\nCores: ").Append(Game.selectedProvince.getCoresDescription());
            if (Game.selectedProvince.getModifiers().Count > 0)
                sb.Append("\nModifiers: ").Append(GetStringExtensions.getString(Game.selectedProvince.getModifiers()));


            // "\nNeighbors " + province.getNeigborsList()
            ;
            Text text = btnOwner.GetComponentInChildren<Text>();
            text.text = "Owner: " + Game.selectedProvince.getCountry();


            btnBuild.interactable = Province.doesCountryOwn.checkIftrue(Game.Player, Game.selectedProvince, out btnBuild.GetComponent<ToolTipHandler>().text);

            btMobilize.GetComponent<ToolTipHandler>().SetText(btnBuild.GetComponent<ToolTipHandler>().text);
            btMobilize.interactable = btnBuild.interactable;


            //if (Game.devMode)
            //    sb.Append("\nColor: ").Append(province.getColorID());
            btAttackThat.interactable = Country.canAttack.isAllTrue(Game.selectedProvince, Game.Player, out btAttackThat.GetComponent<ToolTipHandler>().text);
            btGrandIndependence.interactable = Province.canGetIndependence.isAllTrue(Game.selectedProvince, Game.Player, out btGrandIndependence.GetComponent<ToolTipHandler>().text);
            generaltext.text = sb.ToString();
        }
    }
}