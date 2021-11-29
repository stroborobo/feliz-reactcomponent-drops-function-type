import { MyComponent } from "./Component.js";

export function fn(a, b) {
    return a + b;
}

MyComponent({
    fn: (a, b) => fn(a, b),
    _oneMoreParam: "ignored",
});

