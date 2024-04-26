using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Resources;
using Unity.VisualScripting;

public class InventoryLoader : MonoBehaviour
{
    // inventory
    public bool playerInventory = true;
    public bool containerInventory = false;
    public bool isCupboard = false;
    public bool isShop = false;
    public bool isWorkstation = false;
    public bool isWorkstationOutput = false;
    public bool renderPlayerInv = true;
    public GameObject playerInvUI;
    public GameObject containerInvUI;
    public GameObject UITemplate;
    public GameObject sliderElement;
    public TextMeshProUGUI sliderQuantityText;
    public PlayerInventory playerInv;
    public ContainerInventory containerInv;
    public WorkstationSystem workstation;
    populateBenches[] benches;
    ItemClass selectedItem;
    bool selectedPlayerItem;

    // this is only ever used under very specific circumstances and only exists because theres a major flaw with how i do these that should be fixed but i cant be arsed
    public GameObject playerInventoryObject;

    private void Start() 
    {
        RefreshInventories();

        benches = FindObjectsOfType<populateBenches>();
    }

    public void OnSliderChange(float value)
    {
        sliderQuantityText.text = value.ToString();
    }

    public void SelectItem(int value, bool isPlayer)
    {
        if(isPlayer)
        {
            //get the item
            string[] item = playerInv.formattedInventory[value];

            // if the item like exists. It should be physically impossible for it to not but yknow
            if(playerInv.totalInventory.ContainsKey(item[0]))
            {
                selectedItem = playerInv.totalInventory[item[0]];
                selectedPlayerItem = true;

                // set slider max value to amount of that item
                if (int.TryParse(item[1], out int currentQuantity))
                {
                    transform.Find("Give Items Slider").transform.Find("Slider").gameObject.GetComponent<Slider>().maxValue = currentQuantity;
                }
            }
        }
        else
        {
            //get the item
            string[] item = containerInv.formattedInventory[value];

            // if the item like exists. It should be physically impossible for it to not but yknow
            if(containerInv.totalInventory.ContainsKey(item[0]))
            {
                selectedItem = containerInv.totalInventory[item[0]];
                selectedPlayerItem = false;

                // set slider max value to amount of that item
                if (int.TryParse(item[1], out int currentQuantity))
                {
                    transform.Find("Give Items Slider").transform.Find("Slider").gameObject.GetComponent<Slider>().maxValue = currentQuantity;
                    Debug.Log(currentQuantity);
                }
            }
        }
        sliderElement.SetActive(true);
    }

    public void TransferItems()
    {
        // get how many items to transfer
        int sliderValue = (int)transform.Find("Give Items Slider").transform.Find("Slider").gameObject.GetComponent<Slider>().value;

        // if we are moving an item from the player inventory
        if(selectedPlayerItem)
        {
            if(containerInv)
            {
                // if we can actually take that many
                if(playerInv.inventory.TakeItem(selectedItem.className, sliderValue))
                {
                    if(containerInventory == true)
                    {
                        // if we can actually fit that many
                        if(containerInv.inventory.AddItem(selectedItem.className, sliderValue));

                        if (isShop && containerInv.inventory.IsPotion(selectedItem.className))
                        {
                            foreach (populateBenches bench in benches)
                            {
                                bench.stockShelf();
                            }
                        }

                        //if the container we're interacting with is a cupboard
                        if (isCupboard)
                        {
                            //loop through the containers inventory
                            foreach (string[] a in containerInv.inventory.formattedInventory)
                            {
                                //check if it has a specific item in it
                                if (a[0] == "carpet")
                                {
                                    //activate carpet if its in the container inventory
                                    GameObject furnitureObject = GameObject.Find("carpet_prefab");
                                    furnitureObject.transform.GetChild(0).gameObject.SetActive(true);
                                }
                                if (a[0] == "lamp")
                                {
                                    //activate lamp if its in the container inventory
                                    GameObject furnitureObject = GameObject.Find("lamp_prefab");
                                    furnitureObject.transform.GetChild(0).gameObject.SetActive(true);
                                }
                                if (a[0] == "painting")
                                {
                                    //activate painting if its in the container inventory
                                    GameObject furnitureObject = GameObject.Find("painting_prefab");
                                    furnitureObject.transform.GetChild(0).gameObject.SetActive(true);
                                }
                                if (a[0] == "rack")
                                {
                                    //activate rack if its in the container inventory
                                    GameObject furnitureObject = GameObject.Find("rack_prefab");
                                    furnitureObject.transform.GetChild(0).gameObject.SetActive(true);
                                }
                                if (a[0] == "table")
                                {
                                    //activate table if its in the container inventory
                                    GameObject furnitureObject = GameObject.Find("table_prefab");
                                    furnitureObject.transform.GetChild(0).gameObject.SetActive(true);
                                }
                            }
                        }
                    }
                }
                containerInv.SaveInventory();
            }
            else
            {
                // if theres no container we can assume its just the player inv so these items need to be dropped
                // if we can actually take that many
                if(playerInv.inventory.TakeItem(selectedItem.className, sliderValue))
                {
                    // drop the itms
                    Dropitem(selectedItem.className, sliderValue);
                }
                playerInv.SaveInventory();
            }
        }
        else
        {
            // if we can actually take that many
            if(containerInv.inventory.TakeItem(selectedItem.className, sliderValue))
            {
                // if we can actually fit that many
                if(playerInv.inventory.AddItem(selectedItem.className, sliderValue));

                if (isShop)
                {
                    foreach (populateBenches bench in benches)
                    {
                        bench.unstockShelf();
                    }
                }

                //if item is being taken from the cupboard
                if (isCupboard)
                {
                    //loop through the players inventory
                    foreach (string[] a in playerInv.inventory.formattedInventory)
                    {
                        //check if they have a specific item in it
                        if (a[0] == "carpet")
                        {
                            //remove carpet if they have it in their inventory
                            GameObject furnitureObject = GameObject.Find("carpet_prefab");
                            furnitureObject.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (a[0] == "lamp")
                        {
                            //remove carpet if they have it in their inventory
                            GameObject furnitureObject = GameObject.Find("lamp_prefab");
                            furnitureObject.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (a[0] == "painting")
                        {
                            //remove painting if they have it in their inventory
                            GameObject furnitureObject = GameObject.Find("painting_prefab");
                            furnitureObject.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (a[0] == "rack")
                        {
                            //remove rack if they have it in their inventory
                            GameObject furnitureObject = GameObject.Find("rack_prefab");
                            furnitureObject.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (a[0] == "table")
                        {
                            //remove table if they have it in their inventory
                            GameObject furnitureObject = GameObject.Find("table_prefab");
                            furnitureObject.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        // refresh inventories
        RefreshInventories();
        sliderElement.SetActive(false);
    }

    void Dropitem(string name, int quantity)
    {
        for(int i = 0; i < quantity; i++)
        {
            GameObject itemPrefab;
            
            itemPrefab = Resources.Load<GameObject>(name + "_prefab");
            itemPrefab.GetComponent<CircleCollider2D>().radius=0.5f;
            Debug.Log(name);


            // instantiate the corresponding item object
            Vector3 randomOffset = Random.insideUnitCircle * 2;
            Vector3 spawnPosition = transform.parent.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            GameObject woodItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(ScaleItem(woodItem));
        }
    }

    IEnumerator ScaleItem(GameObject woodItem)
    {
        Vector3 originalScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        float duration = 0.5f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if(woodItem != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                woodItem.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            else
            {
                yield return null;
            }
        }
        if(woodItem != null){woodItem.transform.localScale = targetScale;}
    }


    public void RefreshInventories()
    {
        // if player inventory is enabled show it in the left bit. If not just show the container
        if(playerInventory && renderPlayerInv)
        {
            playerInvUI.SetActive(true);
            PopulateInventoryUI(playerInv.formattedInventory, playerInv.inventoryMaxLength, true);
        }
        if(playerInventory && !renderPlayerInv && isWorkstationOutput)
        {
            PopulateContainerOutputInventoryUI(playerInv.formattedInventory, playerInv.inventoryMaxLength);
        }
        if(containerInventory)
        {
            containerInvUI.SetActive(true);
            // change these to be well not the playerInv. Like the cart inv or something
            PopulateInventoryUI(containerInv.formattedInventory, containerInv.inventoryMaxLength, false);
        }
        if(isWorkstation)
        {
            workstation.CheckPotion();
        }
    }

    // this is only ever used under very specific circumstances and only exists because theres a major flaw with how i do these that should be fixed but i cant be arsed
    private void PopulateContainerOutputInventoryUI(List<string[]> formattedInventory, int inventoryMaxLength)
    {
        // foreach thing in formatted inventory get name and quantity (values 1 and 2) Each one of these will be an object in the ui. Each object will also need an image and the display name which we will get from the item class by querying the total inventory


        GameObject ItemList = playerInventoryObject.transform.Find("ItemsListContent").gameObject;
        foreach (Transform child in ItemList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }


        // first we populate the player inventory

        for (int i = 0; i < formattedInventory.Count; i++)
        {
            string[] array = formattedInventory[i];

            // Prep the object
            GameObject UIElement = Instantiate(UITemplate, ItemList.transform);

            // Get child text object named title (as TMPro Text)
            TextMeshProUGUI titleText = UIElement.transform.Find("Title").GetComponent<TextMeshProUGUI>();

            // Get child image named Image (as a UI Image)
            Image imageComponent = UIElement.transform.Find("Image").GetComponent<Image>();

            // Get child text (child of the image object) as TMPro Text (named quantity)
            TextMeshProUGUI quantityText = imageComponent.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();

            // get the info for it
            quantityText.text = array[1].ToString();
            titleText.text = array[0];
            imageComponent.sprite = Resources.Load<Sprite>(array[0]);

            UIElement.GetComponent<ButtonValue>().value = i;

            // this will probably cause issues at a later date but i cant be arsed to fix it, cheers
            UIElement.GetComponent<ButtonValue>().isPlayer = true;
        }

        // add the empty ones

        int a = inventoryMaxLength - formattedInventory.Count();

        if(a > 0)
        {
            for(int i = 0; i < a; i++)
            {
                // Prep the object
                GameObject UIElement = Instantiate(UITemplate, ItemList.transform);

                // Get child text object named title (as TMPro Text)
                TextMeshProUGUI titleText = UIElement.transform.Find("Title").GetComponent<TextMeshProUGUI>();

                // Get child image named Image (as a UI Image)
                Image imageComponent = UIElement.transform.Find("Image").GetComponent<Image>();

                // Get child text (child of the image object) as TMPro Text (named quantity)
                TextMeshProUGUI quantityText = imageComponent.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();

                // get the info for it

                quantityText.text = "";
                titleText.text = "";
            }
        }
    }

    // we need to get the stuff from the player inventory to populate that part of the ui if its needed

    private void PopulateInventoryUI(List<string[]> formattedInventory, int inventoryMaxLength, bool isPlayer)
    {
        // foreach thing in formatted inventory get name and quantity (values 1 and 2) Each one of these will be an object in the ui. Each object will also need an image and the display name which we will get from the item class by querying the total inventory

        GameObject ItemList;

        // Find the InventoryUI object in the scene
        GameObject inventoryUI = gameObject;

        // Find the child object named ItemsListContent
        GameObject inventories = inventoryUI.transform.Find("Inventories").gameObject;


        if(isPlayer)
        {
            ItemList = inventories.transform.Find("Player").transform.Find("ItemsListContent").gameObject;
            foreach (Transform child in ItemList.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        else
        {
            ItemList = inventories.transform.Find("Container").transform.Find("ItemsListContent").gameObject;
            foreach (Transform child in ItemList.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }


        // first we populate the player inventory

        for (int i = 0; i < formattedInventory.Count; i++)
        {
            string[] array = formattedInventory[i];

            // Prep the object
            GameObject UIElement = Instantiate(UITemplate, ItemList.transform);

            // Get child text object named title (as TMPro Text)
            TextMeshProUGUI titleText = UIElement.transform.Find("Title").GetComponent<TextMeshProUGUI>();

            // Get child image named Image (as a UI Image)
            Image imageComponent = UIElement.transform.Find("Image").GetComponent<Image>();

            // Get child text (child of the image object) as TMPro Text (named quantity)
            TextMeshProUGUI quantityText = imageComponent.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();

            // get the info for it
            quantityText.text = array[1].ToString();
            titleText.text = array[0];
            imageComponent.sprite = Resources.Load<Sprite>(array[0]);

            UIElement.GetComponent<ButtonValue>().value = i;
            UIElement.GetComponent<ButtonValue>().isPlayer = isPlayer;
        }

        // add the empty ones

        int a = inventoryMaxLength - formattedInventory.Count();

        if(a > 0)
        {
            for(int i = 0; i < a; i++)
            {
                // Prep the object
                GameObject UIElement = Instantiate(UITemplate, ItemList.transform);

                // Get child text object named title (as TMPro Text)
                TextMeshProUGUI titleText = UIElement.transform.Find("Title").GetComponent<TextMeshProUGUI>();

                // Get child image named Image (as a UI Image)
                Image imageComponent = UIElement.transform.Find("Image").GetComponent<Image>();

                // Get child text (child of the image object) as TMPro Text (named quantity)
                TextMeshProUGUI quantityText = imageComponent.transform.Find("Quantity").GetComponent<TextMeshProUGUI>();

                // get the info for it

                quantityText.text = "";
                titleText.text = "";
            }
        }
    }
}
