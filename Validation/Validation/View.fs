﻿namespace Mvc.Wpf

open System
open System.Windows

type IView<'E, 'M> =
    inherit IObservable<'E>

    abstract SetBindings : 'M -> unit

[<AbstractClass>]
type View<'E, 'M, 'W when 'W :> Window and 'W : (new : unit -> 'W)>(?window) = 

    let window = defaultArg window (new 'W())

    member this.Window = window
    static member (?) (view : View<'E, 'M, 'W>, name) = 
        match view.Window.FindName name with
        | null -> 
            match view.Window.TryFindResource name with
            | null -> invalidArg "Name" ("Cannot find child control or resource named: " + name)
            | resource -> unbox resource
        | control -> unbox control
    
    interface IView<'E, 'M> with
        member this.Subscribe observer = 
            let xs = List.reduce Observable.merge this.EventStreams 
            xs.Subscribe observer
        member this.SetBindings model = 
            window.DataContext <- model; 
            this.SetBindings model

    abstract EventStreams : IObservable<'E> list
    abstract SetBindings : 'M -> unit

[<AbstractClass>]
type XamlView<'E, 'M>(resourceLocator) = 
    inherit View<'E, 'M, Window>(Application.LoadComponent resourceLocator |> unbox)
