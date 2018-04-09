using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SurvivalSpawner))]
class SurvivalSpawnerEditor : Editor {

	SerializedProperty spawnRange;
	SerializedProperty waves;

	void OnEnable() {
		spawnRange = serializedObject.FindProperty ("spawnTimeRange");
		waves = serializedObject.FindProperty ("waveList");
	}

	public override void OnInspectorGUI () {

		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (spawnRange, true);
		EditorGUILayout.Space ();

		EditorGUILayout.PropertyField (waves, true);
		serializedObject.ApplyModifiedProperties ();
	}
}