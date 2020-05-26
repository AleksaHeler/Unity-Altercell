/*https://en.wikipedia.org/wiki/A*_search_algorithm*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour {

	public float viewDistance = 5;
	public Sprite normal;
	public Sprite alerted;
	Transform player;
	Vector2 start;
	Vector2 goal;
	List<Vector2> openSet = new List<Vector2>();
	List<Vector2> closeSet = new List<Vector2>();
	List<Vector2> total_path = new List<Vector2>();
	Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
	Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>();
	Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>();

	private void Start() {
		FindObjectOfType<PlayerController>().moveEvent += Move;
	}

	public void Move() {
		// Get target player
		player = GameObject.FindGameObjectWithTag("Player").transform;

		// If the player is too far away, dont move
		if (Vector2.Distance(transform.position, player.position) > viewDistance) {
			GetComponent<SpriteRenderer>().sprite = normal;
			return;
		}
		// If the player is close, change the sprite to the one with '!'
		GetComponent<SpriteRenderer>().sprite = alerted;

		//if (h(player.position) > viewDistance * viewDistance) { return; }
		// Get the best path towards it
		List<Vector2> path = AStar();
		// If we can, move
		if (path.Count > 0) {
			Vector2 nextPos = path[1];
			transform.position = new Vector3(nextPos.x, nextPos.y);
		}
		if(transform.position == player.position) {
			FindObjectOfType<PlayerController>().GameOver();
		}
	}

	// A* function
	// Returns the path as a list of vectors
	private List<Vector2> AStar() {

		start = transform.position;
		goal = player.position;

		// The set of discovered nodes that may need to be (re-)expanded.
		// Initially, only the start node is known (current position)
		openSet = new List<Vector2>();
		openSet.Add(start);

		// List of nodes already discovered and explored. 
		// Starts off empty. Once a node has been 'current' it then goes here
		closeSet = new List<Vector2>();

		// For node n, cameFrom[n] is the node immediately preceding it
		// on the cheapest path from start to n currently known.
		cameFrom = new Dictionary<Vector2, Vector2>();

		// For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
		gScore = new Dictionary<Vector2, float>();
		gScore.Add(start, 0);

		// For node n, fScore[n] := gScore[n] + h(n)
		// fScore[n] represents our current best guess as to how short
		// a path from start to finish can be if it goes through n.
		fScore = new Dictionary<Vector2, float>();
		fScore.Add(start, h(start));

		while (openSet.Count >= 0) {
			// This operation can occur in O(1) time if openSet is a min-heap or a priority queue
			Vector2 current = LowestFScore();

			if (current == goal) {
				return reconstruct_path(current);
			}

			// Current node goes into the closed set
			closeSet.Add(current);
			openSet.Remove(current);

			List<Vector2> neighbors = GetNeigbors(current);
			foreach (Vector2 neighbor in neighbors) {
				// Just set the default value to inf if these are new items
				if (!gScore.ContainsKey(current)) { gScore.Add(current, Mathf.Infinity); }
				if (!gScore.ContainsKey(neighbor)) { gScore.Add(neighbor, Mathf.Infinity); }

				// tentative_gScore is the distance from start to the neighbor through current
				float tentative_gScore = gScore[current] + 1;

				if (tentative_gScore < gScore[neighbor]) {
					// This path to neighbor is better than any previous one. Record it!
					if (cameFrom.ContainsKey(neighbor))
						cameFrom[neighbor] = current;
					else
						cameFrom.Add(neighbor, current);

					if (gScore.ContainsKey(neighbor))
						gScore[neighbor] = tentative_gScore;
					else
						gScore.Add(neighbor, tentative_gScore);

					if (fScore.ContainsKey(neighbor))
						fScore[neighbor] = gScore[neighbor] + h(neighbor);
					else
						fScore.Add(neighbor, gScore[neighbor] + h(neighbor));

					if (!closeSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					}
				}
			}
		}
		return new List<Vector2>();
	}

	// returns the node from openSet that has the lowest fScore[] value
	private Vector2 LowestFScore() {
		Vector2 lowestVector = openSet[0];
		float lowestScore = h(openSet[0]);

		foreach (Vector2 v in openSet) {
			// If there is no value for this vector, add it
			if (!fScore.ContainsKey(v))
				fScore[v] = h(v);

			if (fScore[v] < lowestScore) {
				lowestScore = h(v);
				lowestVector = v;
			}
		}
		return lowestVector;
	}

	// h is the heuristic function. h(n) estimates the cost to reach goal from node n.
	private float h(Vector2 n) {
		return (goal.x - n.x) * (goal.x - n.x) + (goal.y - n.y) * (goal.y - n.y);
	}

	private List<Vector2> GetNeigbors(Vector2 current) {
		List<Vector2> neighbors = new List<Vector2>();

		// If this tile is not a wall add it to neighbors
		if (!GameManager.Instance.IsWall((int)current.x + 1, (int)current.y) && !GameManager.Instance.IsMonster((int)current.x + 1, (int)current.y)) { neighbors.Add(new Vector2((int)current.x + 1, (int)current.y)); }
		if (!GameManager.Instance.IsWall((int)current.x - 1, (int)current.y) && !GameManager.Instance.IsMonster((int)current.x - 1, (int)current.y)) { neighbors.Add(new Vector2((int)current.x - 1, (int)current.y)); }
		if (!GameManager.Instance.IsWall((int)current.x, (int)current.y + 1) && !GameManager.Instance.IsMonster((int)current.x, (int)current.y + 1)) { neighbors.Add(new Vector2((int)current.x, (int)current.y + 1)); }
		if (!GameManager.Instance.IsWall((int)current.x, (int)current.y - 1) && !GameManager.Instance.IsMonster((int)current.x, (int)current.y - 1)) { neighbors.Add(new Vector2((int)current.x, (int)current.y - 1)); }

		return neighbors;
	}

	private List<Vector2> reconstruct_path(Vector2 current) {
		total_path = new List<Vector2>();
		total_path.Add(current);
		while (cameFrom.ContainsKey(current)) {
			current = cameFrom[current];
			total_path.Insert(0, current);
		}
		return total_path;
	}

	~MonsterController() {
		FindObjectOfType<PlayerController>().moveEvent -= Move;
	}
}
