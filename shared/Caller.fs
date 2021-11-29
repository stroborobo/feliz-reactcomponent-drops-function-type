module Good.Caller

open Component

let fn a b = a + b
MyComponent fn "ignored" |> ignore