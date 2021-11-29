namespace Plugin 

open Feliz

open Fable
open Fable.AST 
open Fable.AST.Fable

// Tell Fable to scan for plugins in this assembly
[<assembly:ScanForPlugins>]
do()

/// <summary>Transforms a function into a React function component. Make sure the function is defined at the module level</summary>
type MyReactComponentAttribute() =
    inherit MemberDeclarationPluginAttribute()
    override _.FableMinimumVersion = "3.0"

    /// <summary>Transforms call-site into createElement calls</summary>
    override _.TransformCall(_compiler, memb, expr) =
        match expr with
        | Fable.Call(callee, info, typeInfo, range) when info.Args.Length > 1 ->
            let membParams = memb.CurriedParameterGroups |> List.concat
            let kvs =
                List.zip (List.take info.Args.Length membParams) info.Args
                |> List.collect (fun (membParam, expr) ->
                    match membParam.Name with
                    | Some name -> [name, expr, membParam.Type]
                    | None -> [])

            let propsType =
                kvs
                |> List.map (fun (name, _expr, typ) -> name, typ)
                |> AstUtils.makeAnonRecordType
            let propsExpr =
                kvs
                |> List.map (fun (name, expr, _typ) -> name, expr)
                |> AstUtils.objExprTyped propsType

            let info =
                { info with
                    Args = [ propsExpr ]
                    SignatureArgTypes = [ propsType ] }
            Fable.Call(callee, info, typeInfo, range)

        | _ ->
            // return expression as is when it is not a call expression
            expr

    override _.Transform(compiler, _file, decl) =
        if decl.Info.IsValue || decl.Info.IsGetter || decl.Info.IsSetter then
            // Invalid attribute usage
            let errorMessage = sprintf "Expecting a function declation for %s when using [<ReactComponent>]" decl.Name
            compiler.LogWarning(errorMessage, ?range=decl.Body.Range)
            decl
        else
            if decl.Args.Length = 1 && decl.Args.[0].Type = Fable.Type.Unit then
                // remove arguments from functions requiring unit as input
                { decl with Args = [ ] }
            else
                let args =
                    decl.Args
                    |> List.map (fun arg -> arg.DisplayName, arg.Type)
                let propsArg =
                    let name = sprintf "%sInputProps" (AstUtils.camelCase decl.Name)
                    let typ = AstUtils.makeAnonRecordType args
                    AstUtils.makeIdentTyped typ name

                let body =
                    ([], decl.Args) ||> List.fold (fun bindings arg ->
                        let getterKey = arg.DisplayName
                        let getterKind = Fable.ByKey(Fable.ExprKey(AstUtils.makeStrConst getterKey))
                        let getter = Fable.Get(Fable.IdentExpr propsArg, getterKind, arg.Type, None)
                        (arg, getter)::bindings)
                    |> List.rev
                    |> List.fold (fun body (k,v) -> Fable.Let(k, v, body)) decl.Body

                { decl with
                    Args = [propsArg]
                    Body = body }
