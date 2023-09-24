using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGrid : MonoBehaviour
{
    public static MoveGrid instance;

    private void Awake(){
        instance = this;

        GenerateMoveGrid();
        HideMovePoints();
    }

    public MovePoint startPoint;
    public Vector2Int spawnRange;
    public LayerMask ground;
    public LayerMask obstacle;
    public float obstacleCheck;

    public List<MovePoint> allMovePoints =new List<MovePoint>();

    // Start is called before the first frame update
    void Start()
    {
        GenerateMoveGrid();
        //HideMovePoints();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateMoveGrid(){
        for(int x =-spawnRange.x; x <=spawnRange.x;x++){
            for(int y =-spawnRange.y; y <=spawnRange.y;y++){
                RaycastHit hit;
                if(Physics.Raycast(transform.position +new Vector3(x,0f,y),Vector3.down,out hit, 20f,ground)){
                    if(Physics.OverlapSphere(hit.point, obstacleCheck, obstacle).Length ==0){
                        MovePoint newPoint =Instantiate(startPoint, hit.point, transform.rotation);
                        newPoint.transform.SetParent(transform);
                        allMovePoints.Add(newPoint);
                    }
                }
            }
        }
        startPoint.gameObject.SetActive(false);
    }

    public void HideMovePoints(){
        foreach(MovePoint point in allMovePoints){
            point.gameObject.SetActive(false);
        }
    }

    public void ShowPointsInRange(float MoveRange,Vector3 Center){
        HideMovePoints();
        foreach(MovePoint point in allMovePoints){
            if(Vector3.Distance(Center,point.transform.position) <= MoveRange){
                point.gameObject.SetActive(true);

                foreach(Character_Oversee cc in GameManager.instance.charList){
                    if(Vector3.Distance(cc.transform.position, point.transform.position) <0.2f){
                        point.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public List<MovePoint> GetMovePointsInRange(float MoveRange, Vector3 Center){
        List<MovePoint> foundPoints = new List<MovePoint>();

        foreach(MovePoint point in allMovePoints){
            if(Vector3.Distance(Center,point.transform.position) <= MoveRange){
                bool shouldAdd =true;

                foreach(Character_Oversee cc in GameManager.instance.charList){
                    if(Vector3.Distance(cc.transform.position, point.transform.position) <0.2f){
                        shouldAdd =false;
                    }
                }

                if(shouldAdd ==true){
                    foundPoints.Add(point);
                }
            }
        }

        return foundPoints;
    }
}
