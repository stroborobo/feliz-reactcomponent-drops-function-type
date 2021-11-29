#!/usr/bin/env dotnet fsi

open System.IO
open System.Diagnostics

let run env dir tool args =
    printfn "===> [%s] %s %s" dir tool args
    use proc =
        let startInfo =
            ProcessStartInfo(tool, args,
                WorkingDirectory = dir,
                CreateNoWindow = true)
        env |> Seq.iter (fun (k, v) -> startInfo.Environment.Add(k, v))
        new Process(StartInfo = startInfo)
    proc.Start() |> ignore
    proc.WaitForExit()

let runIn = run Seq.empty

let buildAndRun dir =
    let outDir = Path.Combine (dir, "out")
    if Directory.Exists outDir then
        Directory.Delete(outDir, true)

    runIn dir "dotnet" "tool restore"
    runIn dir "dotnet" "fable -o out"
    runIn (Path.Combine (outDir, "shared")) "node" "Caller.js"

let main () =
    Directory.SetCurrentDirectory __SOURCE_DIRECTORY__

    runIn "plugin" "dotnet" "build -c Debug"

    buildAndRun "good"
    buildAndRun "bad"
    buildAndRun "latest"


main()