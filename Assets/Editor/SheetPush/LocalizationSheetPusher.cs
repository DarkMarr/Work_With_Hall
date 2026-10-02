using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;
using UnityEngine;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Sends the Item string table up to the Localization spreadsheet it is bound to.
    ///
    /// Needed because the binding only runs one way in practice: people edit the sheet and pull.
    /// Anything written on the Unity side — the outfit names and descriptions imported from
    /// ALL ITEM DATA, and the carry-on keys someone added before that — exists only in the project
    /// until it is pushed. The collection pulls with RemoveMissingPulledKeys, so the first pull
    /// after that would delete every one of them.
    ///
    /// Pushing is safe in the direction that matters here: the table is a superset of the sheet,
    /// and its values for the rows the sheet already had match what is there.
    /// </summary>
    public static class LocalizationSheetPusher
    {
        private const string COLLECTION = "Item";

        [MenuItem("QuizGame/Setup/Push Item Text To Google Sheet")]
        public static async void Push()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(COLLECTION);
            if (collection == null)
            {
                Debug.LogError($"[SheetPush] String table collection '{COLLECTION}' not found.");
                return;
            }

            var extension = collection.Extensions.OfType<GoogleSheetsExtension>().FirstOrDefault();
            if (extension == null)
            {
                Debug.LogError($"[SheetPush] '{COLLECTION}' has no Google Sheets extension to push through.");
                return;
            }
            if (extension.SheetsServiceProvider == null)
            {
                Debug.LogError("[SheetPush] The extension has no sheets service provider.");
                return;
            }

            var keys = collection.SharedData.Entries.Count;
            Debug.Log($"[SheetPush] Pushing {keys} key(s) from '{COLLECTION}' to spreadsheet "
                + $"{extension.SpreadsheetId}, sheet {extension.SheetId}. Columns: "
                + string.Join(", ", extension.Columns.Select(c => c.Column).ToArray()));

            var provider = extension.SheetsServiceProvider;
            if (!await Authorize(provider)) return;

            var sheets = new GoogleSheets(provider) { SpreadSheetId = extension.SpreadsheetId };
            try
            {
                await sheets.PushStringTableCollectionAsync(
                    extension.SheetId, collection, extension.Columns, new ProgressBarReporter());
            }
            catch (Exception exception)
            {
                Debug.LogError("[SheetPush] Push failed, nothing was written: " + exception);
                return;
            }

            Debug.Log("[SheetPush] Push finished. The sheet is the source of truth again, so a pull "
                + "will no longer delete these keys.");
        }

        /// <summary>
        /// Signs in and hands the finished service to the provider, instead of letting the provider
        /// sign in by itself.
        ///
        /// Its own <c>AuthorizeOAuth</c> calls <c>RunSynchronously</c> on the task the Google
        /// broker returns, and that task is already running, so it throws every time there is no
        /// cached token — which is exactly the first run. Awaiting the same broker call works, so
        /// this does that and then puts the result where the provider keeps it. Everything after
        /// that is the package's own code path, and the provider stays the real asset, so the
        /// OAuth-only parts of a push still recognise it as OAuth.
        ///
        /// The first run opens a browser for the account that owns the spreadsheet to approve.
        /// The token is cached under Library/Google afterwards.
        /// </summary>
        private static async Task<bool> Authorize(SheetsServiceProvider provider)
        {
            var field = typeof(SheetsServiceProvider)
                .GetField("m_SheetsService", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogError("[SheetPush] Could not reach the provider's cached service. The "
                    + "localization package has changed; check SheetsServiceProvider.");
                return false;
            }
            if (field.GetValue(provider) != null) return true;

            Debug.Log("[SheetPush] Signing in to Google. A browser window may open — approve it with "
                + "the account that owns the spreadsheet. Waiting up to three minutes.");

            using (var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(3)))
            {
                try
                {
                    var credential = await provider.AuthorizeOAuthAsync(cancellation.Token);
                    field.SetValue(provider, new SheetsService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = provider.ApplicationName
                    }));
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogError("[SheetPush] Sign-in failed, nothing was written: " + exception.Message);
                    return false;
                }
            }
        }
    }
}
