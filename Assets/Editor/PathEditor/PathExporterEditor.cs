using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(PathExporter))]
public class PathExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //PathExporter exporter = (PathExporter)target;

        if (GUILayout.Button("ExportToJson"))
        {
           // exporter.ExportToJson();
        }

        if (GUILayout.Button("ExportToLua"))
        {
           // exporter.ExportToLua();
        }

        if (GUILayout.Button("ExportToExcel"))
        {
           // exporter.ExportToExcel();
        }
    }
}
