module Good.Caller

open Component

let foo =
    let fn = fun a b -> a + b
    MyComponent fn "foo"