using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Character_Oversee : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    [SerializeField]public Vector3 Destination;
    public bool isEnemy;
    public bool isMoving;

    public float MoveRange =4f, runRange =8f;

    public float meleeRange =1.5f;
    public float meleeDamage =5f;
    [HideInInspector]public List<Character_Oversee> meleeTargets = new List<Character_Oversee>();
    [HideInInspector]public int currentMeleeTarget;

    public float shootRange;
    public float shootDamage;
    [HideInInspector]public List<Character_Oversee> shootTargets = new List<Character_Oversee>();
    [HideInInspector]public int currentShootTarget;
    public Transform shootPoint;
    public Vector3 shotMissRange;

    public float maxHealth =10f;
    [HideInInspector]public float currentHealth;

    public TMP_Text healthText;
    public Slider healthSlider;

    public LineRenderer shootLine;
    public float shotRemainTime =0.5f;
    private float shotRemainCounter;

    public GameObject shotHitEffect,shotMissEffect;

    public GameObject defendObject;
    public bool isDefending;

    public AIBrain brain;

    private void Awake(){
        Destination =transform.position;
        currentHealth =maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateHealthDisplay();

        shootLine.transform.position =Vector3.zero;
        shootLine.transform.rotation =Quaternion.identity;
        shootLine.transform.SetParent(null);

        if(isEnemy ==true && isDefending ==true){
            SetDefending(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving ==true){
            if(GameManager.instance.activePlayer ==this){
                if(Vector3.Distance(transform.position, Destination) <0.2f){
                    isMoving =false;
                    
                    GameManager.instance.FinishedMovement();
                }
            }
        }

        if(shotRemainCounter >0){
            shotRemainCounter -= Time.deltaTime;

            if(shotRemainCounter <=0){
                shootLine.gameObject.SetActive(false);
            }
        }
    }

    public void MoveToPoint(Vector3 PointToMove){
        Destination =PointToMove;
        agent.SetDestination(Destination);
        isMoving =true;
    }

    void ReachDetect(){
        if(Vector3.Distance(transform.position, Destination) >0.2f){
            Debug.Log(Vector3.Distance(transform.position, Destination));
        }
    }

    public void GetMeleeTargets(){
        meleeTargets.Clear();

        if(isEnemy ==false){
            foreach(Character_Oversee cc in GameManager.instance.EnemyTeam){
                if(Vector3.Distance(transform.position, cc.transform.position) <meleeRange){
                    meleeTargets.Add(cc);
                }
            }
        }else{
            foreach(Character_Oversee cc in GameManager.instance.PlayerTeam){
                if(Vector3.Distance(transform.position, cc.transform.position) <meleeRange){
                    meleeTargets.Add(cc);
                }
            }
        }

        if(currentMeleeTarget >=meleeTargets.Count){
            currentMeleeTarget =0;
        }
    }

    public void DoMelee(){
        meleeTargets[currentMeleeTarget].TakeDamage(meleeDamage);

        SFXManager.instance.meleeHit.Play();
    }

    public void TakeDamage(float damageToTake){
        if(isDefending ==true){
            damageToTake *=.5f;
        }

        currentHealth -=damageToTake;

        if(currentHealth <=0){
            currentHealth = 0;
            agent.enabled =false;

            transform.rotation =Quaternion.Euler(-70f,transform.rotation.eulerAngles.y,0f);

            GameManager.instance.charList.Remove(this);
            if(GameManager.instance.PlayerTeam.Contains(this)){
                GameManager.instance.PlayerTeam.Remove(this);
            }
            if(GameManager.instance.EnemyTeam.Contains(this)){
                GameManager.instance.EnemyTeam.Remove(this);
            }

            if(isEnemy ==false){
                SFXManager.instance.deathHuman.Play();
            }else{
                SFXManager.instance.deathRobot.Play();
            }

            GetComponent<Collider>().enabled =false;
        }else{
            SFXManager.instance.takeDamage.Play();
        }

        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay(){
        healthText.text =currentHealth+"/"+maxHealth;

        healthSlider.maxValue =maxHealth;
        healthSlider.value =currentHealth;
    }

    public void GetShootTargets(){
        shootTargets.Clear();

        if(isEnemy ==false){
            foreach(Character_Oversee cc in GameManager.instance.EnemyTeam){
                if(Vector3.Distance(transform.position, cc.transform.position) < shootRange){
                    shootTargets.Add(cc);
                }
            }
        }else{
            foreach(Character_Oversee cc in GameManager.instance.PlayerTeam){
                if(Vector3.Distance(transform.position, cc.transform.position) < shootRange){
                    shootTargets.Add(cc);
                }
            }
        }

        if(currentShootTarget >=shootTargets.Count){
            currentShootTarget =0;
        }
    }

    public void FireShot(){
        Vector3 targetPoint =new Vector3(shootTargets[currentShootTarget].transform.position.x ,shootTargets[currentShootTarget].shootPoint.transform.position.y, shootTargets[currentShootTarget].transform.position.z);
        targetPoint.y = Random.Range(targetPoint.y, shootTargets[currentShootTarget].transform.position.y+.25f);
        
        Vector3 targetOffset =new Vector3(Random.Range(-shotMissRange.x,shotMissRange.x),Random.Range(-shotMissRange.y,shotMissRange.y),
        Random.Range(-shotMissRange.z,shotMissRange.z));
        targetOffset =targetOffset *(Vector3.Distance(shootTargets[currentShootTarget].transform.position,shootPoint.position) /shootRange);
        targetOffset +=targetOffset;

        Vector3 shootDirection =(targetPoint -shootPoint.position).normalized;

        RaycastHit hit;
        if(Physics.Raycast(shootPoint.position,shootDirection, out hit, shootRange)){
            if(hit.collider.gameObject ==shootTargets[currentShootTarget].gameObject){
                shootTargets[currentShootTarget].TakeDamage(shootDamage);
                
                Instantiate(shotHitEffect,hit.point,Quaternion.identity);
            }else{
                PlayerInputMenu.instance.ShowErrorText("Shot Missed!");
                Instantiate(shotMissEffect,hit.point,Quaternion.identity);
            }
            shootLine.SetPosition(0, shootPoint.position);
            shootLine.SetPosition(1, hit.point);

            SFXManager.instance.impact.Play();
        }else{
            PlayerInputMenu.instance.ShowErrorText("Shot Missed!");
            shootLine.SetPosition(0, shootPoint.position);
            shootLine.SetPosition(1, shootPoint.position +(shootDirection *shootRange));
        }

        shootLine.gameObject.SetActive(true);
        shotRemainCounter =shotRemainTime;

        SFXManager.instance.PlayShoot();
    }

    public float CheckShotChance(){
        float shotChance =0f;

        RaycastHit hit;
        Vector3 targetPoint =new Vector3(shootTargets[currentShootTarget].transform.position.x,shootTargets[currentShootTarget].transform.position.y,shootTargets[currentShootTarget].transform.position.z);

        Vector3 shootDirection = (targetPoint -shootPoint.position).normalized;
        if(Physics.Raycast(shootPoint.position, shootDirection, out hit,shootRange)){
            if(hit.collider.gameObject == shootTargets[currentShootTarget].gameObject){
                shotChance += 50f;
            }
        }

        targetPoint.y =shootTargets[currentShootTarget].transform.position.y +.25f;
        shootDirection = (targetPoint -shootPoint.position).normalized;
        if(Physics.Raycast(shootPoint.position, shootDirection, out hit,shootRange)){
            if(hit.collider.gameObject == shootTargets[currentShootTarget].gameObject){
                shotChance += 50f;
            }
        }

        shotChance *= 0.95f;
        shotChance *= 1f -(Vector3.Distance(shootTargets[currentShootTarget].transform.position,shootPoint.position) /shootRange);
        return shotChance;
    }

    public void LookAtTarget(Transform target){
        transform.LookAt(new Vector3(target.position.x,target.position.y, target.position.z), Vector3.up);
    }

    public void SetDefending(bool shouldDefend){
        isDefending =shouldDefend;

        defendObject.SetActive(isDefending);
    }
}
