using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryVisualiser : MonoBehaviour
{

    private Board board;

    public GameObject inventoryHolder;

    public GameObject inventoryItemPrefab;
    public Sprite gemImage;
    public Sprite coinImage;
    public Sprite keyImage;

    public List<GameObject> items;

    private List<Controllable> controllables;

    public void setUpVisualisation(Board board) {
        this.board = board;
        MakeItems();
    }

    private void HandleInventoryChanged(Dictionary<Collectable, int> inventory) {
        setUpVisualisation(board);
    }

    //Get the inventory of the all of the heros
    private void MakeItems()
    {
        // Clear existing items
        foreach (var item in items)
        {
            Destroy(item);
        }
        items.Clear();

        controllables = board.getEntities<Controllable>();
        int itemNumber = 0;
        foreach (Controllable con in controllables)
        {
            //Set up an update for the inventory when it's inventory changes
            con.OnInventoryChanged += HandleInventoryChanged;
            foreach (KeyValuePair<Collectable, int> entry in con.getInventory())
            {
                Collectable col = entry.Key;
                int count = entry.Value;

                if (col.getName() == "gem")
                {
                    GameObject gem = Instantiate(inventoryItemPrefab, Vector3.zero, Quaternion.identity, inventoryHolder.transform);
                    items.Add(gem);
                    gem.transform.localPosition = new Vector3(0, 165 - (itemNumber * 30), 0);
                    //Now we need to set the image
                    gem.GetComponentInChildren<Image>().sprite = gemImage;
                    //Now we need to set the count
                    string countText = "x " + count.ToString();
                    // Get the TextMeshProUGUI component and set the text
                    TextMeshProUGUI textComponent = gem.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = countText;
                    }
                    itemNumber++;
                } else if (col.getName() == "coin")
                {
                    GameObject coin = Instantiate(inventoryItemPrefab, Vector3.zero, Quaternion.identity, inventoryHolder.transform);
                    items.Add(coin);
                    coin.transform.localPosition = new Vector3(0, 165 - (itemNumber * 30), 0);
                    //Now we need to set the image
                    coin.GetComponentInChildren<Image>().sprite = coinImage;
                    //Now we need to set the count
                    string countText = "x " + count.ToString();
                    // Get the TextMeshProUGUI component and set the text
                    TextMeshProUGUI textComponent = coin.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = countText;
                    }
                    itemNumber++;
                } else if (col.getName() == "key") {
                    GameObject key = Instantiate(inventoryItemPrefab, Vector3.zero, Quaternion.identity, inventoryHolder.transform);
                    items.Add(key);
                    key.transform.localPosition = new Vector3(0, 165 - (itemNumber * 30), 0);
                    //Now we need to set the image
                    key.GetComponentInChildren<Image>().sprite = keyImage;
                    //Now we need to set the count
                    string countText = "x " + count.ToString();
                    // Get the TextMeshProUGUI component and set the text
                    TextMeshProUGUI textComponent = key.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = countText;
                    }
                    itemNumber++;
                }
            }
        }
    }
}
