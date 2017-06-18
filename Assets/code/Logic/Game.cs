﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System;

public class Game : ThreadedJob
{
    //static Texture2D mapImage;
    static MyTexture map;
    public static GameObject mapObject;
    internal static GameObject r3dTextPrefab;

    public static Country Player;

    static bool haveToRunSimulation;
    static bool haveToStepSimulation;
    internal static int howMuchPausedWindowsOpen = 0;

    public static System.Random Random = new System.Random();

    public static Province selectedProvince;
    public static List<PopUnit> popsToShowInPopulationPanel = new List<PopUnit>();
    public static List<Factory> factoriesToShowInProductionPanel;

    internal static List<BattleResult> allBattles = new List<BattleResult>();
    internal readonly static Stack<Message> MessageQueue = new Stack<Message>();
    public readonly static Market market = new Market();

    internal static StringBuilder threadDangerSB = new StringBuilder();

    public static DateTime date = new DateTime(0);
    internal static bool devMode = false;
    private static int mapMode;
    private static bool surrended = true;
    internal static Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, selectedProvinceBorderMaterial;
    private static Rect mapBorders;

    internal static List<Province> seaProvinces;
    static VoxelGrid grid;

    public Game()
    {
        //loadImages();
        generateMapImage();
    }
    public void initialize()
    {
        mapBorders = new Rect(0f, 0f, map.getWidth() * Options.cellMultiplier, map.getHeight() * Options.cellMultiplier);

        makeProducts();
        market.initialize();
        makeFactoryTypes();
        makePopTypes();

        updateStatus("Reading provinces..");
        Province.preReadProvinces(Game.map, this);
        seaProvinces = getSeaProvinces();
        deleteSomeProvinces();
        updateStatus("Making grid..");
        grid = new VoxelGrid(map.getWidth(), map.getHeight(), Options.cellMultiplier * map.getWidth(), map, Game.seaProvinces, this);

        updateStatus("Making countries..");
        Country.makeCountries(this);

        updateStatus("Making population..");
        CreateRandomPopulation();

        setStartResources();
        makeHelloMessage();
        updateStatus("Finishing generation..");
    }
    public static void setUnityAPI()
    {
        // Assigns a material named "Assets/Resources/..." to the object.
        defaultCountryBorderMaterial = Resources.Load("materials/CountryBorder", typeof(Material)) as Material;
        defaultProvinceBorderMaterial = Resources.Load("materials/ProvinceBorder", typeof(Material)) as Material;
        selectedProvinceBorderMaterial = Resources.Load("materials/SelectedProvinceBorder", typeof(Material)) as Material;
        r3dTextPrefab = (GameObject)Resources.Load("prefabs/3dProvinceNameText", typeof(GameObject));

        mapObject = GameObject.Find("MapObject");
        Province.generateUnityData(grid);
        Country.setUnityAPI();
        seaProvinces = null;
        grid = null;
        map = null;
    }
    public Rect getMapBorders()
    {
        return mapBorders;
    }
    static List<Province> getSeaProvinces()
    {
        List<Province> res = new List<Province>();
        Province seaProvince;
        for (int x = 0; x < map.getWidth(); x++)
        {
            seaProvince = Province.find(map.GetPixel(x, 0));
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);
            seaProvince = Province.find(map.GetPixel(x, map.getHeight() - 1));
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);
        }
        for (int y = 0; y < map.getHeight(); y++)
        {
            seaProvince = Province.find(map.GetPixel(0, y));
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);
            seaProvince = Province.find(map.GetPixel(map.getWidth() - 1, y));
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);
        }

        seaProvince = Province.find(map.getRandomPixel());
        if (!res.Contains(seaProvince))
            res.Add(seaProvince);

        if (Game.Random.Next(3) == 1)
        {
            seaProvince = Province.find(map.getRandomPixel());
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);
            if (Game.Random.Next(20) == 1)
            {
                seaProvince = Province.find(map.getRandomPixel());
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
            }
        }
        return res;
    }
    internal static void takePlayerControlOfThatCountry(Country country)
    {
        if (country != Country.NullCountry)
        {
            surrended = false;
            Player = country;
            MainCamera.refreshAllActive();
        }
    }

    public static void givePlayerControlToAI()
    {
        surrended = true;
    }
    static private void deleteSomeProvinces()
    {
        //Province.allProvinces.FindAndDo(x => blockedProvinces.Contains(x.getColorID()), x => x.removeProvince());
        foreach (var item in Province.allProvinces.ToArray())
            if (seaProvinces.Contains(item))
            {
                Province.allProvinces.Remove(item);
                //item.removeProvince();
            }
        int howMuchLakes = Province.allProvinces.Count / Options.ProvinceLakeShance + Game.Random.Next(3);
        for (int i = 0; i < howMuchLakes; i++)
            Province.allProvinces.Remove(Province.allProvinces.PickRandom());

    }

    //static void removeProvince(int x, int y)
    //{
    //    var toremove = Province.findProvince(map.GetPixel(x, y));
    //    if (Province.allProvinces.Contains(toremove))
    //    {
    //        toremove.removeProvince();
    //        //UnityEngine.Object.Destroy(toremove.rootGameObject);

    //        //Province.allProvinces.Remove(toremove);
    //    }
    //}

    static private void setStartResources()
    {
        //Country.allCountries[0] is null country
        Country.allCountries[1].getCapital().setResource(Product.Wood);

        //Country.allCountries[0].getCapital().setResource(Product.Wood;
        Country.allCountries[2].getCapital().setResource(Product.Fruit);
        Country.allCountries[3].getCapital().setResource(Product.Gold);
        Country.allCountries[4].getCapital().setResource(Product.Wool);
        Country.allCountries[5].getCapital().setResource(Product.Stone);
        Country.allCountries[6].getCapital().setResource(Product.MetallOre);
    }

    static private void makePopTypes()
    {
        //new PopType(PopType.PopTypes.TribeMen, new Storage(Product.findByName("Food"), 1.5f), "Tribesmen");
        new PopType(PopType.PopTypes.Tribemen, new Storage(Product.findByName("Food"), 1.0f), "Tribesmen", 2f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }));

        new PopType(PopType.PopTypes.Aristocrats, null, "Aristocrats", 4f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }));
        new PopType(PopType.PopTypes.Capitalists, null, "Capitalists", 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }));
        new PopType(PopType.PopTypes.Farmers, new Storage(Product.findByName("Food"), 2.0f), "Farmers", 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }));
        new PopType(PopType.PopTypes.Workers, null, "Workers", 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }));

        //new PopType(PopType.PopTypes.Artisans, null, "Artisans");
        //new PopType(PopType.PopTypes.Soldiers, null, "Soldiers");
    }

    internal static int getMapMode()
    {
        return mapMode;
    }
    public static Color getProvinceColorAccordingToMapMode(Province province)
    {
        switch (mapMode)
        {
            case 0: //political mode                
                return province.getColor();
            case 1: //culture mode
                return Country.allCountries.Find(x => x.getCulture() == province.getMajorCulture()).getColor();
            case 2: //cores mode
                if (Game.selectedProvince == null)
                {
                    if (province.isCoreFor(province.getCountry()))
                        return province.getCountry().getColor();
                    else
                    {
                        var c = province.getRandomCore();
                        if (c == null)
                            return Color.yellow;
                        else
                            return c.getColor();
                    }
                }
                else
                {
                    if (province.isCoreFor(Game.selectedProvince.getCountry()))
                        return Game.selectedProvince.getCountry().getColor();
                    else
                    {
                        if (province.isCoreFor(province.getCountry()))
                            return province.getCountry().getColor();
                        else
                        {
                            var so = province.getRandomCore(x => x.isExist());
                            if (so != null)
                                return so.getColor();
                            else
                            {
                                var c = province.getRandomCore();
                                if (c == null)
                                    return Color.yellow;
                                else
                                    return c.getColor();
                            }
                        }
                    }
                }



            default:
                return default(Color);
        }
    }
    public static void redrawMapAccordingToMapMode(int newMapMode)
    {
        mapMode = newMapMode;
        foreach (var item in Province.allProvinces)
            if (item != Game.selectedProvince)
            {
                item.updateColor(getProvinceColorAccordingToMapMode(item));
            }
    }

    internal static void continueSimulation()
    {
        haveToRunSimulation = true;
    }

    internal static bool isRunningSimulation()
    {
        return haveToRunSimulation || haveToStepSimulation;
    }
    internal static void pauseSimulation()
    {
        haveToRunSimulation = false;
    }
    internal static void makeOneStepSimulation()
    {
        haveToStepSimulation = true;
    }

    static void makeFactoryTypes()
    {
        new FactoryType("Forestry", new Storage(Product.Wood, 2f), null, false);
        new FactoryType("Gold pit", new Storage(Product.Gold, 2f), null, true);
        new FactoryType("Metal pit", new Storage(Product.MetallOre, 2f), null, true);
        new FactoryType("Sheepfold", new Storage(Product.Wool, 2f), null, false);
        new FactoryType("Quarry", new Storage(Product.Stone, 2f), null, true);
        new FactoryType("Orchard", new Storage(Product.Fruit, 2f), null, false);

        PrimitiveStorageSet resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        new FactoryType("Furniture factory", new Storage(Product.Furniture, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 1f));
        new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.MetallOre, 2f));
        new FactoryType("Metal smelter", new Storage(Product.Metal, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wool, 1f));
        new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.Stone, 1f));
        new FactoryType("Cement factory", new Storage(Product.Cement, 3f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Fruit, 0.3333f));
        new FactoryType("Winery", new Storage(Product.Wine, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Smithery", new Storage(Product.ColdArms, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Stone, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Ammunition factory", new Storage(Product.Ammunition, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Firearms factory", new Storage(Product.Firearms, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Artillery factory", new Storage(Product.Artillery, 4f), resourceInput, false);


        new FactoryType("Oil rig", new Storage(Product.Oil, 2f), null, true);
        new FactoryType("Rubber plantation", new Storage(Product.Rubber, 1f), null, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Oil, 1f));
        new FactoryType("Oil refinery", new Storage(Product.Fuel, 2f), resourceInput, false);


        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Machinery factory", new Storage(Product.Machinery, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Rubber, 1f));
        new FactoryType("Car factory", new Storage(Product.Cars, 6f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Artillery, 1f));
        new FactoryType("Tank factory", new Storage(Product.Tanks, 6f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Machinery, 1f));
        new FactoryType("Airplane factory", new Storage(Product.Airplanes, 6f), resourceInput, false);
    }

    void makeProducts()
    {
        updateStatus("Making products..");
        new Product("Food", false, 0.4f);
        new Product("Wood", true, 2.7f);
        new Product("Lumber", false, 8f);
        new Product("Gold", true, 4f);
        new Product("Metal ore", true, 3f);
        new Product("Metal", false, 6f);
        new Product("Wool", true, 1f);
        new Product("Clothes", false, 3f);
        new Product("Furniture", false, 7f);
        new Product("Stone", true, 1f);
        new Product("Cement", false, 2f);
        new Product("Fruit", true, 1f);
        new Product("Wine", false, 3f);
        new Product("Cold arms", false, 13f);
        new Product("Ammunition", false, 13f);
        new Product("Firearms", false, 13f);
        new Product("Artillery", false, 13f);

        new Product("Oil", true, 10f);
        new Product("Fuel", false, 15f);
        new Product("Machinery", false, 8f);
        new Product("Cars", false, 15f);
        new Product("Tanks", false, 20f);
        new Product("Airplanes", false, 20f);
        new Product("Rubber", true, 10f);
    }
    internal static Value getAllMoneyInWorld()
    {
        Value allMoney = new Value(0f);
        foreach (Country country in Country.allCountries)
        {
            allMoney.add(country.cash);
            allMoney.add(country.bank.getReservs());
            foreach (Province pr in country.ownedProvinces)
            {
                foreach (var factory in pr.getProducers())
                    allMoney.add(factory.cash);
            }
        }
        allMoney.add(Game.market.cash);
        return allMoney;
    }
    static void CreateRandomPopulation()
    {
        int chanceForA = 85;

        foreach (Province province in Province.allProvinces)
        {
            if (province.getCountry() == Country.NullCountry)
            {
                Tribemen f = new Tribemen(PopUnit.getRandomPopulationAmount(500, 1000), PopType.tribeMen, province.getCountry().getCulture(), province);
                province.allPopUnits.Add(f);
            }
            else
            {
                PopUnit pop;
                if (!Game.devMode)
                    pop = new Tribemen(PopUnit.getRandomPopulationAmount(1800, 2000), PopType.tribeMen, province.getCountry().getCulture(), province);
                else
                    pop = new Tribemen(2000, PopType.tribeMen, province.getCountry().getCulture(), province);
                province.allPopUnits.Add(pop);

                if (province.getCountry() == Game.Player)
                {
                    //pop = new Tribesmen(20900, PopType.tribeMen, province.getOwner().culture, province);
                    //province.allPopUnits.Add(pop);
                }
                if (!Game.devMode)
                    pop = new Aristocrats(PopUnit.getRandomPopulationAmount(800, 1000), PopType.aristocrats, province.getCountry().getCulture(), province);
                else
                    pop = new Aristocrats(100, PopType.aristocrats, province.getCountry().getCulture(), province);

                pop.cash.set(9000);
                pop.storageNow.add(60f);
                province.allPopUnits.Add(pop);
                if (!Game.devMode)
                {
                    pop = new Capitalists(PopUnit.getRandomPopulationAmount(500, 800), PopType.capitalists, province.getCountry().getCulture(), province);
                    pop.cash.set(9000);
                    province.allPopUnits.Add(pop);

                    pop = new Farmers(PopUnit.getRandomPopulationAmount(5000, 6000), PopType.farmers, province.getCountry().getCulture(), province);
                    pop.cash.set(20);
                    province.allPopUnits.Add(pop);

                }
                //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));

                //if (Procent.GetChance(chanceForA))
                //    province.allPopUnits.Add(
                //    new PopUnit(PopUnit.getRandomPopulationAmount(), PopType.aristocrats, culture, province)
                //    );

            }

        }
    }
    /// <summary>
    /// Makes polygonal Stripe and stores it vertices[] and trianglesList[]
    /// </summary>    
    //static void makePolygonalStripe(float x, float y, float x2, float y2, int xpos1, int ypos1, int xpos2, int ypos2)
    ////void makePolygonalStripe(float x, float y, float x2, float y2)
    //{
    //    //float x = xpos1 * Options.cellMuliplier;
    //    //float y = ypos1 * Options.cellMuliplier;
    //    //float x2 = xpos2 * Options.cellMuliplier;
    //    //float y2 = ypos1 * Options.cellMuliplier;

    //    //if (mapImage.isLeftTopCorner(xpos1, ypos1))
    //    //    x -= Options.cellMuliplier;
    //    //if (mapImage.isRightTopCorner(xpos1, ypos1))
    //    //    x += Options.cellMuliplier;

    //    //if (mapImage.isLeftBottomCorner(xpos1, ypos1))
    //    //    vertices.Add(new Vector3(x + Options.cellMuliplier, y, 0));
    //    //else
    //    vertices.Add(new Vector3(x, y, 0));

    //    //if (mapImage.isRightBottomCorner(xpos2 - 1, ypos1))
    //    //    vertices.Add(new Vector3(x2 - Options.cellMuliplier, y, 0));
    //    //else
    //    vertices.Add(new Vector3(x2, y, 0));

    //    //if (mapImage.isRightTopCorner(xpos2 - 1, ypos1))
    //    //    vertices.Add(new Vector3(x2 - Options.cellMuliplier, y2, 0));
    //    //else
    //    vertices.Add(new Vector3(x2, y2, 0));

    //    //if (mapImage.isLeftTopCorner(xpos1, ypos1))
    //    //    vertices.Add(new Vector3(x + Options.cellMuliplier, y2, 0));
    //    //else
    //    vertices.Add(new Vector3(x, y2, 0));

    //    trianglesList.Add(0 + triangleCounter);
    //    trianglesList.Add(2 + triangleCounter);
    //    trianglesList.Add(1 + triangleCounter);

    //    trianglesList.Add(2 + triangleCounter);
    //    trianglesList.Add(0 + triangleCounter);
    //    trianglesList.Add(3 + triangleCounter);
    //    triangleCounter += 4;
    //}
    //static void checkCoordinateForNeighbors(Province province, int x1, int y1, int x2, int y2)
    //{
    //    if (mapImage.coordinatesExist(x2, y2) && mapImage.isDifferentColor(x1, y1, x2, y2))
    //    {
    //        Province found;
    //        found = Province.findProvince(mapImage.GetPixel(x2, y2));
    //        if (found != null) // for remove edge provinces
    //            province.addNeigbor(found);
    //    }
    //}

    internal static bool isPlayerSurrended()
    {
        return surrended;
    }

    //static void findNeighborprovinces()
    //{
    //    int f = 0;
    //    foreach (var province in Province.allProvinces)
    //    {
    //        f++;
    //        for (int j = 0; j < mapImage.height; j++)
    //            for (int i = 0; i < mapImage.width; i++)
    //            {
    //                Color currentColor = mapImage.GetPixel(i, j);
    //                if (currentColor == province.getColorID())
    //                {
    //                    checkCoordinateForNeighbors(province, i, j, i + 1, j);
    //                    checkCoordinateForNeighbors(province, i, j, i - 1, j);
    //                    checkCoordinateForNeighbors(province, i, j, i, j + 1);
    //                    checkCoordinateForNeighbors(province, i, j, i, j - 1);
    //                }
    //            }
    //    }
    //}
    static void generateMapImage()
    {

        //Texture2D mapImage = new Texture2D(100, 100);
        Texture2D mapImage = new Texture2D(100 + Random.Next(100), 70 + Random.Next(100));
        //Texture2D mapImage = new Texture2D(300, 300);
        Color emptySpaceColor = Color.black;//.setAlphaToZero();
        mapImage.setColor(emptySpaceColor);
        int amountOfProvince;
        if (Game.devMode)
            amountOfProvince = 7;
        else
            amountOfProvince = 12 + Game.Random.Next(8);
        amountOfProvince = 40 + Game.Random.Next(20);
        amountOfProvince = 160 + Game.Random.Next(20);
        amountOfProvince = mapImage.width * mapImage.height / 140 + Game.Random.Next(5);
        //amountOfProvince = 400 + Game.Random.Next(100);
        for (int i = 0; i < amountOfProvince; i++)
            mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), ColorExtensions.getRandomColor());

        int emptyPixels = int.MaxValue;
        Color currentColor = mapImage.GetPixel(0, 0);
        int emergencyExit = 0;
        while (emptyPixels != 0 && emergencyExit < 100)
        {
            emergencyExit++;
            emptyPixels = 0;
            for (int j = 0; j < mapImage.height; j++) // circle by province        
                for (int i = 0; i < mapImage.width; i++)
                {
                    currentColor = mapImage.GetPixel(i, j);
                    if (currentColor == emptySpaceColor)
                        emptyPixels++;
                    else if (currentColor.a == 1f)
                    {
                        mapImage.drawRandomSpot(i, j, currentColor);
                    }
                }
            mapImage.setAlphaToMax();
        }
        mapImage.Apply();
        map = new MyTexture(mapImage);
        Texture2D.Destroy(mapImage);
    }
    static Mesh getMeshID(Color color)
    {
        foreach (var all in Province.allProvinces)
            if (color == all.getColorID())
                return all.landMesh;
        return null;
    }
    //static Mesh getMeshID(int xpos, int ypos)
    //{
    //    Color color = mapImage.GetPixel(xpos, ypos);
    //    foreach (var all in Province.allProvinces)
    //        if (color == all.getColorID())
    //            return all.landMesh;
    //    return null;
    //}
    static private bool movePointRight(Mesh mesh, int xpos, int ypos, int xMove, int yMove)
    {
        Vector3[] editingVertices = mesh.vertices;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 sr = new Vector3(xpos * Options.cellMultiplier, (ypos + 1) * Options.cellMultiplier, 0f);
            //Vector3 sr2 = new Vector3(xpos * Options.cellMuliplier, (ypos ) * Options.cellMuliplier, 0f);
            //if (editingVertices[i] == sr || editingVertices[i] == sr2)
            if (editingVertices[i] == sr)
            {
                editingVertices[i].x += Options.cellMultiplier * xMove / 2;
                mesh.vertices = editingVertices;
                mesh.RecalculateBounds();
                return true;
            }
        }
        return false;
    }
    //static private bool movePointLeft(Mesh mesh, int xpos, int ypos, int xMove, int yMove)
    //{
    //    Vector3[] mesh1Vertices = mesh.vertices;

    //    Mesh mesh2 = getMeshID(xpos + 1, ypos);
    //    Vector3[] mesh2Vertices = mesh2.vertices;
    //    for (int i = 0; i < mesh.vertices.Length; i++)
    //    {
    //        Vector3 sr = new Vector3(xpos * Options.cellMultiplier, (ypos + 2) * Options.cellMultiplier, 0f);
    //        //Vector3 sr2 = new Vector3(xpos * Options.cellMuliplier, (ypos ) * Options.cellMuliplier, 0f);
    //        //if (editingVertices[i] == sr || editingVertices[i] == sr2)
    //        if (mesh1Vertices[i] == sr)
    //        {
    //            mesh1Vertices[i].x += Options.cellMultiplier * xMove / 2;
    //            mesh.vertices = mesh1Vertices;
    //            mesh.RecalculateBounds();
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //static void roundMesh()
    //{
    //    for (int ypos = 0; ypos < mapImage.height; ypos++)
    //    {
    //        for (int xpos = 0; xpos < mapImage.width; xpos++)
    //        {
    //            if (mapImage.isRightTopCorner(xpos, ypos))
    //            {
    //                movePointLeft(getMeshID(xpos, ypos), xpos + 1, ypos - 1, -1, 0);
    //                movePointLeft(getMeshID(xpos + 1, ypos), xpos + 1, ypos - 1, -1, 0);
    //            }
    //            else
    //            if (mapImage.isLeftTopCorner(xpos, ypos))
    //            {
    //                movePointRight(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos, ypos, 1, 0);
    //                movePointRight(getMeshID(mapImage.GetPixel(xpos - 1, ypos)), xpos, ypos, 1, 0);
    //            }
    //            else
    //            if (mapImage.isLeftBottomCorner(xpos, ypos))
    //            {
    //                movePointRight(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos, ypos - 1, 1, 0);
    //                movePointRight(getMeshID(mapImage.GetPixel(xpos - 1, ypos)), xpos, ypos - 1, 1, 0);
    //            }
    //            else
    //            if (mapImage.isRightBottomCorner(xpos, ypos))
    //            {
    //                movePointLeft(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos + 1, ypos - 2, -1, 0);
    //                movePointLeft(getMeshID(mapImage.GetPixel(xpos + 1, ypos)), xpos + 1, ypos - 2, -1, 0);
    //            }
    //        }
    //    }
    //}

    //static void makeProvinces()
    //{
    //    ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();

    //    VoxelGrid grid = new VoxelGrid(mapImage.width, Options.cellMultiplier * 100, mapImage, Game.blockedProvinces);
    //    Color currentProvinceColor = mapImage.GetPixel(0, 0);
    //    int provinceCounter = 0;
    //    for (int j = 0; j < mapImage.height; j++) // circle by province        
    //        for (int i = 0; i < mapImage.width; i++)
    //        {
    //            //var newProvince = Province.findProvince(currentProvinceColor);
    //            //if (currentProvinceColor != mapImage.GetPixel(i, j) && newProvince != null)
    //            if (currentProvinceColor != mapImage.GetPixel(i, j) && !Province.isProvinceCreated(currentProvinceColor))
    //            {


    //                makeProvince(provinceCounter, currentProvinceColor, nameGenerator.generateProvinceName(),
    //                    grid.getMesh(currentProvinceColor), grid.getBorders());
    //                //Province newProvince =
    //                //var np = new Province(nameGenerator.generateProvinceName(), provinceCounter, currentProvinceColor, grid.getMesh(), Product.getRandomResource(false), mapObject);
    //                //Province.allProvinces.Add(np);
    //                provinceCounter++;
    //            }
    //            currentProvinceColor = mapImage.GetPixel(i, j);
    //        }
    //}


    static bool FindProvinceCenters()
    {
        //Vector3 accu = new Vector3(0, 0, 0);
        //foreach (Province pro in Province.allProvinces)
        //{
        //    accu.Set(0, 0, 0);
        //    foreach (var c in pro.mesh.vertices)
        //        accu += c;
        //    accu = accu / pro.mesh.vertices.Length;
        //    pro.centre = accu;
        //}
        return true;

        //short[,] bordersMarkers = new short[mapImage.width, mapImage.height];

        //int foundedProvinces = 0;
        //Color currentColor;
        //short borderDeepLevel = 0;
        //short alphaChangeForLevel = 1;
        //float defaultApha = 1f;
        //int placedMarkers = 456;//random number
        ////while (Province.allProvinces.Count != foundedProvinces)

        //foreach (Province pro in Province.allProvinces)
        //{
        //    borderDeepLevel = -1;
        //    placedMarkers = int.MaxValue;
        //    int emergencyExit = 200;
        //    while (placedMarkers != 0)
        //    {
        //        emergencyExit--;
        //        if (emergencyExit == 0)
        //            break;
        //        placedMarkers = 0;
        //        borderDeepLevel += alphaChangeForLevel;
        //        for (int j = 0; j < mapImage.height; j++) // cicle by province        
        //            for (int i = 0; i < mapImage.width; i++)
        //            {

        //                currentColor = mapImage.GetPixel(i, j);
        //                //if (UtilsMy.isSameColorsWithoutAlpha(currentColor, pro.colorID) && currentColor.a == defaultApha && isThereOtherColorsIn4Negbors(i, j))
        //                // && bordersMarkers[i, j] == borderDeepLevel-1
        //                if (currentColor == pro.colorID  && isThereOtherColorsIn4Negbors(i, j, bordersMarkers, (short)(borderDeepLevel)))
        //                {
        //                    //currentColor.a = borderDeepLevel;
        //                    //mapImage.SetPixel(i, j, currentColor);
        //                    borderDeepLevel ++;
        //                    bordersMarkers[i, j] = borderDeepLevel;
        //                    borderDeepLevel--;
        //                    placedMarkers++;

        //                }
        //            }

        //        //if (placedMarkers == 0) 
        //        //    ;
        //    }
        //    //// found centers!
        //    bool wroteResult = false;
        //    //
        //    for (int j = 0; j < mapImage.height && !wroteResult; j++) // cicle by province, looking where is my centre        
        //        //&& !wroteResult
        //        for (int i = 0; i < mapImage.width && !wroteResult; i++)
        //        {
        //            currentColor = mapImage.GetPixel(i, j);
        //            //if (currentColor.a == borderDeepLevel)
        //            if (currentColor == pro.colorID && bordersMarkers[i, j] == borderDeepLevel - 1)
        //            {
        //                pro.centre = new Vector3((i + 0.5f) * Options.cellMuliplier, (j + 0.5f) * Options.cellMuliplier, 0f);
        //                wroteResult = true;
        //            }
        //        }
        //}
        //return false;
    }
    //static bool isThereOtherColorsIn4Negbors(int x, int y, short[,] bordersMarkers, short borderDeepLevel)
    //{
    //    Color color = mapImage.GetPixel(x, y);
    //    //if (x == 0)
    //    //    return true;
    //    //else
    //    //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x - 1, y), color)) return true;

    //    //if (x == mapImage.width - 1)
    //    //    return true;
    //    //else
    //    //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x + 1, y), color)) return true;
    //    //if (y == 0)
    //    //    return true;
    //    //else
    //    //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x, y - 1), color)) return true;
    //    //if (y == mapImage.height - 1)
    //    //    return true;
    //    //if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x, y + 1), color)) return true;
    //    //return false;
    //    if (x == 0)
    //        return true;
    //    else
    //      if (mapImage.GetPixel(x - 1, y) != color || bordersMarkers[x - 1, y] != borderDeepLevel) return true;

    //    if (x == mapImage.width - 1)
    //        return true;
    //    else
    //        if (mapImage.GetPixel(x + 1, y) != color || bordersMarkers[x + 1, y] != borderDeepLevel) return true;
    //    if (y == 0)
    //        return true;
    //    else
    //        if (mapImage.GetPixel(x, y - 1) != color || bordersMarkers[x, y - 1] != borderDeepLevel) return true;
    //    if (y == mapImage.height - 1)
    //        return true;
    //    if (mapImage.GetPixel(x, y + 1) != color || bordersMarkers[x, y + 1] != borderDeepLevel) return true;
    //    return false;
    //}
    public static void PrepareForNewTick()
    {
        Game.market.sentToMarket.setZero();
        foreach (Country country in Country.getExisting())
        // if (country != Country.NullCountry)
        {
            //country.wallet.moneyIncomethisTurn.set(0);
            country.storageSet.setStatisticToZero();
            country.setStatisticToZero();
            country.staff.setStatisticToZero();
            foreach (Province province in country.ownedProvinces)
            {
                province.BalanceEmployableWorkForce();
                {
                    foreach (var item in province.getProducers())
                        item.setStatisticToZero();

                    //    foreach (PopUnit pop in province.allPopUnits)
                    //{

                    //    pop.setStatisticToZero();

                    //}
                    //foreach (Factory factory in province.allFactories)
                    //{   

                    //    factory.setStatisticToZero();                        
                    //}
                }
            }
        }
    }
    static void makeHelloMessage()
    {
        new Message("Tutorial", "Hi, this is VERY early demo of game-like economy simulator" +
            "\n\nCurrently there is: "
            + "\n\npopulation agents \nbasic trade & production \nbasic warfare \nbasic inventions \nbasic reforms (population can vote for reforms)"
            + " \npopulation demotion\\promotion to other classes \nmigration\\immigration\nassimilation"
            + "\n\nYou play as " + Game.Player.getName() + " country yet there is no much gameplay for now. You can try to growth economy or conquer the world."
            + "\n\nTry arrows or WASD for scrolling map and mouse wheel for scale"
            + "\nEnter key to close top window, space - to pause\\unpause"
            , "Ok");
        ;

    }
    static void loadImages()
    {
        Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;        
        //RawImage ri = GameObject.Find("RawImage").GetComponent<RawImage>();
        //ri.texture = mapImage;
        map = new MyTexture(mapImage);
    }
    private static void calcBattles()
    {
        foreach (Country attackerCountry in Country.getExisting())
        {
            foreach (var attackerArmy in attackerCountry.staff.getAttackingArmies())
            {
                var result = attackerArmy.attack(attackerArmy.getDestination());
                if (result.isAttackerWon())
                {
                    attackerArmy.getDestination().secedeTo(attackerCountry);
                }
                if (result.getAttacker() == Game.Player || result.getDefender() == Game.Player)
                    result.createMessage();
                attackerArmy.sendTo(null); // go home
            }
            attackerCountry.staff.consolidateArmies();
        }
    }
    internal static void stepSimulation()
    {
        if (Game.haveToStepSimulation)
            Game.haveToStepSimulation = false;
        //date = date.AddDays(23);
        date = date.AddYears(1);
        // strongly before PrepareForNewTick
        Game.market.simulatePriceChangeBasingOnLastTurnDate();

        Game.calcBattles(); // should be before PrepareForNewTick cause PrepareForNewTick hires dead workers on factories
        PrepareForNewTick();

        // big PRODUCE circle
        foreach (Country country in Country.getExisting())
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                //Now factories time!               
                foreach (Factory fact in province.allFactories)
                {
                    fact.produce();
                    fact.payTaxes(); // empty for now
                    fact.paySalary(); // workers get gold or food here                   
                }
                foreach (PopUnit pop in province.allPopUnits)
                //That placed here to avoid issues with Aristocrats and clerics
                //Otherwise Aristocrats starts to consume BEFORE they get all what they should
                {
                    if (pop.popType.basicProduction != null)// only Farmers and Tribesmen
                        pop.produce();
                    pop.takeUnemploymentSubsidies();
                }
            }
        //Game.market.ForceDSBRecalculation();
        // big CONCUME circle
        foreach (Country country in Country.getExisting())
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)            
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.buyNeeds();
                }

                foreach (PopUnit pop in province.allPopUnits)
                {
                    if (country.serfdom.status == Serfdom.Allowed || country.serfdom.status == Serfdom.Brutal)
                        if (pop.ShouldPayAristocratTax())
                            pop.PayTaxToAllAristocrats();
                }
                foreach (PopUnit pop in province.allPopUnits)
                {
                    pop.buyNeeds();
                }
            }
        // big AFTER all circle
        foreach (Country country in Country.getExisting())
        {
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.getMoneyFromMarket();
                    factory.changeSalary();
                    factory.PayDividend();
                }
                province.allFactories.RemoveAll(item => item.isToRemove());
                foreach (PopUnit pop in province.allPopUnits)
                {
                    if (pop.popType == PopType.aristocrats || pop.popType == PopType.capitalists || (pop.popType == PopType.farmers && Economy.isMarket.checkIftrue(province.getCountry())))
                        pop.getMoneyFromMarket();

                    //because income come only after consuming, and only after FULL consumption
                    if (pop.canTrade() && pop.hasToPayGovernmentTaxes())
                        // POps who can't trade will pay tax BEFORE consumption, not after
                        // Otherwise pops who can't trade avoid tax
                        pop.payTaxes();

                    pop.calcLoyalty();
                    pop.calcGrowth();
                    pop.calcPromotions();
                    pop.calcDemotions();
                    pop.calcMigrations();
                    pop.calcImmigrations();
                    pop.calcAssimilations();

                    pop.invest();
                    if (Game.Random.Next(20) == 1)
                        pop.putExtraMoneyInBank();
                }
                //if (Game.random.Next(3) == 0)
                //    province.consolidatePops();                
                foreach (PopUnit pop in PopUnit.PopListToAddToGeneralList)
                {
                    PopUnit targetToMerge = pop.province.getSimilarPopUnit(pop);
                    if (targetToMerge == null)
                        pop.province.allPopUnits.Add(pop);
                    else
                        targetToMerge.mergeIn(pop);
                }
                province.allPopUnits.RemoveAll(x => x.getPopulation() == 0);
                PopUnit.PopListToAddToGeneralList.Clear();
                province.think();
            }
            country.think();
        }
    }

    protected override void ThreadFunction()
    {
        initialize();
    }
}
