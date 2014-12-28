using Xamarin.Forms;

namespace MiddleMeeter {
  public class App {
    public static Page GetMainPage() {
      return new NavigationPage(new SearchPage());
    }
  }
}
