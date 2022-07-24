using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using static HSD_Utils;

public class UserInterfaceManager : MonoBehaviour
{
    TextMeshProUGUI coinCount;

    TextMeshProUGUI timerDisplay;

    GameObject activeHeroGroup;
    TextMeshProUGUI heroName;
    GameObject abilitiesGroup;
    Weapon currWeapon;
    TextMeshProUGUI primaryName, secondaryName;
    Image primaryBar, secondaryBar;
    Color cooldownBarGreen = Color.green;
    //Color cooldownBarGrey = Color.grey;
    Color cooldownBarGrey = new Color(3 / 8f, 3 / 8f, 3 / 8f);
    GameObject activeBarsGroup;
    GameObject activeHealthBar, activeStaminaBar;
    Image activeHealthBarFill, activeStaminaBarFill;
    TextMeshProUGUI activeHealthBarLabel, activeStaminaBarLabel;

    GameObject hotbarPartyGroup;
    TextMeshProUGUI switchLabel, firstHeroLabel, firstHeroName, secondHeroLabel, secondHeroName;
    Image firstStamina, firstHealth, secondStamina, secondHealth;

    GameObject statsGroup;
    TextMeshProUGUI strText, dexText, endText, agiText;

    [SerializeField]
    GameObject mapRowPrefab, mapCellPrefab;
    GameObject mapGroup;
    GameObject mapPointer;
    Vector2Int middleShift;
    List<List<RoomData>> mapData;

    GameObject player;
    PartyManager _pm;
    RunManager _rm;
    LevelGenerator _lg;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("/Player");
        _pm = player.GetComponent<PartyManager>();
        _rm = GameObject.Find("/GameControl").GetComponent<RunManager>();
        _lg = GameObject.Find("/GameControl").GetComponent<LevelGenerator>();

        coinCount = transform.Find("CoinCount").gameObject.GetComponent<TextMeshProUGUI>();

        timerDisplay = transform.Find("Timer").gameObject.GetComponent<TextMeshProUGUI>();

        activeHeroGroup = transform.Find("Hotbar/ActiveHero").gameObject;
        abilitiesGroup = activeHeroGroup.transform.Find("Abilities").gameObject;

        heroName = activeHeroGroup.transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>();
        
        Transform primaryBlock = abilitiesGroup.transform.Find("Primary");
        primaryName = primaryBlock.Find("AttackName").gameObject.GetComponent<TextMeshProUGUI>();
        primaryBar = primaryBlock.Find("Cooldown Bar/Inner").gameObject.GetComponent<Image>();
        Transform secondaryBlock = abilitiesGroup.transform.Find("Secondary");
        secondaryName = secondaryBlock.Find("AttackName").gameObject.GetComponent<TextMeshProUGUI>();
        secondaryBar = secondaryBlock.Find("Cooldown Bar/Inner").gameObject.GetComponent<Image>();

        activeBarsGroup = activeHeroGroup.transform.Find("Bars").gameObject;
        activeHealthBar = activeBarsGroup.transform.Find("Health Bar").gameObject;
        activeStaminaBar = activeBarsGroup.transform.Find("Stamina Bar").gameObject;
        activeHealthBarFill = activeHealthBar.transform.Find("Inner").gameObject.GetComponent<Image>();
        activeStaminaBarFill = activeStaminaBar.transform.Find("Inner").gameObject.GetComponent<Image>();
        activeHealthBarLabel = activeHealthBar.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>();
        activeStaminaBarLabel = activeStaminaBar.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>();

        hotbarPartyGroup = transform.Find("Hotbar/Party").gameObject;

        switchLabel = hotbarPartyGroup.transform.Find("Label").gameObject.GetComponent<TextMeshProUGUI>();
        Transform firstHero = hotbarPartyGroup.transform.Find("First");
        firstHeroLabel = firstHero.Find("Label").gameObject.GetComponent<TextMeshProUGUI>();
        firstHeroName = firstHero.Find("NameHolder/HeroName").gameObject.GetComponent<TextMeshProUGUI>();
        firstHealth = firstHero.Find("Health Bar/Inner").gameObject.GetComponent<Image>();
        firstStamina = firstHero.Find("Stamina Bar/Inner").gameObject.GetComponent<Image>();
        
        Transform secondHero = hotbarPartyGroup.transform.Find("Second");
        secondHeroLabel = secondHero.Find("Label").gameObject.GetComponent<TextMeshProUGUI>();
        secondHeroName = secondHero.Find("NameHolder/HeroName").gameObject.GetComponent<TextMeshProUGUI>();
        secondHealth = secondHero.Find("Health Bar/Inner").gameObject.GetComponent<Image>();
        secondStamina = secondHero.Find("Stamina Bar/Inner").gameObject.GetComponent<Image>();

        statsGroup = transform.Find("Hotbar/Stats").gameObject;

        strText = statsGroup.transform.Find("STR").gameObject.GetComponent<TextMeshProUGUI>();
        dexText = statsGroup.transform.Find("DEX").gameObject.GetComponent<TextMeshProUGUI>();
        endText = statsGroup.transform.Find("END").gameObject.GetComponent<TextMeshProUGUI>();
        agiText = statsGroup.transform.Find("AGI").gameObject.GetComponent<TextMeshProUGUI>();

        mapGroup = transform.Find("Map").gameObject;
        mapPointer = transform.Find("Pointer").gameObject;
        //UpdateMap();
    }

    // Update is called once per frame
    void Update()
    {
        PartyMember currHero = _pm.currentHero;
        //
        coinCount.text = "Coins: " + _pm.coinCount.ToString();
        //
        int rawTime = Mathf.FloorToInt(_rm.runTime);
        timerDisplay.text = "Timer:\n" + (rawTime / 60).ToString() + ":" + (rawTime % 60).ToString("D2");
        //
        if (_pm.isSwitching) switchLabel.color = ChangeColorAlpha(switchLabel.color, 1f);
        else switchLabel.color = ChangeColorAlpha(switchLabel.color, 0.5f);

        int[] indexes = new int[2];
        switch (_pm.currentIdx) {
            case 0:
                indexes[0] = 2;
                indexes[1] = 3;
                break;
            case 1:
                indexes[0] = 1;
                indexes[1] = 3;
                break;
            default: // case 2:
                indexes[0] = 1;
                indexes[1] = 2;
                break;
        }
        firstHeroLabel.text = indexes[0].ToString();
        firstHeroName.text = _pm.heroes[indexes[0] - 1].hero.rpgClass + " " + _pm.heroes[indexes[0] - 1].hero.name;
        firstHealth.fillAmount = Mathf.Clamp01(1.0f * _pm.heroes[indexes[0] - 1].currHealth / _pm.heroes[indexes[0] - 1].maxHealth);
        firstStamina.fillAmount = Mathf.Clamp01(_pm.heroes[indexes[0] - 1].stamina / 100f);

        secondHeroLabel.text = indexes[1].ToString();
        secondHeroName.text = _pm.heroes[indexes[1] - 1].hero.rpgClass + " " + _pm.heroes[indexes[1] - 1].hero.name;
        secondHealth.fillAmount = Mathf.Clamp01(1.0f * _pm.heroes[indexes[1] - 1].currHealth / _pm.heroes[indexes[1] - 1].maxHealth);
        secondStamina.fillAmount = Mathf.Clamp01(_pm.heroes[indexes[1] - 1].stamina / 100f);
        //

        if (currHero.mainWep != currWeapon) currWeapon = currHero.mainWep;

        heroName.text = (_pm.currentIdx+1).ToString() + ": " + currHero.hero.rpgClass + " " + currHero.hero.name;

        primaryName.text = currWeapon.primary.ToString().Substring(0, currWeapon.primary.ToString().Length - 8) + "\n" + currWeapon.primary.staminaUse + "%";
        primaryBar.fillAmount = Mathf.Clamp01(1f - (currWeapon.primaryCooldown / currWeapon.primary.cooldown));
        if (currHero.stamina - currWeapon.primary.staminaUse < 0) primaryBar.color = cooldownBarGrey;
        else primaryBar.color = cooldownBarGreen;

        secondaryName.text = currWeapon.secondary.ToString().Substring(0, currWeapon.secondary.ToString().Length - 8) + "\n" + currWeapon.secondary.staminaUse + "%";
        secondaryBar.fillAmount = Mathf.Clamp01(1f - (currWeapon.secondaryCooldown / currWeapon.secondary.cooldown));
        if (currHero.stamina - currWeapon.secondary.staminaUse < 0) secondaryBar.color = cooldownBarGrey;
        else secondaryBar.color = cooldownBarGreen;

        activeHealthBarFill.fillAmount = Mathf.Clamp01(1.0f * currHero.currHealth / currHero.maxHealth);
        activeHealthBarLabel.text = "Health: " + currHero.currHealth + "/" + currHero.maxHealth;
        activeStaminaBarFill.fillAmount = Mathf.Clamp01(currHero.stamina / 100f);
        activeStaminaBarLabel.text = "Stamina: " + currHero.stamina + "%";
        //
        strText.text = "Strength: "     + currHero.hero.strength.ToString();
        dexText.text = "Dexterity: "    + currHero.hero.dexterity.ToString();
        endText.text = "Endurance: "    + currHero.hero.endurance.ToString();
        agiText.text = "Agility: "      + currHero.hero.agility.ToString();
        //
        // UpdateMap();
    }
    public void UpdateMap(Vector2Int? current = null) {
        if (mapData != _lg.mapGrid) {
            mapData = _lg.mapGrid;

            mapPointer.transform.SetParent(transform, false);
            while(mapGroup.transform.childCount > 0) {
                DestroyImmediate(mapGroup.transform.GetChild(0).gameObject);
            }

            int squareDimension = Mathf.Max(mapData.Count, mapData[0].Count);
            
            GameObject rowTemplate = Instantiate(mapRowPrefab,mapGroup.transform,false);
            for(int _ = 0; _ < squareDimension; _++) {
                Instantiate(mapCellPrefab, rowTemplate.transform, false);
            }
            for(int _ = 1; _ < squareDimension; _++) {
                Instantiate(rowTemplate, mapGroup.transform, false);
            }

            middleShift = new Vector2Int((squareDimension - mapData.Count) / 2, (squareDimension - mapData[0].Count) / 2);
            //Vector2Int middleShift = new Vector2Int(0, 0);
            /*
            Debug.Log("MAP");
            Debug.Log(squareDimension + "|" + mapData.Count + "x" + mapData[0].Count + "|" + mapGroup.transform.childCount + "x" + mapGroup.transform.GetChild(0).childCount);
            Debug.Log(middleShift);
            */
            // Fill map with proper values
            // r = ui map row, i = mapData x position
            for (int r = middleShift.x, i = 0; i < mapData.Count; r++, i++) {
                Transform minimapRow = mapGroup.transform.GetChild(r);
                // c = ui map cell, j = mapData y position
                for (int c = middleShift.y, j = 0; j < mapData[i].Count; c++, j++) {
                    Image currentCell = minimapRow.GetChild(c).gameObject.GetComponent<Image>();
                    if (mapData[i][j] != null) {
                        switch (mapData[i][j].mapFeature) {
                            case MapFeature.Corner: currentCell.color = Color.red; break;
                            case MapFeature.DeadEnd: currentCell.color = Color.green; break;
                            case MapFeature.BranchOff: currentCell.color = Color.magenta; break;
                            case MapFeature.Junction: currentCell.color = Color.cyan; break;
                            default:
                                if (mapData[i][j].type == RoomType.Starting) { currentCell.color = Color.yellow; }
                                else { currentCell.color = Color.white; }
                                break;
                        }
                    }
                    if (current != null) {
                        
                        if (i == ((Vector2Int)current).x && j == ((Vector2Int)current).y) {
                            mapPointer.SetActive(true);
                            mapPointer.transform.SetParent(currentCell.gameObject.transform, false);
                            current = null;
                        }
                    }

                    if (mapData[i][j] == null) {
                        currentCell.color = ChangeColorAlpha(currentCell.color, 0f);
                    }
                    else {
                        currentCell.color = ChangeColorAlpha(currentCell.color, 0.5f);
                    }
                }
            }
        }
        else {
            //Debug.Log("MAP RECOVERED");
            // r = ui map row, i = mapData x position
            for (int r = middleShift.x, i = 0; i < mapData.Count; r++, i++) {
                Transform minimapRow = mapGroup.transform.GetChild(r);
                for (int c = middleShift.y, j = 0; j < mapData[i].Count; c++, j++) {
                    Transform currentCell = minimapRow.GetChild(c);
                    if (current != null) {
                        if (i == ((Vector2Int)current).x && j == ((Vector2Int)current).y) {
                            mapPointer.SetActive(true);
                            mapPointer.transform.SetParent(currentCell, false);
                            current = null;
                        }
                    }
                }
            }
        }
    }
}
