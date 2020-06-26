// Learn more about F# at http://fsharp.org

open System
open Iot.Device.CpuTemperature
open System.Threading

[<EntryPoint>]
let main argv =

    let cpuTemperature = CpuTemperature()

    while true do
        if cpuTemperature.IsAvailable then
            printfn "%A" cpuTemperature.Temperature.Celsius
    
        Thread.Sleep(2000)

    0 // return an integer exit code
