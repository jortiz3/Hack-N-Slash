using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SurvivalSpawner))]
class SurvivalSpawnerEditor : Editor {

	SerializedProperty waves;

	void OnEnable() {
		waves = serializedObject.FindProperty ("waveList");
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (waves, true);
		serializedObject.ApplyModifiedProperties ();
	}
}