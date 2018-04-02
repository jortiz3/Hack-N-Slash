using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterSpawner))]
class CharacterSpawnerEditor : Editor {

	SerializedProperty locs;
	SerializedProperty seqLocs;


	SerializedProperty spawns;
	SerializedProperty seqSpawns;

	SerializedProperty randomize;

	SerializedProperty spawnRange;

	void OnEnable() {
		locs = serializedObject.FindProperty ("spawnLocations");
		seqLocs = serializedObject.FindProperty ("sequentialSpawnLocations");

		spawns = serializedObject.FindProperty ("spawnList");
		seqSpawns = serializedObject.FindProperty ("sequentialSpawns");

		randomize = serializedObject.FindProperty ("randomizeAll");

		spawnRange = serializedObject.FindProperty ("spawnTimeRange");
	}

	public override void OnInspectorGUI () {
		randomize.boolValue = EditorGUILayout.ToggleLeft ("Randomize All", randomize.boolValue);

		EditorGUILayout.Space ();

		if (!randomize.boolValue)
			EditorGUILayout.PropertyField (spawnRange, true);

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (locs, true);
		if (!randomize.boolValue)
			seqLocs.boolValue = EditorGUILayout.ToggleLeft ("Spawn using locations sequentially", seqLocs.boolValue);

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (spawns, true);
		if (!randomize.boolValue)
			seqSpawns.boolValue = EditorGUILayout.ToggleLeft ("Instantiate spawns sequentially", seqSpawns.boolValue);

		serializedObject.ApplyModifiedProperties ();
	}
}