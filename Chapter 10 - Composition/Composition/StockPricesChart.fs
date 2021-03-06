﻿namespace FSharp.Windows.Sample

open System.Windows.Data
open System.Drawing
open System.Windows.Forms.DataVisualization.Charting
open System.Collections.ObjectModel

open FSharp.Windows
open FSharp.Windows.UIElements

[<AbstractClass>]
type StockPricesChartModel() = 
    inherit Model()

    abstract StockPrices : ObservableCollection<string * decimal> with get, set

type StockPricesChartView(control) as this =
    inherit PartialView<unit, StockPricesChartModel, StockPricesChartControl>(control)

    do 
        let area = new ChartArea() 
        area.AxisX.MajorGrid.LineColor <- Color.LightGray
        area.AxisY.MajorGrid.LineColor <- Color.LightGray        
        this.Control.StockPricesChart.ChartAreas.Add area
        let series = 
            new Series(
                ChartType = SeriesChartType.Column, 
                Palette = ChartColorPalette.EarthTones, 
                XValueMember = "Item1", 
                YValueMembers = "Item2")
        this.Control.StockPricesChart.Series.Add series
    
    override this.EventStreams = 
        [
            this.Control.AddStock.Click |> Observable.mapTo()
        ]

    override this.SetBindings model = 
        this.Control.StockPricesChart.DataSource <- model.StockPrices
        model.StockPrices.CollectionChanged.Add(fun _ -> this.Control.StockPricesChart.DataBind())

type StockPricesChartController() = 
    inherit Controller<unit, StockPricesChartModel>()

    override this.InitModel model = 
        model.StockPrices <- ObservableCollection()

    override this.Dispatcher = fun() -> 
        Async <| fun model ->
            async {
                let! result = Mvc.startWindow(StockPickerView(), StockPickerController())  
                result |> Option.iter (fun stockInfo ->
                    model.StockPrices.Add(stockInfo.Symbol, stockInfo.LastPrice)
                )
            }

