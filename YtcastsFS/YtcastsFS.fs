module YtcastsFS

open FSharp.Data
open System
open System.Diagnostics
open System.IO

type URL = URL of string

let handleUrl url =
  let document = HtmlDocument.Load(url : string)
  document.CssSelect "a.yt-uix-tile-link"
  |> List.choose (fun x ->
    x.TryGetAttribute "href"
    |> Option.map (fun x -> x.Value()))
  |> List.map ((+) "https://youtube.com" >> URL)

let watchedFile = "./ytcasts.watched.txt"

[<EntryPoint>]
let main argv =
  let watched =
    File.ReadAllLines(watchedFile)
    |> Array.map URL
    |> Set.ofArray
  let writeLine str = File.AppendAllText(watchedFile, "\n" + str, Text.UTF8Encoding())
  let links =
    List.ofArray argv
    |> List.collect handleUrl
    |> Set.ofList
  for URL link in Set.difference links watched do
    let ytdl =
      new Process(
        StartInfo = ProcessStartInfo(
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          FileName = "youtube-dl",
          Arguments = "-o downloads/%(title)s.%(ext)s -x --audio-format mp3 " + link))
    ignore <| ytdl.Start()
    ytdl.BeginOutputReadLine()
    ytdl.WaitForExit()
    printfn "downloaded %s" link
    writeLine link
  printfn "done!"
  0 // return an integer exit code
