using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryVisualiser : MonoBehaviour
{

    private Board board;

    private List<Controllable> controllables;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setUpVisualisation(Board board) {
        this.board = board;
        GetCollectables();
    }

    //Get the inventory of the all of the heros
    private void GetCollectables() {
        controllables = board.getEntities<Controllable>();
        foreach (Controllable con in controllables) {
            Debug.Log(con.getName() + " added to list");
            foreach (Collectable col in con.getInventory()) {
                Debug.Log(col.getName() + " in the inventory of " + con.getName());

            }
        }
    }
}
