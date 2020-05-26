using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public event Action moveEvent;

	[Header("Stats")]
	public int keys = 0;
	public int potions = 0;
	public int treasure = 0;
	public int playerMovesPerMonsterTurn = 2;
	public int remainingTurns;

	[HideInInspector]
	public bool gameOver = false;


	private void Start() {
		remainingTurns = playerMovesPerMonsterTurn;
	}

	void Update() {
		if (gameOver) {
			Camera.main.orthographicSize = 4;
			return;
		} else {
			Camera.main.orthographicSize = 8;
		}

		// Movement
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			Move(0, 1);  // Up
		if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			Move(0, -1); // Down
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			Move(-1, 0); // Left
		if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			Move(1, 0);  // Right
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
			Move(0, 0);  // Skip a turn

		// Actions
		if (Input.GetKeyDown(KeyCode.Space) && potions > 0) { // Drink a potion, extra move
			potions--; remainingTurns++; Debug.Log("Drank a potion");
		}
		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); Debug.Log("Quit"); }
		if (Input.GetKeyDown(KeyCode.R)) { GameManager.Instance.Restart(); } // Restart
	}

	void Move(int _x, int _y) {
		int x = Mathf.RoundToInt(transform.position.x) + _x;
		int y = Mathf.RoundToInt(transform.position.y) + _y;

		// Can't move if there is a wall in the way
		if (GameManager.Instance.IsWall(x, y))
			return;
		// Open the door if we have keys
		if (GameManager.Instance.GridTag(x, y) == "Door") {
			if (keys > 0) {
				GameManager.Instance.GridDestroyGameObject(x, y);
				keys--;
			} else { return; }
		}
		// Collecting keys
		if (GameManager.Instance.GridTag(x, y) == "Key") { // Collect the key
			GameManager.Instance.GridDestroyGameObject(x, y);
			keys++;
		}
		// Collecting potions
		if (GameManager.Instance.GridTag(x, y) == "Potion") { // Collect the key
			GameManager.Instance.GridDestroyGameObject(x, y);
			potions++;
		}
		// Collecting treasure
		if (GameManager.Instance.GridTag(x, y) == "Treasure") { // Collect the key
			GameManager.Instance.GridDestroyGameObject(x, y);
			treasure++;
		}
		// End of the level
		if (GameManager.Instance.GridTag(x, y) == "Exit") { // Collect the key
			transform.position = new Vector3(-1, -1);
			GameManager.Instance.NextLevel();
			return;
		}

		transform.position = new Vector3(x, y);

		// Check for collision with monsters
		try {
			GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
			foreach (GameObject m in monsters) {
				if (m.transform.position == transform.position)
					GameOver();
			}
		} catch { Debug.Log("Error here"); }

		if (remainingTurns > 1)
			remainingTurns--;
		else {
			//enemies make a turn
			try {
				if (moveEvent != null)
					moveEvent();
				remainingTurns = playerMovesPerMonsterTurn;
			} catch { Debug.Log("Error with events"); }
		}
	}

	public void GameOver() {
		/* Restart the game */
		Debug.Log("GameOver");
		FindObjectOfType<UI>().GameOver();
		gameOver = true;
		remainingTurns = 0;
		keys = 0;
		potions = 0;
		treasure = 0;

		// Remove all delegates
		MonsterController[] monsters = FindObjectsOfType<MonsterController>();
		foreach(MonsterController m in monsters) {
			moveEvent -= m.Move;
		}

		//GameManager.Instance.DestryLevel();
		//Destroy(gameObject);
	}
}
