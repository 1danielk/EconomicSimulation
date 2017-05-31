﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class FinancePanel : DragPanel
{
    public Text expensesText, captionText, incomeText, bankText, loanLimitText, depositLimitText, AutoPutInBankText,
        totalText;
    public Slider loanLimit, depositLimit, autoPutInBankLimit;
    public Toggle autoSendMoneyToBank;
    public CanvasGroup loanPanel, depositPanel, bankPanel;
    StringBuilder sb = new StringBuilder();
    // Use this for initialization
    void Start()
    {
        MainCamera.financePanel = this;
        hide();
    }

    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        sb.Clear();
        sb.Append("Finances of ").Append(Game.Player);
        captionText.text = sb.ToString();

        sb.Clear();
        sb.Append("Income:");
        sb.Append("\n Poor tax (").Append(Game.Player.taxationForPoor.getValue()).Append("): ").Append(Game.Player.getPoorTaxIncome());
        sb.Append("\n Rich tax (").Append(Game.Player.taxationForRich.getValue()).Append("): ").Append(Game.Player.getRichTaxIncome());
        sb.Append("\n Gold mines: ").Append(Game.Player.getGoldMinesIncome());
        sb.Append("\n Owned enterprises: ").Append(Game.Player.getOwnedFactoriesIncome());
        sb.Append("\nTotal: ").Append(Game.Player.moneyIncomethisTurn);

        sb.Append("\n\nBalance: ").Append(Game.Player.getBalance());
        sb.Append("\nHave money: ").Append(Game.Player.cash).Append(" + ").Append(Game.Player.deposits).Append(" on bank deposit");
        sb.Append("\nLoans taken: ").Append(Game.Player.loans);
        sb.Append("\nGDP (current prices): ").Append(Game.Player.getGDP()).Append("; GDP per thousand men: ").Append(Game.Player.getGDPPer1000());
        incomeText.text = sb.ToString();
        //sb.Append("\nScreen resolution: ").Append(Screen.currentResolution).Append(" Canvas size: ").Append(MainCamera.topPanel.transform.parent.GetComponentInParent<RectTransform>().rect);

        //sb.Clear();
        //sb.Append("Balance: ").Append(Game.player.getCountryWallet().getBalance());
        //sb.Append("\nHave money: ").Append(Game.player.wallet.haveMoney).Append(" + ").Append(Game.player.deposits).Append(" on bank deposit");
        ////sb.Append("\nGDP (current prices): ").Append(Game.player.getGDP()).Append("; GDP per thousand men: ").Append(Game.player.getGDPPer1000());
        //totalText.text = sb.ToString();

        sb.Clear();
        sb.Append("Expenses: ");
        sb.Append("\n Unemployment subsidies: ").Append(Game.Player.getUnemploymentSubsidiesExpense())
            .Append(" unemployment: ").Append(Game.Player.getUnemployment());
        sb.Append("\n Enterprises subsidies: ").Append(Game.Player.getfactorySubsidiesExpense());
        sb.Append("\n Storage buying: ").Append(Game.Player.getStorageBuyingExpense());
        sb.Append("\nTotal: ").Append(Game.Player.getAllExpenses());
        expensesText.text = sb.ToString();

        sb.Clear();

        sb.Append("\nNational bank: ").Append(Game.Player.bank).Append(" loans: ").Append(Game.Player.bank.getGivenLoans());
        //sb.Append(Game.player.bank).Append(" deposits: ").Append(Game.player.bank.getGivenLoans());
        sb.Append("\nTotal gold (in world): ").Append(Game.getAllMoneyInWorld());
        sb.Append("\n*Government and others could automatically take money from deposits");
        bankText.text = sb.ToString();

        onLoanLimitChange();
        onDepositLimitChange();
        AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
        // loanPanel.interactable = Country.condCanTakeLoan.isAllTrue(Game.player, out loanPanel.GetComponentInChildren<ToolTipHandler>().tooltip);
        //depositPanel.interactable = Country.condCanPutOnDeposit.isAllTrue(Game.player, out depositPanel.GetComponentInChildren<ToolTipHandler>().tooltip);
        if (Game.Player.isInvented(Invention.banking))
            bankPanel.interactable = true;
        else
        {
            bankPanel.interactable = false;
            autoSendMoneyToBank.isOn = false;
        }
    }
    public void show()
    {
        gameObject.SetActive(true);
        panelRectTransform.SetAsLastSibling();
        refresh();
    }



    public void findNoonesEterprises()
    {
        foreach (var item in Province.allProvinces)
        {
            foreach (var fact in item.allFactories)
            {
                if (fact.factoryOwner == null)
                    new Message("", "Null owner in " + item + " " + fact, "Got it");
                else
                if (fact.factoryOwner is PopUnit)
                {
                    var c = fact.factoryOwner as PopUnit;
                    if (c.getPopulation() == 0)
                        new Message("", "Dead pop owner in " + item + " " + fact, "Got it"); ;
                }
                else
                if (fact.factoryOwner is Country)
                {
                    var c = fact.factoryOwner as Country;
                    if (!c.isExist())
                        new Message("", "Dead country owner in " + item + " " + fact, "Got it"); ;
                }
            }
        }
    }
    public void onTakeLoan()
    {
        Value loan = Game.Player.bank.howMuchCanGive(Game.Player);
        loan.multiple(loanLimit.value);
        if (Game.Player.bank.canGiveMoney(Game.Player, loan))
            Game.Player.bank.giveMoney(Game.Player, loan);
        refresh();
    }
    public void onPutInDeposit()
    {
        Game.Player.bank.takeMoney(Game.Player, new Value(Game.Player.cash.multipleOutside(depositLimit.value)));
        refresh();
    }
    public void onLoanLimitChange()
    {
        loanLimitText.text = Game.Player.bank.howMuchCanGive(Game.Player).multipleOutside(loanLimit.value).ToString();
    }

    public void onDepositLimitChange()
    {
        depositLimitText.text = Game.Player.cash.multipleOutside(depositLimit.value).ToString();
    }
    public void onAutoPutInBankLimitChange()
    {
        Game.Player.autoPutInBankLimit = (int)autoPutInBankLimit.value;
        AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
    }
    public void onAutoSendMoneyToBankToggleChange()
    {
        autoPutInBankLimit.interactable = autoSendMoneyToBank.isOn;
        if (!autoSendMoneyToBank.isOn)
        {
            Game.Player.autoPutInBankLimit = 0;
            autoPutInBankLimit.value = 0f;
            AutoPutInBankText.text = Game.Player.autoPutInBankLimit.ToString();
        }

    }
}
