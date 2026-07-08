using System.Reflection;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Blazor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using TheR7angelo.github.io.Pages;
using Color = Mapsui.Styles.Color;
using IFeature = Mapsui.IFeature;

namespace TheR7angelo.github.io.Components;

public partial class MapWorkingAreaSection
{
    [Inject]
    private ILogger<Home> Logger { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private MapControl? _mapControl;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (!firstRender) return;
        _mapControl?.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        if (_mapControl is not null) _mapControl.Map.Info += MapOnInfo;

        const string cityBoundaryGeojson = "TheR7angelo.github.io.Resources.Data.city_boundary.geojson";
        const string companySiteGeojson = "TheR7angelo.github.io.Resources.Data.company_site.geojson";

        var cityBoundaryStyle = new VectorStyle
        {
            Fill = new Brush(Color.FromArgb(127, 63, 81, 181)),
            Line = new Pen
            {
                Color = Color.FromArgb(255, 63, 81, 181),
                Width = 2
            }
        };

        var companySiteStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = 0.8,
            Fill = new Brush(Color.FromArgb(127, 255, 87, 34)),
            Line = new Pen
            {
                Color = Color.FromArgb(255, 255, 255, 255),
                Width = 2
            },
            MaxVisible = 2000
        };

        var layerCityBoundary = CreateLayerFromEmbeddedGeoJson(cityBoundaryGeojson, "City Boundary", cityBoundaryStyle, Logger);
        var layerCompanySite = CreateLayerFromEmbeddedGeoJson(companySiteGeojson, "Company Site", companySiteStyle, Logger);

        if (layerCityBoundary is not null) Layers.Add(layerCityBoundary);
        if (layerCompanySite is not null) Layers.Add(layerCompanySite);

        _mapControl?.Map.Layers.Add(Layers);

        var globalExtent = Layers.Select(layer => layer.Extent)
            .OfType<MRect>()
            .Aggregate<MRect?, MRect?>(null, (current, layerExtent) => current is null
                ? layerExtent
                : current.Join(layerExtent));

        if (globalExtent is null) return;
        var marginX = globalExtent.Width * 0.1;
        var marginY = globalExtent.Height * 0.1;

        var extentWithMargin = globalExtent.Grow(marginX, marginY);

        _mapControl?.Map.Navigator.ZoomToBox(extentWithMargin);
    }

    private List<ILayer> Layers { get; } = [];

    private void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var mapInfo = e.GetMapInfo(Layers);
        if (mapInfo.Feature is null) return;

        ShowFeatureInfoDialogAsync(mapInfo.Feature);
    }

    private void ShowFeatureInfoDialogAsync(IFeature feature)
    {
        var parameters = new DialogParameters<MapWorkingAreaDialog>
        {
            { x => x.Feature, feature }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.ShowAsync<MapWorkingAreaDialog>(string.Empty, parameters, options);
    }

    private GenericCollectionLayer<List<IFeature>>? CreateLayerFromEmbeddedGeoJson(
        string resourceName,
        string layerName,
        IStyle? layerStyle = null,
        ILogger? logger = null)
    {
        logger?.LogInformation("Starting to load GIS layer '{LayerName}' from embedded resource '{ResourceName}'",
            layerName, resourceName);

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                var fileNotFoundEx = new FileNotFoundException($"Embedded resource not found: {resourceName}");
                logger?.LogError(fileNotFoundEx,
                    "Failed to locate embedded resource '{ResourceName}' for layer '{LayerName}'", resourceName,
                    layerName);
                throw fileNotFoundEx;
            }

            using var reader = new StreamReader(stream);
            var geoJsonString = reader.ReadToEnd();

            var geoJsonReader = new GeoJsonReader();
            var featureCollection = geoJsonReader.Read<FeatureCollection>(geoJsonString);

            if (featureCollection is null || featureCollection.Count is 0)
            {
                logger?.LogWarning("The GeoJSON resource '{ResourceName}' yielded no features for layer '{LayerName}'",
                    resourceName, layerName);
                return null;
            }

            var mapsuiFeatures = new List<IFeature>();
            foreach (var ntsFeature in featureCollection)
            {
                var geometry = ntsFeature.Geometry;
                geometry.Apply(new NetTopologySuite.Geometries.CoordinateFilter(c =>
                {
                    var projectedPoint = SphericalMercator.FromLonLat(c.X, c.Y);
                    c.X = projectedPoint.x;
                    c.Y = projectedPoint.y;
                }));

                var mapsuiFeature = new GeometryFeature(geometry);

                if (ntsFeature.Attributes is not null)
                {
                    foreach (var attributeName in ntsFeature.Attributes.GetNames())
                    {
                        mapsuiFeature[attributeName] = ntsFeature.Attributes[attributeName];
                    }
                }

                mapsuiFeatures.Add(mapsuiFeature);
            }

            var layer = new GenericCollectionLayer<List<IFeature>>
            {
                Name = layerName,
                Features = mapsuiFeatures,
                Style = layerStyle
            };

            logger?.LogInformation("Successfully created GIS layer '{LayerName}' with {FeatureCount} features",
                layerName, mapsuiFeatures.Count);
            return layer;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An unhandled exception occurred while generating GIS layer '{LayerName}' from resource '{ResourceName}'",
                layerName, resourceName);
            throw;
        }
    }
}