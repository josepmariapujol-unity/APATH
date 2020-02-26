using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing2 : MonoBehaviour
{
    //[SerializeField] private HeatMapVisual heatMapVisual;
    //[SerializeField] private HeatMapBoolVisual heatMapBoolVisual;
    [SerializeField] private HeatMapGenericVisual heatMapGenericVisual;

    private Grid<HeatMapGridObject> grid;
    private Grid<StringGridObject> stringGrid;

    private void Start()
    {
        //grid = new Grid<HeatMapGridObject>(20, 20, 15f, Vector3.zero, (Grid<HeatMapGridObject> g, int x, int z) => new HeatMapGridObject(g,x,z));
        stringGrid = new Grid<StringGridObject>(20,10,15, Vector3.zero, (Grid<StringGridObject> g, int x, int z) => new StringGridObject(g, x, z));
        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(grid);
    }

    private void Update()
    {
        Vector3 clickPosition = -Vector3.one;
        Plane plane = new Plane(Vector3.up, 0f);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceToPlane;
        if (plane.Raycast(ray, out distanceToPlane))
        {
            clickPosition = ray.GetPoint(distanceToPlane);
        }

        /*
        if (Input.GetMouseButtonDown(0))
        {
            HeatMapGridObject heatMapGridObject = grid.GetGridObject(clickPosition);
            if(heatMapGridObject != null)
            {
                heatMapGridObject.AddValue(5);
            }
        }
        */

        if (Input.GetKeyDown(KeyCode.A)) { stringGrid.GetGridObject(clickPosition).AddLetter("A"); }
        if (Input.GetKeyDown(KeyCode.B)) { stringGrid.GetGridObject(clickPosition).AddLetter("B"); }
        if (Input.GetKeyDown(KeyCode.C)) { stringGrid.GetGridObject(clickPosition).AddLetter("C"); }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { stringGrid.GetGridObject(clickPosition).AddNumber("1"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { stringGrid.GetGridObject(clickPosition).AddNumber("2"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { stringGrid.GetGridObject(clickPosition).AddNumber("3"); }
    }
}

public class HeatMapGridObject
{
    private const int MIN = 0;
    private const int MAX = 100;

    private Grid<HeatMapGridObject> grid;
    private int x;
    private int z;
    private int value;

    public HeatMapGridObject(Grid<HeatMapGridObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public void AddValue(int addValue)
    {
        value += addValue;
        value = Mathf.Clamp(value, MIN, MAX);
        grid.TriggerGridObjectChanged(x, z);
    }

    public float GetValueNormalized()
    {
        return (float)value / MAX;
    }

    public override string ToString()
    {
        return value.ToString();
    }

}
public class StringGridObject
 {
    private Grid<StringGridObject> grid;
    private int x;
    private int z;

    private string letters;
    private string numbers;

    public StringGridObject(Grid<StringGridObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        letters = "";
        numbers = "";
    }

    public void AddLetter(string letter)
    {
        letters += letter;
        grid.TriggerGridObjectChanged(x, z);
    }

    public void AddNumber(string number)
    {
        numbers += number;
        grid.TriggerGridObjectChanged(x, z);
    }

    public override string ToString()
    {
        return letters + "\n" + numbers;
    }
    
}