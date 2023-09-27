using UnityEditor; 
using UnityEngine;
using System;
using System.Reflection;


public class ReloadCsproj: Editor
{
    [MenuItem("Tools/Regen .csproj files")]
    public static void NewMenuOption(){
        SyncSolution(); 
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void SyncSolution()
    {

      var T = Type.GetType("UnityEditor.SyncVS,UnityEditor");
      Debug.Log(String.Format("[SSln]: Got type: {0}", T));
      var syncSolution = T.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static);
      Debug.Log(String.Format("[SSln]: Got method: {0}", syncSolution));
      syncSolution.Invoke(null,null);
    }
}
