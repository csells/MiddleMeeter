using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiddleMeeter {
  public class Geocode {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }

  public class Place {
    public string Name { get; set; }
    public string Vicinity { get; set; }
    public Geocode Location { get; set; }
    public Uri Icon { get; set; }
  }

  class Geocoding {
    public async Task<Geocode> GetGeocodeForLocationAsync(string addr) {
      string url = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}", Uri.EscapeUriString(addr));
      var xml = await (new HttpClient()).GetStringAsync(url);
      var loc = XDocument.Parse(xml).Element("GeocodeResponse").Element("result").Element("geometry").Element("location");
      return new Geocode {
        Latitude = double.Parse(loc.Element("lat").Value),
        Longitude = double.Parse(loc.Element("lng").Value),
      };
    }

    public Geocode GetGreatCircleMidpoint(Geocode g1, Geocode g2) {
      Func<double, double> deg2rad = deg => deg * (Math.PI / 180.0);
      Func<double, double> rad2deg = rad => rad / (Math.PI / 180.0);

      // convert to radians
      var dLon = deg2rad(g2.Longitude - g1.Longitude);
      var lat1 = deg2rad(g1.Latitude);
      var lat2 = deg2rad(g2.Latitude);
      var lon1 = deg2rad(g1.Longitude);

      // calculate the great circle midpoint
      var Bx = Math.Cos(lat2) * Math.Cos(dLon);
      var By = Math.Cos(lat2) * Math.Sin(dLon);
      var lat3 = Math.Atan2(Math.Sin(lat1) + Math.Sin(lat2), Math.Sqrt((Math.Cos(lat1) + Bx) * (Math.Cos(lat1) + Bx) + By * By));
      var lon3 = lon1 + Math.Atan2(By, Math.Cos(lat1) + Bx);

      // convert to degrees
      return new Geocode {
        Latitude = rad2deg(lat3),
        Longitude = rad2deg(lon3),
      };
    }

    public async Task<Place[]> GetNearbyPlaces(Geocode g, string keyword) {
      string request = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/xml?location={0},{1}&rankby=distance&keyword={2}&key={3}", g.Latitude, g.Longitude, keyword, GetGoogleApiKey());
      var xml = await (new HttpClient()).GetStringAsync(request);
      var results = XDocument.Parse(xml).Element("PlaceSearchResponse").Elements("result");

      var places = new List<Place>();
      foreach (var result in results) {
        var loc = result.Element("geometry").Element("location");
        var icon = result.Element("icon").Value;
        places.Add(new Place {
          Name = result.Element("name").Value,
          Icon = !string.IsNullOrWhiteSpace(icon) ? new Uri(icon) : null,
          Vicinity = result.Element("vicinity").Value,
          Location = new Geocode {
            Latitude = double.Parse(loc.Element("lat").Value),
            Longitude = double.Parse(loc.Element("lng").Value),
          },
        });
      }

      return places.ToArray();
    }

    string GetGoogleApiKey() {
      #region HIDDEN
      return "AIzaSyAKyMHMp3scZ4kEcdRu0Kc2F3p5Xv86HfE";
      #endregion
    }

  }
}
