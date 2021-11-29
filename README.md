This is an example repo for a bug I encountered in Fable with Feliz.ReactComponent.

Compare the Caller.js in [good](./good/out/shared/Caller.js) and
[bad](./bad/out/shared/Caller.js).  
The `fn` field is compiled differently:
- in 'good': `fn: (a) => ((b) => fn(a, b))`
- in 'bad': `fn: (a, b) => fn(a, b)`

In the receiving function however it's called as if it were in curried form, so
the 'bad' example will crash, since it's arguments are tupled:

```
return myComponentInputProps.fn(1)(2);
                                  ^

TypeError: myComponentInputProps.fn(...) is not a function
```

First observed in Fable 3.3.0 with Feliz.ReactComponent, so this example is
kinda built from that starting point.

The compiler plugin used here is a trimmed down version of
ReactComponentAttribute, where I tried change the rewritten AST to not contain
`Any`, but the types we know to be expected. Unfortunately this didn't change a
thing. See [MyReactComponent.fs](./plugin/MyReactComponent.fs)

Use `dotnet fsi run.fsx` to build and run the thing through node.