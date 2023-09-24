using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake(){
        instance = this;
    }

    public Character_Oversee activePlayer;
    public List<Character_Oversee> charList =new List<Character_Oversee>();
    public List<Character_Oversee> PlayerTeam =new List<Character_Oversee>();
    public List<Character_Oversee> EnemyTeam =new List<Character_Oversee>();
    private int currChar;

    public int totalTurnPoints =2;
    [HideInInspector]public int TurnPointsRemain;

    public int currActionCost =1;

    public GameObject targetDisplay;

    public bool shouldSpawnAtRandomPoints;
    public List<Transform> playerSpawnPoints =new List<Transform>();
    public List<Transform> enemySpawnPoints =new List<Transform>();

    public bool matchEnd;

    public string levelToLoad;
    
    // Start is called before the first frame update
    void Start()
    {
        charList.AddRange(FindObjectsOfType<Character_Oversee>());
        foreach(Character_Oversee character in charList){
            if(character.isEnemy ==false){
                PlayerTeam.Add(character);
            }else{
                EnemyTeam.Add(character);
            }
        }
        charList.Clear();
        charList.AddRange(PlayerTeam);
        charList.AddRange(EnemyTeam);

        activePlayer =charList[0];

        if(shouldSpawnAtRandomPoints){
            foreach(Character_Oversee cc in PlayerTeam){
                if(playerSpawnPoints.Count >0){
                    int pos =Random.Range(0,playerSpawnPoints.Count);

                    cc.transform.position =playerSpawnPoints[pos].position;
                    playerSpawnPoints.RemoveAt(pos);
                }
            }

            foreach(Character_Oversee cc in EnemyTeam){
                if(enemySpawnPoints.Count >0){
                    int pos =Random.Range(0,enemySpawnPoints.Count);

                    cc.transform.position =enemySpawnPoints[pos].position;
                    enemySpawnPoints.RemoveAt(pos);
                }
            }
        }

        currChar =-1;
        EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        //TurnNumberManager();
    }

    void TurnNumberManager(){
        //so the grid won't update may caused by in turn points use.
        //the grid only redraw when re-enable the isMoving.
        charList[currChar].isMoving =true;
    }

    public void FinishedMovement(){
        SpendTurnPoints();
    }

    public void SpendTurnPoints(){
        TurnPointsRemain -= currActionCost;

        CheckForVictory();

        if(matchEnd ==false){
            if(TurnPointsRemain <=0){
                EndTurn();
            }else{
                if(activePlayer.isEnemy ==false){
                    //MoveGrid.instance.ShowPointsInRange(activePlayer.MoveRange,activePlayer.transform.position);

                    PlayerInputMenu.instance.ShowInputMenu();
                }else{
                    PlayerInputMenu.instance.HideMenus();

                    activePlayer.brain.ChooseAction();
                }
            }
        }
        PlayerInputMenu.instance.UpdateTurnPointText(TurnPointsRemain);
    }

    public void EndTurn(){
        CheckForVictory();

        if(matchEnd ==false){
            currChar++;
            if(currChar >=charList.Count){
                currChar = 0;
            }

            activePlayer =charList[currChar];
            TurnPointsRemain =totalTurnPoints;

            if(activePlayer.isEnemy ==false){
                //MoveGrid.instance.ShowPointsInRange(activePlayer.MoveRange,activePlayer.transform.position);

                PlayerInputMenu.instance.ShowInputMenu();
                PlayerInputMenu.instance.turnPointText.gameObject.SetActive(true);
            }else{
                PlayerInputMenu.instance.HideMenus();
                PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);
                //StartCoroutine(AISkip());
                activePlayer.brain.ChooseAction();
            }
            currActionCost =1;
            PlayerInputMenu.instance.UpdateTurnPointText(TurnPointsRemain);
            activePlayer.SetDefending(false);
        }
    }

    public IEnumerator AISkip(){
        yield return new WaitForSeconds(1f);
        EndTurn();
    }

    public void CheckForVictory(){
        bool allDead =true;

        foreach(Character_Oversee cc in PlayerTeam){
            if(cc.currentHealth >0){
                allDead =false;
            }
        }

        if(allDead){
            PlayerLoses();
        }else{
            allDead =true;

            foreach(Character_Oversee cc in EnemyTeam){
                if(cc.currentHealth >0){
                    allDead =false;
                }
            }

            if(allDead){
                PlayerWins();
            }
        }
    }

    public void PlayerWins(){
        matchEnd =true;
        
        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);
        PlayerInputMenu.instance.resultText.text ="Player Wins!";
        PlayerInputMenu.instance.endBattleButton.SetActive(true);
        PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);
    }

    public void PlayerLoses(){
        matchEnd =true;
        PlayerInputMenu.instance.resultText.gameObject.SetActive(true);
        PlayerInputMenu.instance.resultText.text ="Player Loses!";
        PlayerInputMenu.instance.endBattleButton.SetActive(true);
        PlayerInputMenu.instance.turnPointText.gameObject.SetActive(false);
    }

    public void LeaveBattle(){
        SceneManager.LoadScene(levelToLoad);
    }
}
