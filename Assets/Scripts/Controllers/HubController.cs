using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HubController : MonoBehaviour
{
    public GameController gameController; //Reference to GameController
    public Vector3Int[] HubGenerator; //List of positions and types of triangles (x, y, type)

    [Header("Prefabs")]
    public Rigidbody2D rigidBody; //Reference to this RigidBody2D
    public GameObject[] TrianglePrefabs; //Triangle Prefab 1 (Pointed upwards, turrets CW), Triangle Prefab 2 (Pointed downwards, turrets CCW)

    [Space(10)]
    public bool isDead = true; //If game is over

    private List<List<Vector2Int>> regions;//List of regions of triangles (to handle breakages)
    private Vector2 CenterOfMass; //Calculated center of mass to align spin axis

    //On start
    public void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody2D>();
    }

    //Generate triangles from HubGenerator
    public void GenerateTriangles()
    {
        foreach (var triangle in HubGenerator)
        {
            AddTriangle(triangle, false);
        }
    }

    //On fixed update
    void FixedUpdate()
    {
        if (transform.GetChild(0).childCount <= 1  && !isDead) Die();
        rigidBody.constraints = transform.GetChild(0).childCount<=1||isDead?RigidbodyConstraints2D.FreezeAll:RigidbodyConstraints2D.None;
    }

    //End the game [MIGRATE TO GAMECONTROLLER]
    public void Die()
    {
        isDead = true;
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().Shake(1f, 0.7f);
        gameController.Die();
        rigidBody.velocity = new Vector3(0f, 0f, 0f);
        rigidBody.angularVelocity = 0f;
    }

    //Shoot all triangles of type
    public void ShootTriangles(int type)
    {
        if (!isDead)
        {
            foreach (Transform triangle in transform.GetChild(0))
            {
                TriangleController t = triangle.GetComponent<TriangleController>();
                if (t.type == type) t.Shoot();
            }
        }
    }

    //Get the world position of a triangle on the coordinate grid
    private Vector2 GetWorldPosition(Vector2Int pos)
    {
        //Return the localposition (A,B) of a triangle (X,Y)
        //A = 0.5X - 0.5Y
        //B = -0.88Y [- 0.3(X % 2) for height variation of TrianglePrefab1 and TrianglePrefab2]
        return new Vector2((pos[0] - pos[1]) * 0.5f, Mathf.Abs(pos[0] % 2) * -0.3f - pos[1] * 0.88f);
    }

    //Adds a triangle at the given coordinates
    public bool AddTriangle(Vector3Int triangle, bool addToScore = true)
    {

        //If it doesn't exist
        if (!getIndexList().Contains((Vector2Int)triangle))
        {
            gameController.PlayClip(1);
            if (addToScore) gameController.AddScore(30);
            var t = Instantiate(TrianglePrefabs[Mathf.Abs(triangle[0]) % 2], transform.GetChild(0), false);
            t.transform.localPosition = GetWorldPosition((Vector2Int)triangle);
            t.GetComponent<TriangleController>().pos = (Vector2Int)triangle;
            t.GetComponent<TriangleController>().type = triangle[2];
            UpdateCOM();
            return true;
        }
        return false;
    }

    //Update the center of mass
    public void UpdateCOM()
    {
        //Set CenterOfMass to the average position of the triangles
        CenterOfMass = new Vector2(0f, 0f);
        foreach (Transform triangle in transform.GetChild(0))
        {
            CenterOfMass += (Vector2)triangle.transform.localPosition;
        }

        //Divide by 0 check
        CenterOfMass /= transform.GetChild(0).childCount > 0 ? transform.GetChild(0).childCount : 1;
        transform.GetChild(0).localPosition = -CenterOfMass;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            //Get list of triangle objets
            List<Transform> TriangleObjects = new List<Transform>();
            foreach (Transform x in transform.GetChild(0))
            {
                TriangleObjects.Add(x);
            }
            //Destroy the triangle object closest to the collision
            Destroy(TriangleObjects.OrderBy(x => (collision.contacts[0].point - (Vector2)x.transform.position).sqrMagnitude).FirstOrDefault().gameObject);

            //Effects
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().Shake(0.3f, 0.2f);
            gameController.PlayClip(0);

            //Check for any unattatched regions
            StartCoroutine(CheckForBreaks(null, true));
        }


        if (collision.gameObject.CompareTag("Triangle_Floater") && !isDead)
        {
            //Get closest open space on the hub to collision
            List<Vector2Int> openSpaces = new List<Vector2Int>();
            foreach (Transform HubTriangle in transform.GetChild(0))
            {
                foreach(Vector2Int side in GetOpenSides(HubTriangle.GetComponent<TriangleController>().pos))
                {
                    openSpaces.Add(side);
                }
            }
            Vector2Int TargetCell = openSpaces.OrderBy(x => (collision.contacts[0].point - (Vector2)transform.GetChild(0).TransformPoint(GetWorldPosition(x))).sqrMagnitude).FirstOrDefault();
            
            //Destroy floater, add triangle
            if(AddTriangle(new Vector3Int(TargetCell[0], TargetCell[1], collision.gameObject.transform.GetComponent<FloaterController>().type))) Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().Shake(0.3f, 0.2f);
            gameController.PlayClip(4);
            if (!isDead) gameController.AddScore(100);
            gameController.CreateParticles(collision.transform.position);
        }
    }

    public bool[] GetOpen(Vector2Int pos)
    {
        bool[] output = new bool[3];
        List<Vector2Int> indexList = getIndexList();
        output[0] = !indexList.Contains(pos - new Vector2Int(1, 0));
        output[1] = !indexList.Contains(pos + new Vector2Int(1, 0));
        output[2] = !indexList.Contains(pos + (Mathf.Abs(pos[0]) % 2 == 0 ? new Vector2Int(-1, -1) : new Vector2Int(1, 1)));
        return output;
    }

    public List<Vector2Int> getIndexList()
    {
        List<Vector2Int> indexList = new List<Vector2Int>();
        foreach (Transform x in transform.GetChild(0))
        {
            Vector2Int index = x.GetComponent<TriangleController>().pos;
            indexList.Add(index);
        }
        return indexList;
    }

    public List<Vector2Int> GetOpenSides(Vector2Int pos)
    {
        return GetOpenSides(pos, getIndexList());
    }
    public List<Vector2Int> GetOpenSides(Vector2Int pos, List<Vector2Int> indexList)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        if (!indexList.Contains(pos - new Vector2Int(1, 0))) output.Add(pos - new Vector2Int(1, 0));
        if (!indexList.Contains(pos + new Vector2Int(1, 0))) output.Add(pos + new Vector2Int(1, 0));
        if (!indexList.Contains(pos + (Mathf.Abs(pos[0]) % 2 == 0 ? new Vector2Int(-1, -1) : new Vector2Int(1, 1)))) output.Add(pos + (Mathf.Abs(pos[0]) % 2 == 0 ? new Vector2Int(-1, -1) : new Vector2Int(1, 1)));
        return output;
    }

    public List<Vector2Int> GetClosedSides(Vector2Int pos)
    {
        return GetClosedSides(pos, getIndexList());
    }
    public List<Vector2Int> GetClosedSides(Vector2Int pos, List<Vector2Int> indexList)
    {
        List<Vector2Int> output = new List<Vector2Int>();
        if (indexList.Contains(pos - new Vector2Int(1, 0))) output.Add(pos - new Vector2Int(1, 0));
        if (indexList.Contains(pos + new Vector2Int(1, 0))) output.Add(pos + new Vector2Int(1, 0));
        if (indexList.Contains(pos + (Mathf.Abs(pos[0]) % 2 == 0 ? new Vector2Int(-1, -1) : new Vector2Int(1, 1)))) output.Add(pos + (Mathf.Abs(pos[0]) % 2 == 0 ? new Vector2Int(-1, -1) : new Vector2Int(1, 1)));
        return output;
    }
    public IEnumerator CheckForBreaks(List<Vector2Int> indexList, bool useIndexList)
    {
        yield return null;
        if (useIndexList) indexList = getIndexList();
        regions = new List<List<Vector2Int>>();
        int i = 0;
        while (indexList.Count > 0 && i < 25)
        {
            regions.Add(new List<Vector2Int>());
            indexList = DFS(indexList[0], indexList, i);
            i++;
        }
        foreach (List<Vector2Int> j in regions)
        {
            if (indexList.Count < j.Count)
            {
                indexList = j;
            }
        }
        foreach (Transform triangle in transform.GetChild(0))
        {
            if (!indexList.Contains(triangle.GetComponent<TriangleController>().pos))
            {
                triangle.GetComponent<TriangleController>().MakeFloater();
            }
        }
        yield return null;
        UpdateCOM();
    }

    public List<Vector2Int> DFS(Vector2Int cell, List<Vector2Int> indexList, int RegionNumber)
    {
        regions[RegionNumber].Add(cell);
        indexList.Remove(cell);
        List<Vector2Int> closedSides = GetClosedSides(cell, indexList);
        foreach (Vector2Int v in closedSides)
        {
            indexList = DFS(v, indexList, RegionNumber);
        }
        return indexList;
    }

}
