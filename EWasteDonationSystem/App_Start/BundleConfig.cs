// Project documentation note: This file contains commented code for easier understanding.
using System.Web.Optimization;

namespace EWasteDonationSystem
{
    public class BundleConfig
    {
        // Registers shared and page-specific CSS / JS bundles for the project.
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/assets/styles.css",
                         "~/Content/assets/style.css"));

            bundles.Add(new ScriptBundle("~/bundles/app")
                .Include("~/Scripts/assets/app.js"));
        }
    }
}
