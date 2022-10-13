using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Unity.Services.Ccd.Management.Models;
using System.IO;
using Unity.Services.Ccd.Management.Http;
using System.Security.Cryptography;

namespace Unity.Services.Ccd.Management.Samples
{
    /// <summary>
    /// Sample window to show CCD examples
    ///
    /// SETUP:
    /// 1. Sign in to your Unity developer account
    /// 2. Link your project to a CCD-enabled cloud project
    /// 3. Open window via Window > CCD Sample Window in the toolbar
    /// 4. Run and observe debug logs
    ///
    /// </summary>
    class CcdSampleWindow : EditorWindow
    {
        const string WINDOW_MESSAGE =
@"To view the code:
    1. Navigate to Assets > Samples > CCD Management > {version} > CCD Sample Window
    2. Open 'CCDSampleWindow.cs'
To Setup the Demo:
    1. Sign in to your Unity developer account.
    2. Link your project to a CCD-enabled cloud project via the Services window.
    3. Click 'Begin CCD Sample Demonstration' button and observe debug logs";

        /// <summary>
        /// The name of the file to create. This is mainly for uploading a test file.
        /// </summary>
        const string FILENAME = "TestFileName.txt";
        /// <summary>
        /// The size of the file to create.
        /// </summary>
        const int FILESIZE = 5 * 1024 * 1024;

        /// <summary>
        /// Used to avoid clicking more than once
        /// </summary>
        bool isLoading = false;

        /// <summary>
        /// Used to determine whether or not to clean up the resources after the demo
        /// </summary>
        bool cleanUpResources = false;


        [MenuItem("Window/CCD Sample Window")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CcdSampleWindow), false, "CCD Sample Window");
        }

        async void OnGUI()
        {
            CcdBucket bucket;
            /*
             * Description GUI
             */
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("This is a demonstration on how to utilize the CCD Management SDK", EditorStyles.largeLabel);
            EditorGUILayout.Space(3);
            EditorStyles.helpBox.fontSize = 14;
            EditorGUILayout.HelpBox(WINDOW_MESSAGE, MessageType.Info);

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Options:", EditorStyles.largeLabel);
            EditorGUI.BeginDisabledGroup(isLoading);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Clean Up Resources After Demo"));
            cleanUpResources = EditorGUILayout.Toggle(cleanUpResources); ;
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            var button = GUILayout.Button("Begin CCD Sample Demonstration");
            EditorGUI.EndDisabledGroup();
            if (button)
            {
                try
                {
                    isLoading = true;

                    // To change the CCD environment, utilize this call. By default, CCD will use the default Dashboard environment.
                    //CcdManagement.SetEnvironmentId("<environment_id_here>");

                    //Beginning Org calls
                    var org = await CcdManagement.Instance.GetOrgAsync();
                    Debug.unityLogger.Log("Ccd", $"Org: {org.Name} ({org.Id})");

                    var orgUsage = await CcdManagement.Instance.GetOrgUsageAsync();
                    Debug.unityLogger.Log("Ccd", $"Usage: {string.Join(", ", orgUsage.Usage.Select(x => $"{x.Projectguid}:{x.Quantity}"))}");
                    //Beginning Users calls
                    var userInfo = await CcdManagement.Instance.GetUserInfoAsync();
                    Debug.unityLogger.Log("Ccd", $"User: {userInfo.Name}");

                    //Uncomment to run
                    //var userApiKey = await CcdManagement.Instance.GetUserApiKeyAsync();
                    //Debug.unityLogger.Log($"User Api Key: {userApiKey.Apikey}");

                    //Uncomment to run
                    //var regenKey = await CcdManagement.Instance.RegenerateUserApiKeyAsync();
                    //Debug.unityLogger.Log($"New Api Key: {regenKey.Apikey}");

                    //Listing out existing buckets
                    var existingBuckets = await CcdManagement.Instance.ListBucketsAsync();
                    Debug.unityLogger.Log("Ccd", $"Buckets: {string.Join(", ", existingBuckets.Select(b => b.Name))}");

                    //Creates a new bucket
                    bucket = await CcdManagement.Instance.CreateBucketAsync(new CreateBucketOptions("CCD Sample Test Bucket")
                    {
                        Description = "This is a bucket created by the CCD Sample Script"
                    });
                    Debug.unityLogger.Log("Ccd", $"Created {bucket.Name} ({bucket.Id})");

                    //Updates the newly created bucket
                    bucket = await CcdManagement.Instance.UpdateBucketAsync(new UpdateBucketOptions(bucket.Id, "Updated CCD Sample Test Bucket")
                    {
                        Description = "This is an bucket updated by the CCD Sample Script"
                    });
                    Debug.unityLogger.Log("Ccd", $"Updated {bucket.Name} ({bucket.Id})");

                    //Creating Test File
                    CreateOrUpdateTestFile();
                    var entryModelOptions =
                        new EntryModelOptions(FILENAME, GetMd5Hash(FILENAME), FILESIZE)
                        {
                            ContentType = "application/txt"
                        };
                    var entry = await CcdManagement.Instance.CreateEntryAsync(bucket.Id, entryModelOptions);
                    Debug.unityLogger.Log("Ccd", $"Created Entry: {entry.Entryid}");

                    //Upload the file
                    using (var file = File.OpenRead(FILENAME))
                    {
                        await CcdManagement.Instance.UploadContentAsync(
                            new UploadContentOptions(bucket.Id, entry.Entryid, file)
                            {
                                ChunkSize = 1
                            });
                    }

                    var contentStatus = await CcdManagement.Instance.GetContentStatusAsync(new EntryOptions(bucket.Id, entry.Entryid));
                    Debug.unityLogger.Log("Ccd", $"Status of {entry.Entryid} - {(contentStatus.UploadOffset / contentStatus.UploadLength) * 100}% Uploaded. Retrieved by Entry Id");

                    //Retrieve information about an entry
                    entry = await CcdManagement.Instance.GetEntryAsync(bucket.Id, entry.Entryid);
                    Debug.unityLogger.Log("Ccd", $"Retrieved entry \"{entry.Path}\"");

                    //Updating an entry
                    entryModelOptions.ContentType = "txt";
                    entry = await CcdManagement.Instance.UpdateEntryAsync(new EntryOptions(bucket.Id, entry.Entryid), entryModelOptions);
                    Debug.unityLogger.Log("Ccd", $"Updated entry content type to \"{entry.ContentType}\"");

                    //Updating an entry by its path
                    entryModelOptions.ContentType = "application/txt";
                    entry = await CcdManagement.Instance.UpdateEntryByPathAsync(new EntryByPathOptions(bucket.Id, entry.Path), entryModelOptions);
                    Debug.unityLogger.Log("Ccd", $"Updated entry content type to \"{entry.ContentType}\"");

                    var release1 = await CcdManagement.Instance.CreateReleaseAsync(new CreateReleaseOptions(bucket.Id)
                    {
                        Notes = "This is an automated generated release"
                        //Entries can be added here to specify which items you want in a new release.
                        //If entries are not specified, every item in the bucket will be released.
                    });
                    Debug.unityLogger.Log("Ccd", $"Created release {release1.Releasenum} ({release1.Releaseid})");

                    release1 = await CcdManagement.Instance.UpdateReleaseAsync(bucket.Id, release1.Releaseid, "Updated release notes");
                    Debug.unityLogger.Log("Ccd", $"Updated the release notes to \"{release1.Notes}\"");


                    //Rewrite the file
                    CreateOrUpdateTestFile();
                    //Regenerate hash for updates
                    entryModelOptions.ContentHash = GetMd5Hash(FILENAME);
                    entryModelOptions.UpdateIfExists = true;
                    entry = await CcdManagement.Instance.CreateOrUpdateEntryByPathAsync(new EntryByPathOptions(bucket.Id, entry.Path), entryModelOptions);
                    Debug.unityLogger.Log("Ccd", $"Updated entry hash to \"{entry.ContentHash}\"");

                    //Upload the file again
                    using (var file = File.OpenRead(FILENAME))
                    {
                        await CcdManagement.Instance.UploadContentAsync(
                            new UploadContentOptions(bucket.Id, entry.Entryid, file)
                            {
                                ChunkSize = 1
                            });
                    }

                    //Get the content status of a specified version
                    contentStatus = await CcdManagement.Instance.GetContentStatusVersionAsync(new EntryVersionsOptions(bucket.Id, entry.Entryid, entry.CurrentVersionid));
                    Debug.unityLogger.Log("Ccd", $"Status of {entry.Entryid} - {(contentStatus.UploadOffset / contentStatus.UploadLength) * 100}% Uploaded. Retrieved by Version Id");

                    //Retrieve an entry by path
                    entry = await CcdManagement.Instance.GetEntryByPathAsync(new EntryByPathOptions(bucket.Id, FILENAME));
                    Debug.unityLogger.Log("Ccd", $"Retrieved entry by path {entry.Path}");

                    var release2 = await CcdManagement.Instance.CreateReleaseAsync(new CreateReleaseOptions(bucket.Id)
                    {
                        Notes = "This is an automated generated release"
                        //Entries can be added here to specify which items you want in a new release.
                        //If entries are not specified, every item in the bucket will be released.
                    });
                    Debug.unityLogger.Log("Ccd", $"Created release {release2.Releasenum} ({release2.Releaseid})");


                    var releaseChangeVersion = await CcdManagement.Instance.GetReleaseDiffAsync(
                        new ReleaseDiffOptions(bucket.Id, release1.Releaseid, release2.Releaseid)
                    );
                    Debug.unityLogger.Log("Ccd", $"Changes - Add: {releaseChangeVersion.Add}, Update: {releaseChangeVersion.Update}, Delete: {releaseChangeVersion.Delete}, Unchanged: {releaseChangeVersion.Unchanged}");

                    //Create a badge. When creating a badge, you can specify a ReleaseId or a Release Number but not both.
                    var badge1 = await CcdManagement.Instance.AssignBadgeAsync(new AssignBadgeOptions(bucket.Id, "AutomatedBadge", release1.Releaseid));
                    Debug.unityLogger.Log("Ccd", $"Created badge \"{badge1.Name}\"");

                    var badges = await CcdManagement.Instance.ListBadgesAsync(bucket.Id);
                    Debug.unityLogger.Log("Ccd", $"Badge List: {string.Join(", ", badges.Select(b => b.Name))}");

                    release1 = await CcdManagement.Instance.GetReleaseByBadgeAsync(bucket.Id, badge1.Name);
                    Debug.unityLogger.Log("Ccd", $"Retrieved release {release1.Releasenum} ({release1.Releaseid}) by badge \"{badge1.Name}\"");

                    //Delete a badge
                    //await CcdManagement.Instance.DeleteBadgeAsync(bucket.Id, badge1.Name);

                    //Setting bucket to promotion-only bucket
                    var permission = await CcdManagement.Instance.CreatePermissionAsync(
                        new CreatePermissionsOption(bucket.Id, CcdPermissionCreate.ActionOptions.Write, CcdPermissionCreate.PermissionOptions.Deny)
                    );
                    Debug.unityLogger.Log("Ccd", $"Created permission \"{permission.Action}\" to \"{permission.Permission}\"");


                    //Expect an error because bucket is not promotion-only
                    try
                    {
                        var errorEntryModelOptions = new EntryModelOptions($"{Guid.NewGuid()}/{entry.Path}", entry.ContentHash, entry.ContentSize)
                        {
                            ContentType = entry.ContentType
                        };
                        var errorEntry = await CcdManagement.Instance.CreateEntryAsync(bucket.Id, errorEntryModelOptions);
                    }
                    catch (CcdManagementException e)
                    {
                        Debug.unityLogger.Log("Ccd", $"Expected Exception - Error {e.ErrorCode}: {e.Message}");
                    }

                    //Updating permissions
                    permission = await CcdManagement.Instance.UpdatePermissionAsync(
                        new UpdatePermissionsOption(bucket.Id, CcdPermissionUpdate.ActionOptions.Write, CcdPermissionUpdate.PermissionOptions.Allow)
                    );
                    Debug.unityLogger.Log("Ccd", $"Updated permission \"{permission.Action}\" to \"{permission.Permission}\"");

                    //Deleting a permission
                    await CcdManagement.Instance.DeletePermissionAsync(
                        new UpdatePermissionsOption(bucket.Id, CcdPermissionUpdate.ActionOptions.Write, CcdPermissionUpdate.PermissionOptions.Allow)
                    );

                    if (cleanUpResources)
                    {
                        await CcdManagement.Instance.DeleteBucketAsync(bucket.Id);
                        File.Delete(FILENAME);
                        Debug.unityLogger.Log("Ccd", $"Successfully cleaned up resources");
                    }
                    isLoading = false;
                }
                finally
                {
                    isLoading = false;
                }
                Debug.unityLogger.Log("Ccd", "Finished Demo!");
            }
        }

        private void CreateOrUpdateTestFile()
        {
            byte[] data = new byte[FILESIZE];
            System.Random rng = new System.Random();
            rng.NextBytes(data);
            File.WriteAllBytes(FILENAME, data);
        }

        private string GetMd5Hash(string path)
        {
            string hashString;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hash = md5.ComputeHash(stream);
                    hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            return hashString;
        }
    }
}
