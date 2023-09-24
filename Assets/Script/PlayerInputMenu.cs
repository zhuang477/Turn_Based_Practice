using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInputMenu : MonoBehaviour
{
    public static PlayerInputMenu instance;

    private void Awake(){
        instance = this;
    }

    public GameObject inputMenu, moveMenu, meleeMenu, shootMenu;
    public TMP_Text turnPointText, errorText;

    public float errorDisplayTime =1f;
    private float errorCounter;

    public TMP_Text hitChanceText;
    public TMP_Text resultText;
    public GameObject endBattleButton;


    public void HideMenus(){
        inputMenu.SetActive(false);
        moveMenu.SetActive(false);
        meleeMenu.SetActive(false);
        shootMenu.SetActive(false);
    }

    public void ShowInputMenu(){
        inputMenu.SetActive(true);
    }

    public void ShowMoveMenu(){
        HideMenus();
        moveMenu.SetActive(true);

        ShowMove();

        SFXManager.instance.UISelect.Play();
    }

    public void HideMoveMenu(){
        HideMenus();
        MoveGrid.instance.HideMovePoints();
        ShowInputMenu();
        SFXManager.instance.UICancel.Play();
    }

    public void ShowMove(){
        if(GameManager.instance.TurnPointsRemain >=1){
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.MoveRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currActionCost =1;
        }
        SFXManager.instance.UISelect.Play();
    }

    public void ShowRun(){
        if(GameManager.instance.TurnPointsRemain >=2){
            MoveGrid.instance.ShowPointsInRange(GameManager.instance.activePlayer.runRange, GameManager.instance.activePlayer.transform.position);
            GameManager.instance.currActionCost =2;
        }
        SFXManager.instance.UISelect.Play();
    }

    public void UpdateTurnPointText(int turnPoints){
        turnPointText.text ="Turn Points Remaining: "+turnPoints;
    }

    public void SkipTurn(){
        GameManager.instance.EndTurn();
        SFXManager.instance.UISelect.Play();
    }

    public void ShowMeleeMenu(){
        HideMenus();
        meleeMenu.SetActive(true);
        SFXManager.instance.UISelect.Play();
    }

    public void HideMeleeMenu(){
        HideMenus();
        ShowInputMenu();

        GameManager.instance.targetDisplay.SetActive(false);
        SFXManager.instance.UICancel.Play();
    }

    public void CheckMelee(){
        GameManager.instance.activePlayer.GetMeleeTargets();
        if(GameManager.instance.activePlayer.meleeTargets.Count >0){
            ShowMeleeMenu();

            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position =GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
            GameManager.instance.targetDisplay.transform.Translate(Vector3.up * 1.8f);

            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);
        }else{
            ShowErrorText("No enemies in melee range");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void MeleeHit(){
        GameManager.instance.activePlayer.DoMelee();
        GameManager.instance.currActionCost =1;

        HideMenus();
        //GameManager.instance.SpendTurnPoints();

        StartCoroutine(WaitToEndActionCo(1f));
        SFXManager.instance.UISelect.Play();
    }

    public IEnumerator WaitToEndActionCo(float timeToWait){
        yield return new WaitForSeconds(timeToWait);

        GameManager.instance.SpendTurnPoints();
    }

    public void NextMeleeTarget(){
        GameManager.instance.activePlayer.currentMeleeTarget++;
        if(GameManager.instance.activePlayer.currentMeleeTarget >=GameManager.instance.activePlayer.meleeTargets.Count){
            GameManager.instance.activePlayer.currentMeleeTarget =0;
        }

        GameManager.instance.targetDisplay.transform.position =GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform.position;
        GameManager.instance.targetDisplay.transform.Translate(Vector3.up * 1.8f);

        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.meleeTargets[GameManager.instance.activePlayer.currentMeleeTarget].transform);
        SFXManager.instance.UISelect.Play();
    }

    public void ShowErrorText(string messageToShow){
        errorText.text =messageToShow;
        errorText.gameObject.SetActive(true);

        errorCounter =errorDisplayTime;
    }

    private void Update(){
        if(errorCounter >0){
            errorCounter -=Time.deltaTime;
            if(errorCounter <=0){
                errorText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowShootMenu(){
        HideMenus();
        shootMenu.SetActive(true);

        UpdateHitChance();
        SFXManager.instance.UISelect.Play();
    }

    public void HideShootMenu(){
        HideMenus();
        ShowInputMenu();

        GameManager.instance.targetDisplay.SetActive(false);
        SFXManager.instance.UICancel.Play();
    }

    public void CheckShoot(){
        GameManager.instance.activePlayer.GetShootTargets();

        if(GameManager.instance.activePlayer.shootTargets.Count >0){
            ShowShootMenu();
            GameManager.instance.targetDisplay.SetActive(true);
            GameManager.instance.targetDisplay.transform.position =GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;
            GameManager.instance.targetDisplay.transform.Translate(Vector3.up * 1.8f);

            GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform);
        }else{
            ShowErrorText("No Enemies in Firing Range");
            SFXManager.instance.UICancel.Play();
        }
    }

    public void NextShootTarget(){
        GameManager.instance.activePlayer.currentShootTarget++;
        if(GameManager.instance.activePlayer.currentShootTarget >=GameManager.instance.activePlayer.shootTargets.Count){
            GameManager.instance.activePlayer.currentShootTarget =0;
        }

        GameManager.instance.targetDisplay.transform.position =GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform.position;
        GameManager.instance.targetDisplay.transform.Translate(Vector3.up * 1.8f);

        UpdateHitChance();

        GameManager.instance.activePlayer.LookAtTarget(GameManager.instance.activePlayer.shootTargets[GameManager.instance.activePlayer.currentShootTarget].transform);
        SFXManager.instance.UISelect.Play();
    }

    public void FireShot(){
        GameManager.instance.activePlayer.FireShot();
        GameManager.instance.currActionCost =1;
        HideMenus();

        GameManager.instance.targetDisplay.SetActive(false);
        
        StartCoroutine(WaitToEndActionCo(1f));
        SFXManager.instance.UISelect.Play();
    }

    public void UpdateHitChance(){
        float hitChance =Random.Range(50f,95f);
        hitChanceText.text ="Chance To Hit: "+GameManager.instance.activePlayer.CheckShotChance().ToString("F1") +"%";
    }

    public void Defend(){
        GameManager.instance.activePlayer.SetDefending(true);
        GameManager.instance.EndTurn();
        SFXManager.instance.UISelect.Play();
    }

    public void LeaveBattle(){
        GameManager.instance.LeaveBattle();
    }
}
