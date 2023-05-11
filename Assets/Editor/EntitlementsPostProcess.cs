using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections;
using System.IO;
using System;

public class EntitlementsPostProcess : ScriptableObject
{
    public DefaultAsset entitlementFile = null;

    [PostProcessBuild(999)] 
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        var dummy = ScriptableObject.CreateInstance<EntitlementsPostProcess>();
        var file = dummy.entitlementFile;
        ScriptableObject.DestroyImmediate(dummy);
        if (file == null)
        {
            return;
        }

        // xproj
        var proj_path = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromFile(proj_path);

        // Copy entitlements
        var target_name = "Unity-iPhone";
        var src = AssetDatabase.GetAssetPath(file);
        var file_name = Path.GetFileName(src);
        var dst = Path.Combine(pathToBuiltProject, target_name, file_name);
        FileUtil.CopyFileOrDirectory(src, dst);
        proj.AddFile(Path.Combine(target_name, file_name), file_name);

        string target_guid = proj.GetUnityMainTargetGuid();
        
        proj.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", target_name + "/" + file_name);

        // End of proj write
        proj.WriteToFile(proj_path);
    }
}