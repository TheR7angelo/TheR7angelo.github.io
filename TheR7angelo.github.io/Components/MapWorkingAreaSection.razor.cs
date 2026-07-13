using System.Reflection;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Blazor;
using MudBlazor;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TheR7angelo.github.io.Pages;
using Color = Mapsui.Styles.Color;
using IFeature = Mapsui.IFeature;

namespace TheR7angelo.github.io.Components;

public partial class MapWorkingAreaSection(ILogger<Home> logger, IDialogService dialogService) : IDisposable
{
    private const string CityBoundaryResourceName = "TheR7angelo.github.io.Resources.Data.city_boundary.geojson";
    private const string CompanySiteResourceName = "TheR7angelo.github.io.Resources.Data.company_site.geojson";

    private const string CityBoundaryLayerName = "City Boundary";
    private const string CompanySiteLayerName = "Company Site";

    private const string IconAttributeName = "icon";
    private const string EmbeddedAssetPrefix = "embedded://TheR7angelo.github.io.Resources.Data.Assets.";

    private const double ExtentMarginRatio = 0.1;

    private const double DefaultCompanySiteSymbolScale = 0.8;
    private const double DefaultIconScale = 0.5;

    private const double CompanySiteMaxVisible = 200;

    private const double IconMaxVisible = 200;

    private static readonly Offset DefaultIconOffset = new(0, 75);

    private readonly List<ILayer> _layers = [];

    private MapControl? _mapControl;
    private bool _isMapInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isMapInitialized)  return;

        if (_mapControl is null) return;

        _isMapInitialized = true;

        try
        {
            InitializeMap();

            StateHasChanged();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occur");
            _isMapInitialized = false;
        }
    }

    private void InitializeMap()
    {
        AddBaseMapLayer();
        RegisterMapEvents();
        LoadWorkingAreaLayers();

        if (_layers.Count is 0)
        {
            logger.LogWarning("No GIS layers were loaded");
            return;
        }

        _mapControl!.Map.Layers.Add(_layers);

        ZoomToLayersExtent();
    }

    private void AddBaseMapLayer()
    {
        _mapControl!.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
    }

    private void RegisterMapEvents()
    {
        _mapControl!.Map.Info += MapOnInfo;
    }

    private void LoadWorkingAreaLayers()
    {
        AddLayerIfAvailable(CreateCityBoundaryLayer());
        AddLayerIfAvailable(CreateCompanySiteLayer());
    }

    private ILayer? CreateCityBoundaryLayer()
    {
        return CreateLayerFromEmbeddedGeoJson(
            CityBoundaryResourceName,
            CityBoundaryLayerName,
            CreateCityBoundaryStyle());
    }

    private ILayer? CreateCompanySiteLayer()
    {
        return CreateLayerFromEmbeddedGeoJson(
            CompanySiteResourceName,
            CompanySiteLayerName,
            CreateCompanySiteStyle());
    }

    private void AddLayerIfAvailable(ILayer? layer)
    {
        if (layer is null)
        {
            return;
        }

        _layers.Add(layer);
    }

    private static VectorStyle CreateCityBoundaryStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(Color.FromArgb(127, 63, 81, 181)),
            Line = new Pen
            {
                Color = Color.FromArgb(255, 63, 81, 181),
                Width = 2
            }
        };
    }

    private static SymbolStyle CreateCompanySiteStyle()
    {
        return new SymbolStyle
        {
            SymbolType = SymbolType.Triangle,
            SymbolScale = DefaultCompanySiteSymbolScale,
            Fill = new Brush(Color.FromArgb(127, 255, 87, 34)),
            Line = new Pen
            {
                Color = Color.FromArgb(255, 255, 255, 255),
                Width = 2
            },
            SymbolRotation = 180,
            MaxVisible = CompanySiteMaxVisible
        };
    }

    private void ZoomToLayersExtent()
    {
        var extent = GetGlobalLayersExtent();

        if (extent is null)
        {
            logger.LogWarning("Unable to zoom to layers extent because no valid extent was found");
            return;
        }

        var extentWithMargin = AddMarginToExtent(extent);

        _mapControl!.Map.Navigator.ZoomToBox(extentWithMargin);
    }

    private MRect? GetGlobalLayersExtent()
    {
        return _layers
            .Select(layer => layer.Extent)
            .OfType<MRect>()
            .Aggregate<MRect?, MRect?>(
                null,
                static (current, extent) => current is null
                    ? extent
                    : current.Join(extent));
    }

    private static MRect AddMarginToExtent(MRect extent)
    {
        var marginX = extent.Width * ExtentMarginRatio;
        var marginY = extent.Height * ExtentMarginRatio;

        return extent.Grow(marginX, marginY);
    }

    private void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var mapInfo = e.GetMapInfo(_layers);

        if (mapInfo.Feature is null)
        {
            return;
        }

        ShowFeatureInfoDialog(mapInfo.Feature);
    }

    private void ShowFeatureInfoDialog(IFeature feature)
    {
        var parameters = new DialogParameters<MapWorkingAreaDialog>
        {
            { dialog => dialog.Feature, feature }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        _ = dialogService.ShowAsync<MapWorkingAreaDialog>(
            title: string.Empty,
            parameters,
            options);
    }

    private GenericCollectionLayer<List<IFeature>>? CreateLayerFromEmbeddedGeoJson(
        string resourceName,
        string layerName,
        IStyle? layerStyle = null)
    {
        logger.LogInformation(
            "Loading GIS layer '{LayerName}' from embedded resource '{ResourceName}'",
            layerName,
            resourceName);

        try
        {
            var geoJson = ReadEmbeddedResource(resourceName);
            var featureCollection = ReadFeatureCollection(geoJson);

            if (featureCollection is null || featureCollection.Count is 0)
            {
                logger.LogWarning(
                    "GeoJSON resource '{ResourceName}' contains no features for layer '{LayerName}'",
                    resourceName,
                    layerName);

                return null;
            }

            var features = featureCollection
                .Select(ConvertToMapsuiFeature)
                .ToList();

            logger.LogInformation(
                "Successfully created GIS layer '{LayerName}' with {FeatureCount} features",
                layerName,
                features.Count);

            return new GenericCollectionLayer<List<IFeature>>
            {
                Name = layerName,
                Features = features,
                Style = layerStyle
            };
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create GIS layer '{LayerName}' from embedded resource '{ResourceName}'",
                layerName,
                resourceName);

            throw;
        }
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new FileNotFoundException(
                $"Embedded resource not found: {resourceName}",
                resourceName);
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static FeatureCollection? ReadFeatureCollection(string geoJson)
    {
        var reader = new GeoJsonReader();

        return reader.Read<FeatureCollection>(geoJson);
    }

    private static IFeature ConvertToMapsuiFeature(NetTopologySuite.Features.IFeature ntsFeature)
    {
        var geometry = ProjectToWebMercator(ntsFeature.Geometry);
        var mapsuiFeature = new GeometryFeature(geometry);

        var icon = CopyAttributes(ntsFeature, mapsuiFeature);

        if (!string.IsNullOrWhiteSpace(icon))
        {
            mapsuiFeature.Styles.Add(CreateIconStyle(icon));
        }

        return mapsuiFeature;
    }

    private static Geometry ProjectToWebMercator(Geometry geometry)
    {
        var projectedGeometry = geometry.Copy();

        projectedGeometry.Apply(new CoordinateFilter(coordinate =>
        {
            var projectedPoint = SphericalMercator.FromLonLat(
                coordinate.X,
                coordinate.Y);

            coordinate.X = projectedPoint.x;
            coordinate.Y = projectedPoint.y;
        }));

        return projectedGeometry;
    }

    private static string? CopyAttributes(
        NetTopologySuite.Features.IFeature sourceFeature,
        IFeature targetFeature)
    {
        if (sourceFeature.Attributes is null)
        {
            return null;
        }

        string? icon = null;

        foreach (var attributeName in sourceFeature.Attributes.GetNames())
        {
            var value = sourceFeature.Attributes[attributeName];

            targetFeature[attributeName] = value;

            if (string.Equals(attributeName, IconAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                icon = value?.ToString();
            }
        }

        return icon;
    }

    private static ImageStyle CreateIconStyle(string icon)
    {
        return new ImageStyle
        {
            Image = $"{EmbeddedAssetPrefix}{icon}",
            SymbolScale = DefaultIconScale,
            Offset = DefaultIconOffset,
            MaxVisible = IconMaxVisible
        };
    }

    public void Dispose()
    {
        _mapControl?.Map.Info -= MapOnInfo;
        GC.SuppressFinalize(this);
    }
}