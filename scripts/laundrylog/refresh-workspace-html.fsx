#r "../../src/CheddarBooks.LaundryLog.UI/bin/Debug/net10.0/CheddarBooks.LaundryLog.UI.dll"

open System
open System.IO
open CheddarBooks.LaundryLog.UI

let repoRoot =
    Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

let workspaceRoot =
    Path.Combine(repoRoot, "workspace", "laundrylog", "html")

let ensureDirectory (path: string) =
    Directory.CreateDirectory(path) |> ignore

let writeFile (path: string) (content: string) =
    let directoryPath =
        Path.GetDirectoryName(path)

    if not (String.IsNullOrWhiteSpace(directoryPath)) then
        ensureDirectory directoryPath

    File.WriteAllText(path, content)

let utcNow = DateTime.UtcNow

let updateState : ScreenPathUpdateState =
    { Version = String.Format("screen-path::{0:yyyyMMddHHmmssfff}", utcNow)
      UpdatedAtUtc = utcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
      PollIntervalMs = 3000 }

let nmUpdateState : NmPathUpdateState =
    { Version = String.Format("nm-path::{0:yyyyMMddHHmmssfff}", utcNow)
      UpdatedAtUtc = utcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
      PollIntervalMs = 3000 }

let aemPathHtml =
    SliceHtmlRenderer.renderDocument
        (SliceRenderOptions.classicEventModel "LaundryLog PATH 1 CommandSlice/ViewSlice")
        (SliceHtmlExamples.path1ManualLocationWasherDryer ())

let screenHtml =
    ScreenHtmlRenderer.renderDocument
        "LaundryLog Screen Components"
        "Deterministic HTML/CSS proving ground for the current LaundryLog screens."
        (ScreenHtmlExamples.laundryLogBaseScreens ())

let screenPathState = ScreenPathHtmlExamples.path1StartupToFirstEntry ()
let nmPathState = NmPathHtmlExamples.path1FirstLaunchFirstEntry ()

let screenPathHtml =
    ScreenPathHtmlRenderer.renderDocumentWithUpdateState screenPathState (Some updateState)

let screenPathUpdateManifest =
    ScreenPathHtmlRenderer.renderUpdateManifestScript updateState

let nmPathHtml =
    NmPathHtmlRenderer.renderDocumentWithUpdateState nmPathState (Some nmUpdateState)

let nmPathUpdateManifest =
    NmPathHtmlRenderer.renderUpdateManifestScript nmUpdateState

let aemPathFile =
    Path.Combine(workspaceRoot, "aem-paths", "LaundryLog_PATH1_CommandSlice_ViewSlice.html")

let screenFile =
    Path.Combine(workspaceRoot, "screens", "LaundryLog_ScreenComponents.html")

let screenPathFile =
    Path.Combine(workspaceRoot, "screen-paths", "LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.html")

let screenPathUpdateFile =
    Path.Combine(workspaceRoot, "screen-paths", "LaundryLog_PATH1_AppStarted_NeedLocation_FirstEntry.update.js")

let nmPathFile =
    Path.Combine(workspaceRoot, "nm-paths", "LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.html")

let nmPathUpdateFile =
    Path.Combine(workspaceRoot, "nm-paths", "LaundryLog_PATH1_NM_FirstLaunch_FirstEntry.update.js")

writeFile aemPathFile aemPathHtml
writeFile screenFile screenHtml
writeFile screenPathFile screenPathHtml
writeFile screenPathUpdateFile screenPathUpdateManifest
writeFile nmPathFile nmPathHtml
writeFile nmPathUpdateFile nmPathUpdateManifest

printfn "Refreshed workspace HTML artifacts:"
printfn "- %s" aemPathFile
printfn "- %s" screenFile
printfn "- %s" screenPathFile
printfn "- %s" screenPathUpdateFile
printfn "- %s" nmPathFile
printfn "- %s" nmPathUpdateFile
printfn "Screen path update version: %s" updateState.Version
printfn "NM path update version: %s" nmUpdateState.Version
