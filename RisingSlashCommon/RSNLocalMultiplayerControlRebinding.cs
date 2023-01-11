using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Mono.Cecil;
using Rewired;
using UnityEngine;

namespace RisingSlash.FP2Mods.RisingSlashCommon;

public class RSNLocalMultiplayerControlRebinding : MonoBehaviour
{
    public static RSNLocalMultiplayerControlRebinding instance;
    
    public MenuOption btnRSNPlayer;
    public GameObject btnType;
    public MenuControls currentMenu;
    public int playerNum = 0;
    public int playerNumMaxPlayers = 8;

    public GameObject deviceNameText;

    public static void AllowLocalMultiplayerRebinds()
    {
        if (instance == null)
        {
            var go = new GameObject();
            go.name = "RSN Multiplayer Rebind Enabler";
            instance = go.AddComponent<RSNLocalMultiplayerControlRebinding>();
            DontDestroyOnLoad(go);
        }
    }

    public void Awake()
    {
        if (instance != null)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Update()
    {
        try
        {
            if (ConvenienceMethods.bHasSceneChanged)
            {
                btnType = null;
                btnRSNPlayer = null;
                currentMenu = null;
            }
    
            if (currentMenu == null)
            {
                currentMenu = Component.FindObjectOfType<MenuControls>();
            }
    
            if (currentMenu != null && (currentMenu.menuOptions.Length < 5 && btnRSNPlayer == null))
            {
                var goRSNPlayer = GameObject.Instantiate(currentMenu.menuOptions[0].gameObject);
                btnRSNPlayer = goRSNPlayer.GetComponent<MenuOption>();
                goRSNPlayer.name = "RSN Player Button";
                goRSNPlayer.transform.parent = currentMenu.menuOptions[0].transform;
                goRSNPlayer.transform.position += new Vector3(0f, -40f, 0f);
    
                var topLabel = goRSNPlayer.transform.Find("type_label");
                if (topLabel != null)
                {
                    topLabel.GetComponent<TextMesh>().text = "For Multiplayer:";
                }
    
                var currentSelectionText = goRSNPlayer.transform.Find("type");
                if (currentSelectionText != null)
                {
                    currentSelectionText.GetComponent<TextMesh>().text = ":";
                }
                
                deviceNameText = goRSNPlayer.transform.Find("devicename").gameObject;
                if (deviceNameText != null)
                {
                    deviceNameText.GetComponent<TextMesh>().text = $"Player {playerNum}";
                }
    
                MenuOption[] newMenuOptions =
                {
                    currentMenu.menuOptions[0], 
                    currentMenu.menuOptions[1],
                    currentMenu.menuOptions[2], 
                    currentMenu.menuOptions[3],
                    btnRSNPlayer
                };
                currentMenu.menuOptions = newMenuOptions;
            }
    
            if (deviceNameText != null)
            {
                deviceNameText.GetComponent<TextMesh>().text = $"Player {playerNum}";
            }
        }
        catch (Exception e)
        {
            ConvenienceMethods.LogExceptionError(e);
        }
    }

    public static void LoadKeybindsForPlayer(int playerNum)
    {
        ReadOnlyCollection<KeyMapping> keysList = InputControl.getKeysList();
        IniFile iniFile = new IniFile("controls-p" + playerNum);;
        iniFile.Load("controls-p" + playerNum);
        foreach (KeyMapping item in keysList)
        {
            string text = iniFile.Get("Controls." + item.name + ".primary");
            if (text != string.Empty)
            {
                item.primaryInput = CustomInputFromString(text);
            }
            text = iniFile.Get("Controls." + item.name + ".secondary");
            if (text != string.Empty)
            {
                item.secondaryInput = CustomInputFromString(text);
            }
            text = iniFile.Get("Controls." + item.name + ".third");
            if (text != string.Empty)
            {
                item.thirdInput = CustomInputFromString(text);
            }
        }
    }
    
    public static void SaveKeybindsForPlayer(int playerNum)
    {
        ReadOnlyCollection<KeyMapping> keysList = InputControl.getKeysList();
        IniFile iniFile = new IniFile("controls-p" + playerNum);
        foreach (KeyMapping item in keysList)
        {
            iniFile.Set("Controls." + item.name + ".primary", item.primaryInput.ToString());
            iniFile.Set("Controls." + item.name + ".secondary", item.secondaryInput.ToString());
            iniFile.Set("Controls." + item.name + ".third", item.thirdInput.ToString());
        }
        iniFile.Save("controls-p" + playerNum);
    }

    public static CustomInput CustomInputFromString(string text)
    {
        CustomInput ci;

        var methodInfo = typeof(Controls)
            .GetMethod("customInputFromString", BindingFlags.Static | BindingFlags.NonPublic);
        object[] textParm = new[] { (object)text };
        ci = (CustomInput)methodInfo.Invoke(null, textParm);
        
        return ci;
    }
    
    public static FPPlayerInput ProcessRewiredForPlayerNum(int playerNum)
    {
        var input = new FPPlayerInput();
        var rewiredPlayerInput = Rewired.ReInput.players.GetPlayer(playerNum);
        input.upPress = false;
        input.downPress = false;
        input.leftPress = false;
        input.rightPress = false;
        float num = 0.5f;
        
        if (rewiredPlayerInput.GetButton("Right"))
        {
            if (!input.right)
            {
                input.rightPress = true;
            }
            input.right = true;
        }
        else
        {
            input.right = false;
        }
        if (rewiredPlayerInput.GetButton("Left"))
        {
            if (!input.left)
            {
                input.leftPress = true;
            }
            input.left = true;
        }
        else
        {
            input.left = false;
        }
        if (rewiredPlayerInput.GetButton("Up"))
        {
            if (!input.up)
            {
                input.upPress = true;
            }
            input.up = true;
        }
        else
        {
            input.up = false;
        }
        if (rewiredPlayerInput.GetButton("Down"))
        {
            if (!input.down)
            {
                input.downPress = true;
            }
            input.down = true;
        }
        else
        {
            input.down = false;
        }
        input.jumpPress = rewiredPlayerInput.GetButtonDown("Jump");
        input.jumpHold = rewiredPlayerInput.GetButton("Jump");
        input.attackPress = rewiredPlayerInput.GetButtonDown("Attack");
        input.attackHold = rewiredPlayerInput.GetButton("Attack");
        input.specialPress = rewiredPlayerInput.GetButtonDown("Special");
        input.specialHold = rewiredPlayerInput.GetButton("Special");
        input.guardPress = rewiredPlayerInput.GetButtonDown("Guard");
        input.guardHold = rewiredPlayerInput.GetButton("Guard");
        input.confirm = (input.jumpPress | InputControl.GetButtonDown(Controls.buttons.pause));
        input.cancel = (input.attackPress | Input.GetKey(KeyCode.Escape));

        return input;
    }

    public static void Dab(MenuControls menuControls, int playerNum)
    {
        var player = ReInput.players.GetPlayer(playerNum);
        ControllerType selectedControllerType = (ControllerType)GetPrivateInstanceField(menuControls, "selectedControllerType");
        player.controllers.maps.LoadDefaultMaps(selectedControllerType);
        var mi = menuControls.GetType().GetMethod("MapRewired", BindingFlags.Instance | BindingFlags.NonPublic);
        mi.Invoke(menuControls, null);
    }

    public static object GetPrivateInstanceField(object obj, string fieldName)
    {
        object result = null;

        var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        result = fieldInfo.GetValue(obj);

        return result;
    }
}


/*
private void Action_SaveControls()
	{
		if (FPSaveManager.inputSystem == 0)
		{
			InputControl.setKey("Up", keyMapping[0].primaryInput, keyMapping[0].secondaryInput, keyMapping[0].thirdInput);
			InputControl.setKey("Down", keyMapping[1].primaryInput, keyMapping[1].secondaryInput, keyMapping[1].thirdInput);
			InputControl.setKey("Left", keyMapping[2].primaryInput, keyMapping[2].secondaryInput, keyMapping[2].thirdInput);
			InputControl.setKey("Right", keyMapping[3].primaryInput, keyMapping[3].secondaryInput, keyMapping[3].thirdInput);
			InputControl.setKey("Jump", keyMapping[4].primaryInput, keyMapping[4].secondaryInput, keyMapping[4].thirdInput);
			InputControl.setKey("Attack", keyMapping[5].primaryInput, keyMapping[5].secondaryInput, keyMapping[5].thirdInput);
			InputControl.setKey("Special", keyMapping[6].primaryInput, keyMapping[6].secondaryInput, keyMapping[6].thirdInput);
			InputControl.setKey("Guard", keyMapping[7].primaryInput, keyMapping[7].secondaryInput, keyMapping[7].thirdInput);
			InputControl.setKey("Pause", keyMapping[8].primaryInput, keyMapping[8].secondaryInput, keyMapping[8].thirdInput);
			Controls.save();
			FPSaveManager.SavePrefs();
		}
		else
		{
			ReInput.userDataStore.Save();
		}
	}
	
	
	*/