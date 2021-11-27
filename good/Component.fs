module Good.Component

open Plugin

[<MyReactComponent>]
let MyComponent fn _oneMoreParam =
    let value = fn 1 2
    value

