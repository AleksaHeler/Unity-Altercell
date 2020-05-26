using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// Stats
	[Header("Stats")]
	public int currentLevel = 1;

	// Parameters
	[Header("Parameters")]
	public int startRoomCount = 3;
	// Sprites where each pixel is a tile
	public List<Texture2D> rooms; // there are 12 rooms, 1st is empty, 2nd is the exit

	[Header("Prefabs")]
	public GameObject playerPrefab;
	public GameObject wallPrefab;
	public GameObject doorPrefab;
	public GameObject exitPrefab;
	public GameObject monsterPrefab;
	public GameObject keyPrefab;
	public GameObject potionPrefab;
	public GameObject treasurePrefab;

	// Variables
	// Grid with coordinates of rooms
	List<int[]> gridList = new List<int[]>(); // (the array has two elements, x and y coordinate)
	List<Tile> grid = new List<Tile>();

	int keyCount = 0;
	bool levelHasTreasure = false;

	#region Singleton pattern
	private static GameManager _instance;
	public static GameManager Instance { get { return _instance; } }
	private void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	#endregion

	void Start() {
		//GenerateLevel();
	}

	#region Level generator
	public void GenerateLevel() {
		// Reset key count and gridList
		keyCount = 0;
		gridList = new List<int[]>();

		// Only 50% chance that this level has a treasure
		levelHasTreasure = Mathf.RoundToInt(Random.Range(0.0f, 1.0f)) == 0 ? false : true;

		// Add room with coordinates 0,0 to the grid
		gridList.Add(new int[] { 0, 0 }); // Add room at coordinates 0,0

		// List with indexes of squares that can become a room
		// (the array has two elements, x and y coordinate)
		List<int[]> list = new List<int[]>();
		// Get the four cells around 0,0 and add them to a list
		list.Add(new int[] { 0, 1 });
		list.Add(new int[] { 0, -1 });
		list.Add(new int[] { 1, 0 });
		list.Add(new int[] { -1, 0 });

		// Choose how many rooms we want based on current level
		int roomCount = startRoomCount + currentLevel;
		Debug.Log("Current level: " + currentLevel + "\t Room Count: " + roomCount);

		// Find coordinates of all rooms
		for (int i = 0; i < roomCount - 1; i++) {
			// Randomly select a coordinate from the list
			int index = Random.Range(0, list.Count);
			int[] coord = list[index];
			// Make it a room
			gridList.Add(new int[] { coord[0], coord[1] });
			// Remove it from the list of possible rooms
			list.RemoveAt(index);
			// Add adjacent rooms to the list if they are not a room already
			if (!ContainsArray(gridList, new int[] { coord[0] + 1, coord[1] })) list.Add(new int[] { coord[0] + 1, coord[1] });
			if (!ContainsArray(gridList, new int[] { coord[0] - 1, coord[1] })) list.Add(new int[] { coord[0] - 1, coord[1] });
			if (!ContainsArray(gridList, new int[] { coord[0], coord[1] + 1 })) list.Add(new int[] { coord[0], coord[1] + 1 });
			if (!ContainsArray(gridList, new int[] { coord[0], coord[1] - 1 })) list.Add(new int[] { coord[0], coord[1] - 1 });
		}

		InstantiateRoom(gridList[0][0], gridList[0][1], 0); // First will always be empty
		InstantiateRoom(gridList[gridList.Count - 1][0], gridList[gridList.Count - 1][1], 1); // Last will always be exit

		// Instantiate rooms
		for (int i = gridList.Count - 2; i >= 1; i--) {
			int[] room = gridList[i];
			InstantiateRoom(room[0], room[1], Random.Range(2, rooms.Count));
		}

		// In the end, spawn the player if there is already not one
		if (GameObject.FindGameObjectWithTag("Player") == null)
			Instantiate(playerPrefab, new Vector3(-1, -1), Quaternion.identity);
		else
			GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(-1, -1);
	}

	private void InstantiateRoom(int x, int y, int type) {
		int a = x, b = y; // room index
		x = x * 8 - 4; // world coordinates
		y = y * 8 - 4;
		// Loop trough the room type image
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 8; j++) {
				// Get the pixel color and spawn apropriate tile
				Color pixelColor = rooms[type].GetPixel(i, j);

				if (pixelColor.a == 0)
					continue;
				if (pixelColor.Equals(Color.black))
					Spawn(wallPrefab, x + i, y + j);
				if (pixelColor.Equals(Color.green)) { // Spawn one of these items
					if (keyCount < 3) { // 3 keys per level
						Spawn(keyPrefab, x + i, y + j);
						keyCount++;
					} else if (levelHasTreasure) { // Spawn the treasure if the level has it
						Spawn(treasurePrefab, x + i, y + j);
						levelHasTreasure = false;
					} else { // Spawn potions
						Spawn(potionPrefab, x + i, y + j);
					}
				}
				if (pixelColor.Equals(Color.red))
					Spawn(monsterPrefab, x + i, y + j);
				if (pixelColor.Equals(Color.blue))
					Spawn(exitPrefab, x + i, y + j);
				if (pixelColor.Equals(new Color(1, 1, 0))) // yellow
					Spawn(doorPrefab, x + i, y + j);
			}
		}

		// As we generate the room, check if the adjacent rooms are empty and fill walls accordingly
		if (!ContainsArray(gridList, new int[] { a + 1, b })) { // Generate walls right
			Spawn(wallPrefab, x + 7, y + 3);
			Spawn(wallPrefab, x + 7, y + 4);
		}
		if (!ContainsArray(gridList, new int[] { a - 1, b })) { // Generate walls left
			Spawn(wallPrefab, x, y + 3);
			Spawn(wallPrefab, x, y + 4);
		}
		if (!ContainsArray(gridList, new int[] { a, b + 1 })) { // Generate walls up
			Spawn(wallPrefab, x + 3, y + 7);
			Spawn(wallPrefab, x + 4, y + 7);
		}
		if (!ContainsArray(gridList, new int[] { a, b - 1 })) { // Generate walls down
			Spawn(wallPrefab, x + 3, y);
			Spawn(wallPrefab, x + 4, y);
		}
	}

	// Check if a list of arrays contains an array by value
	bool ContainsArray(List<int[]> l, int[] arr) {
		// Go trough all items
		foreach (int[] item in l) {
			// If it is a match return true
			if (item[0] == arr[0] && item[1] == arr[1])
				return true;
		}
		return false;
	}
	#endregion

	#region Helper functions
	/// <summary>
	/// Returns the gameobject at the given coordinates, or null if grid is empty
	/// </summary>
	public void GridDestroyGameObject(int x, int y) {
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y) {
				Destroy(t.obj);
				grid.Remove(t);
				return;
			}
		}
	}

	public GameObject GridGameObject(int x, int y) {
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y)
				return t.obj;
		}
		return null;
	}

	/// <summary>
	/// Returns the string tag of the gameobject at the given coordinates, or null if grid is empty
	/// </summary>
	public string GridTag(int x, int y) {
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y)
				return t.obj.tag;
		}
		return null;
	}

	/// <summary>
	/// Returns true if the given grid cell is empty
	/// </summary>
	public bool GridEmpty(int x, int y) {
		// Returns true if the grid is empty at the given coordinates
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Returns true if the given grid cell is a wall
	/// </summary>
	public bool IsWall(int x, int y) {
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y && t.obj.tag.Equals("Wall"))
				return true;
		}
		return false;
	}
	public bool IsMonster(int x, int y) {
		foreach (Tile t in grid) {
			if (t.x == x && t.y == y && t.obj.tag.Equals("Monster"))
				return true;
		}
		return false;
	}

	public void NextLevel() {
		// Destroy current level
		DestryLevel();
		// generate another level
		currentLevel++;
		GenerateLevel();
	}

	public void DestryLevel() {
		// Remove all delegates
		PlayerController player = FindObjectOfType<PlayerController>();
		MonsterController[] monsters = FindObjectsOfType<MonsterController>();
		foreach (MonsterController m in monsters) {
			player.moveEvent -= m.Move;
		}
		int gridCount = grid.Count;
		for (int i = gridCount - 1; i >= 0; i--) {
			Tile t = grid[i];
			// Don't remove the player
			if (t.obj.tag == "Player")
				continue;
			// Destroy gameobject and remove it from array
			Destroy(t.obj);
			grid.Remove(t);
		}
	}

	public void Restart() { 
		Debug.Log("Restart");
		DestryLevel();
		currentLevel = 1;
		GenerateLevel();
		FindObjectOfType<PlayerController>().gameOver = false;
	}
	#endregion

	// Instantiate a gameobject at the given coordinates
	private void Spawn(GameObject obj, int x, int y) {
		Tile item = new Tile();
		item.x = x;
		item.y = y;
		item.obj = Instantiate(obj, new Vector3(x, y), Quaternion.identity, transform);
		grid.Add(item);
	}
}
