﻿using UnityEngine;
using System.Linq;

using IceCream;

public class IceCreamModule : MonoBehaviour {

    // Bomb and Module
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;

    // Buttons
    public KMSelectable LeftButton;
    public KMSelectable RightButton;
    public KMSelectable SellButton;

    // Labels
    public TextMesh CustomerLabel;
    public TextMesh FlavourLabel;

    // Indicators
    public Renderer[] IndicatorLights;
    public Material[] IndicatorMaterials;

    // Module Identification (for logging)
    int moduleId;
    static int moduleIdCounter = 1;

    // Flavour Definitions
    class Flavour {
        public string name;
        public Allergies[] allergies;

        public Flavour(string name, params Allergies[] alg) {
            this.name = name;
            this.allergies = alg;
        }
    }
    Flavour[] Flavours = new Flavour[] {
        new Flavour("Tutti Frutti", Allergies.Fruit, Allergies.Raspberry, Allergies.Cherry, Allergies.Strawberry), // 0
        new Flavour("Rocky Road", Allergies.Chocolate, Allergies.Nuts, Allergies.Marshmallows), // 1
        new Flavour("Raspberry Ripple", Allergies.Raspberry, Allergies.Fruit), // 2
        new Flavour("Double Chocolate", Allergies.Chocolate), // 3
        new Flavour("Double Strawberry", Allergies.Strawberry, Allergies.Fruit), // 4
        new Flavour("Cookies and Cream", Allergies.Cookies), // 5
        new Flavour("Neapolitan", Allergies.Strawberry, Allergies.Chocolate, Allergies.Fruit), // 6
        new Flavour("Mint Chocolate Chip", Allergies.Chocolate, Allergies.Mint), // 7
        new Flavour("The Classic", Allergies.Chocolate, Allergies.Cherry, Allergies.Fruit), // 8
        new Flavour("Plain") // 9
    };
    int[][] FlavourLists = new int[][] {
        new int[] {5, 6, 0, 8, 1, 3, 7, 4, 2, 9}, // More LIT than UNLIT indicators.
        new int[] {3, 7, 6, 1, 0, 4, 5, 2, 8, 9}, // Empty port plate.
        new int[] {6, 0, 5, 2, 4, 7, 3, 8, 1, 9}, // 3 or more batteries.
        new int[] {4, 5, 1, 8, 6, 3, 0, 2, 7, 9}  // Otherwise
    };

    // Allergy Definitions
    enum Allergies {
        Chocolate,
        Strawberry,
        Raspberry,
        Nuts,
        Cookies,
        Mint,
        Fruit,
        Cherry,
        Marshmallows
    }

    // Customer Definitions
    string[] CustomerNames = new string[] { "Mike", "Tim", "Tom", "Dave", "Adam", "Cheryl", "Sean", "Ashley", "Jessica", "Taylor", "Simon", "Sally", "Jade", "Sam", "Gary", "Victor", "George", "Jacob", "Pat", "Bob" };
    int[,,] AllergyTable = new int[,,] {
        { {1, 5, 0}, {6, 8, 3}, {0, 7, 1}, {4, 3, 2}, {3, 6, 1} }, // Mike
        { {0, 8, 3}, {2, 1, 4}, {4, 3, 5}, {2, 6, 7}, {1, 4, 3} }, // Tim
        { {8, 4, 5}, {1, 6, 7}, {2, 5, 6}, {3, 7, 5}, {3, 6, 1} }, // Tom
        { {2, 6, 7}, {0, 1, 4}, {8, 2, 3}, {7, 8, 1}, {5, 7, 3} }, // Dave
        { {3, 4, 1}, {3, 6, 2}, {0, 2, 1}, {2, 4, 7}, {8, 5, 6} }, // Adam
        { {1, 6, 3}, {7, 5, 2}, {1, 4, 5}, {4, 2, 0}, {3, 7, 5} }, // Cheryl
        { {4, 6, 1}, {2, 3, 6}, {1, 5, 7}, {6, 8, 2}, {2, 7, 4} }, // Sean
        { {6, 2, 5}, {4, 1, 7}, {0, 8, 2}, {1, 2, 6}, {3, 6, 7} }, // Ashley
        { {4, 2, 6}, {1, 2, 3}, {0, 3, 4}, {6, 5, 0}, {4, 7, 8} }, // Jessica
        { {6, 3, 5}, {5, 1, 2}, {4, 2, 6}, {7, 1, 0}, {3, 7, 2} }, // Taylor
        { {0, 3, 5}, {1, 6, 4}, {5, 4, 8}, {2, 0, 7}, {7, 3, 6} }, // Simon
        { {4, 6, 3}, {1, 0, 2}, {6, 7, 4}, {2, 5, 8}, {0, 3, 1} }, // Sally
        { {3, 7, 1}, {0, 8, 2}, {7, 1, 3}, {6, 7, 8}, {4, 5, 1} }, // Jade
        { {2, 4, 1}, {7, 8, 0}, {3, 4, 6}, {1, 0, 3}, {6, 5, 2} }, // Sam
        { {1, 2, 5}, {6, 8, 0}, {3, 2, 1}, {7, 4, 5}, {1, 8, 4} }, // Gary
        { {0, 3, 1}, {2, 5, 7}, {3, 4, 6}, {6, 7, 1}, {5, 3, 0} }, // Victor
        { {8, 1, 2}, {6, 4, 8}, {0, 4, 3}, {1, 6, 4}, {3, 2, 5} }, // George
        { {7, 3, 2}, {1, 5, 6}, {5, 4, 7}, {3, 4, 0}, {6, 2, 1} }, // Jacob
        { {5, 6, 2}, {1, 3, 6}, {3, 4, 7}, {2, 0, 5}, {8, 1, 3} }, // Pat
        { {5, 6, 8}, {2, 1, 0}, {4, 8, 2}, {4, 2, 5}, {0, 5, 1} }  // Bob
    };

    // Input Tracking
    int currentFlavour = 0;
    int[][] flavourOptions;
    int currentStage = 0;
    int maxStages = 3;

    // Conditional Tallies
    bool hasEmptyPortPlate;
    int lastDigit;

    // Solutions
    int[] list = null;
    int[] solCustomerNames;
    int[] solution;

    void Start() {
        moduleId = moduleIdCounter++;

        LeftButton.OnInteract += delegate { HandlePress(-1); return false; };
        RightButton.OnInteract += delegate { HandlePress(1); return false; };
        SellButton.OnInteract += delegate { HandlePress(0); return false; };

        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate() {
        // Conditional tally lookup.
        foreach (object[] plate in BombInfo.GetPortPlates()) {
            if (plate.Length == 0) {
                hasEmptyPortPlate = true;
                break;
            }
        }
        foreach (int digit in BombInfo.GetSerialNumberNumbers()) {
            lastDigit = digit;
        }

        GenerateSolutions();
        UpdateDisplays();
    }

    void GenerateSolutions() {
        solCustomerNames = new int[maxStages];
        solution = new int[maxStages];
        flavourOptions = new int[maxStages][];

        // Choose correct flavour list if not done already.
        if (list == null) {
            if (BombInfo.GetOnIndicators().Count() > BombInfo.GetOffIndicators().Count()) { list = FlavourLists[0]; } 
            else if (hasEmptyPortPlate) { list = FlavourLists[1]; } 
            else if (BombInfo.GetBatteryCount() >= 3) { list = FlavourLists[2]; } 
            else { list = FlavourLists[3]; }
        }

        // Generate solution per stage.
        for (int i = 0; i < maxStages; i++) {
            solCustomerNames[i] = -1;
            int customerID = -1;
            int[] stageFlavours = new int[5];
            int flavourId = -1;

            // Create list of selectable flavours for the stage.
            for (int si = 0; si < 4; si++) {
                stageFlavours[si] = -1;
                while (System.Array.Exists(stageFlavours, x => x == flavourId))
                    flavourId = Random.Range(0, Flavours.Length - 2);
                stageFlavours[si] = flavourId;
            }
            stageFlavours[4] = 9;

            // Choose a customer for the stage.
            while (System.Array.Exists(solCustomerNames, x => x == customerID))
                customerID = Random.Range(0, CustomerNames.Length - 1);

            // Determine if the customer is allergic to any of the chosen flavours.
            bool[] bad = new bool[stageFlavours.Length];
            for (int j = 0; j < 3; j++) {
                int allergy = AllergyTable[customerID, lastDigit / 2, j];
                for (int k = 0; k < stageFlavours.Length; k++) {
                    if (Flavours[stageFlavours[k]].allergies.Contains((Allergies)allergy) && !bad[k]) {
                        bad[k] = true;
                    }
                }
            }

            // Determine which acceptable flavour comes first in the flavour list.
            int lowestNum = 9;
            int sol = 4;
            for (int l = 0; l < stageFlavours.Length; l++) {
                if (!bad[l]) {
                    if (System.Array.IndexOf(list, stageFlavours[l]) < lowestNum) {
                        lowestNum = System.Array.IndexOf(list, stageFlavours[l]);
                        sol = l;
                    }
                }
            }

            // Record solution.
            solution[i] = sol;
            solCustomerNames[i] = customerID;
            flavourOptions[i] = stageFlavours;

            Debug.LogFormat("[Ice Cream #{0}] Stage {1} Flavour Options: '{2}', '{3}', '{4}', '{5}', '{6}'", moduleId, i + 1, Flavours[flavourOptions[i][0]].name, Flavours[flavourOptions[i][1]].name, Flavours[flavourOptions[i][2]].name, Flavours[flavourOptions[i][3]].name, Flavours[flavourOptions[i][4]].name);
        }

        Debug.LogFormat("[Ice Cream #{0}] Solution: '{1}', '{2}', '{3}'", moduleId, Flavours[flavourOptions[0][solution[0]]].name, Flavours[flavourOptions[1][solution[1]]].name, Flavours[flavourOptions[2][solution[2]]].name);
        Debug.LogFormat("[Ice Cream #{0}] Customers: '{1}', '{2}', '{3}'", moduleId, CustomerNames[solCustomerNames[0]], CustomerNames[solCustomerNames[1]], CustomerNames[solCustomerNames[2]]);
    }

    void UpdateDisplays() {
        CustomerLabel.text = CustomerNames[solCustomerNames[currentStage]];
        FlavourLabel.text = Flavours[flavourOptions[currentStage][currentFlavour]].name;
    }

    void HandlePress(int button) {
        Audio.PlaySoundAtTransform("tick", this.transform);

        switch (button) {
            case -1:
                RotateFlavours(-1);
                LeftButton.AddInteractionPunch(0.1f);
                break;
            case 1:
                RotateFlavours(1);
                RightButton.AddInteractionPunch(0.1f);
                break;
            case 0:
                Submit();
                SellButton.AddInteractionPunch(0.1f);
                break;
            default:
                BombModule.HandleStrike();
                break;
        }
    }

    void RotateFlavours(int dir) {
        // Choose next flavour to display.
        currentFlavour += dir;
        if (currentFlavour < 0) currentFlavour += flavourOptions[currentStage].Length;
        else if (currentFlavour >= flavourOptions[currentStage].Length) currentFlavour -= flavourOptions[currentStage].Length;

        UpdateDisplays();
    }

    void Submit() {
        if ((int)(BombInfo.GetTime() / 60) % 2 == 0) { // Check if submitted on an even minute.
            if (currentStage < maxStages && currentFlavour == solution[currentStage]) {
                Debug.LogFormat("[Ice Cream #{0}] Flavour '{1}' for customer '{2}' submitted correctly.", moduleId, Flavours[flavourOptions[currentStage][currentFlavour]].name, CustomerNames[solCustomerNames[currentStage]]);
                currentStage++;
                if (currentStage >= maxStages) {
                    BombModule.HandlePass();
                } else {
                    UpdateDisplays();
                }
            } else {
                if (currentStage < maxStages) {
                    Debug.LogFormat("[Ice Cream #{0}] Flavour '{1}' for customer '{2}' submitted incorrectly.", moduleId, Flavours[flavourOptions[currentStage][currentFlavour]].name, CustomerNames[solCustomerNames[currentStage]]);
                    currentStage = 0;
                    BombModule.HandleStrike();
                    GenerateSolutions();
                    UpdateDisplays();
                }
            }
        } else {
            if (currentStage < maxStages) {
                Debug.LogFormat("[Ice Cream #{0}] Flavour '{1}' for customer '{2}' submitted while parlour is closed.", moduleId, Flavours[flavourOptions[currentStage][currentFlavour]].name, CustomerNames[solCustomerNames[currentStage]]);
                currentStage = 0;
                BombModule.HandleStrike();
                GenerateSolutions();
                UpdateDisplays();
            }
        }

        // Update stage indicator.
        if (currentStage > 0) { IndicatorLights[0].material = IndicatorMaterials[0]; } else { IndicatorLights[0].material = IndicatorMaterials[1]; }
        if (currentStage > 1) { IndicatorLights[1].material = IndicatorMaterials[0]; } else { IndicatorLights[1].material = IndicatorMaterials[1]; }
        if (currentStage > 2) { IndicatorLights[2].material = IndicatorMaterials[0]; } else { IndicatorLights[2].material = IndicatorMaterials[1]; }
    }
}