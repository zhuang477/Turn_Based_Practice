using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public Character_Oversee charO;
    public float waitBeforeActing =1f,waitAfterActing =1f, waitBeforeShooting =.5f;
    public float moveChance =60f, defendChance =25f, skipChance =15f;
    [Range(0f, 100f)]
    public float ignoreShootChance =20f, moveRandomChance =50f;

    public void ChooseAction(){
        StartCoroutine(ChooseCo());
    }

    public IEnumerator ChooseCo(){
        yield return new WaitForSeconds(waitBeforeActing);

        bool actionTaken =false;

        charO.GetMeleeTargets();
        if(charO.meleeTargets.Count >0){
            charO.currentMeleeTarget =Random.Range(0, charO.meleeTargets.Count);
            GameManager.instance.currActionCost =1;

            StartCoroutine(WaitToEndAction(waitAfterActing));
            charO.DoMelee();

            actionTaken =true;
        }

        charO.GetShootTargets();
        if(actionTaken ==false && charO.shootTargets.Count >0){
            if(Random.Range(0f,100f) > ignoreShootChance){
                List<float> hitChances =new List<float>();

                for(int i = 0; i< charO.shootTargets.Count; i++){
                    charO.currentShootTarget =i;
                    charO.LookAtTarget(charO.shootTargets[i].transform);
                    hitChances.Add(charO.CheckShotChance());
                }

                float highestChance =0f;
                for(int i=0;i<hitChances.Count;i++){
                    if(hitChances[i]> highestChance){
                        highestChance = hitChances[i];
                        charO.currentShootTarget =i;
                    } else if(hitChances[i] ==highestChance){
                        if(Random.value >.5f){
                            charO.currentShootTarget =i;
                        }
                    }
                }

                if(highestChance >0f){
                    charO.LookAtTarget(charO.shootTargets[charO.currentShootTarget].transform);

                    actionTaken =true;
                    StartCoroutine(WaitToShoot());
                }
            }
        }

        if(actionTaken ==false){

            float actionDecision =Random.Range(0f, moveChance +defendChance+ skipChance);

            if(actionDecision <moveChance){
                float moveRandom =Random.Range(0f,100f);
                List<MovePoint> potentialMovePoints =new List<MovePoint>();
                int selectedPoint =0;

                if(moveRandom > moveRandomChance){
                    int nearestPlayer =0;

                    for(int i=1; i<GameManager.instance.PlayerTeam.Count; i++){
                        if(Vector3.Distance(transform.position, GameManager.instance.PlayerTeam[nearestPlayer].transform.position)
                        > Vector3.Distance(transform.position, GameManager.instance.PlayerTeam[i].transform.position)){
                            nearestPlayer =i;
                        }
                    }

                    if(Vector3.Distance(transform.position, GameManager.instance.PlayerTeam[nearestPlayer].transform.position) >charO.MoveRange && GameManager.instance.TurnPointsRemain >=2){
                        potentialMovePoints =MoveGrid.instance.GetMovePointsInRange(charO.runRange, transform.position);
                        float closestDistance =1000f;
                        for(int i=0;i <potentialMovePoints.Count;i++){
                            if(Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position) <closestDistance){
                                closestDistance =Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedPoint =i;
                            }
                        }

                        GameManager.instance.currActionCost =2;
                    }else{
                        potentialMovePoints =MoveGrid.instance.GetMovePointsInRange(charO.MoveRange, transform.position);
                        float closestDistance =1000f;
                        for(int i=0;i <potentialMovePoints.Count;i++){
                            if(Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position) <closestDistance){
                                closestDistance =Vector3.Distance(GameManager.instance.PlayerTeam[nearestPlayer].transform.position, potentialMovePoints[i].transform.position);
                                selectedPoint =i;
                            }
                        }

                        GameManager.instance.currActionCost =1;
                    }
                }else{
                    potentialMovePoints =MoveGrid.instance.GetMovePointsInRange(charO.MoveRange, transform.position);

                    selectedPoint =Random.Range(0,potentialMovePoints.Count);
                    GameManager.instance.currActionCost =1;
                }

                charO.MoveToPoint(potentialMovePoints[selectedPoint].transform.position);
            }else if(actionDecision <moveChance +defendChance){
                charO.SetDefending(true);

                GameManager.instance.currActionCost =GameManager.instance.TurnPointsRemain;
                StartCoroutine(WaitToEndAction(waitAfterActing));
            }else{
                GameManager.instance.EndTurn();
            }
        }
    }

    IEnumerator WaitToEndAction(float timeToWait){
        yield return new WaitForSeconds(timeToWait);
        GameManager.instance.SpendTurnPoints();
    }

    IEnumerator WaitToShoot(){
        yield return new WaitForSeconds(waitBeforeShooting);
        charO.FireShot();

        GameManager.instance.currActionCost =1;

        StartCoroutine(WaitToEndAction(waitAfterActing));
    }
}
