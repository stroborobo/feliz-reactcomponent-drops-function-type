import { MyComponent } from "./Component.js";

export function fn(a, b) {
    return a + b;
}

void MyComponent({
    fn: (a) => ((b) => fn(a, b)),
    _oneMoreParam: "ignored",
});

