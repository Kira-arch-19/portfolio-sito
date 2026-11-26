using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Portfolio.Tests.Helpers;

internal sealed class FakeTempDataProvider : ITempDataProvider
{
    private readonly IDictionary<string, object> _values =
        new Dictionary<string, object>();

    public IDictionary<string, object> LoadTempData(HttpContext context) =>
        new Dictionary<string, object>(_values);

    public void SaveTempData(
        HttpContext context,
        IDictionary<string, object> values)
    {
        _values.Clear();
        foreach (var item in values)
            _values[item.Key] = item.Value;
    }
}
