using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SurvivalSpawner))]
class SurvivalSpawnerEditor : Editor {

	SerializedProperty spawns;

	SerializedProperty spawnRange;

	SerializedProperty warnings;

	void OnEnable() {
		spawns = serializedObject.FindProperty ("spawnList");

		spawnRange = serializedObject.FindProperty ("spawnTimeRange");

		warnings = serializedObject.FindProperty ("waveWarningsList");
	}

	public override void OnInspectorGUI () {

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (spawnRange, true);

		EditorGUILayout.Space ();

		//EditorGUILayout.PropertyField (locs, true);
		//seqLocs.boolValue = EditorGUILayout.ToggleLeft ("Spawn using locations sequentially", seqLocs.boolValue);

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (spawns, true);

		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (warnings, true);

		serializedObject.ApplyModifiedProperties ();
	}
}